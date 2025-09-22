# MCP Server

The Coalesce MCP (Model Context Protocol) server enables AI assistants to interact with your Coalesce projects by providing code generation tools and template resources. This allows AI assistants like Copilot to understand your project structure and generate appropriate code.

## What is MCP?

Model Context Protocol (MCP) is an open standard for connecting AI assistants to external data sources and tools. The Coalesce MCP server implements this protocol to provide AI assistants with:

- **Code Generation**: Trigger Coalesce code generation from AI conversations
- **Template Features**: Query available template features and access template files. For example, "Add the user photos feature from the Coalesce template to my project".

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

### `coalesce_get_template_features`

Retrieves information about available template features or lists all features.

**Usage**: The AI can query the files involved in a particular feature of the Coalesce template, and then use those files to apply the feature to an existing project.

## Available Resources

### `coalesce://template-file/{filePath}`

Provides access to Coalesce template files by path.

**Usage**: You can add individual source files from the Coalesce template as context to your session and ask the agent to use them to upgrade your project. This can be useful if you want to apply newer code patterns from the project template to your project that are not part of any specific feature.
