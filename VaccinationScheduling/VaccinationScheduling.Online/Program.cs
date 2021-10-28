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
            Machines machines = new(global);

            List<List<(int, int)>> verify = new();
            foreach (Job job in jobs)
            {
                (int machine1, int machine2, int tFirstJab, int tSecondJab) = machines.FindGreedySpot(job);
                //(machine1, machine2, tFirstJob, tSecondJob) = machines.FindSmartGreedySpot(job);
                // Verify the current machine
/*                if (tFirstJob < job.MinFirstIntervalStart || tFirstJob > job.MaxFirstIntervalStart || tFirstJob + job.MinGapIntervalStarts > tSecondJob || tFirstJob + job.MaxGapIntervalStarts < tSecondJob)
                {
                    throw new Exception("Illegal scheduling!");
                }*/
                machines.ScheduleJobs(machine1, machine2, tFirstJab, tSecondJab);
                Console.WriteLine((machine1, machine2, tFirstJab, tSecondJab));
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
                verify[machine1].Add((tFirstJab, machines.freeRangesFirstJab.JabLength));
                verify[machine2].Add((tSecondJab, machines.freeRangesSecondJab.JabLength));

                Console.WriteLine(new Schedule(tFirstJob, machine1 + 1, tSecondJob, machine2 + 1));
            }

/*            StringBuilder visualisation = new StringBuilder();
            // Verify the scheduled jabs do not overlap
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
            }
            WriteDebugLine($"Number machines used:" + machines.NrMachines);
        }
    }
}
