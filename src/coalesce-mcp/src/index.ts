#!/usr/bin/env node

import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { registerCoalesceCodeGenTool } from "./tools/coalesce-generate.js";
import { registerTemplateFeatureResource } from "./tools/template-features.js";

// Create the MCP server
const server = new McpServer({
  name: "coalesce-mcp-server",
  version: "0.1.0",
});

// Register tools and resources
registerCoalesceCodeGenTool(server);
registerTemplateFeatureResource(server);

// Start the server
async function main() {
  const transport = new StdioServerTransport();
  await server.connect(transport);
  console.error("Coalesce MCP server running on stdio");
}

main().catch((error) => {
  console.error("Failed to start server:", error);
  process.exit(1);
});
