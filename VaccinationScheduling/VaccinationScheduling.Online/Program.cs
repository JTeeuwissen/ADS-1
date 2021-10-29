using System;
using System.Numerics;
using VaccinationScheduling.Shared.BigNumbers;
using static VaccinationScheduling.Shared.Extensions;

namespace VaccinationScheduling.Online
{
    public static class Program
    {
        /// <summary>
        /// Entrypoint of the online application. Reads loops through each job and schedules it immediately.
        /// </summary>
        public static void Main()
        {
            // Choose algorithm
            bool useStickyAlgorithm = true;

            BigGlobal global = BigReadUtils.ReadGlobal();
            JobEnumerable jobs = new(global);
            Machines machines = new(global);

            int machine1, machine2;
            BigInteger tFirstJab, tSecondJab;
            // Go through each job one by one
            foreach (BigJob job in jobs)
            {
                // Finds a spot and returns the machine numbers and timestamps
                if (useStickyAlgorithm)
                {
                    // Use sticky greedy algorithm
                    (machine1, machine2, tFirstJab, tSecondJab) = machines.FindStickyGreedySpot(job);
                }
                else
                {
                    // Use greedy algorithm
                    (machine1, machine2, tFirstJab, tSecondJab) = machines.FindGreedySpots(job);
                }

                // Update the tree with the given schedules.
                machines.ScheduleJobs(machine1, machine2, tFirstJab, tSecondJab, useStickyAlgorithm);
                // Print output to console
                Console.WriteLine(new BigSchedule(machine1, machine2, tFirstJab, tSecondJab));
            }

            // Output the amount of machines used
            WriteDebugLine($"{machines.NrMachines} machines");
        }
    }
}
