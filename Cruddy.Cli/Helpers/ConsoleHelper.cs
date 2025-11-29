namespace Cruddy.Cli.Helpers
{
    /// <summary>
    /// Helper for consistent console output with colors and formatting
    /// </summary>
    public static class ConsoleHelper
    {
        public static void WriteSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ {message}");
            Console.ResetColor();
        }

        public static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ {message}");
            Console.ResetColor();
        }

        public static void WriteWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠ {message}");
            Console.ResetColor();
        }

        public static void WriteInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"ℹ {message}");
            Console.ResetColor();
        }

        public static void WriteHeader(string message)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"═══ {message} ═══");
            Console.ResetColor();
            Console.WriteLine();
        }

        public static void WriteList(string item)
        {
            Console.WriteLine($"  • {item}");
        }
    }
}