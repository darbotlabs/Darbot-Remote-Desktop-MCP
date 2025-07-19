using RetroRDP.MCPServer.Services;
using RetroRDP.MCPServer.Tools;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/mcp-server-.log", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "RetroRDP MCP Server", 
        Version = "v1",
        Description = "Model Context Protocol server for RetroRDP client integration"
    });
});

// Configure HTTP client for local connector
builder.Services.AddHttpClient<LocalConnector>();

// Register MCP services
builder.Services.AddSingleton<IMCPServer, MCPServer>();
builder.Services.AddSingleton<ILocalConnector, LocalConnector>();

// Register MCP tools
builder.Services.AddSingleton<IMCPTool, ConnectRDPTool>();
builder.Services.AddSingleton<IMCPTool, ListSessionsTool>();
builder.Services.AddSingleton<IMCPTool, ConfigureSessionTool>();

// Add CORS for web client access
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Start MCP server
var mcpServer = app.Services.GetRequiredService<IMCPServer>();
await mcpServer.StartAsync();

// Graceful shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("Shutting down MCP server...");
    mcpServer.StopAsync().GetAwaiter().GetResult();
});

Log.Information("Starting RetroRDP MCP Server on {Urls}", string.Join(", ", builder.WebHost.GetSetting("urls")?.Split(';') ?? new[] { "http://localhost:5000" }));

app.Run();

// Ensure to flush and close the logger when the application exits
Log.CloseAndFlush();