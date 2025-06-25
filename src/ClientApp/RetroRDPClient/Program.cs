using System;
using System.Runtime.InteropServices;

namespace RetroRDPClient
{
    // Cross-platform application entry point
    // Console version for cross-platform compatibility
    // WPF version runs automatically on Windows
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // On Windows, attempt to start the WPF application if available
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    // Try to create WPF app using reflection to avoid compilation errors on non-Windows
                    var appType = Type.GetType("RetroRDPClient.App");
                    if (appType != null)
                    {
                        var app = Activator.CreateInstance(appType);
                        var initMethod = appType.GetMethod("InitializeComponent");
                        var runMethod = appType.GetMethod("Run", Type.EmptyTypes);
                        
                        initMethod?.Invoke(app, null);
                        runMethod?.Invoke(app, null);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("WPF Application class not found. Building on non-Windows platform?");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to start WPF application: {ex.Message}");
                    Console.WriteLine("Falling back to console mode...");
                }
            }

            // Console mode for non-Windows platforms or testing
            ShowConsoleVersion();
        }

        static void ShowConsoleVersion()
        {
            // Display retro cyber theme banner in console
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
    ╔══════════════════════════════════════════════════════════════════╗
    ║                                                                  ║
    ║    🎮  RETRO CYBER RDP CLIENT  🎗️                              ║
    ║                                                                  ║
    ║    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  ║
    ║    ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ║
    ║    ████████████████████████████████████████████████████████████  ║
    ║                                                                  ║
    ║         Modern Multi-Session Remote Desktop Client               ║
    ║         Retro-Futuristic Design • AI Integration                 ║
    ║                                                                  ║
    ╚══════════════════════════════════════════════════════════════════╝
            ");

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("    LEVEL 2: UI IMPLEMENTATION - WPF VERSION READY ✓");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("    Status:");
            Console.WriteLine("    • WPF Application with retro cyber theme");  
            Console.WriteLine("    • Sidebar Copilot interface implemented");
            Console.WriteLine("    • Tabbed RDP session interface ready");
            Console.WriteLine("    • Fluent design materials applied");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("    UI Components Created:");
            Console.WriteLine("    • MainWindow.xaml - Grid layout (20% sidebar + 80% main)");
            Console.WriteLine("    • Left sidebar with AI AssistBot chat interface");
            Console.WriteLine("    • TabControl with sample RDP sessions");
            Console.WriteLine("    • Retro-cyber styling with neon glow effects");
            Console.WriteLine("    • Responsive design and interaction handling");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("    Windows detected: WPF UI will launch automatically!");
            }
            else
            {
                Console.WriteLine("    Note: This console version shows the implementation status.");
                Console.WriteLine("    Deploy to Windows with WPF to see the full interface!");
            }
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("    Level 2 Implementation Complete:");
            Console.WriteLine("    ✓ Main window layout with Grid columns");
            Console.WriteLine("    ✓ Copilot-style left sidebar with chat");
            Console.WriteLine("    ✓ Tabbed interface for RDP sessions");
            Console.WriteLine("    ✓ Retro-cyber theme with neon effects");
            Console.WriteLine("    ✓ Fluent design materials integration");
            Console.WriteLine("    ✓ Responsive UI components");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("    Press any key to continue...");
            
            // Handle redirected input gracefully
            try
            {
                Console.ReadKey();
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("    (Console input redirected - continuing...)");
                System.Threading.Thread.Sleep(1000);
            }

            Console.ResetColor();
        }
    }
}
