using System;
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
        public static void Main()
        {
            Global global = ReadUtils.ReadGlobal();
            JobEnumerable jobs = new(global);
            Machines machines = new(global);

            List<List<(int, int)>> verify = new();
            foreach (Job job in jobs)
            {
                (int machine1, int machine2, int tFirstJob, int tSecondJob) = machines.FindGreedySpot(job);
                machines.ScheduleJobs(machine1, machine2, tFirstJob, tSecondJob);
                // Verify the solution (Only temporary)
                if (machine1 >= verify.Count || machine2 >= verify.Count)
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
                verify[machine2].Add((tSecondJob, machines.freeRangesSecondJob.JobLength));

                Console.WriteLine(new Schedule(tFirstJob, machine1, tSecondJob, machine2));
            }

            // Verify the scheduled jobs do not overlap
            for (int i = 0; i < verify.Count; i++)
            {
                verify[i] = verify[i].OrderBy(x => x.Item1).ToList();
                bool firstItem = true;
                int minimumNextItem = 0;
                for (int j = 0; j < verify[i].Count; j++)
                {
                    if (firstItem)
                    {
                        minimumNextItem = verify[i][j].Item1 + verify[i][j].Item2;
                        firstItem = false;
                        continue;
                    }
                    if (verify[i][j].Item1 < minimumNextItem)
                    {
                        throw new Exception($"An invalid solution got returned on machine {i} at time {verify[i][j].Item1}");
                    }
                    minimumNextItem = verify[i][j].Item1 + verify[i][j].Item2;
                }
            }
            WriteDebugLine($"Number machines used:" + machines.NrMachines);
        }
    }
}
