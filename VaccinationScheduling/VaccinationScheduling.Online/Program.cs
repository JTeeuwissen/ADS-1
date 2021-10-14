using System.Collections.Generic;
using System.Linq;
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
                // Returns whether a job was added.
                bool ScheduleJob()
                {
                    foreach (MachineSchedule schedule in schedules)
                    {
                        if (schedule.FindGreedySpot(job) is not var (tFirstJob, tSecondJob)) continue;
                        schedule.ScheduleJobs(tFirstJob, tSecondJob);
                        return true;
                    }

                    return false;
                }

                if (ScheduleJob()) continue;

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