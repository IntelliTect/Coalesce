import Vue, {
  computed,
  defineComponent,
  getCurrentInstance,
  inject,
  nextTick,
  ref,
  watch,
} from "vue";
import { AxiosRequestConfig, AxiosResponse } from "axios";

import { mount } from "@vue/test-utils";
import { delay, destroy, mountData, mockEndpoint } from "./test-utils";

import { ModelType } from "../src/metadata";
import {
  AxiosClient,
  AxiosItemResult,
  AxiosListResult,
  ItemApiState,
} from "../src/api-client";
import { mapToModel } from "../src/model";
import {
  ListViewModel,
  ViewModel,
  ViewModelCollection,
  defineProps,
} from "../src/viewmodel";

import {
  CaseListViewModel,
  CaseViewModel,
  CompanyViewModel,
  ComplexModelListViewModel,
  ComplexModelViewModel,
  PersonViewModel,
  TestViewModel,
} from "../../test-targets/viewmodels.g";
import {
  StudentViewModel,
  CourseViewModel,
  AdvisorViewModel,
  StudentListViewModel,
} from "./targets.viewmodels";
import { Student, Advisor, Course, Grade } from "./targets.models";
import * as metadata from "./targets.metadata";
import { metaBase } from "./targets.metadata";
import { ComplexModel, Test } from "@test-targets/models.g";

function mockItemResult<T>(success: boolean, object: T) {
  return vitest.fn().mockResolvedValue(<AxiosItemResult<T>>{
    data: {
      wasSuccessful: success,
      object: object,
    },
  });
}

describe("ViewModel", () => {
  test("is assignable to generic untyped ViewModel", () => {
    const vm: ViewModel = new StudentViewModel();
  });

  describe.each(["$load", "$save", "$delete", "$bulkSave"] as const)(
    "%s",
    (callerName) => {
      test("caller is lazily created", async () => {
        // This test ensures that vue doesn't create them for us
        // as a consequence of it trying to enumerate properties
        // on our objects in order to add reactivity.

        const vue = mountData({
          student: new StudentViewModel(),
        });

        // ViewModel shouldn't have its own property descriptor for callerName.
        // The descriptor will be on the prototype.
        expect(
          Object.getOwnPropertyDescriptor(vue.student, callerName)
        ).toBeUndefined();

        // Access the caller to cause it to be created.
        vue.student[callerName];

        // Instance should now have its own caller.
        expect(
          Object.getOwnPropertyDescriptor(vue.student, callerName)
        ).not.toBeUndefined();
        expect(vue.student[callerName]).toBeInstanceOf(ItemApiState);

        // Multiple access should yield the same instance.
        expect(vue.student[callerName]).toBe(vue.student[callerName]);

        // Instances should be reactive:
        const callbackFn = vitest.fn();
        vue.$watch(`student.${callerName}.isLoading`, callbackFn);
        vue.student[callerName].isLoading = true;
        await vue.$nextTick();
        // Watcher should have been triggered for isLoading.
        expect(callbackFn).toBeCalledTimes(1);
      });
    }
  );

  describe.each(["$load", "$save", "$bulkSave"] as const)(
    "%s",
    (callerName) => {
      describe("model props are set when watchers observe end of loading", () => {
        beforeEach(() => {
          return mockEndpoint("/students/", (req) => {
            return {
              wasSuccessful: true,
              object: <Student>{ studentId: 1, name: "Bob" },
            };
          }).destroy;
        });

        test("isLoading==false", async () => {
          const student = new StudentViewModel({ studentId: 1 });
          const vue = mountData({ student });

          let capturedName;
          const callbackFn = vitest.fn((n) => {
            if (n === false) {
              // DO NOT ASSERT INSIDE THE WATCHER CALLBACK. Vue will swallow the error.
              capturedName = student.name;
            }
          });
          vue.$watch(`student.${callerName}.isLoading`, callbackFn);
          student[callerName].apply(student); // .apply() works around https://github.com/microsoft/TypeScript/issues/49866

          // Run a bunch of cycles to let everything flush through.
          for (let i = 0; i < 20; i++) await vue.$nextTick();

          // Watcher should have been triggered for isLoading=true, then isLoading=false.
          expect(callbackFn.mock.calls[0][0]).toBe(true);
          expect(callbackFn.mock.calls[1][0]).toBe(false);
          expect(capturedName).toBe("Bob");
        });

        test("wasSuccessful==true", async () => {
          const student = new StudentViewModel({ studentId: 1 });
          const vue = mountData({ student });

          let capturedName;
          const callbackFn = vitest.fn((n) => {
            // DO NOT ASSERT INSIDE THE WATCHER CALLBACK. Vue will swallow the error.
            capturedName = student.name;
          });
          vue.$watch(`student.${callerName}.wasSuccessful`, callbackFn);
          student[callerName].apply(student); // .apply() works around https://github.com/microsoft/TypeScript/issues/49866

          // Run a bunch of cycles to let everything flush through.
          for (let i = 0; i < 20; i++) await vue.$nextTick();

          expect(callbackFn.mock.calls[0][0]).toBe(true);
          expect(capturedName).toBe("Bob");
        });
      });
    }
  );

  describe.each(["$save", "$bulkSave"] as const)("%s", (callerName) => {
    test("checks client validation", async () => {
      const student = new StudentViewModel();
      student.$addRule("name", "rule1", (v) => v == "a" || "Test Error 1.");
      student.$addRule("name", "rule2", (v) => v == "b" || "Test Error 2.");

      expect(() => {
        return student[callerName].apply(student); // .apply() works around https://github.com/microsoft/TypeScript/issues/49866
      }).rejects.toThrowError(
        "- validation failed: Test Error 1, Test Error 2."
      );
    });
  });

  describe("$save", () => {
    const saveMock = mockItemResult(true, <Student>{
      studentId: 3,
      name: "Bob",
      studentAdvisorId: null,
      advisor: null,
    });

    test("updates collection navigation entities with new PK", async () => {
      const student = new StudentViewModel({
        name: "Bob",
        courses: [{ name: "a" }, { name: "b" }],
      });

      student.$apiClient.save = mockItemResult(true, <Student>{
        studentId: 3,
        name: "Bob",
        // It must still work if the server returns an empty array.
        // The unsaved items (i.e. those without a PK) should be preserved on the client.
        courses: [] as Course[],
        studentAdvisorId: null,
        advisor: null,
      });
      await student.$save();

      expect(student.courses).toHaveLength(2);
      expect(student.courses![0].name).toBe("a");
      expect(student.courses![0].studentId).toBe(3);
      expect(student.courses![1].name).toBe("b");
      expect(student.courses![1].studentId).toBe(3);
    });

    test("updates $parent's FK prop with new PK", async () => {
      const student = new StudentViewModel({
        name: "Bob",
        advisor: { name: "Phoebe" },
      });

      const advisor = student.advisor!;
      expect(advisor.$parent).toBe(student);

      advisor.$apiClient.save = mockItemResult(true, <Advisor>{
        advisorId: 3,
        name: "Phoebe",
      });
      await advisor.$save();

      expect(advisor.advisorId).toBe(3);
      expect(student.studentAdvisorId).toBe(3);
    });

    test("saves initialData by default", async () => {
      // This test ensures that the initial data for a model is saved by surgical saves.

      const student = new StudentViewModel({
        studentId: 1,
        name: "Bob",
      });

      const saveMock = (student.$apiClient.save = mockItemResult(true, <
        Student
      >{
        studentId: 1,
        name: "Bob",
      }));

      await student.$save();

      expect(saveMock.mock.calls[0][0]).toBe(student);
      // This could also be null
      expect(saveMock.mock.calls[0][1].fields).toMatchObject([
        "studentId",
        "name",
      ]);
    });

    test("when save mode is 'surgical', saves only dirty props", async () => {
      const student = new StudentViewModel({
        studentId: 1,
        name: "Bob",
      });

      const saveMock = (student.$apiClient.save = mockItemResult(true, <
        Student
      >{
        studentId: 1,
        name: "Bob",
      }));

      student.$saveMode = "surgical";
      student.$setPropDirty("name");
      const promise = student.$save();
      expect(student.$savingProps).toEqual(new Set(["studentId", "name"]));
      await promise;
      expect(student.$savingProps).toEqual(new Set());

      expect(saveMock.mock.calls[0][0]).toBe(student);
      // Fields should contain only dirty props.
      expect(saveMock.mock.calls[0][1].fields).toMatchObject([
        "studentId",
        "name",
      ]);
    });

    /** Note: This test used to assert the opposite - that we always save all props
     * for a save of a model without a PK. However, doing so breaks https://github.com/IntelliTect/Coalesce/issues/276
     * (Default values in C# were not respected for reference types, but they were respected for value types).
     * I can't find any documented reason why I made it save all props during a Create when I made the change in Jan 2020,
     * and since we have a documented reason for it to work otherwise now (Jan 2023), the behavior is changing.
     */
    test("when save mode is 'surgical' and model lacks PK, saves only dirty props", async () => {
      const student = new StudentViewModel({
        name: "Bob",
      });

      const saveMock = (student.$apiClient.save = mockItemResult(true, <
        Student
      >{
        studentId: 1,
        name: "Bob",
      }));

      student.$saveMode = "surgical";
      await student.$save();

      expect(saveMock.mock.calls[0][0]).toBe(student);
      expect(saveMock.mock.calls[0][1].fields).toEqual(["name"]);
    });

    test("when save mode is 'whole', saves all props", async () => {
      const student = new StudentViewModel({
        name: "Bob",
      });

      const saveMock = (student.$apiClient.save = mockItemResult(true, <
        Student
      >{
        studentId: 1,
        name: "Bob",
      }));

      student.$saveMode = "whole";
      const promise = student.$save();
      expect(student.$savingProps).toEqual(new Set(["name"]));
      await promise;
      expect(student.$savingProps).toEqual(new Set());

      expect(saveMock.mock.calls[0][0]).toBe(student);
      // Save was not made with specific fields.
      expect(saveMock.mock.calls[0][1].fields).toBeFalsy();
      // Dirty flags were cleared.
      expect(student.$getPropDirty("name")).toBe(false);
      expect(student.$isDirty).toBe(false);
    });

    test("when save fails, dirty props stay dirty", async () => {
      const student = new StudentViewModel({
        name: "Bob",
      });

      try {
        student.$apiClient.save = vitest
          .fn()
          .mockRejectedValue(new Error("reject"));

        student.$saveMode = "surgical";
        expect(student.$getPropDirty("name")).toBe(true);

        await student.$save();
      } catch {
        // Ignore the expected error
      } finally {
        // Fields should be re-flagged dirty since the save failed.
        expect(student.$isDirty).toBe(true);
        expect(student.$getPropDirty("name")).toBe(true);
      }
    });

    test("saves override data when save mode is 'whole'", async () => {
      const student = new StudentViewModel({
        name: "Bob",
      });

      const saveMock = (student.$apiClient.save = mockItemResult(true, <
        Student
      >{
        studentId: 1,
        name: "Bob",
      }));

      student.$saveMode = "whole";
      const promise = student.$save({ grade: Grade.Freshman });
      expect(student.$savingProps).toEqual(new Set(["name", "grade"]));
      await promise;
      expect(student.$savingProps).toEqual(new Set());

      expect(saveMock.mock.calls[0][0].name).toBe("Bob");
      expect(saveMock.mock.calls[0][0].grade).toBe(Grade.Freshman);
      // Save was not made with specific fields.
      expect(saveMock.mock.calls[0][1].fields).toBeFalsy();
    });

    test("saves override data when save mode is 'surgical'", async () => {
      const student = new StudentViewModel({
        studentId: 1,
        name: "Bob",
      });

      const saveMock = (student.$apiClient.save = mockItemResult(true, <
        Student
      >{
        studentId: 1,
        name: "Bob",
      }));

      student.$saveMode = "surgical";
      const promise = student.$save({ grade: Grade.Freshman });
      expect(student.$savingProps).toEqual(
        new Set(["studentId", "name", "grade"])
      );
      await promise;
      expect(student.$savingProps).toEqual(new Set());

      expect(saveMock.mock.calls[0][0].name).toBe("Bob");
      expect(saveMock.mock.calls[0][0].grade).toBe(Grade.Freshman);
      // Save includes our override field.
      expect(saveMock.mock.calls[0][1].fields).toMatchObject([
        "studentId",
        "name",
        "grade",
      ]);
    });

    test("$savingProps is reactive", async () => {
      const student = new StudentViewModel({
        name: "Bob",
      });

      student.$apiClient.save = mockItemResult(true, {});

      let triggered = false;
      watch(
        () => student.$savingProps.has("name"),
        () => (triggered = true)
      );

      student.$saveMode = "surgical";
      const promise = student.$save();
      await promise;
      expect(triggered).toBeTruthy();
    });

    test("responses from $load while $save in progress do not overwrite props being saved", async () => {
      // Scenario: user is typing out a string on a model that autosaves.
      // This model is also being periodically auto-reloaded from the server.

      // 99% of the time, this works fine, but every once in a while,
      // a load will start just before a save, or even soon enough after a save
      // that the server will still read the old pre-save data from the database.
      // This causes the server to send stale data back to the client that will
      // not reflect the newest data sent back with the save.

      // If the user were to make another edit to the value before either API call comes back,
      // there would be no problem since the value would be dirty and therefore be skipped
      // when handling the response from the load.

      // However, if the changed field is non-dirty (which is the case just after performing
      // a save - this allows for detecting changes that happen /during/ the save) and *remains*
      // non-dirty until the response comes back from the load API, the stale value from the server
      // will overwrite the field, and this stale value won't match the one we just sent
      // in the still-pending save call (which is the same as the local copy of that value).

      // The result of this is that the value bound to the HTML input/textarea changes, which
      // at least for v-textarea components resets the user's cursor to the end of the field.
      // If the user was typing in the middle of the field, this presents a very bad UX.
      // (If the user was typing at the end of the field, they probably won't notice this bug).

      var student = new StudentViewModel({
        studentId: 1,
        name: "bob",
        currentCourse: new CourseViewModel({ courseId: 1 }),
      });
      student.$isDirty = false;
      student.currentCourse!.$isDirty = false;

      var list = new StudentListViewModel();
      list.$items.push(student);

      const saveMock = (student.$apiClient.save = vitest.fn(async () => {
        await delay(160);
        return {
          data: {
            wasSuccessful: true,
            object: {
              studentId: 1,
              name: "bob2",
              currentCourseId: 2,
              currentCourse: { courseId: 2 },
            },
          },
        } as AxiosItemResult<any>;
      }));

      const loadMock = (list.$apiClient.list = vitest.fn(async () => {
        await delay(80);
        return {
          data: {
            wasSuccessful: true,
            list: [
              {
                studentId: 1,
                name: "bob",
                currentCourseId: 1,
                currentCourse: { courseId: 1 },
              },
            ],
          },
        } as AxiosListResult<any>;
      }));

      const vue = mountData({ student });

      // Act: Trigger a load that will finish while a save is still pending.
      list.$load();
      student.name = "bob2";
      student.currentCourse = new CourseViewModel({ courseId: 2 });
      student.$save();

      // Let the load finish
      await delay(100);
      expect(saveMock).toBeCalledTimes(1);
      expect(loadMock).toBeCalledTimes(1);
      expect(student.$save.isLoading).toBeTruthy();
      expect(list.$load.isLoading).toBeFalsy();

      // Assert: Props that are still actively saving should be preserved.
      expect(student.name).toBe("bob2");
      expect(student.currentCourseId).toBe(2);
      expect(student.currentCourse!.courseId).toBe(2);

      // Let the save finish
      await delay(100);
      expect(student.$save.isLoading).toBeFalsy();
      expect(student.name).toBe("bob2");
      expect(student.currentCourseId).toBe(2);
      expect(student.currentCourse!.courseId).toBe(2);
    });

    test("loads that are older than the most recent save are ignored", async () => {
      var student = new StudentViewModel({
        studentId: 1,
        name: "bob",
        currentCourse: new CourseViewModel({ courseId: 1 }),
      });
      student.$isDirty = false;
      student.currentCourse!.$isDirty = false;

      var list = new StudentListViewModel();
      list.$items.push(student);

      const saveMock = (student.$apiClient.save = vitest.fn(async () => {
        await delay(80);
        return {
          data: {
            wasSuccessful: true,
            object: {
              studentId: 1,
              name: "bob2",
              currentCourseId: 2,
              currentCourse: { courseId: 2 },
            },
          },
        } as AxiosItemResult<any>;
      }));

      const loadMock = (list.$apiClient.list = vitest.fn(async () => {
        await delay(160);
        return {
          data: {
            wasSuccessful: true,
            list: [
              {
                studentId: 1,
                name: "bob",
                currentCourseId: 1,
                currentCourse: { courseId: 1 },
              },
            ],
          },
        } as AxiosListResult<any>;
      }));

      const vue = mountData({ student });

      // Act: Trigger a load that will finish while a save is still pending.
      list.$load();
      student.name = "bob2";
      student.currentCourse = new CourseViewModel({ courseId: 2 });
      student.$save();

      // Let the save finish
      await delay(100);
      expect(saveMock).toBeCalledTimes(1);
      expect(loadMock).toBeCalledTimes(1);
      expect(student.$save.isLoading).toBeFalsy();
      expect(list.$load.isLoading).toBeTruthy();

      // Assert: Props that we saved are still there:
      expect(student.name).toBe("bob2");
      expect(student.currentCourseId).toBe(2);
      expect(student.currentCourse!.courseId).toBe(2);

      // Let the load finish
      await delay(100);
      expect(student.$load.isLoading).toBeFalsy();
      // Response from the load should be ignored since it was older than the save.
      expect(student.name).toBe("bob2");
      expect(student.currentCourseId).toBe(2);
      expect(student.currentCourse!.courseId).toBe(2);
    });

    test("creation with parent that was explicitly late loaded by key", async () => {
      const saveEndpoint = mockEndpoint(
        "/students/save",
        vitest.fn((req) => ({
          wasSuccessful: true,
          object: {
            studentId: 1,
            studentAdvisorId: 3,
          },
        }))
      );

      const student = new StudentViewModel({
        name: "bob",
      });
      const advisor = (student.advisor = new AdvisorViewModel());

      // Weird scenario: After loading the principal entity from the server (emulated here),
      // `student.advisor` is still non-null, but `student.studentAdvisorId` is null.
      advisor.$loadCleanData({ advisorId: 7 });

      await student.$save();

      expect(saveEndpoint.mock.calls[0][0].data).toBe(
        "name=bob&studentAdvisorId=7"
      );

      saveEndpoint.destroy();
    });
  });

  describe("$bulkSave", () => {
    test("deep creation with circular references", async () => {
      const response = {
        refMap: {} as any,
        object: {
          studentId: 1,
          name: "scott",
          courses: [{ courseId: 2, name: "CS101" }],
          advisor: {
            name: "bob",
            advisorId: 3,
            students: [
              { studentId: 1, name: "scott" },
              { studentId: 4, name: "steve" },
            ],
          },
        },
      };

      const endpoint = mockEndpoint(
        "/students/bulkSave",
        vitest.fn((req) => ({
          wasSuccessful: true,
          ...JSON.parse(JSON.stringify(response)), // deep clone
        }))
      );

      const student = new StudentViewModel();
      student.name = "scott";

      // new child
      student.$addChild("courses", { name: "CS101" });

      // new parent, with children not added by $addChild.
      student.advisor = new AdvisorViewModel({
        name: "bob",
        students: [
          student, // circular reference
          { name: "steve" }, // item in child collection with null reference nav prop to its parent.
        ],
      });
      const originalAdvisor = student.advisor;
      const originalSteve = student.advisor.students[1];
      const originalCourse = student.courses![0];

      // Setup the ref map on the response so that existing instances may be preserved
      response.refMap[student.$stableId] = 1;
      response.refMap[originalCourse.$stableId] = 2;
      response.refMap[originalAdvisor.$stableId] = 3;
      response.refMap[originalSteve.$stableId] = 4;

      await student.$bulkSave();

      expect(JSON.parse(endpoint.mock.calls[0][0].data)).toMatchObject({
        items: [
          {
            action: "save",
            type: "Student",
            data: { studentId: null, studentAdvisorId: null, name: "scott" },
            refs: {
              studentId: student.$stableId,
              studentAdvisorId: originalAdvisor.$stableId,
            },
            root: true,
          },
          {
            action: "save",
            type: "Advisor",
            data: { advisorId: null, name: "bob" },
            refs: { advisorId: originalAdvisor.$stableId },
          },
          {
            action: "save",
            type: "Course",
            data: { courseId: null, name: "CS101", studentId: null },
            refs: {
              courseId: originalCourse.$stableId,
              studentId: student.$stableId,
            },
          },
          {
            action: "save",
            type: "Student",
            data: { studentId: null, name: "steve" },
            refs: {
              studentId: originalSteve.$stableId,
              studentAdvisorId: originalAdvisor.$stableId,
            },
          },
        ],
      });

      // Preserves non-circular instances:
      // Reference nav:
      expect(student.advisor === originalAdvisor).toBeTruthy();
      // Collection navs:
      expect(student.advisor.students[1] === originalSteve).toBeTruthy();
      expect(student.courses![0] === originalCourse).toBeTruthy();

      // Preserves *circular* instances:
      expect(student.advisor.students[0] === student).toBeTruthy();

      expect(student.$bulkSave.wasSuccessful).toBeTruthy();
      expect(student.studentId).toBe(1);
      expect(student.courses).toHaveLength(1);
      expect(student.courses![0].name).toBe("CS101");
      expect(student.courses![0].courseId).toBe(2);
      expect(student.advisor.students).toHaveLength(2);

      endpoint.destroy();
    });

    test("unsaved principal excluded by predicate", async () => {
      const response = {
        refMap: {} as any,
        object: {
          studentId: 1,
          name: "scott",
        },
      };

      const endpoint = mockEndpoint(
        "/students/bulkSave",
        vitest.fn((req) => ({
          wasSuccessful: true,
          ...JSON.parse(JSON.stringify(response)), // deep clone
        }))
      );

      const student = new StudentViewModel({ name: "scott" });

      // new parent, with children not added by $addChild.
      student.advisor = new AdvisorViewModel({ name: "bob" });
      // Add a failing validation rule to ensure that failed validation
      // on predicate-excluded models does not block saves.
      student.advisor.$addRule("name", "test", (v) => v == "foo" || "invalid");
      const originalAdvisor = student.advisor;

      // Setup the ref map on the response so that existing instances may be preserved
      response.refMap[student.$stableId] = 1;

      await student.$bulkSave({
        predicate(entity, action) {
          if (entity instanceof AdvisorViewModel) return false;
          return true;
        },
      });

      expect(JSON.parse(endpoint.mock.calls[0][0].data)).toMatchObject({
        items: [
          {
            action: "save",
            type: "Student",
            data: { studentId: null, studentAdvisorId: null, name: "scott" },
            refs: {
              studentId: student.$stableId,
              // There should NOT be a ref for `studentAdvisorId` since the
              // unsaved advisor was excluded by predicate.
            },
            root: true,
          },
        ],
      });

      // Preserves non-circular instances:
      // Reference nav:
      expect(student.advisor === originalAdvisor).toBeTruthy();
      expect(student.$bulkSave.wasSuccessful).toBeTruthy();
      expect(student.studentId).toBe(1);

      endpoint.destroy();
    });

    test("deletion", async () => {
      const loadEndpoint = mockEndpoint("/students/get", (req) => ({
        wasSuccessful: true,
        object: {
          studentId: 1,
          name: "bob",
          courses: [{ courseId: 2, name: "CS101" }],
          studentAdvisorId: 3,
          advisor: { advisorId: 3 },
        },
      }));

      const bulkSaveEndpoint = mockEndpoint(
        "/students/bulkSave",
        vitest.fn((req) => ({
          wasSuccessful: true,
          object: {
            studentId: 1,
            name: "bob",
            // We're assuming that `advisor` has a SetNull cascading delete on the server.
          },
        }))
      );

      const student = new StudentViewModel();
      await student.$load(1);

      // Delete a dependent item in a collection navigation
      const course = student.courses![0];
      course.$remove();

      // Delete a principal item in a reference navigation
      const advisor = student.advisor!;
      student.advisor!.$remove();

      expect(student.$removedItems?.length).toBe(2);

      await student.$bulkSave();
      expect(student.courses).toHaveLength(0);
      expect(student.advisor).toBeNull();
      expect(student.studentAdvisorId).toBeNull();
      expect(student.$removedItems?.length).toBeFalsy();

      expect(JSON.parse(bulkSaveEndpoint.mock.calls[0][0].data)).toMatchObject({
        items: [
          {
            action: "none",
            type: "Student",
            data: { studentId: 1 },
            refs: { studentId: student.$stableId },
            root: true,
          },
          {
            action: "delete",
            type: "Course",
            data: { courseId: 2 },
            refs: { courseId: course.$stableId },
          },
          {
            action: "delete",
            type: "Advisor",
            data: { advisorId: 3 },
            refs: { advisorId: advisor.$stableId },
          },
        ],
      });

      loadEndpoint.destroy();
      bulkSaveEndpoint.destroy();
    });

    test("creation of principal entity", async () => {
      const bulkSaveEndpoint = mockEndpoint(
        "/students/bulkSave",
        vitest.fn((req) => ({
          wasSuccessful: true,
          object: {
            studentId: 1,
            studentAdvisorId: 2,
            advisor: { advisorId: 2, name: "bob" },
            // We're assuming that `advisor` has a SetNull cascading delete on the server.
          },
        }))
      );

      const student = new StudentViewModel({ studentId: 1 });
      student.$isDirty = false;

      // Advisor is a new principal entity we're creating for an existing dependent (student).
      const advisor = (student.advisor = new AdvisorViewModel({ name: "bob" }));
      await student.$bulkSave();

      expect(JSON.parse(bulkSaveEndpoint.mock.calls[0][0].data)).toMatchObject({
        items: [
          {
            action: "save",
            type: "Student",
            data: {
              studentId: 1,
              studentAdvisorId: null,
            },
            refs: {
              studentId: student.$stableId,
              studentAdvisorId: advisor.$stableId,
            },
            root: true,
          },
          {
            action: "save",
            type: "Advisor",
            data: {
              advisorId: null,
              name: "bob",
            },
            refs: { advisorId: advisor.$stableId },
          },
        ],
      });

      bulkSaveEndpoint.destroy();
    });

    test("removing an item and then reloading purges local $removedItems", async () => {
      const loadEndpoint = mockEndpoint("/students/get", (req) => ({
        wasSuccessful: true,
        object: {
          studentId: 1,
          name: "bob",
          courses: [{ courseId: 2, name: "CS101" }],
        },
      }));

      const student = new StudentViewModel();
      await student.$load(1);

      const course = student.courses![0];
      course.$remove();

      expect(student.$removedItems?.length).toBe(1);
      expect(student.$removedItems).toContain(course);
      expect(course.$isRemoved).toBeTruthy();
      expect(student.courses).toHaveLength(0);

      await student.$load(1);

      expect(student.$removedItems?.length).toBeFalsy();
      expect(student.courses).toHaveLength(1);
      expect(student.courses![0].$isRemoved).toBeFalsy();

      loadEndpoint.destroy();
    });

    test("creation of root entity with no dirty props", async () => {
      const bulkSaveEndpoint = mockEndpoint(
        "/students/bulkSave",
        vitest.fn((req) => ({
          wasSuccessful: true,
          object: {
            studentId: 1,
          },
        }))
      );

      const student = new StudentViewModel();
      await student.$bulkSave();

      expect(JSON.parse(bulkSaveEndpoint.mock.calls[0][0].data)).toMatchObject({
        items: [
          {
            action: "save",
            type: "Student",
            data: { studentId: null },
            refs: { studentId: student.$stableId },
            root: true,
          },
        ],
      });

      bulkSaveEndpoint.destroy();
    });

    test("save of dirty child under nondirty nonroot parent", async () => {
      const response = {
        refMap: {} as any,
        object: {
          studentId: 1,
          name: "scott",
          advisor: {
            name: "bob",
            advisorId: 3,
            students: [{ studentId: 4, name: "steve" }],
          },
        },
      };

      const endpoint = mockEndpoint(
        "/students/bulkSave",
        vitest.fn((req) => ({
          wasSuccessful: true,
          ...JSON.parse(JSON.stringify(response)), // deep clone
        }))
      );

      const student = new StudentViewModel();
      student.studentId = 1;
      student.name = "scott";

      const originalAdvisor = (student.advisor = new AdvisorViewModel({
        name: "bob",
        advisorId: 3,
      }));
      student.advisor.$isDirty = false;
      student.$isDirty = false;

      const originalSteve = originalAdvisor.$addChild("students", {
        name: "steve",
      });

      // Setup the ref map on the response so that existing instances may be preserved
      response.refMap[student.$stableId] = 1;
      response.refMap[originalAdvisor.$stableId] = 3;
      response.refMap[originalSteve.$stableId] = 4;

      await student.$bulkSave();

      expect(JSON.parse(endpoint.mock.calls[0][0].data)).toMatchObject({
        items: [
          {
            action: "none",
            type: "Student",
            data: { studentId: student.studentId },
            refs: {
              studentId: student.$stableId,
            },
            root: true,
          },
          {
            action: "save",
            type: "Student",
            data: { studentId: null, name: "steve", studentAdvisorId: 3 },
            refs: {
              studentId: originalSteve.$stableId,
            },
          },
        ],
      });

      expect(student.$bulkSave.wasSuccessful).toBeTruthy();
      expect(student.studentId).toBe(1);
      expect(student.advisor.students).toHaveLength(1);
      expect(student.advisor.students[0] === originalSteve).toBeTruthy();

      endpoint.destroy();
    });

    test("add of child to existing parent without nav/fk on child", async () => {
      const endpoint = mockEndpoint(
        "/Company/bulkSave",
        vitest.fn((req) => ({
          wasSuccessful: true,
        }))
      );

      const parent = new CompanyViewModel();
      parent.$loadCleanData({ companyId: 1, name: "existing parent" });

      // Act
      const newChild = new PersonViewModel({ firstName: "bob" });
      parent.employees?.push(newChild);
      await parent.$bulkSave();

      // Assert
      expect(JSON.parse(endpoint.mock.calls[0][0].data)).toMatchObject({
        items: [
          {
            action: "none",
            type: "Company",
            data: { companyId: parent.companyId },
            refs: {
              companyId: parent.$stableId,
            },
            root: true,
          },
          {
            action: "save",
            type: "Person",
            data: {
              personId: null,
              firstName: "bob",
            },
            refs: {
              personId: newChild.$stableId,
              companyId: parent.$stableId,
            },
          },
        ],
      });

      endpoint.destroy();
    });

    test("existing nondirty child of existing parent without nav/fk on child does not save", async () => {
      // SCENARIO: A new child object gets saved by virtue of existing in a child collection.
      // However, this child object isn't reloaded by the response of the save.

      const endpoint = mockEndpoint(
        "/Company/bulkSave",
        vitest.fn((req) => ({
          wasSuccessful: true,
        }))
      );

      const parent = new CompanyViewModel();
      parent.$loadCleanData({
        companyId: 1,
        name: "existing parent",
        employees: [{ personId: 1, firstName: "bob" }],
      });

      // Sanity check: The employee lacks a reference nav or FK to its parent
      // but is not dirty and does have a PK.
      parent.employees![0].company = null;
      parent.employees![0].companyId = null;
      parent.employees![0].$isDirty = false;

      // Act
      await parent.$bulkSave();

      // Assert
      expect(JSON.parse(endpoint.mock.calls[0][0].data)).toMatchObject({
        items: [
          {
            action: "none",
            type: "Company",
            data: { companyId: parent.companyId },
            refs: {
              companyId: parent.$stableId,
            },
            root: true,
          },
          // There should NOT be a "Person" model in the payload,
          // since despite missing a foreign key to its parent,
          // it isn't dirty and does exist on the server.
        ],
      });

      endpoint.destroy();
    });

    test("options.additionalRoots", async () => {
      const endpoint = mockEndpoint(
        "/Company/bulkSave",
        vitest.fn((req) => ({
          wasSuccessful: true,
        }))
      );

      const company1 = new CompanyViewModel({ name: "new company1" });
      const company2 = new CompanyViewModel({ name: "new company2" });

      // Act
      await company1.$bulkSave({
        additionalRoots: [company2],
      });

      // Assert
      expect(JSON.parse(endpoint.mock.calls[0][0].data)).toMatchObject({
        items: [
          {
            action: "save",
            type: "Company",
            data: { companyId: null, name: "new company1" },
            refs: {
              companyId: company1.$stableId,
            },
            root: true,
          },
          {
            action: "save",
            type: "Company",
            data: { companyId: null, name: "new company2" },
            refs: {
              companyId: company2.$stableId,
            },
          },
        ],
      });

      endpoint.destroy();
    });

    test("creation with parent that was explicitly late loaded by key - does not include ref to existing parent", async () => {
      const bulkSaveEndpoint = mockEndpoint(
        "/students/bulkSave",
        vitest.fn((req) => ({
          wasSuccessful: true,
          object: {
            studentId: 1,
            studentAdvisorId: 3,
          },
        }))
      );

      const advisor = new AdvisorViewModel();
      const student = new StudentViewModel({
        advisor: advisor,
      });

      // Weird scenario: After loading the principal entity from the server (emulated here),
      // `student.advisor` is still non-null, but `student.studentAdvisorId` is null.
      advisor.$loadCleanData({ advisorId: 7 });

      await student.$bulkSave();

      expect(JSON.parse(bulkSaveEndpoint.mock.calls[0][0].data)).toMatchObject({
        items: [
          {
            action: "save",
            type: "Student",
            data: {
              studentId: null,
              // `mapToDto` will discover the foreign key through the nav prop
              studentAdvisorId: 7,
            },
            refs: {
              studentId: student.$stableId,
              // Since the FK has a known server value, it should not be `ref`'d.
            },
            root: true,
          },
        ],
      });

      bulkSaveEndpoint.destroy();
    });
  });

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
      const vue = mountData({ student });

      // Length of a "tick" in milliseconds. Smaller == faster test run.
      // If this is too small then the test wont work.
      const tickLength = 30;

      var savePromise: Promise<any>;
      const saveMock = (student.$apiClient.save = vitest
        .fn()
        .mockImplementation((dto: any) => {
          // Map the parameter to a new model to simulate the save response.
          // This MUST be a mapping to prevent issues with same object references.
          const result = {
            ...mapToModel(dto, student.$metadata),
            studentId: 1,
          };

          return (savePromise = new Promise((resolve) =>
            setTimeout(() => {
              resolve(<AxiosItemResult<Student>>{
                data: {
                  wasSuccessful: true,
                  object: result,
                },
              });
            }, tickLength)
          ));
        }));

      const waitTick = () => delay(30);

      student.$startAutoSave(vue, { wait: tickLength / 2 });
      expect(student.$isDirty).toBe(false);

      student.name = "Steve";
      expect(student.$isDirty).toBe(true);
      expect(student.$getPropDirty("name")).toBe(true);

      await waitTick();
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
      await waitTick();

      // There should be another save in flight now.
      // Model shouldn't be dirty since we just sent off the current state.
      expect(student.$isDirty).toBe(false);
      await savePromise!;
      await waitTick();

      expect(saveMock).toBeCalledTimes(2);

      // This is the crux of what we're testing here -
      // The second save should have the PK returned by the first save.
      expect((saveMock.mock.calls[0][0] as Student).name).toBe("Steve-o");
      expect((saveMock.mock.calls[1][0] as Student).studentId).toBe(1);

      // For good measure.
      student.$stopAutoSave();
    });

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
          students: [{ studentId: 2, name: "grandchild", studentAdvisorId: 1 }],
        },
      };
      const student = new StudentViewModel();
      student.$loadCleanData(studentData);

      const vue = mountData({ student });

      // Start autosave on the whole tree (could have just as well done it on the advisor only)
      // We use a big delay because the only thing that matters here is that autosave
      // is enabled. A save doesn't (and shouldn't) be actually performed during this test.
      student.$startAutoSave(vue, { deep: true, wait: 1000000 });

      // Arbitrarily set the advisor as dirty. We don't actually need to modify a property.
      student.advisor!.$isDirty = true;

      // Update our incoming data with new values coming from the server.
      studentData.name = "root-updated";
      studentData.advisor.name = "child-updated";
      studentData.advisor.students[0].name = "grandchild-updated";
      student.$loadCleanData(studentData);

      // The two non-dirty models should have been updated with the new data.
      expect(student.name).toBe("root-updated");
      expect(student.$isDirty).toBeFalsy();
      expect(student.advisor!.students[0].name).toBe("grandchild-updated");
      expect(student.advisor!.students[0].$isDirty).toBeFalsy();

      // The dirty model should NOT have been updated,
      // and it should still be marked dirty.
      expect(student.advisor!.name).toBe("child");
      expect(student.advisor!.$isDirty).toBeTruthy();
    });

    test("non-saved properties cannot be marked dirty", async () => {
      // Rationale: non-saved properties have no important distinction between clean and dirty,
      // as they're only ever read from the server - never are they sent to the server.

      // If non-saved properties can be marked dirty, we can land in a state where they
      // will never be refreshed with the most recent server value via $loadCleanData
      // when $loadCleanData is being called by hand outside of a $save().

      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine" },
      });
      var student = new StudentViewModel();
      student.$loadCleanData(studentModel);

      // precondition:
      expect(student.$metadata.props.advisor.dontSerialize).toBeTruthy();

      // Wholesale marking the object is dirty shouldn't mark nonserialized props.
      student.$isDirty = true;
      expect(student.$getPropDirty("advisor")).toBeFalsy();

      // Neither should explicit targeting of the prop directly.
      student.$setPropDirty("advisor", true);
      expect(student.$getPropDirty("advisor")).toBeFalsy();
    });

    test("$loadCleanData won't trigger autosave on root", async () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine" },
      });
      var student = new StudentViewModel();
      student.$loadCleanData(studentModel);
      const saveMock = (student.$apiClient.save = vitest
        .fn()
        .mockImplementation((dto: any) => {
          throw "Save shouldn't have been called";
        }));
      const vue = mountData({ student });

      student.$startAutoSave(vue, { wait: 0 });

      studentModel.name = "NewName";
      student.$loadCleanData(studentModel);
      await delay(10);

      expect(saveMock).toBeCalledTimes(0);
    });

    test("$loadCleanData won't trigger autosave on ref nav", async () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine" },
      });
      var student = new StudentViewModel();
      student.$loadCleanData(studentModel);
      const saveMock = (student.advisor!.$apiClient.save = vitest
        .fn()
        .mockImplementation((dto: any) => {
          throw "Save shouldn't have been called";
        }));
      const vue = mountData({ student });

      student.advisor!.$startAutoSave(vue, { wait: 0 });

      studentModel.advisor!.name = "NewName";
      student.$loadCleanData(studentModel);
      await delay(10);

      expect(saveMock).toBeCalledTimes(0);
    });

    test("$loadCleanData won't trigger autosave on collection nav", async () => {
      var studentModel = new Student({
        studentId: 1,
        courses: [{ courseId: 7, name: "foo", studentId: 1 }],
      });
      var student = new StudentViewModel();
      student.$loadCleanData(studentModel);
      const saveMock = (student.courses![0].$apiClient.save = vitest
        .fn()
        .mockImplementation((dto: any) => {
          throw "Save shouldn't have been called";
        }));
      const vue = mountData({ student });

      student.courses![0].$startAutoSave(vue, { wait: 0 });

      studentModel.courses![0].name = "NewName";
      student.$loadCleanData(studentModel);
      await delay(10);

      expect(saveMock).toBeCalledTimes(0);
    });

    test("triggers save immediately if model is dirty", async () => {
      var student = new StudentViewModel({
        studentId: 1,
        name: "bob",
      });
      student.$isDirty = false;
      const saveMock = (student.$apiClient.save = vitest
        .fn()
        .mockResolvedValue(<AxiosItemResult<any>>{
          data: { wasSuccessful: true },
        }));
      const vue = mountData({ student });

      // Start auto save. Model shouldn't be dirty, so it shouldn't hit the API.
      expect(student.$isDirty).toBe(false);
      student.$startAutoSave(vue, { wait: 0 });
      await delay(10);
      expect(saveMock).toBeCalledTimes(0);

      // Reset autosave, dirty the model, and restart autosave.
      // It should trigger immediately.
      student.$stopAutoSave();
      student.name += "2";
      expect(student.$isDirty).toBe(true);
      student.$startAutoSave(vue, { wait: 0 });
      await delay(10);
      expect(saveMock).toBeCalledTimes(1);
    });

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
      var student = new StudentViewModel();
      student.$loadCleanData({ name: "bob" });

      const saveMock = (student.$apiClient.save = mockItemResult(true, {
        studentId: 1,
        name: "bob",
      }));
      const vue = mountData({ student });

      // Start auto save. Model shouldn't be dirty, but does lack a PK, so autosave should trigger.
      expect(student.$isDirty).toBe(false);
      expect(student.$primaryKey).toBeFalsy();
      student.$startAutoSave(vue, { wait: 0 });
      expect(student.$isAutoSaveEnabled).toBeTruthy();
      await delay(10);
      expect(saveMock).toBeCalledTimes(1);
    });

    test("triggers save immediately if model is empty but valid", async () => {
      var student = new StudentViewModel();

      const saveMock = (student.$apiClient.save = mockItemResult(true, {
        studentId: 1,
      }));
      const vue = mountData({ student });

      // Start auto save. Model shouldn't be dirty, but does lack a PK, so autosave should trigger.
      expect(student.$isDirty).toBe(false);
      expect(student.$primaryKey).toBeFalsy();
      student.$startAutoSave(vue, { wait: 0 });
      await delay(10);
      expect(saveMock).toBeCalledTimes(1);
    });

    const loadMock = vitest.fn().mockResolvedValue(<AxiosItemResult<Student>>{
      data: {
        wasSuccessful: true,
        object: {
          studentId: 1,
          name: "bob",
        },
      },
    });

    test("does not trigger if enabled while model load is pending", async () => {
      var student = new StudentViewModel();
      const saveMock = (student.$apiClient.save = vitest
        .fn()
        .mockResolvedValue(<AxiosItemResult<Student>>{
          data: { wasSuccessful: true },
        }));
      student.$apiClient.get = loadMock;

      const vue = mountData({ student });

      student.$load(1);
      student.$startAutoSave(vue, { wait: 0 });
      await delay(10);
      expect(saveMock).toBeCalledTimes(0);
    });

    test("does not trigger if enabled just before loading model", async () => {
      var student = new StudentViewModel();
      const saveMock = (student.$apiClient.save = vitest
        .fn()
        .mockResolvedValue(<AxiosItemResult<Student>>{
          data: { wasSuccessful: true },
        }));
      student.$apiClient.get = loadMock;

      const vue = mountData({ student });

      student.$startAutoSave(vue, { wait: 0 });
      student.$load(1);
      await delay(10);
      expect(saveMock).toBeCalledTimes(0);
    });

    test("does not trigger if enabled just before loading model and model load fails", async () => {
      var student = new StudentViewModel();
      const saveMock = (student.$apiClient.save = vitest
        .fn()
        .mockResolvedValue(<AxiosItemResult<Student>>{
          data: { wasSuccessful: true },
        }));
      student.$apiClient.get = vitest.fn().mockResolvedValue(<
        AxiosItemResult<Student>
      >{
        data: {
          wasSuccessful: false,
          message: "Not found",
        },
      });

      const vue = mountData({ student });

      student.$startAutoSave(vue, { wait: 0 });
      student.$load(1);
      await delay(10);
      expect(saveMock).toBeCalledTimes(0);
    });

    test("does not trigger if model has errors", async () => {
      var viewModel = new AdvisorViewModel({
        advisorId: 1,
        name: null,
      });
      const saveMock = (viewModel.$apiClient.save = vitest
        .fn()
        .mockResolvedValue(<AxiosItemResult<Advisor>>{
          data: { wasSuccessful: true },
        }));

      const vue = mountData({ viewModel });

      viewModel.$startAutoSave(vue, { wait: 0 });
      expect([...viewModel.$getErrors()]).toHaveLength(1);
      await delay(10);
      expect(saveMock).toBeCalledTimes(0);

      viewModel.name = "Bob";
      expect([...viewModel.$getErrors()]).toHaveLength(0);
      await delay(10);
      expect(saveMock).toBeCalledTimes(1);
    });

    test("does not infinitely trigger if API call fails", async () => {
      var student = new StudentViewModel({
        name: "bob",
      });
      const saveMock = (student.$apiClient.save = vitest
        .fn()
        .mockRejectedValue(new Error()));

      const vue = mountData({ student });
      student.$startAutoSave(vue, { wait: 0 });

      student.name = "bob";
      await delay(100);
      expect(saveMock).toBeCalledTimes(1);
    });

    test("debounces correctly", async () => {
      var student = new StudentViewModel({
        studentId: 1,
        name: "bob",
      });
      student.$isDirty = false;
      const saveMock = (student.$apiClient.save = vitest
        .fn()
        .mockResolvedValue(<AxiosItemResult<any>>{
          data: { wasSuccessful: true },
        }));
      const vue = mountData({ student });

      student.$startAutoSave(vue, {
        wait: 50,
        debounce: { trailing: true, maxWait: 300 },
      });
      // Dirty the model every 5 ms.
      const userInputSimulator = setInterval(() => {
        student.name = Math.random().toString();
      }, 5);

      // Our maxWait has been passed so we expect one save to have been made:
      delay(330).then(() => expect(saveMock).toBeCalledTimes(1));

      // Stop user input. This will cause the next save to happen in `wait`ms (50):
      delay(450).then(() => clearInterval(userInputSimulator));

      // The second save should have happened. Await this one so it blocks the test completion.
      // 550 was chosen because its less than 2x `maxWait`, but still more than `wait`ms after we
      // stopped user input.
      await delay(550);
      expect(saveMock).toBeCalledTimes(2);
    });

    test("works with setup()", async () => {
      var student = new StudentViewModel({
        studentId: 1,
        name: "bob",
      });
      student.$isDirty = false;
      const saveMock = (student.$apiClient.save = mockItemResult(true, {}));

      const wrapper = mount(
        defineComponent({
          template: "<div></div>",
          setup() {
            student.name += "2";
            expect(student.$isDirty).toBe(true);

            // Autosave should trigger immediately since the model is dirty.
            student.$startAutoSave(getCurrentInstance()!.proxy!, { wait: 0 });
          },
        })
      );

      await delay(10);
      expect(saveMock).toBeCalledTimes(1);

      // Watchers should be destroyed on component unmount.
      destroy(wrapper);
      expect((student as any)._autoSaveState.active).toBe(false);
    });

    test("useAutoSave works with setup()", async () => {
      var student = new StudentViewModel({
        studentId: 1,
        name: "bob",
      });
      student.$isDirty = false;
      const saveMock = (student.$apiClient.save = mockItemResult(true, {}));

      const wrapper = mount(
        defineComponent({
          template: "<div></div>",
          setup() {
            student.name += "2";
            expect(student.$isDirty).toBe(true);

            // Autosave should trigger immediately since the model is dirty.
            student.$useAutoSave({ wait: 0 });
          },
        })
      );

      await delay(10);
      expect(saveMock).toBeCalledTimes(1);

      // Watchers should be destroyed on component unmount.
      destroy(wrapper);
      expect((student as any)._autoSaveState.active).toBe(false);
    });

    describe("deep", () => {
      test("propagates to existing related objects", async () => {
        var studentModel = new Student({
          studentId: 1,
          courses: [{ studentId: 1, courseId: 7, name: "foo" }],
          studentAdvisorId: 3,
          advisor: { advisorId: 3, name: "Bob" },
        });
        var student = new StudentViewModel(studentModel);
        const saveMock =
          (student.courses![0].$apiClient.save =
          student.advisor!.$apiClient.save =
          student.$apiClient.save =
            vitest.fn().mockResolvedValue(<AxiosItemResult<any>>{
              data: { wasSuccessful: true },
            }));
        const vue = mountData({ student });

        student.$startAutoSave(vue, { wait: 0, deep: true });

        student.courses![0].name = "NewName";
        student.advisor!.name = "NewName";
        student.name = "NewName";

        await delay(10);

        // One save expected for each model that changed.
        expect(saveMock).toBeCalledTimes(3);
      });

      test("propagates to new collection navigation items", async () => {
        var studentModel = new Student({
          studentId: 1,
        });
        var student = new StudentViewModel(studentModel);
        student.$isDirty = false;
        const vue = mountData({ student });

        student.$startAutoSave(vue, { wait: 0, deep: true });

        const newModel = student.$addChild("courses");
        const saveMock = (newModel.$apiClient.save = vitest
          .fn()
          .mockResolvedValue(<AxiosItemResult<any>>{
            data: { wasSuccessful: true },
          }));

        await delay(10);

        // Autosave should have triggered for the new object.
        expect(saveMock).toBeCalledTimes(1);
      });

      test("propagates to new reference navigation items", async () => {
        var studentModel = new Student({
          studentId: 1,
        });
        var student = new StudentViewModel(studentModel);
        student.$isDirty = false;
        const vue = mountData({ student });

        student.$apiClient.save = vitest.fn();
        student.$startAutoSave(vue, { wait: 0, deep: true });

        const newModel = (student.advisor = new AdvisorViewModel());
        const saveMock = (newModel.$apiClient.save = vitest
          .fn()
          .mockResolvedValue(<AxiosItemResult<Student>>{
            data: {
              wasSuccessful: true,
            },
          }));

        // Dirty the model to ensure that it has a reason to trigger.
        newModel.name = "bob";
        await delay(10);

        // Autosave should have triggered for the new object.
        expect(saveMock).toBeCalledTimes(1);
      });

      test("when autosave of child returns different PK, does not trigger save of parent", async () => {
        var parent = new StudentViewModel({
          studentId: 2,
          name: "Steve",
          currentCourseId: 10,
          currentCourse: {
            courseId: 10,
            studentId: 2,
            name: "Physics",
          },
        });

        const childSaveMock = (parent.currentCourse!.$apiClient.save = vitest
          .fn()
          .mockResolvedValue(<AxiosItemResult<Course>>{
            data: {
              wasSuccessful: true,
              object: {
                // Saving the child returns a different object.
                // Common pattern for immutable records in Coalesce.
                courseId: 11,
                name: "Math",
                studentId: 2,
                student: {
                  studentId: 2,
                  name: "Steve",
                  currentCourseId: 11,
                },
              },
            },
          }));

        const parentSaveMock = (parent.$apiClient.save = vitest
          .fn()
          .mockImplementation((dto: any) => {
            throw "Save shouldn't have been called";
          }));

        parent.$isDirty = false;
        parent.currentCourse!.$isDirty = false;
        const vue = mountData({ parent });
        parent.$startAutoSave(vue, { wait: 1, deep: true });

        // Dirty the model to trigger autosave
        parent.currentCourse!.name = "Math";
        await delay(100);

        expect(childSaveMock).toHaveBeenCalledTimes(1);
        expect(parentSaveMock).toHaveBeenCalledTimes(0);

        expect(parent.currentCourseId).toBe(11);
        expect(parent.currentCourse?.courseId).toBe(11);
        expect(parent.currentCourse?.name).toBe("Math");
      });
    });
  });

  describe("$addChild", () => {
    test("creates ViewModels", () => {
      var student = new StudentViewModel();
      const course = student.$addChild("courses");

      expect(course).toBeInstanceOf(CourseViewModel);
    });

    test("new model has $parent and $parentCollection set", () => {
      var student = new StudentViewModel();
      const course = student.$addChild("courses");

      // @ts-ignore testing private field
      expect(course.$parent).toBe(student);
      // @ts-ignore testing private field
      expect(course.$parentCollection).toBe(student.courses);
    });

    test("new model has foreign key and $isDirty set", () => {
      var student = new StudentViewModel({ studentId: 3 });
      const course = student.$addChild("courses") as CourseViewModel;

      expect(course).toBeInstanceOf(CourseViewModel);
      expect(course.studentId).toBe(student.studentId);
      expect(course.student).toBe(student);
      expect(course.$isDirty).toBe(true);
      expect(course.$getPropDirty("studentId")).toBe(true);
    });

    test("new model has collections initialized to empty arrays", () => {
      var advisor = new AdvisorViewModel({ advisorId: 3 });
      const student = advisor.$addChild("students") as StudentViewModel;

      expect(student.courses).not.toBeNull();
      expect(student.courses).toHaveLength(0);
    });

    test("new model has initial data flagged dirty", () => {
      var advisor = new AdvisorViewModel({ advisorId: 3 });
      advisor.$isDirty = false;

      const student = advisor.$addChild("students", {
        name: "bob",
        advisor,
        courses: [{ name: "CS101" }],
      }) as StudentViewModel;

      // The name and courses come from a partial raw model,
      // which should be loaded as dirty into new ViewModel instances:
      expect(student.$getPropDirty("name")).toBeTruthy();
      expect(student.courses![0].$getPropDirty("name")).toBeTruthy();

      // Foreign key for the advisor should be populated as dirty:
      expect(student.$getPropDirty("studentAdvisorId")).toBeTruthy();
      expect(student.studentAdvisorId).toBe(advisor.advisorId);

      // The advisor itself was already a non-dirty ViewModel instance,
      // so it should not have been flagged as dirty.
      expect(student.advisor?.$isDirty).toBeFalsy();
    });
  });

  describe("validation", () => {
    test("$removeRule adds ignore for metadata-provided rule", () => {
      var viewModel = new AdvisorViewModel();
      expect([...viewModel.$getErrors()]).toHaveLength(1);

      viewModel.$removeRule("name", "required");
      expect([...viewModel.$getErrors()]).toHaveLength(0);
    });

    test("$removeRule removes custom rule", () => {
      var viewModel = new AdvisorViewModel();
      viewModel.$addRule("name", "required2", (v) => !!v || "custom");
      expect([...viewModel.$getErrors()]).toEqual([
        "Name is required.",
        "custom",
      ]);

      viewModel.$removeRule("name", "required2");
      expect([...viewModel.$getErrors()]).toEqual(["Name is required."]);
    });

    test("$removeRule removes metadata rule and leaves custom rule", () => {
      var viewModel = new AdvisorViewModel();
      viewModel.$addRule("name", "required2", (v) => !!v || "custom");
      expect([...viewModel.$getErrors()]).toEqual([
        "Name is required.",
        "custom",
      ]);

      viewModel.$removeRule("name", "required");
      expect([...viewModel.$getErrors()]).toEqual(["custom"]);
    });

    test("$addRule overrides metadata-provided rule", () => {
      var viewModel = new AdvisorViewModel();
      expect([...viewModel.$getErrors()]).toEqual(["Name is required."]);

      viewModel.$addRule("name", "required", (v) => !!v || "custom");
      expect([...viewModel.$getErrors()]).toEqual(["custom"]);
    });

    test("$addRule adds additional rules when prop has metadata-provided rules", () => {
      var viewModel = new AdvisorViewModel();
      expect([...viewModel.$getErrors()]).toEqual(["Name is required."]);

      viewModel.$addRule("name", "required2", (v) => !!v || "custom");
      expect([...viewModel.$getErrors()]).toEqual([
        "Name is required.",
        "custom",
      ]);
    });

    test("$addRule adds rules when prop has no metadata-provided rules", () => {
      var viewModel = new AdvisorViewModel();
      expect([...viewModel.$getErrors()]).toEqual(["Name is required."]);

      viewModel.$addRule("advisorId", "required", (v) => !!v || "custom");
      expect([...viewModel.$getErrors()]).toEqual([
        "custom",
        "Name is required.",
      ]);
    });
  });

  describe("getters/setters", () => {
    describe("value getters/setters", () => {
      test.each([
        ["number", () => new Student({ studentId: 1 })],
        ["string", () => new Student({ studentId: 1, name: "bob" })],
        ["boolean", () => new Student({ studentId: 1, isEnrolled: true })],
        [
          "date",
          () =>
            new Student({
              studentId: 1,
              birthDate: new Date("1990-01-02T03:04:05.000-08:00"),
            }),
        ],
      ])(
        "%s setter doesn't trigger reactivity for unchanged value",
        async (_, factory) => {
          var student = new StudentViewModel();
          student.$loadCleanData(factory());

          const vue = mountData({ student });
          const watchCallback = vitest.fn();
          vue.$watch("student", watchCallback, { deep: true });

          student.$loadCleanData(factory());
          await vue.$nextTick();

          // Exact same model was reloaded. There should be no changes.
          expect(watchCallback).toBeCalledTimes(0);
        }
      );
    });

    describe("object getter/setters", () => {
      test("object setter converts to Model", () => {
        var advisor = new AdvisorViewModel();
        advisor.$loadCleanData({
          studentWrapperObject: {
            name: "bob",
            student: {
              studentId: 1,
              name: "bob",
            },
          },
        });

        expect(advisor.studentWrapperObject!.$metadata).toBe(
          metadata.DisplaysStudent
        );
        expect(advisor.studentWrapperObject!.student?.$metadata).toBe(
          metadata.Student
        );
      });

      test("object setter does not throw when newValue is null and oldValue is non-null", () => {
        var advisor = new AdvisorViewModel();
        advisor.$loadCleanData({
          studentWrapperObject: {
            name: "bob",
            student: {
              studentId: 1,
              name: "bob",
            },
          },
        });

        advisor.studentWrapperObject!.student = null;
        advisor.studentWrapperObject = null;
      });
    });

    describe("collection navigation getter/setters", () => {
      test("types", () => {
        const model = new ComplexModelViewModel({});
        const plain: ComplexModel = model;

        model.tests = model.tests;
        model.tests = [];
        model.tests = null;
        model.tests.push({ testName: "bob" });
      });

      test("new instance collection initialized", () => {
        const model = new ComplexModelViewModel();
        expect(model.tests).not.toBeFalsy();
      });

      test("new instance with missing collection initialized", () => {
        const model = new ComplexModelViewModel({ name: "bob" });
        expect(model.tests).not.toBeFalsy();
      });

      test("setter creates new ViewModelCollection", () => {
        const model = new ComplexModelViewModel();
        model.tests = [];

        expect(model.tests.push).not.toBe(Array.prototype.push);
        expect(model.tests.$metadata).toBe(model.$metadata.props.tests);
      });

      test("collection creates ViewModel when Model is pushed", () => {
        const model = new ComplexModelViewModel();
        model.tests = [];

        model.tests.push(
          new Test({
            testName: "Seagull",
          })
        );

        expect(model.tests[0]).toBeInstanceOf(TestViewModel);
        expect(model.tests[0].testName).toBe("Seagull");
      });

      test("collection creates ViewModel when array containing a Model is set", () => {
        const model = new ComplexModelViewModel();

        model.tests = [
          new Test({
            testName: "Seagull",
          }),
        ];

        expect(model.tests[0]).toBeInstanceOf(TestViewModel);
        expect(model.tests[0].testName).toBe("Seagull");
        expect(model.tests[0].$parent).toBe(model);
        expect(model.tests[0].$parentCollection).toBe(model.tests);
      });

      test("collection is reactive for push", async () => {
        const model = new ComplexModelViewModel();
        model.tests = [];

        const watchCallback = vitest.fn();
        watch(model.tests, watchCallback);

        model.tests.push(new TestViewModel());

        await nextTick();

        expect(watchCallback).toBeCalledTimes(1);
        expect(model.tests).toHaveLength(1);
      });
    });

    describe("model collection (non-navigation) getter/setters", () => {
      test("setter creates new ViewModelCollection", () => {
        const model = new ComplexModelViewModel();
        model.unmappedCollectionOfMappedModels = [];

        expect(model.unmappedCollectionOfMappedModels.push).not.toBe(
          Array.prototype.push
        );
        expect(model.unmappedCollectionOfMappedModels.$metadata).toBe(
          model.$metadata.props.unmappedCollectionOfMappedModels
        );
      });

      test("collection creates ViewModel when Model is pushed", () => {
        const model = new ComplexModelViewModel();
        model.unmappedCollectionOfMappedModels = [];

        model.unmappedCollectionOfMappedModels.push(
          new Test({
            testName: "Seagull",
          })
        );

        expect(model.unmappedCollectionOfMappedModels[0]).toBeInstanceOf(
          TestViewModel
        );
        expect(model.unmappedCollectionOfMappedModels[0].testName).toBe(
          "Seagull"
        );
      });
    });

    describe("reference navigation & FK getter/setters", () => {
      test("setter copies foreign key", () => {
        var student = new StudentViewModel();
        var advisor = new AdvisorViewModel({ advisorId: 3 });
        student.advisor = advisor;
        expect(student.studentAdvisorId).toBe(advisor.advisorId);
      });

      test("setter creates ViewModel when provided a Model", () => {
        const model = new ComplexModelViewModel();

        model.singleTest = new Test({ testId: 1 });

        expect(model.singleTest).toBeInstanceOf(TestViewModel);
      });

      test("clears FK when reference is nulled", () => {
        var student = new StudentViewModel({
          studentAdvisorId: 3,
          advisor: { advisorId: 3, name: "Delphine" },
        });

        student.advisor = null;
        expect(student.studentAdvisorId).toBeNull();
      });

      test("updates FK when reference is changed", () => {
        var student = new StudentViewModel({
          studentAdvisorId: 3,
          advisor: { advisorId: 3, name: "Delphine" },
        });

        student.advisor = { advisorId: 4, name: "Beth" } as any;
        expect(student.studentAdvisorId).toBe(4);
      });

      test("clears reference when FK is nulled", () => {
        var student = new StudentViewModel({
          studentAdvisorId: 3,
          advisor: { advisorId: 3, name: "Delphine" },
        });

        student.studentAdvisorId = null;
        expect(student.advisor).toBeNull();
      });

      test("maintains reference when FK is nulled if reference has null PK", () => {
        var student = new StudentViewModel();

        student.advisor = new AdvisorViewModel({
          advisorId: null,
          name: "Beth",
        });
        student.studentAdvisorId = null;
        expect(student.advisor!.name).toBe("Beth");
      });

      test("sets null FK when reference with null PK is set", () => {
        var student = new StudentViewModel({
          studentAdvisorId: 3,
        });

        student.advisor = new AdvisorViewModel({
          advisorId: null,
          name: "Beth",
        });
        expect(student.advisor!.name).toBe("Beth");
        expect(student.studentAdvisorId).toBeNull();
      });

      test("clears reference when FK no longer matches", () => {
        var student = new StudentViewModel({
          studentAdvisorId: 3,
          advisor: { advisorId: 3, name: "Delphine" },
        });

        student.studentAdvisorId = 4;
        expect(student.advisor).toBeNull();
      });

      test("maintains reference when FK is set to same value", () => {
        var student = new StudentViewModel({
          studentAdvisorId: 3,
          advisor: { advisorId: 3, name: "Delphine" },
        });

        const originalAdvisor = student.advisor;
        student.studentAdvisorId = 3;
        expect(student.advisor).toBe(originalAdvisor);
      });
    });
  });

  describe("$loadFromModel", () => {
    test("preserves & updates existing reference navigations when key is same", () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine" },
      });
      var student = new StudentViewModel(studentModel);

      var currentAdvisor = student.advisor;
      studentModel.advisor!.name = "Beth";
      student.$loadCleanData(studentModel);

      // Reference should be the same.
      expect(student.advisor).toBe(currentAdvisor);
      // Properties should be updated, and model shoudn't have been marked dirty.
      expect(student.advisor!.name).toBe("Beth");
      expect(student.advisor!.$isDirty).toBe(false);
    });

    test("overwrites existing reference navigations when key is different", () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine" },
      });
      var student = new StudentViewModel(studentModel);

      var currentAdvisor = student.advisor;
      studentModel.studentAdvisorId = 4;
      studentModel.advisor!.name = "Beth";
      studentModel.advisor!.advisorId = 4;
      student.$loadCleanData(studentModel);

      // Reference should be different.
      expect(student.advisor).not.toBe(currentAdvisor);
      // Properties should be updated, and new model shoudn't be dirty.
      expect(student.advisor!.name).toBe("Beth");
      expect(student.advisor!.$isDirty).toBe(false);
    });

    test("preserves existing reference navigation when incoming ref is null but key is same and non-null", () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine" },
      });
      var student = new StudentViewModel(studentModel);

      var currentAdvisor = student.advisor;
      studentModel.advisor = null;
      student.$loadCleanData(studentModel);

      // Reference should be the same.
      expect(student.advisor).toBe(currentAdvisor);
    });

    test("clears existing reference navigation when incoming ref is null but key doesn't match", () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine" },
      });
      var student = new StudentViewModel(studentModel);

      // Make the incoming reference null,
      // and make the key not match the current key.
      studentModel.studentAdvisorId = 4;
      studentModel.advisor = null;
      student.$loadCleanData(studentModel);

      // Reference should have been cleared out.
      expect(student.advisor).toBeNull();
    });

    test("clears existing reference navigation when incoming ref and key are null, and existing object has a PK", () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine" },
      });
      var student = new StudentViewModel(studentModel);
      expect(student.advisor).not.toBeNull();

      studentModel.studentAdvisorId = null;
      studentModel.advisor = null;
      student.$loadCleanData(studentModel);

      // Reference should have been cleared out.
      expect(student.advisor).toBeNull();
      expect(student.studentAdvisorId).toBeNull();
    });

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
      var advisor = (student.advisor = new AdvisorViewModel());

      // Simulate first save against Student.
      student.$loadCleanData({
        studentId: 1,
        studentAdvisorId: null,
        advisor: null,
      });
      // Advisor reference should have been preserved because Advisor is an unsaved model with a null PK.
      expect(student.advisor).toBe(advisor);
    });

    test("updates foreign keys from navigation props' PKs when navigation prop is iterated first", () => {
      // Precondition: This tests the behavior if the navigation prop is iterated
      // BEFORE the FK prop.
      // Assert that the precondition now holds.
      const values = Object.values(metadata.Student.props);
      expect(values.indexOf(metadata.Student.props.advisor)).toBeLessThan(
        values.indexOf(metadata.Student.props.studentAdvisorId)
      );

      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        advisor: { advisorId: 3, name: "Delphine" },
      });
      var student = new StudentViewModel(studentModel);

      studentModel.studentAdvisorId = 4;
      studentModel.advisor!.advisorId = 4;
      studentModel.advisor!.name = "Beth";
      student.$loadCleanData(studentModel);

      // FK on course should have been updated
      // with the PK from the student object.

      // There was a bug where the PK was being sourced from the wrong object.
      // This only happened in rare cases where the nav prop was iterated before the FK prop.
      expect(student.studentAdvisorId).toBe(4);
    });

    test("updates foreign keys from navigation props' PKs when navigation prop is iterated second", () => {
      // Precondition: This tests the behavior if the navigation prop is iterated
      // AFTER the FK prop.
      // Assert that the precondition holds.
      const values = Object.values(metadata.Course.props);
      expect(values.indexOf(metadata.Course.props.student)).toBeGreaterThan(
        values.indexOf(metadata.Course.props.studentId)
      );

      var courseModel = new Course({
        courseId: 1,
        studentId: 3,
        student: { studentId: 3, name: "Delphine" },
      });
      var course = new CourseViewModel(courseModel);

      courseModel.studentId = 4;
      courseModel.student!.studentId = 4;
      courseModel.student!.name = "Beth";
      course.$loadCleanData(courseModel);

      // FK on course should have been updated
      // with the PK from the student object.

      // There was a bug where the PK was being sourced from the wrong object.
      // This only happened in rare cases where the nav prop was iterated before the FK prop.
      expect(course.studentId).toBe(4);
    });

    test("sets FK and navigation when loading object with only navigation property when navigation prop is iterated first", () => {
      // Assert that the precondition now holds.
      const values = Object.values(metadata.Student.props);
      expect(values.indexOf(metadata.Student.props.advisor)).toBeLessThan(
        values.indexOf(metadata.Student.props.studentAdvisorId)
      );

      var advisor = new AdvisorViewModel({ advisorId: 1, name: "Steve" });
      var student = new StudentViewModel();
      student.$loadCleanData({ advisor });

      expect(student.advisor).toBe(advisor);
      expect(student.studentAdvisorId).toBe(1);
    });

    test("sets FK and navigation when loading object with only navigation property when navigation prop is iterated second", () => {
      // Assert that the precondition now holds.
      const values = Object.values(metadata.Course.props);
      expect(values.indexOf(metadata.Course.props.student)).toBeGreaterThan(
        values.indexOf(metadata.Course.props.studentId)
      );

      var student = new StudentViewModel({ studentId: 1, name: "Steve" });
      var course = new CourseViewModel();
      course.$loadCleanData({ student });

      expect(course.student).toBe(student);
      expect(course.studentId).toBe(1);
    });

    test("preserves existing collection navigation when incoming is null", () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        courses: [{ courseId: 7, name: "foo" }],
      });
      var student = new StudentViewModel(studentModel);
      const existingCollection = student.courses;

      studentModel.courses = null;
      student.$loadCleanData(studentModel);

      // Collection should not have been cleared out.
      expect(student.courses).not.toBeNull();
      expect(student.courses).toBe(existingCollection);
      expect(student.courses![0].courseId).toBe(7);
    });

    test("clears existing collection navigation when incoming is []", () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 3,
        courses: [{ courseId: 7, name: "foo" }],
      });
      var student = new StudentViewModel(studentModel);
      const existingCollection = student.courses;

      studentModel.courses = [];
      student.$loadCleanData(studentModel);

      // Collection shouldn't be null, but should be empty.
      expect(student.courses).not.toBeNull();
      expect(student.courses).toBe(existingCollection);
      expect(student.courses).toHaveLength(0);
    });

    test("preserves existing collection navigation items when keys are same", () => {
      var studentModel = new Student({
        studentId: 1,
        courses: [
          { courseId: 7, name: "foo" },
          { courseId: 9, name: "bar" },
        ],
      });
      var student = new StudentViewModel(studentModel);
      const existingCollection = student.courses;

      var originalFoo = student.courses![0];
      var originalBar = student.courses![1];

      studentModel.courses![0].name = "baz";
      studentModel.courses![1].courseId = 11;
      studentModel.courses![1].name = "biz";
      student.$loadCleanData(studentModel);

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
    });

    test.each([
      ["delete from start", [{ courseId: 8 }, { courseId: 9 }]],
      ["delete from middle", [{ courseId: 7 }, { courseId: 9 }]],
      ["delete from end", [{ courseId: 7 }, { courseId: 8 }]],
      ["delete multiple", [{ courseId: 7 }]],
      ["replace start", [{ courseId: 1 }, { courseId: 8 }, { courseId: 9 }]],
      ["replace middle", [{ courseId: 7 }, { courseId: 1 }, { courseId: 9 }]],
      ["replace end", [{ courseId: 7 }, { courseId: 8 }, { courseId: 1 }]],
      [
        "add start",
        [{ courseId: 1 }, { courseId: 7 }, { courseId: 8 }, { courseId: 9 }],
      ],
      [
        "add middle",
        [{ courseId: 7 }, { courseId: 1 }, { courseId: 8 }, { courseId: 9 }],
      ],
      [
        "add end",
        [{ courseId: 7 }, { courseId: 8 }, { courseId: 9 }, { courseId: 1 }],
      ],
      ["reorder", [{ courseId: 9 }, { courseId: 8 }, { courseId: 7 }]],
      ["reorder", [{ courseId: 7 }, { courseId: 9 }, { courseId: 8 }]],
      ["reorder", [{ courseId: 9 }, { courseId: 7 }, { courseId: 8 }]],
      ["reorder", [{ courseId: 8 }, { courseId: 7 }, { courseId: 9 }]],
      ["reorder", [{ courseId: 8 }, { courseId: 9 }, { courseId: 7 }]],
    ])("collection navigation is reactive to %s", async (name, data) => {
      var studentModel = new Student({
        studentId: 1,
        courses: [{ courseId: 7 }, { courseId: 8 }, { courseId: 9 }],
      });
      var student = new StudentViewModel(studentModel);
      const existingCollection = student.courses;

      const vue = mountData({ student });
      const watchCallback = vitest.fn();
      watch(student.courses!, watchCallback);

      studentModel.courses = data.map((x) => new Course(x));
      student.$loadCleanData(studentModel);

      await vue.$nextTick();

      // Collection instance should have been kept the same.
      expect(student.courses).toBe(existingCollection);

      // Verify that instances look right.
      student.courses!.forEach((c) => {
        expect(c).toBeInstanceOf(CourseViewModel);
        expect(c.$parent).toBe(student);
        expect(c.$parentCollection).toBe(student.courses);
      });

      // Watcher should have been triggered because its contents changed
      expect(watchCallback).toBeCalledTimes(1);
    });

    test("collection navigation is not reactive when nothing changes", async () => {
      var studentModel = new Student({
        studentId: 1,
        courses: [
          { courseId: 7, name: "foo" },
          { courseId: 9, name: "bar" },
        ],
      });
      var student = new StudentViewModel(studentModel);
      const existingCollection = student.courses;

      const vue = mountData({ student });
      const watchCallback = vitest.fn();
      vue.$watch("student.courses", watchCallback);

      student.$loadCleanData(studentModel);

      await vue.$nextTick();

      // Collection instance should have been kept the same.
      expect(student.courses).toBe(existingCollection);

      // Watcher should not have been triggered because its contents shouldnt have changed
      expect(watchCallback).toBeCalledTimes(0);
    });

    test("doesnt stackoverflow when creating new recursive object structures", () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 1,
        advisor: { name: "Seagull", advisorId: 1 },
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

    test("doesnt stackoverflow when updating existing recursive object structures", () => {
      var studentModel = new Student({
        studentId: 1,
        studentAdvisorId: 1,
        advisor: { name: "Seagull", advisorId: 1 },
      });
      studentModel.advisor!.students = [studentModel];

      const student = new StudentViewModel();
      student.$loadCleanData(studentModel);

      // Act
      // Now that the ViewModel structure exists, try to update it.
      student.$loadCleanData(studentModel);

      // Assert
      // First expectation: We made it this far without stackoverflowing.

      // Second, different ways of traversing to the same VM should result in the same reference.
      expect(student.advisor).toBe(student.advisor!.students[0].advisor);

      // The root VM (`student`) should also be subject to this logic,
      // so the root should be the same instance seen in the advisor's students array.
      expect(student).toBe(student.advisor!.students[0]);
    });

    test("child object is reactive", async () => {
      var course = new CourseViewModel({
        student: { studentId: 1, name: "Steve" },
      });

      let triggered = false;
      watch(
        () => course.student?.name,
        () => (triggered = true)
      );
      course.$loadCleanData({ student: { studentId: 1, name: "Steve2" } });

      await delay(1);
      expect(triggered).toBe(true);
    });

    test.each([false, true])(
      "wipes unsaved items from collections when requested (purgeUnsaved: %s)",
      async (purgeUnsaved) => {
        const student = new StudentViewModel({
          courses: [
            // Saved (has PK), won't be purged:
            { courseId: 1, name: "CS101" },
            // Will be purged as unsaved if purgeUnsaved is true:
            { name: "CS102" },
          ],
        });

        student.$loadCleanData(
          {
            courses: [{ courseId: 1, name: "CS101" }],
          },
          purgeUnsaved
        );

        expect(student.courses?.length).toBe(purgeUnsaved ? 1 : 2);
      }
    );
  });

  describe("$delete", () => {
    test("removes deleted item from parent collection if item has PK", async () => {
      var student = new StudentViewModel(
        new Student({
          studentId: 1,
          courses: [{ courseId: 7, name: "foo" }],
        })
      );

      const course = student.courses![0];

      const deleteMock = (course.$apiClient.delete = vitest
        .fn()
        .mockResolvedValue(<AxiosItemResult<any>>{
          data: { wasSuccessful: true },
        }));

      await course.$delete();

      expect(deleteMock).toBeCalledTimes(1);
      expect(student.courses).toHaveLength(0);
    });

    test("removes deleted item from parent collection if item has no PK", async () => {
      var student = new StudentViewModel(
        new Student({
          studentId: 1,
          courses: [{ name: "foo" }],
        })
      );

      const course = student.courses![0];

      const deleteMock = (course.$apiClient.delete = vitest
        .fn()
        .mockResolvedValue(<AxiosItemResult<any>>{
          data: { wasSuccessful: true },
        }));

      await course.$delete();

      expect(deleteMock).toBeCalledTimes(0);
      expect(student.courses).toHaveLength(0);
    });
  });

  describe("$isDirty", () => {
    test("is reactive", async () => {
      var vm = new StudentViewModel();
      await delay(1);

      let triggered = false;
      watch(
        () => vm.$isDirty,
        () => (triggered = true)
      );
      vm.name = "bob";
      await delay(1);

      expect(triggered).toBeTruthy();
    });

    test("serializable collections are dirtied when mutated directly", async () => {
      var vm = new ComplexModelViewModel();
      vm.$isDirty = false;
      await delay(1);

      vm.intCollection = [42];
      expect(vm.$isDirty).toBeTruthy();

      vm.$isDirty = false;
      vm.intCollection.push(99);
      expect(vm.$isDirty).toBeTruthy();

      // Loading of new clean value doesn't trigger dirty:
      // This ensures that the watcher is ran synchronously,
      // since an async watcher would miss the $isDirty=false set that happens
      // after loading clean data.
      vm.$isDirty = false;
      vm.$loadCleanData({ intCollection: [42, 99] });
      expect(vm.$isDirty).toBeFalsy();
    });
  });

  describe("ctor", () => {
    test("preserves ViewModel instances in initialData", () => {
      var advisorVM = new AdvisorViewModel({
        advisorId: 1,
        name: "Steve",
      });

      var studentVM = new StudentViewModel({
        advisor: advisorVM,
      });

      // Should be the exact same reference.
      expect(studentVM.advisor).toBe(advisorVM);
    });

    describe("default values", () => {
      const meta = {
        ...metaBase(),
        get keyProp() {
          return this.props.id;
        },
        get displayProp() {
          return this.props.name;
        },
        props: {
          id: {
            name: "id",
            displayName: "id",
            role: "primaryKey",
            type: "number",
          },
          name: {
            name: "name",
            displayName: "Name",
            role: "value",
            type: "string",
            defaultValue: "Allen",
          },
        },
      } as ModelType;

      class TestVm extends ViewModel<{
        $metadata: typeof meta;
        id: number | null;
        name: string | null;
      }> {
        declare id: number | null;
        declare name: string | null;
      }
      defineProps(TestVm as any, meta);

      test("populates default values as dirty", () => {
        // If default values don't get marked dirty, they won't get saved
        // which could result in a mismatch between what the user sees in the UI
        // and what actually happens on the server.
        const instance = new TestVm(meta, null!);

        expect(instance.name).toBe("Allen");
        expect(instance.$getPropDirty("name")).toBe(true);
      });

      test("default values do not overwrite initial data", () => {
        const instance = new TestVm(meta, null!, { name: "Bob" });

        expect(instance.name).toBe("Bob");
        expect(instance.$getPropDirty("name")).toBe(true);
      });

      test("default values fill holes in initial data", () => {
        const instance = new TestVm(meta, null!, { id: 42 });

        expect(instance.id).toBe(42);
        expect(instance.name).toBe("Allen");
      });

      test("default values don't supersede nulls in initial data", () => {
        // When instantiating a viewmodel from an existing object, deliberate
        // nulls on the existing object shouldn't be replaced by default values.
        const instance = new TestVm(meta, null!, { id: 42, name: null });

        expect(instance.id).toBe(42);
        expect(instance.name).toBe(null);
      });
    });
  });

  describe("codegen", () => {
    test("emits optional method parameters as optional", async () => {
      const mock = mockEndpoint(
        "/ComplexModel/methodWithOptionalParams",
        vitest.fn((req) => ({
          wasSuccessful: true,
        }))
      );

      await new ComplexModelViewModel({
        complexModelId: 3,
      }).methodWithOptionalParams(42);

      // The request payload should have only included the parameters we actually provided.
      // The others should have been omitted entirely.
      const req: AxiosRequestConfig = mock.mock.lastCall?.[0];
      expect(req.data).toEqual("id=3&requiredInt=42");
    });
  });
});

describe("ListViewModel", () => {
  test("is assignable to generic untyped ListViewModel", () => {
    const vm: ListViewModel = new ComplexModelListViewModel();
  });

  describe("$load & $items", () => {
    let list: ComplexModelListViewModel;
    let includeAdditionalItemAtStart: boolean;

    beforeEach(() => {
      includeAdditionalItemAtStart = false;
      list = new ComplexModelListViewModel();
      mockEndpoint(
        "/ComplexModel/list",
        vitest.fn(() => {
          return {
            wasSuccessful: true,
            list: [
              ...(includeAdditionalItemAtStart
                ? [new ComplexModel({ complexModelId: 3, name: "John" })]
                : []),
              new ComplexModel({ complexModelId: 1, name: "Steve" }),
              new ComplexModel({ complexModelId: 2, name: "Bob" }),
            ],
            page: 1,
            pageSize: 10,
            pageCount: 1,
            totalCount: 2,
          };
        })
      );
    });

    test("$items is a ViewModelCollection initially", () => {
      // There was a mistake for a very long time where $items
      // would return a new, empty plain array when not initialized
      // and would return a new instance on every access.
      const items = list.$items;
      expect(items).toBe(list.$items);

      items.push({ name: "bob" });
      expect(items[0]).toBeInstanceOf(ComplexModelViewModel);
    });

    test("$items initializer doesn't trigger reactivity.", async () => {
      // Vue 3.4.15+ got a lot more pedantic about mutations occurring in computed getters.
      // It is now producing (harmless) warnings due to our lazy init of $items.
      // Make sure we're not doing that anymore.
      const warn = vi.spyOn(console, "warn");

      const itemsLength = computed(() => list.$items.length);
      itemsLength.value.toString();

      expect(warn).not.toHaveBeenCalled();
    });

    test("$items.filter produces a plain array", () => {
      // Much like how `reactive([1,2,3]).filter(...)` produces a plain array,
      // so too should ViewModelCollection.filter produce a plain array,
      // not a new ViewModelCollection instance (since VMC is itself reactive).

      // If we fail do to this, then subsequent mutations of the result of `filter`
      // will trigger reactivity, which is bad in a `computed` which will trigger infinite reactivity.

      const filteredItems = list.$items.filter((x) => x != null);
      expect(filteredItems).not.toBeInstanceOf(ViewModelCollection);

      // filteredItems should be entirely nonreactive:
      let changed = false;
      watch(filteredItems, () => (changed = true), {
        flush: "sync",
        deep: true,
      });
      filteredItems.push(new ComplexModelViewModel());
      expect(changed).toBeFalsy();
    });

    test("props on objects in $items are reactive", async () => {
      await list.$load();

      const vue = mount(
        defineComponent({
          template: "<div></div>",
          data() {
            return { list };
          },
          computed: {
            name() {
              return this.list.$items[1].name;
            },
          },
        })
      ).vm;
      const watchCallback = vitest.fn();
      vue.$watch("name", watchCallback);

      await vue.$nextTick();
      expect(watchCallback).toBeCalledTimes(0);

      list.$items[1].name = "Heidi";
      await vue.$nextTick();

      expect(watchCallback).toBeCalledTimes(1);
      expect(list.$items[1].name).toBe("Heidi");
    });

    test("$items is reactive", async () => {
      await list.$load();

      const vue = mountData({ list });
      const watchCallback = vitest.fn();
      watch(list.$items, watchCallback);

      list.$items.push(
        new ComplexModelViewModel({ complexModelId: 3, name: "Heidi" })
      );

      await vue.$nextTick();

      expect(watchCallback).toBeCalledTimes(1);
      expect(list.$items).toHaveLength(3);
    });

    test("$items is reactive when first usage is a read", async () => {
      const seekVal = ref(1);

      // There are two computeds here because only the very first
      // read of $items on a new ListViewModel was nonreactive (and only in vue2).
      const comp = computed(() => {
        return list.$items.find((i) => i.complexModelId == seekVal.value);
      });
      const comp2 = computed(() => {
        return list.$items.find((i) => i.complexModelId == seekVal.value + 1);
      });
      expect(comp.value).toBeUndefined();
      expect(comp2.value).toBeUndefined();

      await list.$load();

      expect(comp.value === list.$items[0]).toBeTruthy();
      expect(comp2.value === list.$items[1]).toBeTruthy();
    });

    test("identical loads do not trigger reactivity on $items", async () => {
      // This tests a performance scenario - avoid triggering
      // reactivity on list.$items if absolutely nothing changed.

      const vue = mountData({ list });

      // First load. Will only include the first 2 items.
      await list.$load();

      // Start watching the list's items collection
      await vue.$nextTick();
      const watchCallback = vitest.fn();
      vue.$watch("list.$items", watchCallback);

      // Reload the list with the exact same response from the API.
      await vue.$nextTick();
      await list.$load();
      await vue.$nextTick();

      // The watcher should not have been triggered.
      expect(watchCallback).toBeCalledTimes(0);
    });

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

      const vue = mount(
        defineComponent({
          template: "<div></div>",
          data() {
            return { list };
          },
          computed: {
            name() {
              return this.list.$items[0].name;
            },
          },
        })
      ).vm;
      const watchCallback = vitest.fn();

      // First load. Will only include the first 2 items.
      await list.$load();
      expect(list.$items).toHaveLength(2);

      // Next load should include a new item at the start of the list.
      includeAdditionalItemAtStart = true;
      await list.$load();
      expect(list.$items).toHaveLength(3);

      // Start watching the name of the first item in the list.
      await vue.$nextTick();
      vue.$watch("name", watchCallback);

      // Change the name of the first item in the list.
      list.$items[0].name = "Heidi";
      await vue.$nextTick();

      // The watcher should have been triggered.
      expect(watchCallback).toBeCalledTimes(1);
    });

    test("preserves same objects for same-keyed results", async () => {
      await list.$load();

      const item0 = list.$items[0],
        item1 = list.$items[1];

      // Modify a prop in the initial set of items to
      // make sure that the objects are reloaded with the incoming data.
      item0.name = "Heidi";

      await list.$load();

      expect(item0).toBe(list.$items[0]);
      expect(item1).toBe(list.$items[1]);
      expect(item0.name).toBe("Steve");
    });
  });

  describe("autoload", () => {
    test("works with setup()", async () => {
      var list = new StudentListViewModel();
      const loadMock = (list.$apiClient.list = mockItemResult(true, {}));

      const wrapper = mount(
        defineComponent({
          template: "<div></div>",
          setup() {
            list.$startAutoLoad(getCurrentInstance()!.proxy!, { wait: 0 });
            list.$params.search = "a";
          },
        })
      );

      await delay(10);
      expect(loadMock).toBeCalledTimes(1);

      // Watchers should be destroyed on component unmount.
      destroy(wrapper);
      expect((list as any)._autoLoadState.active).toBe(false);
    });

    test("useAutoLoad works with setup()", async () => {
      var list = new StudentListViewModel();
      const loadMock = (list.$apiClient.list = mockItemResult(true, {}));

      const wrapper = mount(
        defineComponent({
          template: "<div></div>",
          setup() {
            list.$useAutoLoad({ wait: 0 });
            list.$params.search = "a";
          },
        })
      );

      await delay(10);
      expect(loadMock).toBeCalledTimes(1);

      // Watchers should be destroyed on component unmount.
      destroy(wrapper);
      expect((list as any)._autoLoadState.active).toBe(false);
    });
  });

  describe("autosave", () => {
    test("propagates to new items", async () => {
      const list = new CaseListViewModel();
      const vue = mountData({ list });

      const saveMock = mockEndpoint(
        "/Case/save",
        vitest.fn(() => ({ wasSuccessful: true }))
      );

      list.$startAutoSave(vue, { wait: 0 });
      const item = new CaseViewModel({ title: "bob" });
      list.$items.push(item);

      expect(item.$isDirty).toBe(true);
      expect([...item.$getErrors()]).toHaveLength(0);

      await delay(10);

      expect(item.$isDirty).toBe(false);
      expect(saveMock).toBeCalledTimes(1);
    });

    test("propagates to existing items", async () => {
      const list = new CaseListViewModel();
      const vue = mountData({ list });

      const saveMock = mockEndpoint(
        "/Case/save",
        vitest.fn(() => ({ wasSuccessful: true }))
      );

      const item = new CaseViewModel({ title: "bob" });
      list.$items.push(item);

      list.$startAutoSave(vue, { wait: 0 });

      await delay(10);

      expect(item.$isDirty).toBe(false);
      expect(saveMock).toBeCalledTimes(1);
    });

    test("stops on existing items", async () => {
      const list = new CaseListViewModel();
      const vue = mountData({ list });

      const saveMock = mockEndpoint(
        "/Case/save",
        vitest.fn(() => ({ wasSuccessful: true }))
      );

      const item = new CaseViewModel({ title: "foo" });
      item.$isDirty = false;
      list.$items.push(item);

      list.$startAutoSave(vue, { wait: 0 });
      list.$stopAutoSave();

      item.title = "bar";

      await delay(10);

      expect(item.$isDirty).toBe(true);
      expect(saveMock).toBeCalledTimes(0);

      const item2 = new CaseViewModel();
      list.$items.push(item2);
      await delay(10);
      expect(item2.$isAutoSaveEnabled).toBeFalsy();
    });
  });
});
