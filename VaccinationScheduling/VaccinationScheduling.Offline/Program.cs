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

            // TODO Sort jobs
            // TODO Schedule
            // TODO Output
        }
    }
}