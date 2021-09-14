using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccinationScheduling.Shared
{
    public struct Parameters
    {
        public int TFirstDose;
        public int TSecondDose;
        public int TGap;

        public Parameters(int tFirstDose, int tSecondDose, int tGap)
        {
            TFirstDose = tFirstDose;
            TSecondDose = tSecondDose;
            TGap = tGap;
        }
    }
}
