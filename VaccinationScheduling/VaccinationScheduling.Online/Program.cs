using System;
using System.Collections.Generic;
using VaccinationScheduling.Shared;
using VaccinationScheduling.Shared.Machine;

namespace VaccinationScheduling.Online
{
    public class Program
    {
        private Global global;
        private JobEnumerable jobs;

        List<MachineSchedule> schedules = new List<MachineSchedule>();

        public Program()
        {
            global = ReadUtils.ReadGlobal();
            jobs = new JobEnumerable(global);

            foreach (Job job in jobs)
            {
                bool added = false;

                for (int i = 0; i < schedules.Count; i++)
                {
                    (int, int) freespot = schedules[i].FindGreedySpot(job);
                    if (freespot == (-1, -1))
                    {
                        continue;
                    }

                    Console.WriteLine($"Found free spot on machine nr: {i}");
                    schedules[i].ScheduleJobs(freespot.Item1, freespot.Item2);
                    added = true;
                    break;
                }

                if (!added)
                {
                    Console.WriteLine("No free spot found, adding a new machine");
                    schedules.Add(new MachineSchedule(global));
                    schedules[schedules.Count - 1].ScheduleJobs(job.MinFirstIntervalStart, job.MinFirstIntervalStart + job.MinGapIntervalStarts);
                }
            }

            Console.WriteLine($"Solution has {schedules.Count} number of machines");
        }

        public static void Main()
        {
            // Escape the static environment
            new Program();
        }
    }
}
