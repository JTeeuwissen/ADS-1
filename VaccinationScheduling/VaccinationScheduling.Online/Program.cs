﻿using System;
using System.IO;
using System.Numerics;
using VaccinationScheduling.Shared;
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
            Console.SetIn(new StreamReader("C:\\Users\\gerard\\Downloads\\bigints.txt"));

            BigGlobal global = BigReadUtils.ReadGlobal();
            JobEnumerable jobs = new(global);
            Machines machines = new(global);

            // Go through each job one by one
            foreach (BigJob job in jobs)
            {
                // Finds a spot and returns the machine numbers and timestamps
                (int machine1, int machine2, BigInteger tFirstJab, BigInteger tSecondJab) = machines.FindGreedySpot(job);
                machines.ScheduleJobs(machine1, machine2, tFirstJab, tSecondJab);
                // Print where the job is scheduled
                Console.WriteLine(new Schedule(machine1, machine2, tFirstJab, tSecondJab));
            }

            // Output the amount of machines used
            WriteDebugLine($"Number machines used:" + machines.NrMachines);
        }
    }
}
