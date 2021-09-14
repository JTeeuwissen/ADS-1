using VaccinationScheduling.Shared;

namespace VaccinationScheduling.Online
{
    public class Program
    {
        private Global global;
        private JobEnumerable jobs;

        public Program()
        {
            global = ReadUtils.ReadGlobal();
            jobs = new JobEnumerable(global);

            // TODO loop through
            // TODO Schedule
            // TODO Output
        }

        public static void Main()
        {
            // Escape the static environment
            new Program();
        }
    }
}
