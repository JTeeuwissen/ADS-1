using System;
using System.Numerics;

namespace VaccinationScheduling.Shared.BigNumber
{
    public static class BigReadUtils
    {
        /// <summary>
        /// Read the global variables.
        /// </summary>
        /// <returns>The global variables.</returns>
        public static BigGlobal ReadGlobal()
        {
            return new BigGlobal(ReadNumber(), ReadNumber(), ReadNumber());
        }

        /// <summary>
        /// Read a job.
        /// Will trow an exception if no more patients are added in online mode.
        /// </summary>
        /// <returns>The job.</returns>
        public static BigJob ReadJob(BigGlobal global)
        {
            string[] values = Console.ReadLine()!.Split(',');
            return new BigJob(global, BigInteger.Parse(values[0]), BigInteger.Parse(values[1]), BigInteger.Parse(values[2]), BigInteger.Parse(values[3]));
        }

        /// <summary>
        /// Read a number from the console.
        /// </summary>
        /// <returns>The number.</returns>
        public static BigInteger ReadNumber() => BigInteger.Parse(Console.ReadLine()!);
    }
}
