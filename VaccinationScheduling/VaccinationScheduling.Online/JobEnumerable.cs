using System.Collections;
using System.Collections.Generic;
using VaccinationScheduling.Shared;

namespace VaccinationScheduling.Online
{
    public class JobEnumerable : IEnumerable<Job>
    {
        private readonly Global _global;

        public JobEnumerable(Global global)
        {
            _global = global;
        }

        // Get the enumerator used in a foreach loop.
        public IEnumerator<Job> GetEnumerator()
        {
            while (true)
            {
                Job job;
                try
                {
                    job =  ReadUtils.ReadJob(_global);
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
