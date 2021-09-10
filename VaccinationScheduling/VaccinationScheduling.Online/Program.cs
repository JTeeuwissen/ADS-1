using System;
using System.Collections.Generic;
using VaccinationScheduling.Shared;

namespace VaccinationScheduling.Online
{
    public class Program
    {
        private static Global _global;
        private static List<Job> _jobs = new();

        public static void Main()
        {
            _global = ReadUtils.ReadGlobal();

            while (true)
            {
                try
                {
                    Job job = ReadUtils.ReadJob();
                    _jobs.Add(job);
                }
                catch (Exception e)
                {
                    // There are no more patients
                    Console.WriteLine(e);
                    break;
                }

                // TODO Schedule
            }

            // TODO Output
        }
    }
}