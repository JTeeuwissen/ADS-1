using System.Collections;
using System.Collections.Generic;
using VaccinationScheduling.Shared;

namespace VaccinationScheduling.Online
{
    public class JobEnumerable : IEnumerable<Job>
    {
        public IEnumerator<Job> GetEnumerator()
        {
            while (true)
            {
                Job job;
                try
                {
                    job =  ReadUtils.ReadJob();
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