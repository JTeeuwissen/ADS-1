using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Range = VaccinationScheduling.Online.Tree.Range;

namespace VaccinationScheduling.Online
{
    /// <summary>
    /// Class containing overlap between a range and a start and endttime.
    /// </summary>
    public class Overlap
    {
        public bool StartOverlaps;
        public bool EndOverlaps;
        public bool Length1;
        public BigInteger OverlapStart;
        public BigInteger OverlapEnd;

        /// <summary>
        /// Converts a range and tStart and tEnd to an overlap item containing information about how it overlaps.
        /// </summary>
        /// <param name="range">Range to get the overlap with</param>
        /// <param name="tStart">Start of the range to check overlap with range with</param>
        /// <param name="tEnd">Range needs to get compared to this endtime</param>
        public Overlap(Range range, BigInteger tStart, BigInteger tEnd)
        {
            // Start overlaps
            if (range.Start >= tStart)
            {
                OverlapStart = range.Start;
                StartOverlaps = range.InLeftItem != null;
            }
            // Start does not overlap
            else
            {
                StartOverlaps = false;
                OverlapStart = tStart;
            }

            // Bool identifiyng whether a flush spot exists
            Length1 = range.Start == range.EndMaybe;

            // Endtime of overlap cannot cannot be equal if range goes to infinity or is higher
            if (range.EndMaybe == null)
            {
                OverlapEnd = tEnd;
                EndOverlaps = false;
            }
            else if (range.EndMaybe > tEnd)
            {
                OverlapEnd = tEnd;
                EndOverlaps = false;
            }
            // End overlaps
            else
            {
                EndOverlaps = range.InRightItem != null;
                OverlapEnd = (int)range.EndMaybe;
            }

            // Range can be of length two, then bothpossible times it can be scheduled at are next to another job on the same machine.
            if (range.Start + 1 == range.EndMaybe && range.InLeftItem != null && range.InRightItem != null)
            {
                EndOverlaps = true;
                StartOverlaps = true;
            }
        }
    }
}
