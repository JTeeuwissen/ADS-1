using System.Linq;
using VaccinationScheduling.Shared;

namespace VaccinationScheduling.Offline
{
    public static class Program
    {
        private static Global _global;
        private static Job[] _jobs;

        public static void Main()
        {
            _global = ReadUtils.ReadGlobal();
            int jobCount = ReadUtils.ReadNumber();

            _jobs = Enumerable.Range(0, jobCount).Select(_ => ReadUtils.ReadJob()).ToArray();

            // TODO Schedule
            // TODO Output
        }
    }
}