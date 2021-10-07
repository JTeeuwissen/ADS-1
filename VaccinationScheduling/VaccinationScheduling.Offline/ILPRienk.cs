using Google.OrTools.LinearSolver;
using LinearExpr = Google.OrTools.Sat.LinearExpr;

namespace VaccinationScheduling.Offline
{
    static class ILPRienk
    {
        public static Solver CreateSolver(int jobCount, int machineCount, int maxTime, (int r1, int d1)[] firstFeasibleIntervals, int[] patientDelays, int[] secondFeasibleIntervalLengths)
        {
            // Create the linear solver with the GLOP backend.
            Solver solver = Solver.CreateSolver("GLOP");

            // 
            Variable[,,,] J = new Variable[jobCount, 2, machineCount, maxTime];

            for (int i = 0; i < jobCount; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < machineCount; k++)
                    {
                        for (int t = 0; t < maxTime; t++)
                        {
                            J[i, j, k, t] = solver.MakeNumVar(0, 1, $"J_{i}_{j}_{k}_{t}");
                        }
                    }
                }
            }

            // Variable which keeps track of if a machine is used
            Variable[] M = new Variable[machineCount];
            for (int k = 0; k < machineCount; k++)
            {
                M[k] = solver.MakeIntVar(0, 1, $"M_{k}");
            }


            //solver.Minimize(LinearExpr.Sum(M)));

            return solver;
        }
    }
}
