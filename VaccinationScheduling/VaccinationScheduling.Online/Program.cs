using System;
using System.Linq;
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

            List<List<(int, int)>> verify = new List<List<(int, int)>>();

            foreach (Job job in jobs)
            {
                (int, int, int, int) foundTimeSlots = machines.FindGreedySpot(job);
                machines.ScheduleJobs(foundTimeSlots.Item1, foundTimeSlots.Item2, foundTimeSlots.Item3, foundTimeSlots.Item4);

                // Verify the solution
                if (foundTimeSlots.Item1 == machines.NrMachines - 1 || foundTimeSlots.Item2 == machines.NrMachines - 1)
                {
                    verify.Add(new List<(int, int)>());
                }
                if (verify[foundTimeSlots.Item1].Count == 0)
                {
                    verify[foundTimeSlots.Item1] = new List<(int, int)>();
                }
                if (verify[foundTimeSlots.Item2].Count == 0)
                {
                    verify[foundTimeSlots.Item2] = new List<(int, int)>();
                }
                verify[foundTimeSlots.Item1].Add((foundTimeSlots.Item3, machines.freeRangesFirstJob.JobLength));
                verify[foundTimeSlots.Item2].Add((foundTimeSlots.Item4, machines.freeRangesSecondJob.JobLength));
            }

            for (int i = 0; i < verify.Count; i++)
            {
                List<(int, int)> list = verify[i];
                list = list.OrderBy(x => x.Item1).ToList();
                bool firstItem = true;
                int minimumNextItem = 0;
                for (int j = 0; j < list.Count; j++)
                {
                    if (firstItem)
                    {
                        minimumNextItem = list[j].Item1 + list[j].Item2;
                        firstItem = false;
                        continue;
                    }
                    if (list[j].Item1 < minimumNextItem)
                    {
                        throw new Exception($"An invalid solution got returned on machine {i} at time {list[j].Item1}");
                    }
                    minimumNextItem = list[j].Item1 + list[j].Item2;
                }
            }
        }
    }
}
