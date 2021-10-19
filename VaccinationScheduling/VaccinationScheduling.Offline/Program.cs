using System;
using System.Diagnostics;
using System.Linq;
using VaccinationScheduling.Shared;

namespace VaccinationScheduling.Offline
{
    public static class Program
    {
        public static void Main()
        {
            Global global = ReadUtils.ReadGlobal();
            int jobCount = ReadUtils.ReadNumber();

            Job[] jobs = Enumerable.Range(0, jobCount).Select(_ => ReadUtils.ReadJob(global)).ToArray();

            Schedule[] schedules = ILPSolver.Solve(global, jobs);

            foreach (Schedule schedule in schedules) Console.WriteLine(schedule);
            
            Extensions.WriteDebugLine(SchedulePrettier.ToPrettyString(global, schedules));

            //TODO remove
            for (int i = 0; i < jobs.Length; i++)
            {
                Job job = jobs[i];
                Schedule schedule = schedules[i];
                Debug.Assert(schedule.T1 + global.TimeFirstDose + global.TimeGap + job.ExtraDelay <= schedule.T2);
            }

            // TODO Sort jobs
            // TODO Schedule
            // TODO Output
        }
    }
}