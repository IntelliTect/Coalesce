<template>
  <div class="analyzer-rules">
    <template v-for="(rules, category) in rulesByCategory" :key="category">
      <h2 :id="category.toLowerCase().replace(/\s+/g, '-')">
        {{ category }}
        <a
          class="header-anchor"
          :href="'#' + category.toLowerCase().replace(/\s+/g, '-')"
          :aria-label="`Permalink to &quot;${category}&quot;`"
          >​</a
        >
      </h2>

      <template v-for="rule in rules" :key="rule.id">
        <h3 :id="rule.id" class="rule-heading">
          {{ rule.id }}
          <a
            class="header-anchor"
            :href="'#' + rule.id"
            :aria-label="`Permalink to &quot;${rule.id}&quot;`"
            >​</a
          >
        </h3>
        {{ rule.description }}
      </template>
    </template>
  </div>
</template>

<style lang="scss">
.analyzer-rules {
  .rule-heading {
    margin: 1.5rem 0 0.5rem 0;
  }
}
</style>

<script setup lang="ts">
import analyzerReleasesContent from "../../src/IntelliTect.Coalesce.Analyzer/AnalyzerReleases.Shipped.md?raw";

interface AnalyzerRule {
  id: string;
  category: string;
  severity: string;
  description: string;
}

interface AnalyzerRelease {
  version: string;
  rules: AnalyzerRule[];
}

const releases = parseAnalyzerReleases(analyzerReleasesContent);

// Flatten all rules from all releases into a single array
const allRules = releases.flatMap((release) => release.rules);

// Group rules by category
const rulesByCategory = allRules.reduce(
  (acc, rule) => {
    if (!acc[rule.category]) {
      acc[rule.category] = [];
    }
    acc[rule.category].push(rule);
    return acc;
  },
  {} as Record<string, AnalyzerRule[]>,
);

function parseAnalyzerReleases(content: string): AnalyzerRelease[] {
  const lines = content.split("\n");
  const releases: AnalyzerRelease[] = [];
  let currentRelease: AnalyzerRelease | null = null;
  let inRulesSection = false;

  for (const line of lines) {
    const trimmedLine = line.trim();

    // Skip empty lines and comments
    if (!trimmedLine || trimmedLine.startsWith(";")) {
      continue;
    }

    // Check for release headers (## Release X.X)
    const releaseMatch = trimmedLine.match(/^##\s+Release\s+(.+)$/);
    if (releaseMatch) {
      currentRelease = {
        version: releaseMatch[1],
        rules: [],
      };
      releases.push(currentRelease);
      inRulesSection = false;
      continue;
    }

    // Check for "New Rules" section
    if (trimmedLine === "### New Rules") {
      inRulesSection = true;
      continue;
    }

    // Check for table header (skip it)
    if (trimmedLine.startsWith("Rule ID") || trimmedLine.startsWith("-----")) {
      continue;
    }

    // Parse rule lines
    if (inRulesSection && currentRelease && trimmedLine.includes("|")) {
      const parts = trimmedLine
        .split("|")
        .map((p) => p.trim())
        .filter((p) => p);

      if (parts.length >= 4) {
        const rule: AnalyzerRule = {
          id: parts[0],
          category: parts[1],
          severity: parts[2],
          description: parts[3],
        };
        currentRelease.rules.push(rule);
      }
    }
  }

  return releases;
}
</script>
