using System.Collections.Generic;
using VaccinationScheduling.Shared;
using VaccinationScheduling.Shared.Machine;
using static VaccinationScheduling.Shared.Extensions;

namespace VaccinationScheduling.Online
{
    public static class Program
    {
        public static void Main()
        {
            Global global = ReadUtils.ReadGlobal();
            JobEnumerable jobs = new(global);
            List<MachineSchedule> schedules = new();

            foreach (Job job in jobs)
            {
                bool added = false;

                foreach (MachineSchedule schedule in schedules)
                {
                    (int, int) freespot = schedule.FindGreedySpot(job);
                    if (freespot == (-1, -1))
                        continue;

                    schedule.ScheduleJobs(freespot.Item1, freespot.Item2);
                    added = true;
                    break;
                }

                if (added) continue;
                WriteDebugLine("No free spot found, adding a new machine");

                schedules.Add(new MachineSchedule(global));
                schedules[^1].ScheduleJobs(
                    job.MinFirstIntervalStart,
                    job.MinFirstIntervalStart + job.MinGapIntervalStarts
                );
            }

            WriteDebugLine($"Solution has {schedules.Count} number of machines");
        }
    }
}