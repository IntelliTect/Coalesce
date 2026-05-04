import * as model from "../src/model";
import type { Value } from "../src/metadata";
import { shortStringify } from "./test-utils";
import { format, subYears } from "date-fns";
import { toDate } from "date-fns-tz";

import {
  ComplexModel as ComplexModelMeta,
  Person as PersonMeta,
  Test as TestMeta,
  Case as CaseMeta,
} from "@test-targets/metadata.g";
import { Case } from "@test-targets/models.g";
import { DisplaysModel } from "./model.shared";

const cmProps = ComplexModelMeta.props;

const basicModel = {
  $metadata: ComplexModelMeta,
  complexModelId: 1,
  name: "Bob",
};

describe("modelDisplay", () => {
  test("for object without $metadata, throws", () => {
    expect(() => model.modelDisplay({} as any)).toThrow(/has no \$metadata/);
  });

  test("returns value for value prop", () => {
    const meta = basicModel.$metadata;
    expect(meta.displayProp).toBe(meta.props.name);

    expect(model.modelDisplay(basicModel)).toBe(basicModel.name);
  });

  test("returns value for object prop", () => {
    expect(
      model.modelDisplay({
        $metadata: DisplaysModel,
        model: basicModel,
      }),
    ).toBe(basicModel.name);
  });
});

describe("propDisplay - resolves property", () => {
  test("by name", () => {
    expect(model.propDisplay(basicModel, "name")).toBe(basicModel.name);
  });

  test("by object", () => {
    expect(model.propDisplay(basicModel, cmProps.name)).toBe(basicModel.name);
  });
});

describe("display", () => {
  test("returns json for object without displayProp", () => {
    const typeMeta = {
      ...DisplaysModel,
      displayProp: undefined,
    };
    const instance = {
      $metadata: typeMeta,
      name: "Bob",
    };
    expect(model.modelDisplay(instance)).toBe('{"name":"Bob"}');
  });
});

interface DisplayData {
  meta: Value;
  model?: any;
  display: string | null;
  options?: model.DisplayOptions;
  error?: string;
}

function undisplayable(meta: Value, ...values: any[]) {
  return values.map((value) => {
    return {
      meta,
      model: value,
      error: `unparsable .* for ${meta.type} '${meta.name}'`,
    } as DisplayData;
  });
}

describe.each(<DisplayData[]>[
  { meta: cmProps.complexModelId, model: null, display: null },
  { meta: cmProps.complexModelId, model: undefined, display: null },
  { meta: cmProps.complexModelId, model: 1, display: "1" },
  { meta: cmProps.complexModelId, model: "1", display: "1" },
  ...undisplayable(cmProps.complexModelId, "abc", {}, [], true, new Date()),

  { meta: cmProps.name, model: null, display: null },
  { meta: cmProps.name, model: undefined, display: null },
  { meta: cmProps.name, model: "Bob", display: "Bob" },
  { meta: cmProps.name, model: 123, display: "123" },
  { meta: cmProps.name, model: true, display: "true" },
  ...undisplayable(cmProps.name, {}, [], new Date()),

  { meta: cmProps.dateTimeOffset, model: null, display: null },
  { meta: cmProps.dateTimeOffset, model: undefined, display: null },
  {
    meta: cmProps.dateTimeOffset,
    model: new Date(1990, 0, 2, 3, 4, 5),
    display: "1/2/1990 3:04:05 AM",
  },
  {
    meta: { ...cmProps.dateTimeOffset, dateKind: "time" },
    model: new Date(1990, 0, 2, 3, 4, 5),
    display: "3:04:05 AM",
  },
  {
    meta: { ...cmProps.dateTimeOffset, dateKind: "date" },
    model: new Date(1990, 0, 2, 3, 4, 5),
    display: "1/2/1990",
  },
  {
    meta: cmProps.dateTimeOffset,
    model: new Date(1990, 0, 2, 3, 4, 5),
    display: "1990",
    options: { format: "yyyy" },
  },
  {
    meta: cmProps.dateTimeOffset,
    model: toDate("2014-10-25T13:46:20+02:00"),
    display: "2014-10-25 07:46:20 EDT",
    options: {
      format: {
        format: "yyyy-MM-dd HH:mm:ss zzz",
        timeZone: "America/New_York",
      },
    },
  },
  {
    meta: cmProps.dateTimeOffset,
    model: subYears(new Date(), 2),
    display: "about 2 years ago",
    options: { format: { distance: true } },
  },
  {
    meta: cmProps.dateTimeOffset,
    model: subYears(new Date(), 2),
    display: "about 2 years",
    options: { format: { distance: true, addSuffix: false } },
  },
  {
    meta: cmProps.dateTimeOffset,
    model: "1990-01-02T03:04:05.000-08:00",
    // We define the expected using date-fns's format to make this test timezone-independent.
    display: format(
      new Date("1990-01-02T03:04:05.000-08:00"),
      "M/d/yyyy h:mm:ss aa",
    ),
  },
  ...undisplayable(
    cmProps.dateTimeOffset,
    true,
    123,
    "abc",
    {},
    [],
    new Date("!!Invalid"),
  ),

  { meta: PersonMeta.props.gender, model: undefined, display: null },
  { meta: PersonMeta.props.gender, model: null, display: null },
  { meta: PersonMeta.props.gender, model: 1, display: "Male" },
  { meta: PersonMeta.props.gender, model: "1", display: "Male" },
  // Non-explicitly-defined enums should display the number value
  // since there's nothing better to show:
  { meta: PersonMeta.props.gender, model: 111, display: "111" },
  ...undisplayable(PersonMeta.props.gender, true, "abc", {}, [], new Date()),

  { meta: cmProps.isActive, model: undefined, display: null },
  { meta: cmProps.isActive, model: null, display: null },
  { meta: cmProps.isActive, model: true, display: "true" },
  { meta: cmProps.isActive, model: "true", display: "true" },
  ...undisplayable(cmProps.isActive, 123, "abc", {}, [], new Date()),

  // Collection
  { meta: cmProps.tests, model: undefined, display: null },
  { meta: cmProps.tests, model: null, display: null },
  { meta: cmProps.tests, model: [], display: "" },
  {
    meta: cmProps.tests,
    model: ["CSCD 210", "CSCD 211", "MATH 301"].map((testName, i) => {
      return {
        $metadata: TestMeta,
        testName: testName,
        testId: i,
      };
    }),
    display: "CSCD 210, CSCD 211, MATH 301",
  },
  {
    meta: cmProps.tests,
    model: Array(10).map((testName, i) => {
      return {
        $metadata: TestMeta,
        testName: "CS10" + i,
        testId: i,
      };
    }),
    display: "10 items",
  },
  ...undisplayable(cmProps.tests, "abc", 123, {}, true, new Date()),

  // Many-to-many collection
  {
    meta: CaseMeta.props.caseProducts,
    model: new Case({
      caseProducts: ["Foo", "Bar", "Baz"].map((name) => {
        return {
          product: {
            name,
          },
        };
      }),
    }).caseProducts,
    display: "Foo, Bar, Baz",
  },

  // Model/Object
  { meta: cmProps.singleTest, model: null, display: null },
  {
    meta: cmProps.singleTest,
    model: { testId: 1, testName: "Steve" },
    display: "Steve",
  },
])("valueDisplay", (x) => {
  const {
    meta,
    model: modelValue,
    display,
    error,
    options,
  } = x as typeof x & { error?: string };

  describe(meta.type, () => {
    const expectedOutcomeDesc = error
      ? `throws /${error}/`
      : `returns ${shortStringify(display)}`;

    test(`for ${shortStringify(modelValue)}, ${expectedOutcomeDesc}`, () => {
      const doMap = () => model.valueDisplay(modelValue, meta, options);
      if (error) {
        expect(doMap).toThrow(new RegExp(error));
        return;
      }

      expect(doMap()).toBe(display);
    });
  });
});

describe("valueDisplay", () => {
  describe("date + defaultTimeZone", () => {
    test("date noOffset does not use defaultTimeZone", () => {
      model.setDefaultTimeZone("America/Adak");
      const result = model.valueDisplay(
        toDate("2014-10-25T13:46:20"),
        cmProps.dateTime,
        {
          format: "yyyy-MM-dd HH:mm:ss",
        },
      );

      // Output is what we put in.
      // Should not be shifted by TZ since dates without offsets
      // are meant to be timezone agnostic.
      expect(result).toBe("2014-10-25 13:46:20");
    });

    test("date with offset uses defaultTimeZone", () => {
      model.setDefaultTimeZone("America/Adak");
      const result = model.valueDisplay(
        toDate("2014-10-25T13:46:20+02:00"),
        cmProps.dateTimeOffset,
        {
          format: "yyyy-MM-dd HH:mm:ss zzzz",
        },
      );

      // Output is shifted from UTC+2 to UTC-9, a total of 11 hours (1300 hours to 0200 hours).
      expect(result).toBe("2014-10-25 02:46:20 Hawaii-Aleutian Daylight Time");
    });
  });
});
