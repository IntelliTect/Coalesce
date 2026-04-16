#!/usr/bin/env node

import { McpServer } from "tmcp";
import { ZodV3JsonSchemaAdapter } from "@tmcp/adapter-zod-v3";
import { StdioTransport } from "@tmcp/transport-stdio";
import { registerCoalesceCodeGenTool } from "./tools/coalesce-generate.js";
import { registerTemplateFeatureResource } from "./tools/template-features.js";
import { registerUpgradeTools } from "./tools/upgrade-prompt.js";
import packageJson from "../package.json" with { type: "json" };

// Create the adapter for Zod schema validation
const adapter = new ZodV3JsonSchemaAdapter();

// Create the MCP server
const server = new McpServer(
  {
    name: "coalesce-mcp",
    version: packageJson.version,
    description: "MCP server for IntelliTect's Coalesce framework.",
  },
  {
    adapter,
    capabilities: {
      tools: { listChanged: true },
      resources: { listChanged: true },
      prompts: { listChanged: true },
    },
  },
);

// Register tools, resources, and prompts
registerCoalesceCodeGenTool(server);
registerTemplateFeatureResource(server);
registerUpgradeTools(server);

// Start the server
const transport = new StdioTransport(server);
transport.listen();
