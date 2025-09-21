# VanillaMcp

VanillaMcp is a simple dependency-free JSON-RPC server example for handling MCP tool calls.

## Features

- **JSON-RPC API**: Communicate with the server using standard JSON-RPC requests.
- **Extensible Tooling**: Easily add new tools to the API.

## API Endpoints

- `POST /mcp`: Main endpoint for all JSON-RPC requests.

### Supported Methods

- `initialize`: Initialize the protocol and get server capabilities.
- `tools/list`: List available tools and their input schemas.
- `tools/call`: Call a tool by name with parameters.

## Getting Started

1. **Build and Run**:

`dotnet build dotnet run`

2. **Send Requests**: Use any HTTP client to POST JSON-RPC requests to `/mcp`.
3. **Example mcpservers.json** to configure in LMStudio:
```json
{
  "mcpServers": {
    "VanillaMCP": {
      "url": "http://localhost:1234/mcp"
    }
  }
}
```
## License

This project is licensed under the GNU General Public License v3.0. See the [LICENSE](LICENSE) file for details.
