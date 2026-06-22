import { describe, it, expect } from "vitest";
import {
  DataSourceParameters,
  SaveParameters,
  FilterParameters,
  ListParameters,
} from "../src/api-client";

describe("Parameter Classes - Partial Constructor Support", () => {
  describe("DataSourceParameters", () => {
    it("should initialize with defaults when no data provided", () => {
      const params = new DataSourceParameters();
      expect(params.includes).toBeNull();
      expect(params.dataSource).toBeNull();
      expect(params.refResponse).toBe(false);
    });

    it("should initialize with partial data", () => {
      const params = new DataSourceParameters({
        includes: "relations",
        refResponse: true,
      });
      expect(params.includes).toBe("relations");
      expect(params.refResponse).toBe(true);
      expect(params.dataSource).toBeNull();
    });

    it("should merge partial data with defaults", () => {
      const params = new DataSourceParameters({ includes: "test" });
      expect(params.includes).toBe("test");
      expect(params.refResponse).toBe(false);
      expect(params.dataSource).toBeNull();
    });
  });

  describe("FilterParameters", () => {
    it("should initialize with defaults when no data provided", () => {
      const params = new FilterParameters();
      expect(params.search).toBeNull();
      expect(params.filter).toEqual({});
      expect(params.includes).toBeNull();
      expect(params.refResponse).toBe(false);
    });

    it("should initialize with partial data", () => {
      const params = new FilterParameters({
        search: "test",
        filter: { status: "active" },
        includes: "relations",
      });
      expect(params.search).toBe("test");
      expect(params.filter).toEqual({ status: "active" });
      expect(params.includes).toBe("relations");
      expect(params.refResponse).toBe(false);
    });

    it("should merge partial data with defaults", () => {
      const params = new FilterParameters({ search: "query" });
      expect(params.search).toBe("query");
      expect(params.filter).toEqual({});
      expect(params.includes).toBeNull();
    });
  });

  describe("ListParameters", () => {
    it("should initialize with defaults when no data provided", () => {
      const params = new ListParameters();
      expect(params.page).toBe(1);
      expect(params.pageSize).toBe(10);
      expect(params.noCount).toBeNull();
      expect(params.orderBy).toBeNull();
      expect(params.orderByDescending).toBeNull();
      expect(params.fields).toBeNull();
      expect(params.search).toBeNull();
      expect(params.filter).toEqual({});
      expect(params.includes).toBeNull();
      expect(params.refResponse).toBe(false);
    });

    it("should initialize with partial data", () => {
      const params = new ListParameters({
        page: 2,
        pageSize: 20,
        search: "test",
        orderBy: "name",
        fields: ["id", "name"],
      });
      expect(params.page).toBe(2);
      expect(params.pageSize).toBe(20);
      expect(params.search).toBe("test");
      expect(params.orderBy).toBe("name");
      expect(params.fields).toEqual(["id", "name"]);
      expect(params.filter).toEqual({});
      expect(params.noCount).toBeNull();
    });

    it("should support filter in partial data", () => {
      const params = new ListParameters({
        filter: { status: "active", type: "user" },
      });
      expect(params.filter).toEqual({ status: "active", type: "user" });
      expect(params.page).toBe(1);
      expect(params.pageSize).toBe(10);
    });

    it("should merge partial data with defaults", () => {
      const params = new ListParameters({
        page: 3,
        search: "query",
        includes: "relations",
      });
      expect(params.page).toBe(3);
      expect(params.pageSize).toBe(10);
      expect(params.search).toBe("query");
      expect(params.includes).toBe("relations");
      expect(params.orderBy).toBeNull();
      expect(params.orderByDescending).toBeNull();
    });

    it("should support all inherited properties", () => {
      const params = new ListParameters({
        page: 5,
        pageSize: 50,
        search: "test",
        filter: { active: true },
        includes: "relations",
        refResponse: true,
      });
      expect(params.page).toBe(5);
      expect(params.pageSize).toBe(50);
      expect(params.search).toBe("test");
      expect(params.filter).toEqual({ active: true });
      expect(params.includes).toBe("relations");
      expect(params.refResponse).toBe(true);
    });
  });

  describe("SaveParameters", () => {
    it("should initialize with defaults when no data provided", () => {
      const params = new SaveParameters();
      expect(params.fields).toBeNull();
      expect(params.includes).toBeNull();
      expect(params.refResponse).toBe(false);
    });

    it("should initialize with partial data", () => {
      const params = new SaveParameters({
        fields: ["name", "email"],
        includes: "relations",
      });
      expect(params.fields).toEqual(["name", "email"]);
      expect(params.includes).toBe("relations");
      expect(params.refResponse).toBe(false);
    });

    it("should merge partial data with defaults", () => {
      const params = new SaveParameters({ refResponse: true });
      expect(params.fields).toBeNull();
      expect(params.refResponse).toBe(true);
      expect(params.includes).toBeNull();
    });
  });

  describe("Chaining partial constructors", () => {
    it("should work with nested class hierarchy", () => {
      // This tests that the inheritance chain works correctly
      const listParams = new ListParameters({
        page: 2,
        pageSize: 25,
        search: "test",
        filter: { type: "user" },
      });

      expect(listParams).toBeInstanceOf(ListParameters);
      expect(listParams).toBeInstanceOf(FilterParameters);
      expect(listParams).toBeInstanceOf(DataSourceParameters);

      expect(listParams.page).toBe(2);
      expect(listParams.pageSize).toBe(25);
      expect(listParams.search).toBe("test");
      expect(listParams.filter).toEqual({ type: "user" });
    });
  });
});
