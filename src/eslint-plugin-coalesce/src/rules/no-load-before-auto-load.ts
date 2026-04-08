import type { Rule } from "eslint";

/**
 * Rule: no-load-before-auto-load
 *
 * Detects when `$load()` is called on the same object that also has
 * `$useAutoLoad()` or `$startAutoLoad()` called, suggesting to use
 * `$useAutoLoad({ immediate: true })` instead.
 */
const rule: Rule.RuleModule = {
  meta: {
    type: "suggestion",
    docs: {
      description:
        "Disallow calling $load() alongside $useAutoLoad()/$startAutoLoad() - use the `immediate` option instead",
    },
    messages: {
      useImmediate:
        "Don't call `$load()` separately when using `{{autoMethod}}()`. Use `{{autoMethod}}({ immediate: true })` instead.",
    },
    schema: [],
  },
  create(context) {
    // Track calls to $load, $useAutoLoad, $startAutoLoad per function scope per object.
    // Key: serialized callee object source text. Value: array of call info.
    type CallInfo = {
      objectText: string;
      methodName: string;
      node: Rule.Node;
    };

    const scopeStacks: CallInfo[][] = [];

    function currentScope() {
      return scopeStacks[scopeStacks.length - 1];
    }

    function enterScope() {
      scopeStacks.push([]);
    }

    function exitScope() {
      const calls = scopeStacks.pop();
      if (!calls) return;

      // Group calls by object text
      const byObject = new Map<string, CallInfo[]>();
      for (const call of calls) {
        const existing = byObject.get(call.objectText);
        if (existing) {
          existing.push(call);
        } else {
          byObject.set(call.objectText, [call]);
        }
      }

      // For each object, check if both $load and an auto-load method are called
      for (const [, objCalls] of byObject) {
        const loadCalls = objCalls.filter((c) => c.methodName === "$load");
        const autoLoadCalls = objCalls.filter(
          (c) =>
            c.methodName === "$useAutoLoad" ||
            c.methodName === "$startAutoLoad",
        );

        if (loadCalls.length > 0 && autoLoadCalls.length > 0) {
          for (const loadCall of loadCalls) {
            context.report({
              node: loadCall.node,
              messageId: "useImmediate",
              data: {
                autoMethod: autoLoadCalls[0].methodName,
              },
            });
          }
        }
      }
    }

    function getObjectText(node: Rule.Node): string | null {
      return context.sourceCode.getText(node);
    }

    return {
      // Track function scopes
      FunctionDeclaration: enterScope,
      FunctionExpression: enterScope,
      ArrowFunctionExpression: enterScope,
      Program: enterScope,

      "FunctionDeclaration:exit": exitScope,
      "FunctionExpression:exit": exitScope,
      "ArrowFunctionExpression:exit": exitScope,
      "Program:exit": exitScope,

      CallExpression(node) {
        if (
          node.callee.type !== "MemberExpression" ||
          node.callee.property.type !== "Identifier"
        ) {
          return;
        }

        const methodName = node.callee.property.name;
        if (
          methodName !== "$load" &&
          methodName !== "$useAutoLoad" &&
          methodName !== "$startAutoLoad"
        ) {
          return;
        }

        const objectText = getObjectText(node.callee.object as Rule.Node);
        if (!objectText) return;

        const scope = currentScope();
        if (scope) {
          scope.push({
            objectText,
            methodName,
            node: node as unknown as Rule.Node,
          });
        }
      },
    };
  },
};

export default rule;
