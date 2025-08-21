# Coalesce MCP Server

A Model Context Protocol (MCP) server for the Coalesce framework.

## Overview

This MCP server provides AI assistants with access to Coalesce framework functionality and metadata. It enables intelligent code generation, analysis, and development assistance for Coalesce applications.

## Installation

```bash
npm install
npm run build
```

## Development

```bash
npm run dev  # Watch mode with TypeScript compilation
```

## Usage

The server runs on stdio transport and can be connected to any MCP-compatible client:

```bash
npm start
```

### Configuration

To use with Claude Desktop, add this server to your `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "coalesce": {
      "command": "node",
      "args": ["path/to/coalesce-mcp/build/index.js"]
    }
  }
}
```

## Available Tools

Currently includes:

- `generate_coalesce_code` - Run Coalesce code generation for a project
  - **Input**: `configPath` - Path to the coalesce.json configuration file
  - **Output**: Results of the code generation process including any output or error messages
  - **Requirements**: The `dotnet coalesce` command must be available in the system PATH

- `get_template_feature_files` - Get code files needed to add specific template features to an existing project
  - **Input**: 
    - `features` - Array of template feature names (e.g., ['Identity', 'DarkMode', 'AuditLogs'])
    - `templatePath` - Optional path to template directory (defaults to bundled template)
  - **Output**: All code files and their contents needed to implement the requested features
  - **Features Available**: Identity, MicrosoftAuth, GoogleAuth, OtherOAuth, LocalAuth, UserPictures, TrackingBase, AuditLogs, DarkMode, ExampleModel, OpenAPI, Tenancy, TenantCreateSelf, TenantCreateAdmin, TenantCreateExternal, TenantMemberInvites, AppInsights, Hangfire, AIChat, EmailAzure, EmailSendGrid, AzurePipelines, GithubActions

## Architecture

- **TypeScript**: Fully typed implementation
- **MCP SDK**: Built on the official Model Context Protocol SDK
- **Modular**: Tools are organized in separate files under `src/tools/` for easy extension
- **Clean Structure**: Each tool is self-contained with its own schema, validation, and execution logic

## Future Features

This server will be extended to provide:

- Coalesce metadata exploration
- Advanced code generation assistance  
- API documentation access
- Entity Framework model analysis
- Vue.js component generation guidance
- Project scaffolding and setup
- Configuration validation

## License

Apache-2.0
