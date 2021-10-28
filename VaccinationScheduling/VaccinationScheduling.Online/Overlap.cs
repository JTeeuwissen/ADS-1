using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Range = VaccinationScheduling.Online.Tree.Range;

namespace VaccinationScheduling.Online
{
    public class Overlap
    {
        public bool StartOverlaps;
        public bool EndOverlaps;
        public bool Length1;
        public int OverlapStart;
        public int OverlapEnd;

        public Overlap(Range range, int tStart, int tEnd)
        {
            getSmartOverlapWithRange(range, tStart, tEnd);
        }

        private void getSmartOverlapWithRange(Range range, int tStart, int tEnd)
        {
            if (range.Start >= tStart)
            {
                OverlapStart = range.Start;
                StartOverlaps = range.InLeftItem != null;
            }
            else
            {
                StartOverlaps = false;
                OverlapStart = tStart;
            }

            Length1 = range.Start == range.EndMaybe;
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
            else
            {
                EndOverlaps = range.InRightItem != null;
                OverlapEnd = (int)range.EndMaybe;
            }

            if (range.Start + 1 == range.EndMaybe && range.InLeftItem != null && range.InRightItem != null)
            {
                EndOverlaps = true;
                StartOverlaps = true;
            }
        }
    }
}
