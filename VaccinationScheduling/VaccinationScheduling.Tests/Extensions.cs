using System;
using System.IO;

namespace VaccinationScheduling.Tests
{
    internal static class Extensions
    {
        public static void SetInput(string filePath)
        {
            Console.SetIn(new StreamReader(filePath));
        }
    }
}