

import Vue from 'vue';
import { AxiosClient, AxiosItemResult } from '../src/api-client'
import { mapToDto, mapToModel } from '../src/model';

import { StudentViewModel, CourseViewModel, AdvisorViewModel } from './targets.viewmodels';
import { Student, Advisor, Course } from './targets.models';
import * as metadata from './targets.metadata';
import { ViewModelCollection } from '../src/viewmodel';
import { ItemResult } from '../lib/api-client';
import { AxiosResponse } from 'axios';


async function waitFor(ms: number) {
   await new Promise(resolve => setTimeout(resolve, ms));
}

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
    expect(saveMock.mock.calls.length).toBe(1);
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

    expect(saveMock.mock.calls.length).toBe(2);

    // This is the crux of what we're testing here -
    // The second save should have the PK returned by the first save.
    expect((saveMock.mock.calls[0][0] as Student).name).toBe("Steve-o");
    expect((saveMock.mock.calls[1][0] as Student).studentId).toBe(1);

    // For good measure.
    student.$stopAutoSave();
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

    expect(saveMock.mock.calls.length).toBe(0);
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
    
    expect(saveMock.mock.calls.length).toBe(0);
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
    
    expect(saveMock.mock.calls.length).toBe(0);
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
  })

  test("collection is reactive for push", async () => {
    var student = new StudentViewModel();
    student.courses = [];

    const vue = new Vue({ data: { student } });
    const watchCallback = jest.fn();
    vue.$watch('student.courses', watchCallback);

    student.courses.push(new CourseViewModel);

    await vue.$nextTick();

    expect(watchCallback.mock.calls).toHaveLength(1);
    expect(student.courses).toHaveLength(1);
  })
})

describe("reference navigation getter/setters", () => {
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

  test("preserves existing reference navigation when incoming ref is null but key is same", () => {
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

  test("clears existing reference navigation when incoming ref and key are null", () => {
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

    studentModel.courses = null;
    student.$loadFromModel(studentModel);

    // Collection should have been cleared out.
    expect(student.courses).not.toBeNull();
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

    studentModel.courses = [];
    student.$loadFromModel(studentModel);

    // Collection shouldn't be null, but should be empty.
    expect(student.courses).not.toBeNull();
    expect(student.courses!.length).toBe(0);
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

    var originalFoo = student.courses![0];
    var originalBar = student.courses![1];

    studentModel.courses![0].name = "baz";
    studentModel.courses![1].courseId = 11;
    studentModel.courses![1].name = "biz";
    student.$loadFromModel(studentModel);

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
})

describe("$delete", () => {
  test("removes deleted item from parent collection", async () => {

    var student = new StudentViewModel(new Student({
      studentId: 1,
      courses: [ { courseId: 7, name: "foo" }, ]
    }));

    const course = student.courses![0];

    const deleteMock = course.$apiClient.delete = 
      jest.fn().mockResolvedValue(<AxiosItemResult<any>>{ data: { wasSuccessful: true } });

    await course.$delete();

    expect(deleteMock.mock.calls.length).toBe(1);
    expect(student.courses!.length).toBe(0);
  })
})


/* TODO: 
  Handle $parent in $loadFromModel (like knockout does).
  Deep autosave (propagate a parent's autosave state to its related models) */