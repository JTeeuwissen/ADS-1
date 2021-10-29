using System.Collections;
using System.Collections.Generic;
using VaccinationScheduling.Shared.BigNumber;

namespace VaccinationScheduling.Online
{
    public class JobEnumerable : IEnumerable<BigJob>
    {
        private readonly BigGlobal _global;

        public JobEnumerable(BigGlobal global)
        {
            _global = global;
        }

        // Get the enumerator used in a foreach loop.
        public IEnumerator<BigJob> GetEnumerator()
        {
            while (true)
            {
                BigJob job;
                try
                {
                    job = BigReadUtils.ReadJob(_global);
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
