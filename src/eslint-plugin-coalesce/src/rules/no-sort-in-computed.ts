import type { Rule } from "eslint";

/**
 * Rule: no-sort-in-computed
 *
 * Detects `.sort()` calls inside `computed()` callbacks, which can cause
 * infinite reactivity loops. Suggests using `.toSorted()` instead.
 *
 * Allows `.sort()` when chained after methods that produce new arrays
 * (map, filter, flatMap, concat, toReversed, toSpliced, slice),
 * or when called on array spread expressions like `[...arr].sort()`.
 */
const rule: Rule.RuleModule = {
  meta: {
    type: "problem",
    docs: {
      description:
        "Disallow in-place `.sort()` in `computed()` functions to prevent infinite reactivity loops",
    },
    messages: {
      noSort:
        "In-place `.sort()` in `computed()` functions can produce infinite reactivity loops. Use `.toSorted()` instead.",
    },
    schema: [],
  },
  create(context) {
    // Track whether we're inside a computed() callback
    let computedDepth = 0;

    // Track variables initialized with new arrays inside computed callbacks.
    // Stack of sets, one per computed depth.
    const localArrayVars: Set<string>[] = [];

    const newArrayMethods = new Set([
      "map",
      "filter",
      "flatMap",
      "concat",
      "toReversed",
      "toSpliced",
      "toSorted",
      "slice",
      "flat",
    ]);

    function isNewArrayProducer(node: Rule.Node): boolean {
      // Array literal: [].sort() or [...arr].sort()
      if (node.type === "ArrayExpression") {
        return true;
      }

      // Chained after a method that returns a new array: arr.map(...).sort()
      if (
        node.type === "CallExpression" &&
        node.callee.type === "MemberExpression" &&
        node.callee.property.type === "Identifier" &&
        newArrayMethods.has(node.callee.property.name)
      ) {
        return true;
      }

      // Local variable initialized with an array literal
      if (node.type === "Identifier") {
        for (let i = localArrayVars.length - 1; i >= 0; i--) {
          if (localArrayVars[i].has(node.name)) return true;
        }
      }

      return false;
    }

    return {
      CallExpression(node) {
        // Check if this is a computed() call
        if (
          node.callee.type === "Identifier" &&
          node.callee.name === "computed"
        ) {
          computedDepth++;
          localArrayVars.push(new Set());
          return;
        }

        // Check if we're inside a computed and this is a .sort() call
        if (computedDepth > 0) {
          if (
            node.callee.type === "MemberExpression" &&
            node.callee.property.type === "Identifier" &&
            node.callee.property.name === "sort"
          ) {
            // Allow if called on something that produces a new array
            if (!isNewArrayProducer(node.callee.object as Rule.Node)) {
              context.report({
                node: node as unknown as Rule.Node,
                messageId: "noSort",
              });
            }
          }
        }
      },

      "CallExpression:exit"(node: Rule.Node & { type: "CallExpression" }) {
        if (
          node.callee.type === "Identifier" &&
          node.callee.name === "computed"
        ) {
          computedDepth--;
          localArrayVars.pop();
        }
      },

      VariableDeclarator(node: Rule.Node & { type: "VariableDeclarator" }) {
        if (
          computedDepth > 0 &&
          node.id.type === "Identifier" &&
          node.init?.type === "ArrayExpression"
        ) {
          localArrayVars[localArrayVars.length - 1].add(node.id.name);
        }
      },
    };
  },
};

export default rule;
