namespace Cruddy.Cli.Helpers
{
    /// <summary>
    /// Helper for consistent console output with colors and formatting
    /// </summary>
    public static class ConsoleHelper
    {
        public static void WriteSuccess(string message)
        {
            Console.WriteLine($"✓ {message}");
        }

        public static void WriteError(string message)
        {
            Console.WriteLine($"✗ {message}");
        }

        public static void WriteWarning(string message)
        {
            Console.WriteLine($"⚠ {message}");
        }

        public static void WriteInfo(string message)
        {
            Console.WriteLine($" {message}");
        }

        public static void WriteHeader(string message)
        {
            Console.WriteLine();
            Console.WriteLine($"═══ {message} ═══");
            Console.WriteLine();
        }

        public static void WriteList(string item)
        {
            Console.WriteLine($"  • {item}");
        }
    }
}