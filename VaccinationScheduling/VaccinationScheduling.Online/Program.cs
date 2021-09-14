using VaccinationScheduling.Shared;

namespace VaccinationScheduling.Online
{
    public class Program
    {
        private static Global _global;
        private static JobEnumerable _jobs;

        public static void Main()
        {
            _global = ReadUtils.ReadGlobal();
            _jobs = new JobEnumerable();
            
            // TODO Schedule
            // TODO Output
        }
    }
}