using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.IO;

namespace RetroRDPClient.Services
{
    /// <summary>
    /// Centralized logging service using Serilog for production-ready logging
    /// Logs to both console and file, with file logs stored in AppData
    /// </summary>
    public static class LoggingService
    {
        private static Logger? _logger;
        private static ILoggerFactory? _loggerFactory;
        private static string? _logFilePath;

        /// <summary>
        /// Initialize the logging service with file and console output
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // Create logs directory in AppData
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var logDirectory = Path.Combine(appDataPath, "RetroRDPClient", "logs");
                Directory.CreateDirectory(logDirectory);

                _logFilePath = Path.Combine(logDirectory, "rdpclient.log");

                // Configure Serilog with both file and console sinks
                _logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File(
                        _logFilePath,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB limit
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();

                // Create Microsoft.Extensions.Logging factory
                _loggerFactory = LoggerFactory.Create(builder =>
                    builder.AddSerilog(_logger, dispose: false));

                Log.Logger = _logger;
                _logger.Information("LoggingService initialized successfully. Log file: {LogFilePath}", _logFilePath);
            }
            catch (Exception ex)
            {
                // Fallback to console if file logging fails
                _logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .CreateLogger();
                
                _logger.Error(ex, "Failed to initialize file logging, using console only");
            }
        }

        /// <summary>
        /// Get a logger instance for a specific type
        /// </summary>
        public static ILogger<T> GetLogger<T>()
        {
            if (_loggerFactory == null)
            {
                Initialize();
            }
            return _loggerFactory!.CreateLogger<T>();
        }

        /// <summary>
        /// Get the Serilog logger directly
        /// </summary>
        public static Logger GetSerilogLogger()
        {
            if (_logger == null)
            {
                Initialize();
            }
            return _logger!;
        }

        /// <summary>
        /// Get the path to the current log file
        /// </summary>
        public static string? GetLogFilePath() => _logFilePath;

        /// <summary>
        /// Log RDP session connection attempt (ensuring no sensitive data is logged)
        /// </summary>
        public static void LogRdpConnectionAttempt(string sessionId, string host, int port, string username)
        {
            GetSerilogLogger().Information("RDP connection attempt: SessionId={SessionId}, Host={Host}, Port={Port}, Username={Username}", 
                sessionId, host, port, username);
        }

        /// <summary>
        /// Log RDP session connection success
        /// </summary>
        public static void LogRdpConnectionSuccess(string sessionId, string host)
        {
            GetSerilogLogger().Information("RDP connection successful: SessionId={SessionId}, Host={Host}", sessionId, host);
        }

        /// <summary>
        /// Log RDP session connection failure
        /// </summary>
        public static void LogRdpConnectionFailure(string sessionId, string host, string error)
        {
            GetSerilogLogger().Warning("RDP connection failed: SessionId={SessionId}, Host={Host}, Error={Error}", 
                sessionId, host, error);
        }

        /// <summary>
        /// Log RDP session disconnection
        /// </summary>
        public static void LogRdpDisconnection(string sessionId, string host, string reason)
        {
            GetSerilogLogger().Information("RDP session disconnected: SessionId={SessionId}, Host={Host}, Reason={Reason}", 
                sessionId, host, reason);
        }

        /// <summary>
        /// Log AI assistant command execution
        /// </summary>
        public static void LogAiCommandExecution(string command, string result, bool success)
        {
            GetSerilogLogger().Information("AI command executed: Command={Command}, Success={Success}, Result={Result}", 
                command, success, result);
        }

        /// <summary>
        /// Log application startup
        /// </summary>
        public static void LogApplicationStartup()
        {
            GetSerilogLogger().Information("RetroRDP Client application started. Version: {Version}, OS: {OS}", 
                Environment.Version, Environment.OSVersion);
        }

        /// <summary>
        /// Log application shutdown
        /// </summary>
        public static void LogApplicationShutdown()
        {
            GetSerilogLogger().Information("RetroRDP Client application shutting down");
        }

        /// <summary>
        /// Log performance metrics for RDP sessions
        /// </summary>
        public static void LogPerformanceMetrics(string sessionId, int cpuUsage, long memoryUsage, double responseTime)
        {
            GetSerilogLogger().Information("RDP session performance: SessionId={SessionId}, CPU={CpuUsage}%, Memory={MemoryUsage}MB, ResponseTime={ResponseTime}ms", 
                sessionId, cpuUsage, memoryUsage / (1024 * 1024), responseTime);
        }

        /// <summary>
        /// Shutdown the logging service properly
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                _logger?.Information("LoggingService shutting down");
                Log.CloseAndFlush();
                _loggerFactory?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during logging shutdown: {ex.Message}");
            }
        }
    }
}