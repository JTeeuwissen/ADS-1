using System.Collections.Generic;
using System.Linq;
using VaccinationScheduling.Shared;

namespace VaccinationScheduling.Offline
{
    public class Constants
    {
        public static (int jabCount, int iMax, int mMax, int tMax, int[] r, int[] d, int[] x, int[] l) GetConstants(Global global, IReadOnlyCollection<Job> jobs)
        {
            int iMax = jobs.Count;
            // ReSharper disable once InlineTemporaryVariable
            int mMax = iMax;
            int tMax = jobs.Select(job => job.FirstIntervalEnd + job.ExtraDelay + job.SecondIntervalLength).Max() +
                       global.TimeGap;

            int[] r = jobs.Select(job => job.FirstIntervalStart).ToArray();
            int[] d = jobs.Select(job => job.FirstIntervalEnd).ToArray();
            int[] x = jobs.Select(job => job.ExtraDelay).ToArray();
            int[] l = jobs.Select(job => job.SecondIntervalLength).ToArray();

            return (jabCount : 2, iMax, mMax, tMax, r, d, x, l);
        }
    }
}