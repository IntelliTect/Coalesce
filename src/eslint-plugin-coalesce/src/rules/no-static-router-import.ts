import type { Rule } from "eslint";

/**
 * Rule: no-static-router-import
 *
 * Detects direct default imports of the router instance in Vue component files,
 * which almost always create circular dependencies.
 * Suggests using `useRouter()` from vue-router instead.
 *
 * Only flags default imports where the local name is "router" from paths
 * containing "router" (e.g. '@/router', '../router', './router').
 * Only applies to .vue files.
 */
const rule: Rule.RuleModule = {
  meta: {
    type: "problem",
    docs: {
      description:
        "Disallow direct import of the router instance in Vue components - use `useRouter()` instead",
    },
    messages: {
      useRouter:
        "Importing `router` directly into components can produce circular imports. Use `const router = useRouter()` from `vue-router` instead.",
    },
    schema: [],
  },
  create(context) {
    const filename = context.filename;
    if (!filename.endsWith(".vue")) {
      return {};
    }

    return {
      ImportDeclaration(node) {
        const source = node.source.value;
        if (typeof source !== "string") return;

        // Check if importing from a path that contains "router"
        if (!/\brouter\b/i.test(source)) return;

        // Check if there's a default import specifier named "router"
        for (const specifier of node.specifiers) {
          if (
            specifier.type === "ImportDefaultSpecifier" &&
            specifier.local.name === "router"
          ) {
            context.report({
              node: node as unknown as Rule.Node,
              messageId: "useRouter",
            });
          }
        }
      },
    };
  },
};

export default rule;
