using System;
using System.Collections.Generic;
using VaccinationScheduling.Shared;
using static VaccinationScheduling.Shared.Extensions;
using VaccinationScheduling.Online.Tree;
using VaccinationScheduling.Online.List;

namespace VaccinationScheduling.Online
{
    public static class Program
    {
        public static void Main()
        {
            Global global = ReadUtils.ReadGlobal();
            JobEnumerable jobs = new(global);
            Machines machines = new Machines(global);

            machines.freeRangesFirstJob.RemoveRange(4, 7, 1);
            machines.freeRangesSecondJob.RemoveRange(4, 7, 1);

            int nrMachines = 0;

            foreach (Job job in jobs)
            {
                machines.FindGreedySpot(job);
                // Console.WriteLine(new Schedule(tFirstJob, machineIndex, tSecondJob, machineIndex));
                /*// We have to create a new machine
                (tFirstJob, firstMachine, tSecondJob, secondMachine) = machine.FindGreedySpot(job)
                if (machine.FindGreedySpot(job) is not var (tFirstJob, tSecondJob))
                {
                    machine.ScheduleJobs(tFirstJob, tSecondJob);
                    WriteDebugLine("No free spot found, adding a new machine");
                }

                machines.Add(new Machine(global));
                machines[^1].ScheduleJobs(
                    job.MinFirstIntervalStart,
                    job.MinFirstIntervalStart + job.MinGapIntervalStarts
                );
                Console.WriteLine(new Schedule(job.MinFirstIntervalStart, machines.Count, job.MinFirstIntervalStart + job.MinGapIntervalStarts, machines.Count));*/
            }
        }
    }
}
