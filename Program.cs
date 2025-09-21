using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<McpServer>();

var app = builder.Build();

app.MapPost("/mcp", (HttpContext httpContext, CancellationToken cancellationToken, McpServer mcpServer) => mcpServer.HandleRequest(httpContext, cancellationToken));

await app.RunAsync("http://localhost:1234");

public class McpServer
{
    public async Task HandleRequest(HttpContext context, CancellationToken cancellationToken)
    {
        var request = await JsonSerializer.DeserializeAsync<JsonElement>(context.Request.Body);

        var requestMethod = request.GetProperty("method").GetString();

        var requestParameters = request.TryGetProperty("params", out var paramsProp) ? paramsProp : default;

        var requestId = request.TryGetProperty("id", out var idProp)
            ? idProp.GetInt32()
            : 0;

        var result = requestMethod switch
        {
            "initialize" => OnInitialize(requestParameters),
            "notifications/initialized" => StatusOkResult(),
            "notifications/cancelled" => StatusOkResult(),
            "tools/list" => OnToolsList(),
            "tools/call" => OnToolsCall(requestParameters),
            _ => ErrorResult($"Unknown method: {requestMethod}")
        };

        var response = new
        {
            jsonrpc = "2.0",
            id = requestId,
            result
        };

        await context.Response.WriteAsJsonAsync(response);
    }

    object OnInitialize(JsonElement parameters)
    {
        var protocolVersion = parameters.GetProperty("protocolVersion").GetString();

        return new
        {
            protocolVersion,
            capabilities = new
            {
                resources = new { subscribe = true },
                tools = new { listChanged = true }
            },
            serverInfo = new
            {
                name = "vanilla-mcp",
                version = "1.0.0"
            }
        };
    }

    object StatusOkResult() => new { status = "ok" };

    object ErrorResult(string errorMessage) => new { error = errorMessage };

    object OnToolsList()
    {
        return new
        {
            tools = new[]
            {
            new Tool {
                name = "get_system_status",
                description = "Checks the status of the system",
                inputSchema = new {
                    type = "object",
                    properties = new { },
                    required = Array.Empty<string>()
                }
            },
            new Tool {
                name = "get_devices",
                description = "Retrieves the list of sonos devices in the system",
                inputSchema = new {
                    type = "object",
                    properties = new { },
                    required = Array.Empty<string>()
                }
            },
            new Tool {
                name = "get_device_volume",
                description = "Retrieves the current volume level of a device by ID",
                inputSchema = new {
                    type = "object",
                    properties = new {
                        id = new {
                            type = "string",
                            description = "The ID of the device"
                        }
                    },
                    required = new[] { "id" }
                }
            },
            new Tool {
                name = "set_device_volume",
                description = "Sets the volume level of a device by ID",
                inputSchema = new {
                    type = "object",
                    properties = new {
                        id = new {
                            type = "string",
                            description = "The ID of the device"
                        },
                        volume = new {
                            type = "integer",
                            description = "The new volume level (0–100)"
                        }
                    },
                    required = new[] { "id", "volume" }
                }
            }
        }
        };
    }

    object OnToolsCall(JsonElement parameters)
    {
        var toolName = parameters.GetProperty("name").GetString();
        var arguments = parameters.GetProperty("arguments");

        return toolName switch
        {
            "get_system_status" => WrapTextResult("System is running"),
            "get_devices" => HandleGetDevices(),
            "get_device_volume" => HandleGetDeviceVolume(arguments),
            "set_device_volume" => HandleSetDeviceVolume(arguments),
            _ => WrapTextResult($"Unknown tool: {toolName}")
        };
    }

    object HandleGetDevices()
    {
        return WrapJsonResult(new[]
        {
            new AudioDevice { Id = "1", Name = "Living Room", Room = "Living Room", Model = "Soundbar" },
            new AudioDevice { Id = "2", Name = "Kitchen", Room = "Kitchen", Model = "Speaker" },
            new AudioDevice { Id = "3", Name = "Bedroom", Room = "Bedroom", Model = "Speaker" }
        });
    }

    object HandleGetDeviceVolume(JsonElement arguments)
    {
        var id = arguments.GetProperty("id").GetString();

        int volume = id switch
        {
            "1" => 25,
            "2" => 40,
            "3" => 15,
            _ => -1
        };

        if (volume == -1)
        {
            return WrapTextResult($"No device found with id {id}");
        }

        return WrapTextResult($"Volume for device {id} is {volume}");
    }

    object HandleSetDeviceVolume(JsonElement arguments)
    {
        var id = arguments.GetProperty("id").GetString();
        var volume1 = arguments.GetProperty("volume").GetInt32();

        return WrapTextResult($"Volume for device {id} set to {volume1}");
    }

    object WrapTextResult(string text)
    {
        return new
        {
            content = new object[]
            {
            new { type = "text", text }
            }
        };
    }

    object WrapJsonResult(object data) =>
        new
        {
            content = new object[]
            {
            new { type = "text", text = JsonSerializer.Serialize(data) }
            }
        };
}

public class Tool
{
    public required string name { get; set; }
    public required string description { get; set; }
    public required object inputSchema { get; set; }
}

public class AudioDevice
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Room { get; set; }
    public required string Model { get; set; }
}