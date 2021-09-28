using System;

namespace VaccinationScheduling.Shared
{
    public static class Extensions
    {
        public static void WriteDebugLine(string value)
        {
#if DEBUG
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(value);
            Console.ForegroundColor = currentColor;
#endif
        }
    }
}