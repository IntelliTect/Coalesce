

import Vue from 'vue';
import { AxiosClient, AxiosItemResult } from '../src/api-client'
import { mapToDto, mapToModel } from '../src/model';

import { StudentViewModel } from './targets.viewmodels';
import { Student } from './targets.models';

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

    student.$startAutoSave(vue, tickLength/2);
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

})