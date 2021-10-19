using System;
using System.Collections.Generic;
using VaccinationScheduling.Shared;
using static VaccinationScheduling.Shared.Extensions;

namespace VaccinationScheduling.Online
{
    public static class Program
    {
        public static void Main()
        {
            Global global = ReadUtils.ReadGlobal();
            JobEnumerable jobs = new(global);
            List<Machine> machines = new();

            foreach (Job job in jobs)
            {
                // Returns whether a job was added.
                bool ScheduleJob()
                {
                    for (int machineIndex = 0; machineIndex < machines.Count; machineIndex++)
                    {
                        Machine machine = machines[machineIndex];
                        if (machine.FindGreedySpot(job) is not var (tFirstJob, tSecondJob)) continue;
                        Console.WriteLine(new Schedule(tFirstJob, machineIndex, tSecondJob, machineIndex));
                        machine.ScheduleJobs(tFirstJob, tSecondJob);
                        return true;
                    }

                    return false;
                }

                if (ScheduleJob()) continue;

                WriteDebugLine("No free spot found, adding a new machine");

                machines.Add(new Machine(global));
                machines[^1].ScheduleJobs(
                    job.MinFirstIntervalStart,
                    job.MinFirstIntervalStart + job.MinGapIntervalStarts
                );
                Console.WriteLine(new Schedule(job.MinFirstIntervalStart, machines.Count, job.MinFirstIntervalStart + job.MinGapIntervalStarts, machines.Count));
            }
        }
    }
}