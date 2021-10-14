using System;
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

#if DEBUG
            Console.WriteLine(SchedulePrettier.ToPrettyString(global, schedules));
#endif

            // TODO Sort jobs
            // TODO Schedule
            // TODO Output
        }
    }
}