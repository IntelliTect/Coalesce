# MCP Server

The Coalesce MCP (Model Context Protocol) server enables AI assistants to interact with your Coalesce projects by providing code generation tools, template resources, and upgrade guidance. This allows AI assistants like Copilot to understand your project structure and generate appropriate code.

## What is MCP?

Model Context Protocol (MCP) is an open standard for connecting AI assistants to external data sources and tools. The Coalesce MCP server implements this protocol to provide AI assistants with:

- **Code Generation**: Trigger Coalesce code generation from AI conversations
- **Template Features**: Query available template features and access template files. For example, "Add the user photos feature from the Coalesce template to my project".
- **Upgrade Assistance**: Guide an AI agent through upgrading your project's Coalesce dependencies, comparing against the latest template and changelog.

## Installation

For projects created from the Coalesce template, MCP server configuration is included in the VS Code workspace settings. If you're looking to add this to an existing project, or to your user settings, choose from the command palette either `MCP: Open Workspace Folder MCP Configuration` or `MCP: Open User Configuration` and add the following to the file:

```json
{
  "servers": {
    "coalesce": {
      "command": "npx",
      "args": ["coalesce-mcp@latest"]
    }
  }
}
```

This configuration will be used by both VS Code and Visual Studio.

## Available Tools

### `coalesce_generate`

Runs Coalesce code generation for your project. 

**Usage**: This tool helps alleviate common agent confusion about exactly how to run code generation, or which directory to run it in. It also alleviates the need to grant permission to run a console command, since permanent consent can be granted to this tool.

### `get_template_features`

Retrieves information about available template features or lists all features.

**Usage**: The AI can query the files involved in a particular feature of the Coalesce template, and then use those files to apply the feature to an existing project.

### `read_template_file`

Reads the contents of a file from the Coalesce project template, or lists all available template files when called with no arguments. Supports fuzzy matching on file paths.

**Usage**: Useful during upgrades or when comparing your project's configuration against the latest template. For example, an agent can read the template's `package.json` or `vite.config.ts` and compare it against your project's version.

### `read_changelog`

Reads the Coalesce changelog, optionally filtered to only show entries newer than a specified version.

**Usage**: During upgrades, the agent uses this to identify breaking changes, new features, and template changes between your current version and the latest.

## Available Prompts

### `upgrade`

A step-by-step guide that walks an AI agent through upgrading a Coalesce project. The prompt instructs the agent to:

1. Determine the project's current Coalesce version
2. Review the changelog for breaking changes and new features
3. Update Coalesce NuGet and npm package versions
4. Compare third-party dependencies against the template
5. Compare configuration files (vite config, eslint, tsconfig, etc.)
6. Address any breaking changes
7. Run post-upgrade validation (build, code generation, tests)

**Usage**: In VS Code, use the prompt from chat by typing `/mcp` and selecting the "Upgrade Coalesce" prompt. The agent will then interactively walk through the upgrade process using the `read_template_file` and `read_changelog` tools.

## Available Resources

### `coalesce://template-file/{filePath}`

Provides access to Coalesce template files by path.

**Usage**: You can add individual source files from the Coalesce template as context to your session and ask the agent to use them to upgrade your project. This can be useful if you want to apply newer code patterns from the project template to your project that are not part of any specific feature.

In VS Code, click "Add Context..." -> "MCP Resources..." -> "Coalesce" and browse the list that appears.
