using System.Collections;
using System.Collections.Generic;
using VaccinationScheduling.Shared;

namespace VaccinationScheduling.Online
{
    public class JobEnumerable : IEnumerable<Job>
    {
        private Global global;

        public JobEnumerable(Global global)
        {
            this.global = global;
        }

        public IEnumerator<Job> GetEnumerator()
        {
            while (true)
            {
                Job job;
                try
                {
                    job =  ReadUtils.ReadJob(global);
                }
                catch
                {
                    // There are no more patients
                    break;
                }
                yield return job;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
