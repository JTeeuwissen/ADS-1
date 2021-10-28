using System;
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
            Machines machines = new(global);
            
            foreach (Job job in jobs)
            {
                (int machine1, int machine2, int tFirstJab, int tSecondJab) = machines.FindGreedySpot(job);
                machines.ScheduleJobs(machine1, machine2, tFirstJab, tSecondJab);
                Console.WriteLine((machine1, machine2, tFirstJab, tSecondJab));
            }

            WriteDebugLine($"Number machines used:" + machines.NrMachines);
        }
    }
}
