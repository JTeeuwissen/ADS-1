using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using VaccinationScheduling.Shared;
using static VaccinationScheduling.Shared.Extensions;
using VaccinationScheduling.Online.Tree;
using VaccinationScheduling.Online.List;
using System.Text;

namespace VaccinationScheduling.Online
{
    public static class Program
    {
        // User can select custom input file from input directory
        private static void customInput()
        {
            Console.SetIn(new StreamReader($"..\\..\\..\\..\\VaccinationScheduling.Tests\\Input\\Online\\10000.in"));
        }

        public static void Main()
        {
            //customInput();
            Global global = ReadUtils.ReadGlobal();
            JobEnumerable jobs = new(global);
            Machines machines = new Machines(global);

            List<List<(int, int)>> verify = new List<List<(int, int)>>();
            int machine1, machine2, tFirstJob, tSecondJob;
            foreach (Job job in jobs)
            {
                (machine1, machine2, tFirstJob, tSecondJob) = machines.FindGreedySpot(job);
                //(machine1, machine2, tFirstJob, tSecondJob) = machines.FindSmartGreedySpot(job);
                // Verify the current machine
/*                if (tFirstJob < job.MinFirstIntervalStart || tFirstJob > job.MaxFirstIntervalStart || tFirstJob + job.MinGapIntervalStarts > tSecondJob || tFirstJob + job.MaxGapIntervalStarts < tSecondJob)
                {
                    throw new Exception("Illegal scheduling!");
                }*/
                machines.ScheduleJobs(machine1, machine2, tFirstJob, tSecondJob);
                Console.WriteLine((machine1, machine2, tFirstJob, tSecondJob));
                // Verify the solution (Only temporary)
/*                if (machine1 >= verify.Count || machine2 >= verify.Count)
                {
                    verify.Add(new List<(int, int)>());
                }
                if (verify[machine1].Count == 0)
                {
                    verify[machine1] = new List<(int, int)>();
                }
                if (verify[machine2].Count == 0)
                {
                    verify[machine2] = new List<(int, int)>();
                }
                verify[machine1].Add((tFirstJob, machines.freeRangesFirstJob.JobLength));
                verify[machine2].Add((tSecondJob, machines.freeRangesSecondJob.JobLength));*/
            }

/*            StringBuilder visualisation = new StringBuilder();
            // Verify the scheduled jobs do not overlap
            for (int i = 0; i < verify.Count; i++)
            {
                verify[i] = verify[i].OrderBy(x => x.Item1).ToList();
                int minimumNextItem = 0;
                for (int j = 0; j < verify[i].Count; j++)
                {
                    if (j == 0)
                    {
                        minimumNextItem = verify[i][j].Item1 + verify[i][j].Item2;
                        continue;
                    }
                    if (verify[i][j].Item1 < minimumNextItem)
                    {
                        throw new Exception($"An invalid solution got returned on machine {i} at time {verify[i][j].Item1}");
                    }
                    visualisation.Append('.', verify[i][j].Item1 - minimumNextItem);
                    visualisation.Append('$', verify[i][j].Item2);
                    minimumNextItem = verify[i][j].Item1 + verify[i][j].Item2;
                }
                visualisation.AppendLine();
            }*/
            Console.WriteLine($"Number machines used:" + machines.NrMachines);
/*
            Console.WriteLine(visualisation.ToString());*/
        }
    }
}
