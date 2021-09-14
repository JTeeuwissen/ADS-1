using System.Linq;
using VaccinationScheduling.Shared;

namespace VaccinationScheduling.Offline
{
    public class Program
    {
        private Global global;
        private Job[] jobs;

        public Program()
        {
            global = ReadUtils.ReadGlobal();
            int jobCount = ReadUtils.ReadNumber();

            jobs = Enumerable.Range(0, jobCount).Select(_ => ReadUtils.ReadJob(global)).ToArray();

            // TODO Sort jobs
            // TODO Schedule
            // TODO Output
        }

        public static void Main()
        {
            // Easy way to get out of static
            new Program();
        }
    }
}
