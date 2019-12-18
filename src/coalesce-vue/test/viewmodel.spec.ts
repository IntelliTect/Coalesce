

import Vue from 'vue';
import { AxiosClient, AxiosItemResult, AxiosListResult, ItemApiState } from '../src/api-client'
import { mapToDto, mapToModel } from '../src/model';

import { StudentViewModel, CourseViewModel, AdvisorViewModel, StudentListViewModel } from './targets.viewmodels';
import { Student, Advisor, Course } from './targets.models';
import * as metadata from './targets.metadata';
import { ViewModelCollection } from '../src/viewmodel';
import { ItemResult } from '../lib/api-client';
import { AxiosResponse } from 'axios';


async function waitFor(ms: number) {
   await new Promise(resolve => setTimeout(resolve, ms));
}

describe("ViewModel", () => {

  describe.each(
    ['$load', '$save', '$delete',] as const
  )("%s", (callerName) => {
    test("caller is lazily created", async () => {
      // This test ensures that vue doesn't create them for us
      // as a consequence of it trying to enumerate properties
      // on our objects in order to add reactivity.

      const vue = new Vue({
        data: {
          student: new StudentViewModel
        }
      });

      // ViewModel shouldn't have its own property descriptor for callerName.
      // The descriptor will be on the prototype.
      expect(Object.getOwnPropertyDescriptor(vue.student, callerName))
        .toBeUndefined()

      // Access the caller to cause it to be created.
      vue.student[callerName];

      // Instance should now have its own caller.
      expect(Object.getOwnPropertyDescriptor(vue.student, callerName))
        .not.toBeUndefined();
      expect(vue.student[callerName]).toBeInstanceOf(ItemApiState);
      
      // Multiple access should yield the same instance.
      expect(vue.student[callerName]).toBe(vue.student[callerName]);

      // Instances should be reactive:
      const callbackFn = jest.fn();
      vue.$watch(`student.${callerName}.isLoading`, callbackFn);
      vue.student[callerName].isLoading = true;
      await vue.$nextTick();
      // Watcher should have been triggered for isLoading.
      expect(callbackFn).toBeCalledTimes(1);

    })
  })

  describe("save", () => {
    const saveMock = jest.fn().mockResolvedValue(<AxiosItemResult<Student>>{data: {
      wasSuccessful: true,
      object: {
        studentId: 3,
        name: "Bob",
        studentAdvisorId: null,
        advisor: null,
      }
    }});

    test("updates collection navigation entities with new PK", async () => {
      const student = new StudentViewModel({
        name: "Bob",
        courses: [
          { name: "a", },
          { name: "b", },
        ]
      });

      student.$apiClient.save = jest.fn().mockResolvedValue(<AxiosItemResult<Student>>{data: {
        wasSuccessful: true,
        object: {
          studentId: 3,
          name: "Bob",
          // It must still work if the server returns an empty array.
          // The unsaved items (i.e. those without a PK) should be preserved on the client.
          courses: [] as Course[],
          studentAdvisorId: null,
          advisor: null,
        }
      }});
      await student.$save()

      expect(student.courses).toHaveLength(2);
      expect(student.courses![0].name).toBe("a");
      expect(student.courses![0].studentId).toBe(3);
      expect(student.courses![1].name).toBe("b");
      expect(student.courses![1].studentId).toBe(3);
    })

    test("updates $parent's FK prop with new PK", async () => {
      const student = new StudentViewModel({
        name: "Bob",
        advisor: { name: "Phoebe" },
      });

      const advisor = student.advisor!;
      expect((advisor as any).$parent).toBe(student);

      advisor.$apiClient.save = jest.fn().mockResolvedValue(<AxiosItemResult<Advisor>>{data: {
        wasSuccessful: true,
        object: {
          advisorId: 3,
          name: "Phoebe",
        }
      }});
      await advisor.$save()

      expect(advisor.advisorId).toBe(3);
      expect(student.studentAdvisorId).toBe(3);
    })
  })

  describe("autoSave", () => {

    test("when model is dirtied while creation save is in-flight, new PK is still set", async () => {
      // When an auto-save is performed on a new object (i.e. a create, not an update),
      // and then that object is modified while the save is in flight,
      // we need to be sure that the PK that comes back in the save response
      // is still set on the object, even if all the other data fields are not
      // (because the object is dirty, we don't load incoming data so we don't
      // overwrite any changes the user has been made while the save was in-flight).

      // If the PK doesn't get set, the second save for the newly-dirty data
      // will also trigger a create, instead of updating the newly-created object.

        
      const student = new StudentViewModel();
      const vue = new Vue({
        data: {
          student
        }
      });

      // Length of a "tick" in milliseconds. Smaller == faster test run.
      // If this is too small then the test wont work.
      const tickLength = 30;

      var savePromise: Promise<any>;
      const saveMock = student.$apiClient.save = jest
        .fn()
        .mockImplementation((dto: any) => {
          // Map the parameter to a new model to simulate the save response.
          // This MUST be a mapping to prevent issues with same object references.
          const result = {
            ...mapToModel(dto, student.$metadata),
            studentId: 1
          };

          return savePromise = new Promise(resolve => setTimeout(() => {
            resolve(<AxiosItemResult<Student>> {
              data: {
                wasSuccessful: true,
                object: result
              }
            })
          }, tickLength)
          )
        });

      const waitTick = async () => await new Promise(resolve => setTimeout(resolve, tickLength))

      student.$startAutoSave(vue, { wait: tickLength/2 });
      expect(student.$isDirty).toBe(false);

      student.name = "Steve";
      expect(student.$isDirty).toBe(true);

      await waitTick()
      expect(saveMock).toBeCalledTimes(1);
      // First save should have been "Steve", with no PK.
      expect((saveMock.mock.calls[0][0] as Student).name).toBe("Steve");
      expect((saveMock.mock.calls[0][0] as Student).studentId).toBe(null);

      // The save is now in-flight. Make another change to the model.
      expect(student.$isDirty).toBe(false);
      student.name = "Steve-o";
      expect(student.$isDirty).toBe(true);

      await savePromise!;
      // Wait a little longer after the save resolves
      // for the API client to process the response, for the next save to kick off, etc.
      await waitTick()

      // There should be another save in flight now.
      // Model shouldn't be dirty since we just sent off the current state.
      expect(student.$isDirty).toBe(false);
      await savePromise!;
      await waitTick()

      expect(saveMock).toBeCalledTimes(2);

      // This is the crux of what we're testing here -
      // The second save should have the PK returned by the first save.
      expect((saveMock.mock.calls[0][0] as Student).name).toBe("Steve-o");
      expect((saveMock.mock.calls[1][0] as Student).studentId).toBe(1);

      // For good measure.
      student.$stopAutoSave();
    })

    
    test("when nested model is autosaving and dirty when parent model is reloaded, nested model is not updated", async () => {
      // If hierarchy of models is reloaded by virtue of some root model being reloaded,
      // but a child model has been editing and its changes are still pending a save,
      // we don't want to overwrite those local changes.

      // However, if there is a grandchild model (child of the child),
      // and that grandchild is not dirty, it SHOULD be updated.
      // Only the basic properties of the dirty model (child, in this case) should be skipped.

      const studentData = {
        studentId: 1,
        name: "root",
        studentAdvisorId: 1,
        advisor: {
          advisorId: 1,
          name: "child",
          students: [
            {studentId: 2, name: "grandchild"}
          ]
        }
      }
      const student = new StudentViewModel(studentData);
      const vue = new Vue({
        data: {
          student
        }
      });

      // Start autosave on the whole tree (could have just as well done it on the advisor only)
      student.$startAutoSave(vue, { deep: true, wait: 0 })

      // Arbitrarily set the advisor as dirty. We don't actually need to modify a property.
      student.advisor!.$isDirty = true;

      // Update our incoming data with new values coming from the server.
      studentData.name = "root-updated";
      studentData.advisor.name = "child-updated";
      studentData.advisor.students[0].name = "grandchild-updated";
      student.$loadFromModel(studentData)

      // The two non-dirty models should have been updated with the new data.
      expect(student.name).toBe("root-updated");
      expect(student.$isDirty).toBeFalsy();
      expect(student.advisor!.students[0].name).toBe("grandchild-updated");
      expect(student.advisor!.students[0].$isDirty).toBeFalsy();

      // The dirty model should NOT have been updated,
      // and it should still be marked dirty.
      expect(student.advisor!.name).toBe("child");
      expect(student.advisor!.$isDirty).toBeTruthy();
    })

    test("$loadFromModel won't trigger autosave on root", async () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine", }
      });
      var student = new StudentViewModel(studentModel);
      const saveMock = student.$apiClient.save = 
        jest.fn().mockImplementation((dto: any) => { throw "Save shouldn't have been called" });
      const vue = new Vue({ data: { student } });

      student.$startAutoSave(vue, {wait: 0});

      studentModel.name = "NewName";
      student.$loadFromModel(studentModel);
      await waitFor(10);

      expect(saveMock).toBeCalledTimes(0);
    })

    test("$loadFromModel won't trigger autosave on ref nav", async () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine", }
      });
      var student = new StudentViewModel(studentModel);
      const saveMock = student.advisor!.$apiClient.save = 
        jest.fn().mockImplementation((dto: any) => { throw "Save shouldn't have been called" });
      const vue = new Vue({ data: { student } });

      student.advisor!.$startAutoSave(vue, {wait: 0});

      studentModel.advisor!.name = "NewName";
      student.$loadFromModel(studentModel);
      await waitFor(10);
      
      expect(saveMock).toBeCalledTimes(0);
    })

    test("$loadFromModel won't trigger autosave on collection nav", async () => {
      var studentModel = new Student({
        studentId: 1,
        courses: [ { courseId: 7, name: "foo" }, ]
      });
      var student = new StudentViewModel(studentModel);
      const saveMock = student.courses![0].$apiClient.save = 
        jest.fn().mockImplementation((dto: any) => { throw "Save shouldn't have been called" });
      const vue = new Vue({ data: { student } });

      student.courses![0].$startAutoSave(vue, {wait: 0});

      studentModel.courses![0].name = "NewName";
      student.$loadFromModel(studentModel);
      await waitFor(10);
      
      expect(saveMock).toBeCalledTimes(0);
    })

    test("triggers save immediately if model is dirty", async () => {
      var student = new StudentViewModel({
        studentId: 1, name: "bob",
      });
      const saveMock = student.$apiClient.save = 
        jest.fn().mockResolvedValue(<AxiosItemResult<any>>{data: {wasSuccessful: true}});
      const vue = new Vue({ data: { student } });

      // Start auto save. Model shouldn't be dirty, so it shouldn't hit the API.
      expect(student.$isDirty).toBe(false);
      student.$startAutoSave(vue, {wait: 0});
      await waitFor(10);
      expect(saveMock).toBeCalledTimes(0);

      // Reset autosave, dirty the model, and restart autosave.
      // It should trigger immediately.
      student.$stopAutoSave();
      student.name += "2";
      expect(student.$isDirty).toBe(true);
      student.$startAutoSave(vue, {wait: 0});
      await waitFor(10);
      expect(saveMock).toBeCalledTimes(1);
    })

    test("triggers save immediately if model lacks a PK", async () => {
      /*
        Rationale: In coalesce-vue, autosave is always an explicit choice that is made.
        This is in contrast to Knockout, where autosave is always on by default for all ViewModel instances.

        In Knockout, you wouldn't want to have a new record created in your DB 
        just because instantiated some ViewModel, especially if that ViewModel isn't attached
        to any other objects.

        In coalesce-vue, because autosave is an explicit choice, you're either
          1) Explicitly enabling autosave on a single VM instance,
              in which case you've declared intent that you want that instance to be saved
          2) Attaching a ViewModel to an object graph that has deep autosaves enabled,
              in which case you're already declared intent that you want that entire object graph saved.
              In this scenario, you could always use a predicate when setting up deep autosaves
              in order to prevent this behavior - just add an explicit check for $isDirty.
      */
      var student = new StudentViewModel({
        name: "bob",
      });
      const saveMock = student.$apiClient.save = 
        jest.fn().mockResolvedValue(<AxiosItemResult<any>>{data: {wasSuccessful: true}});
      const vue = new Vue({ data: { student } });

      // Start auto save. Model shouldn't be dirty, but does lack a PK, so autosave should trigger.
      expect(student.$isDirty).toBe(false);
      expect(student.$primaryKey).toBeFalsy();
      student.$startAutoSave(vue, {wait: 0});
      await waitFor(10);
      expect(saveMock).toBeCalledTimes(1);
    })


    const loadMock = 
      jest.fn().mockResolvedValue(<AxiosItemResult<Student>>{data: {
        wasSuccessful: true,
        object: {
          studentId: 1,
          name: 'bob'
        }
      }});

    test("does not trigger if enabled while model load is pending", async () => {
      var student = new StudentViewModel();
      const saveMock = student.$apiClient.save = 
        jest.fn().mockResolvedValue(<AxiosItemResult<Student>>{data: {wasSuccessful: true}});
      student.$apiClient.get = loadMock;

      const vue = new Vue({ data: { student } });

      student.$load(1);
      student.$startAutoSave(vue, {wait: 0});
      await waitFor(10);
      expect(saveMock).toBeCalledTimes(0);
    })

    test("does not trigger if enabled just before loading model", async () => {
      var student = new StudentViewModel();
      const saveMock = student.$apiClient.save = 
        jest.fn().mockResolvedValue(<AxiosItemResult<Student>>{data: {wasSuccessful: true}});
      student.$apiClient.get = loadMock;

      const vue = new Vue({ data: { student } });

      student.$startAutoSave(vue, {wait: 0});
      student.$load(1);
      await waitFor(10);
      expect(saveMock).toBeCalledTimes(0);
    })

    test("does not trigger if model has errors", async () => {
      var viewModel = new AdvisorViewModel({
        advisorId: 1, name: null
      });
      const saveMock = viewModel.$apiClient.save = 
        jest.fn().mockResolvedValue(<AxiosItemResult<Advisor>>{data: {wasSuccessful: true}});

      const vue = new Vue({ data: { viewModel } });

      viewModel.$startAutoSave(vue, {wait: 0});
      expect([...viewModel.$getErrors()]).toHaveLength(1)
      await waitFor(10);
      expect(saveMock).toBeCalledTimes(0);

      viewModel.name = "Bob";
      expect([...viewModel.$getErrors()]).toHaveLength(0)
      await waitFor(10);
      expect(saveMock).toBeCalledTimes(1);
    })

    describe("deep", () => {
      test("propagates to existing related objects", async () => {
      
        var studentModel = new Student({
          studentId: 1,
          courses: [ { studentId: 1, courseId: 7, name: "foo" }, ],
          studentAdvisorId: 3,
          advisor: { advisorId: 3, name: "Bob" }
        });
        var student = new StudentViewModel(studentModel);
        const saveMock = 
          student.courses![0].$apiClient.save = 
          student.advisor!.$apiClient.save = 
          student.$apiClient.save = 
          jest.fn().mockResolvedValue(<AxiosItemResult<any>>{data: {wasSuccessful: true}});
        const vue = new Vue({ data: { student } });

        student.$startAutoSave(vue, { wait: 0, deep: true });

        student.courses![0].name = "NewName";
        student.advisor!.name = "NewName";
        student.name = "NewName";

        await waitFor(10);
        
        // One save expected for each model that changed.
        expect(saveMock).toBeCalledTimes(3);
      })

      test("propagates to new collection navigation items", async () => {
      
        var studentModel = new Student({
          studentId: 1,
        });
        var student = new StudentViewModel(studentModel);
        const vue = new Vue({ data: { student } });

        student.$startAutoSave(vue, { wait: 0, deep: true });

        const newModel = student.$addChild("courses");
        const saveMock = 
          newModel.$apiClient.save = 
          jest.fn().mockResolvedValue(<AxiosItemResult<any>>{data: {wasSuccessful: true}});

        await waitFor(10);
        
        // Autosave should have triggered for the new object.
        expect(saveMock).toBeCalledTimes(1);
      })

      test("propagates to new reference navigation items", async () => {
      
        var studentModel = new Student({
          studentId: 1,
        });
        var student = new StudentViewModel(studentModel);
        const vue = new Vue({ data: { student } });

        student.$startAutoSave(vue, { wait: 0, deep: true });

        const newModel = student.advisor = new AdvisorViewModel();
        const saveMock = 
          newModel.$apiClient.save = 
          jest.fn().mockResolvedValue(<AxiosItemResult<Student>>{data: {
            wasSuccessful: true,
          }});

        // Dirty the model to ensure that it has a reason to trigger.
        newModel.name = "bob";
        await waitFor(10);
        
        // Autosave should have triggered for the new object.
        expect(saveMock).toBeCalledTimes(1);
      })
    })
  })

  describe("$addChild", () => {

    test("creates ViewModels", () => {
      var student = new StudentViewModel();
      const course = student.$addChild("courses");

      expect(course).toBeInstanceOf(CourseViewModel);
    })

    test("new model has $parent and $parentCollection set", () => {
      var student = new StudentViewModel();
      const course = student.$addChild("courses");

      expect(course.$parent).toBe(student);
      expect(course.$parentCollection).toBe(student.courses);
    })

    test("new model has foreign key and $isDirty set", () => {
      var student = new StudentViewModel({studentId: 3});
      const course: CourseViewModel = student.$addChild("courses");

      expect(course).toBeInstanceOf(CourseViewModel);
      expect(course.studentId).toBe(student.studentId);
      expect(course.$isDirty).toBe(true);
    })

    test("new model has collections initialized to empty arrays", () => {
      var advisor = new AdvisorViewModel({advisorId: 3});
      const student: StudentViewModel = advisor.$addChild("students");
      
      expect(student.courses).not.toBeNull();
      expect(student.courses).toHaveLength(0);
    })
  })

  describe("validation", () => {
    test("$removeRule adds ignore for metadata-provided rule", () => {
      var viewModel = new AdvisorViewModel();
      expect([...viewModel.$getErrors()]).toHaveLength(1)

      viewModel.$removeRule('name', 'required');
      expect([...viewModel.$getErrors()]).toHaveLength(0)
    })

    test("$removeRule removes custom rule", () => {
      var viewModel = new AdvisorViewModel();
      viewModel.$addRule('name', 'required2', v => !!v || 'custom');
      expect([...viewModel.$getErrors()]).toEqual([
        'Name is required.',
        'custom'
      ])

      viewModel.$removeRule('name', 'required2');
      expect([...viewModel.$getErrors()]).toEqual([
        'Name is required.'
      ])
    })

    test("$removeRule removes metadata rule and leaves custom rule", () => {
      var viewModel = new AdvisorViewModel();
      viewModel.$addRule('name', 'required2', v => !!v || 'custom');
      expect([...viewModel.$getErrors()]).toEqual([
        'Name is required.',
        'custom'
      ])

      viewModel.$removeRule('name', 'required');
      expect([...viewModel.$getErrors()]).toEqual([
        'custom'
      ])
    })

    test("$addRule overrides metadata-provided rule", () => {
      var viewModel = new AdvisorViewModel();
      expect([...viewModel.$getErrors()]).toEqual([
        'Name is required.'
      ])

      viewModel.$addRule('name', 'required', v => !!v || 'custom');
      expect([...viewModel.$getErrors()]).toEqual([
        'custom'
      ])
    })

    test("$addRule adds additional rules when prop has metadata-provided rules", () => {
      var viewModel = new AdvisorViewModel();
      expect([...viewModel.$getErrors()]).toEqual([
        'Name is required.'
      ])

      viewModel.$addRule('name', 'required2', v => !!v || 'custom');
      expect([...viewModel.$getErrors()]).toEqual([
        'Name is required.',
        'custom'
      ])
    })

    test("$addRule adds rules when prop has no metadata-provided rules", () => {
      var viewModel = new AdvisorViewModel();
      expect([...viewModel.$getErrors()]).toEqual([
        'Name is required.'
      ])

      viewModel.$addRule('advisorId', 'required', v => !!v || 'custom');
      expect([...viewModel.$getErrors()]).toEqual([
        'custom',
        'Name is required.',
      ])
    })
  })

  describe("getters/setters", () => {
    
    describe("value getters/setters", () => {
      test.each([
        ["number", () => new Student({studentId: 1})],
        ["string", () => new Student({studentId: 1, name: "bob"})],
        ["boolean", () => new Student({studentId: 1, isEnrolled: true})],
        ["date", () => new Student({studentId: 1, birthDate: new Date("1990-01-02T03:04:05.000-08:00")})]
      ])("%s setter doesn't trigger reactivity for unchanged value", async (_, factory) => {
        // Workaround for Jest's bad typescript support.
        const modelFactory = factory as unknown as () => Student

        var student = new StudentViewModel(modelFactory());
        
        const vue = new Vue({ data: { student } });
        const watchCallback = jest.fn();
        vue.$watch('student', watchCallback, { deep: true });

        student.$loadFromModel(modelFactory());
        await vue.$nextTick();

        // Exact same model was reloaded. There should be no changes.
        expect(watchCallback).toBeCalledTimes(0);
      })
    })

    describe("collection navigation getter/setters", () => {
      test("setter creates new ViewModelCollection", () => {
        var student = new StudentViewModel();
        student.courses = [];

        expect(student.courses.push).not.toBe(Array.prototype.push);
        expect(student.courses.push).toBe(ViewModelCollection.prototype.push);
      })

      test("collection creates ViewModel when Model is pushed", () => {
        var student = new StudentViewModel();
        student.courses = [];

        // The typings don't explicitly allow this, so we must cast to any.
        // The use case is an input component that provides models (instead of ViewModels).
        student.courses.push(new Course({
          courseId: 1,
          name: "Seagull"
        }) as any);

        expect(student.courses[0]).toBeInstanceOf(CourseViewModel);
        expect(student.courses[0].name).toBe("Seagull");
      })

      test("collection creates ViewModel when array containing a Model is set", () => {
        var student = new StudentViewModel();

        // The typings don't explicitly allow this, so we must cast to any.
        // The use case is an input component that provides models (instead of ViewModels).
        student.courses = [
          new Course({
            courseId: 1,
            name: "Seagull"
          }) as any
        ];

        expect(student.courses[0]).toBeInstanceOf(CourseViewModel);
        expect(student.courses[0].name).toBe("Seagull");
        expect((student.courses[0] as any).$parent).toBe(student);
        expect((student.courses[0] as any).$parentCollection).toBe(student.courses);
      })

      test("collection is reactive for push", async () => {
        var student = new StudentViewModel();
        student.courses = [];

        const vue = new Vue({ data: { student } });
        const watchCallback = jest.fn();
        vue.$watch('student.courses', watchCallback);

        student.courses.push(new CourseViewModel);

        await vue.$nextTick();

        expect(watchCallback).toBeCalledTimes(1);
        expect(student.courses).toHaveLength(1);
      })
    })

    describe("reference navigation & FK getter/setters", () => {
      test("setter copies foreign key", () => {
        var student = new StudentViewModel();
        var advisor = new AdvisorViewModel({advisorId: 3});
        student.advisor = advisor;
        expect(student.studentAdvisorId).toBe(advisor.advisorId);
      })

      test("setter creates ViewModel when provided a Model", () => {
        var student = new StudentViewModel();
        var advisor: Advisor = mapToModel({advisorId: 3}, metadata.Advisor);

        // The typings don't explicitly allow this, so we must cast to any.
        // The use case is an input component that provides models (instead of ViewModels).
        (student as any).advisor = advisor;

        expect(advisor).not.toBeInstanceOf(AdvisorViewModel);
        expect(student.advisor).toBeInstanceOf(AdvisorViewModel);
      })

      test("clears FK when reference is nulled", () => {
        var student = new StudentViewModel({
          studentAdvisorId: 3,
          advisor: { advisorId: 3, name: "Delphine", }
        });

        student.advisor = null;
        expect(student.studentAdvisorId).toBeNull();
      })

      test("updates FK when reference is changed", () => {
        var student = new StudentViewModel({
          studentAdvisorId: 3,
          advisor: { advisorId: 3, name: "Delphine", }
        });

        student.advisor = { advisorId: 4, name: "Beth", } as any;
        expect(student.studentAdvisorId).toBe(4);
      })

      test("clears reference when FK is nulled", () => {
        var student = new StudentViewModel({
          studentAdvisorId: 3,
          advisor: { advisorId: 3, name: "Delphine", }
        });

        student.studentAdvisorId = null;
        expect(student.advisor).toBeNull();
      })

      test("maintains reference when FK is nulled if reference has null PK", () => {
        var student = new StudentViewModel();

        student.advisor = new AdvisorViewModel({ advisorId: null, name: "Beth" });
        student.studentAdvisorId = null;
        expect(student.advisor!.name).toBe("Beth");
      })

      test("sets null FK when reference with null PK is set", () => {
        var student = new StudentViewModel({
          studentAdvisorId: 3
        });

        student.advisor = new AdvisorViewModel({ advisorId: null, name: "Beth" });
        expect(student.advisor!.name).toBe("Beth");
        expect(student.studentAdvisorId).toBeNull();
      })

      test("clears reference when FK no longer matches", () => {
        var student = new StudentViewModel({
          studentAdvisorId: 3,
          advisor: { advisorId: 3, name: "Delphine", }
        });

        student.studentAdvisorId = 4;
        expect(student.advisor).toBeNull();
      })

      test("maintains reference when FK is set to same value", () => {
        var student = new StudentViewModel({
          studentAdvisorId: 3,
          advisor: { advisorId: 3, name: "Delphine", }
        });

        const originalAdvisor = student.advisor;
        student.studentAdvisorId = 3;
        expect(student.advisor).toBe(originalAdvisor);
      })
    })
  })

  describe("$loadFromModel", () => {
    test("preserves & updates existing reference navigations when key is same", () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine", }
      });
      var student = new StudentViewModel(studentModel);

      var currentAdvisor = student.advisor;
      studentModel.advisor!.name = "Beth";
      student.$loadFromModel(studentModel);

      // Reference should be the same.
      expect(student.advisor).toBe(currentAdvisor);
      // Properties should be updated, and model shoudn't have been marked dirty.
      expect(student.advisor!.name).toBe("Beth");
      expect(student.advisor!.$isDirty).toBe(false);
    })

    test("overwrites existing reference navigations when key is different", () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine", }
      });
      var student = new StudentViewModel(studentModel);

      var currentAdvisor = student.advisor;
      studentModel.studentAdvisorId = 4;
      studentModel.advisor!.name = "Beth";
      studentModel.advisor!.advisorId = 4;
      student.$loadFromModel(studentModel);

      // Reference should be different.
      expect(student.advisor).not.toBe(currentAdvisor);
      // Properties should be updated, and new model shoudn't be dirty.
      expect(student.advisor!.name).toBe("Beth");
      expect(student.advisor!.$isDirty).toBe(false);
    })

    test("preserves existing reference navigation when incoming ref is null but key is same and non-null", () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine", }
      });
      var student = new StudentViewModel(studentModel);

      var currentAdvisor = student.advisor;
      studentModel.advisor = null;
      student.$loadFromModel(studentModel);

      // Reference should be the same.
      expect(student.advisor).toBe(currentAdvisor);
    })

    test("clears existing reference navigation when incoming ref is null but key doesn't match", () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine", }
      });
      var student = new StudentViewModel(studentModel);

      // Make the incoming reference null,
      // and make the key not match the current key.
      studentModel.studentAdvisorId = 4;
      studentModel.advisor = null;
      student.$loadFromModel(studentModel);

      // Reference should have been cleared out.
      expect(student.advisor).toBeNull();
    })

    test("clears existing reference navigation when incoming ref and key are null, and existing object has a PK", () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine", }
      });
      var student = new StudentViewModel(studentModel);
      expect(student.advisor).not.toBeNull();

      studentModel.studentAdvisorId = null;
      studentModel.advisor = null;
      student.$loadFromModel(studentModel);

      // Reference should have been cleared out.
      expect(student.advisor).toBeNull();
      expect(student.studentAdvisorId).toBeNull();
    })

    test("preserves existing reference navigation when incoming ref and key are null, and existing object has no PK", () => {
      /*
        Scenario: We instantiate an object (Student), and a related reference (Advisor) at the same time.
        We want to save both of these objects, but there's a chicken-and-egg scenario.
        The reference (Advisor) needs to be saved before the (Student)'s FK can be set.

        In a situation where both objects are being autosaved,
        we want to ensure that there is eventual consistency
        regardless of whether the saves are initiated in the order 
        (Student,Advisor,Student) or (Advisor,Student).

        To ensure this happens, we need to ensure that the `advisor` navigation isn't nulled out
        when the first save against the Student finishes.
      */
      var student = new StudentViewModel();
      var advisor = student.advisor = new AdvisorViewModel();

      // Simulate first save against Student.
      student.$loadFromModel({studentId: 1, studentAdvisorId: null, advisor: null});
      // Advisor reference should have been preserved because Advisor is an unsaved model with a null PK.
      expect(student.advisor).toBe(advisor);
    })

    test("updates foreign keys from navigation props' PKs when navigation prop is iterated second", () => {

      // Precondition: This tests the behavior if the navigation prop is iterated
      // AFTER the FK prop.
      // First, remove and re-add the nav prop so it lands at the end.
      const navProp = metadata.Student.props.advisor;
      delete metadata.Student.props.advisor;
      metadata.Student.props.advisor = navProp;

      // Then, assert that the precondition now holds.
      const values = Object.values(metadata.Student.props);
      expect(values.indexOf(metadata.Student.props.advisor))
        .toBeGreaterThan(values.indexOf(metadata.Student.props.studentAdvisorId))

      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine", }
      });
      var student = new StudentViewModel(studentModel);

      studentModel.studentAdvisorId = 4;
      studentModel.advisor!.advisorId = 4;
      studentModel.advisor!.name = "Beth";
      student.$loadFromModel(studentModel);

      // FK on student should have been updated
      // with the PK from the advisor object.

      // There was a bug where the PK was being sourced from the wrong object.
      // This only happened in rare cases where the nav prop was iterated before the FK prop.
      expect(student.studentAdvisorId).toBe(4);
    })

    test("preserves existing collection navigation when incoming is null", () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        courses: [
          { courseId: 7, name: "foo" },
        ]
      });
      var student = new StudentViewModel(studentModel);
      const existingCollection = student.courses;

      studentModel.courses = null;
      student.$loadFromModel(studentModel);

      // Collection should not have been cleared out.
      expect(student.courses).not.toBeNull();
      expect(student.courses).toBe(existingCollection);
      expect(student.courses![0].courseId).toBe(7);
    })

    test("clears existing collection navigation when incoming is []", () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        courses: [
          { courseId: 7, name: "foo" },
        ]
      });
      var student = new StudentViewModel(studentModel);
      const existingCollection = student.courses;

      studentModel.courses = [];
      student.$loadFromModel(studentModel);

      // Collection shouldn't be null, but should be empty.
      expect(student.courses).not.toBeNull();
      expect(student.courses).toBe(existingCollection);
      expect(student.courses).toHaveLength(0);
    })

    test("preserves existing collection navigation items when keys are same", () => {
      var studentModel = new Student({
        studentId: 1,
        courses: [
          { courseId: 7, name: "foo" },
          { courseId: 9, name: "bar" }
        ]
      });
      var student = new StudentViewModel(studentModel);
      const existingCollection = student.courses;

      var originalFoo = student.courses![0];
      var originalBar = student.courses![1];

      studentModel.courses![0].name = "baz";
      studentModel.courses![1].courseId = 11;
      studentModel.courses![1].name = "biz";
      student.$loadFromModel(studentModel);

      // Collection instance should have been kept the same.
      expect(student.courses).toBe(existingCollection);
      expect(student.courses).toHaveLength(2);
      
      // Key maintained: reference should be the same.
      expect(student.courses![0]).toBe(originalFoo);
      // Properties should be updated, and model shoudn't be marked dirty.
      expect(student.courses![0].name).toBe("baz");
      expect(student.courses![0].$isDirty).toBe(false);

      // Original item not present in updated items. 
      // Should not be same reference.
      expect(student.courses![1]).not.toBe(originalFoo);
      expect(student.courses![1]).not.toBe(originalBar);
      // New item should have expected properties, and be non-dirty.
      expect(student.courses![1].courseId).toBe(11);
      expect(student.courses![1].name).toBe("biz");
      expect(student.courses![1].$isDirty).toBe(false);
    })

    test.each([
      [ "delete from start", [{ courseId: 8 }, { courseId: 9 }] ],
      [ "delete from middle", [{ courseId: 7 }, { courseId: 9 }] ],
      [ "delete from end", [{ courseId: 7 }, { courseId: 8 }] ],
      [ "delete multiple", [{ courseId: 7 }] ],
      [ "replace start", [{ courseId: 1 }, { courseId: 8 }, { courseId: 9 }] ],
      [ "replace middle", [{ courseId: 7 }, { courseId: 1 }, { courseId: 9 }] ],
      [ "replace end", [{ courseId: 7 }, { courseId: 8 }, { courseId: 1 }] ],
      [ "add start", [{ courseId: 1 }, { courseId: 7 }, { courseId: 8 }, { courseId: 9 }] ],
      [ "add middle", [{ courseId: 7 }, { courseId: 1 }, { courseId: 8 }, { courseId: 9 }] ],
      [ "add end", [{ courseId: 7 }, { courseId: 8 }, { courseId: 9 }, { courseId: 1 }] ],
      [ "reorder", [{ courseId: 9 }, { courseId: 8 }, { courseId: 7 }] ],
      [ "reorder", [{ courseId: 7 }, { courseId: 9 }, { courseId: 8 }] ],
      [ "reorder", [{ courseId: 9 }, { courseId: 7 }, { courseId: 8 }] ],
      [ "reorder", [{ courseId: 8 }, { courseId: 7 }, { courseId: 9 }] ],
      [ "reorder", [{ courseId: 8 }, { courseId: 9 }, { courseId: 7 }] ],
    ])
      ("collection navigation is reactive to %s", async (name, data) => {
      var studentModel = new Student({
        studentId: 1,
        courses: [
          { courseId: 7 },
          { courseId: 8 },
          { courseId: 9 }
        ]
      });
      var student = new StudentViewModel(studentModel);
      const existingCollection = student.courses;

      const vue = new Vue({
        data: { student },
      });
      const watchCallback = jest.fn();
      vue.$watch('student.courses', watchCallback);

      studentModel.courses = (data as any[]).map(x => new Course(x));
      student.$loadFromModel(studentModel);
      
      await vue.$nextTick()

      // Collection instance should have been kept the same.
      expect(student.courses).toBe(existingCollection);

      // Verify that instances look right.
      student.courses!.forEach(c => {
        expect(c).toBeInstanceOf(CourseViewModel);
        expect((c as any).$parent).toBe(student);
        expect((c as any).$parentCollection).toBe(student.courses);
      })

      // Watcher should have been triggered because its contents changed
      expect(watchCallback).toBeCalledTimes(1);
    })

    test("collection navigation is not reactive when nothing changes", async () => {
      var studentModel = new Student({
        studentId: 1,
        courses: [
          { courseId: 7, name: "foo" },
          { courseId: 9, name: "bar" }
        ]
      });
      var student = new StudentViewModel(studentModel);
      const existingCollection = student.courses;

      const vue = new Vue({
        data: { student },
      });
      const watchCallback = jest.fn();
      vue.$watch('student.courses', watchCallback);

      student.$loadFromModel(studentModel);

      await vue.$nextTick()

      // Collection instance should have been kept the same.
      expect(student.courses).toBe(existingCollection);

      // Watcher should not have been triggered because its contents shouldnt have changed
      expect(watchCallback).toBeCalledTimes(0);
    })

    test('doesnt stackoverflow on recursive object structures', () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 1,
        advisor: {name: "Seagull", advisorId: 1},
      });
      studentModel.advisor!.students = [studentModel];

      const student = new StudentViewModel(studentModel);

      // First expectation: We made it this far without stackoverflowing.

      // Second, different ways of traversing to the same VM should result in the same reference.
      expect(student.advisor).toBe(student.advisor!.students[0].advisor);

      // The root VM (`student`) should also be subject to this logic,
      // so the root should be the same instance seen in the advisor's students array.
      expect(student).toBe(student.advisor!.students[0]);    
    });
  })

  describe("$delete", () => {
    test("removes deleted item from parent collection if item has PK", async () => {
      var student = new StudentViewModel(new Student({
        studentId: 1,
        courses: [ { courseId: 7, name: "foo" }, ]
      }));

      const course = student.courses![0];

      const deleteMock = course.$apiClient.delete = 
        jest.fn().mockResolvedValue(<AxiosItemResult<any>>{ data: { wasSuccessful: true } });

      await course.$delete();

      expect(deleteMock).toBeCalledTimes(1);
      expect(student.courses).toHaveLength(0);
    })
    
    test("removes deleted item from parent collection if item has no PK", async () => {
      var student = new StudentViewModel(new Student({
        studentId: 1,
        courses: [ { name: "foo" }, ]
      }));

      const course = student.courses![0];

      const deleteMock = course.$apiClient.delete = 
        jest.fn().mockResolvedValue(<AxiosItemResult<any>>{ data: { wasSuccessful: true } });

      await course.$delete();

      expect(deleteMock).toBeCalledTimes(0);
      expect(student.courses).toHaveLength(0);
    })
  })

  describe("ctor", () => {
    test("preserves ViewModel instances in initialData", () => {
      var advisorVM = new AdvisorViewModel({
        advisorId: 1,
        name: "Steve",
      });

      var studentVM = new StudentViewModel({
        advisor: advisorVM
      });

      // Should be the exact same reference.
      expect(studentVM.advisor).toBe(advisorVM);
    })
  })
})

describe("ListViewModel", () => {
  describe("$load & $items", () => {

    let list: StudentListViewModel;
    let includeAdditionalItemAtStart: boolean;

    beforeEach(() => {
      includeAdditionalItemAtStart = false;
      list = new StudentListViewModel;
      list.$apiClient.list = jest
        .fn()
        .mockImplementation((dto: any) => {
          return Promise.resolve(<AxiosListResult<Student>> {
            data: {
              wasSuccessful: true,
              list: [
                ...(includeAdditionalItemAtStart 
                  ? [new Student({studentId:3, name: 'John'})]
                  : []),
                new Student({studentId:1, name: 'Steve'}),
                new Student({studentId:2, name: 'Bob'}),
              ],
              page: 1,
              pageSize: 10,
              pageCount: 1,
              totalCount: 2,
            }
          })
        });
    })

    test("props on objects in $items are reactive", async () => {
      await list.$load();

      const vue = new Vue({
        data: { list },
        computed: {
          name() {
            return this.list.$items[1].name
          }
        }
      });
      const watchCallback = jest.fn();
      vue.$watch('name', watchCallback);

      await vue.$nextTick();
      expect(watchCallback).toBeCalledTimes(0);

      list.$items[1].name = "Heidi";
      await vue.$nextTick();

      expect(watchCallback).toBeCalledTimes(1);
      expect(list.$items[1].name).toBe("Heidi");
    })

    test("$items is reactive", async () => {
      await list.$load();

      const vue = new Vue({data: { list }});
      const watchCallback = jest.fn();
      vue.$watch('list.$items', watchCallback);

      list.$items.push(new StudentViewModel({studentId: 3, name: "Heidi"}))

      await vue.$nextTick();

      expect(watchCallback).toBeCalledTimes(1);
      expect(list.$items).toHaveLength(3);
    })

    test("identical loads do not trigger reactivity on $items", async () => {
      // This tests a performance scenario - avoid triggering
      // reactivity on list.$items if absolutely nothing changed.

      const vue = new Vue({
        data: { list }
      });
        
      // First load. Will only include the first 2 items.
      await list.$load();

      // Start watching the list's items collection
      await vue.$nextTick();
      const watchCallback = jest.fn();
      vue.$watch('list.$items', watchCallback);

      // Reload the list with the exact same response from the API.
      await vue.$nextTick();
      await list.$load();
      await vue.$nextTick();

      // The watcher should not have been triggered.
      expect(watchCallback).toBeCalledTimes(0);
    })

    test("newly loaded additional items are reactive", async () => {
      // This test is a doozy. 
      // There was an issue where if a list endpoint returns a new,
      // never-before-seen item at the start of its result set
      // when the result set also includes items that it HAS already seen,
      // the new items wouldn't be reactive.

      // This was happening because of the following process:
      // 1) `new ViewModelCollection` is created. It is non-reactive (as expected).
      // 2) a new ViewModel for the never-before-seen item is created
      //    because no item with a matching PK is found in the previous list contents.
      // 3) The first previously-seen item is handled by the .map() call.
      //    As part of being mapped, its $parentCollection property is set to the VMC.
      //    Because the VMC is not yet reactive, but the previously-seen item IS reactive
      //    (since the same ViewModel instance will be reused),
      //    upon setting the VMC to the $parentCollection property, Vue will make the VMC reactive.
      // 4) After mapping all items to either new or updated ViewModels,
      //    the results of the map() call are passed to the `.push()` method of the VMC.
      //    However, this is the `.push()` method from BEFORE the VMC was made reactive.
      //    
      // The issue is that we were grabbing the `.push()` method before doing the mapping,
      // not accounting for the fact that the .push() on the VMC could change as part of the mapping.
      
      const vue = new Vue({
        data: { list },
        computed: {
          name() {
            return this.list.$items[0].name
          }
        }
      });
      const watchCallback = jest.fn();
        
      // First load. Will only include the first 2 items.
      await list.$load();
      expect(list.$items).toHaveLength(2);

      // Next load should include a new item at the start of the list.
      includeAdditionalItemAtStart = true;
      await list.$load();
      expect(list.$items).toHaveLength(3);

      // Start watching the name of the first item in the list.
      await vue.$nextTick();
      vue.$watch('name', watchCallback);

      // Change the name of the first item in the list.
      list.$items[0].name = "Heidi"
      await vue.$nextTick();

      // The watcher should have been triggered.
      expect(watchCallback).toBeCalledTimes(1);
    })

    test("preserves same objects for same-keyed results", async () => {
      await list.$load();

      const item0 = list.$items[0]
      , item1 = list.$items[1];

      // Modify a prop in the initial set of items to
      // make sure that the objects are reloaded with the incoming data.
      item0.name = "Heidi";

      await list.$load();

      expect(item0).toBe(list.$items[0]);
      expect(item1).toBe(list.$items[1]);
      expect(item0.name).toBe('Steve');
    })

  })
})

/* TODO: 
  Deep autosave (propagate a parent's autosave state to its related models) */