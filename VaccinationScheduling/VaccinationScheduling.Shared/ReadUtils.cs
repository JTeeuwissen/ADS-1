using System;

namespace VaccinationScheduling.Shared
{
    public static class ReadUtils
    {
        /// <summary>
        /// Read the global variables.
        /// </summary>
        /// <returns>The global variables.</returns>
        public static Global ReadGlobal()
        {
            return new Global(ReadNumber(), ReadNumber(), ReadNumber());
        }

        /// <summary>
        /// Read a job.
        /// Will trow an exception if no more patients are added in online mode.
        /// </summary>
        /// <returns>The job.</returns>
        public static Job ReadJob(Global global)
        {
            string[] values = Console.ReadLine()!.Split(',');
            return new Job(global, int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]), int.Parse(values[3]));
        }

        /// <summary>
        /// Read a number from the console.
        /// </summary>
        /// <returns>The number.</returns>
        public static int ReadNumber() => int.Parse(Console.ReadLine()!);
    }
}
