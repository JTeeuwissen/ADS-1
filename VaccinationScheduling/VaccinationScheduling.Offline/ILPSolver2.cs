using System;
using System.Linq;
using Google.OrTools.Sat;
using VaccinationScheduling.Shared;

namespace VaccinationScheduling.Offline
{
    static class ILPSolver2
    {
        private const int JabCount = 2;
        public static Schedule[] Solve(Global global, Job[] jobs)
        {
            int jobCount = jobs.Length;
            // ReSharper disable once InlineTemporaryVariable
            int machineCount = jobCount;
            int maxTime = jobs.Select(job => job.FirstIntervalEnd + job.ExtraDelay + job.SecondIntervalLength).Max() +
                          global.TGap;

            (int FirstIntervalStart, int FirstIntervalEnd)[] firstFeasibleIntervals =
                jobs.Select(job => (job.FirstIntervalStart, job.FirstIntervalEnd)).ToArray();
            int[] patientDelays = jobs.Select(job => job.ExtraDelay).ToArray();
            int[] secondIntervalLengths = jobs.Select(job => job.SecondIntervalLength).ToArray();

            // Create the model
            CpModel model = new CpModel();

            // Create the variables

            // t[i,d] is an array of numeric integer variables,
            // which determines the start time of patient i's jab of dose d.
            IntVar[,] T = new IntVar[jobCount, JabCount];

            for (int i = 0; i < jobCount; i++)
                for (int d = 0; d < JabCount; d++)
                    T[i, d] = model.NewIntVar(0, maxTime, $"T_{i}_{d}");

            // S[i,j,t,d] is an 3D array of booleans, which will be set to true
            // if patient i takes timeslot t in hospital j for dose d

            IntVar[,,,] S = new IntVar[jobCount, machineCount, maxTime + 1, JabCount];

            for (int i = 0; i < jobCount; i++)
                for (int j = 0; j < machineCount; j++)
                    for (int t = 0; t <= maxTime; t++)
                        for (int d = 0; d < JabCount; d++)
                            S[i, j, t, d] = model.NewBoolVar($"S_{i}_{j}_{t}_{d}");

            // G[j] is an array of booleans, which will be set to true
            // if hospital h is used.
            IntVar[] G = new IntVar[machineCount];

            for (int j = 0; j < machineCount; j++)
                G[j] = model.NewBoolVar($"G_{j}");

            // C[i,t,d] is an array of booleans, determining consecutive elements for patient i for dose d
            IntVar[,,] C = new IntVar[jobCount, maxTime + 1, JabCount];

            for (int i = 0; i < jobCount; i++)
                for (int d = 0; d < JabCount; d++)
                    for (int t = 0; t <= maxTime; t++)
                        C[i, t, d] = model.NewBoolVar($"C_{i}_{t}_{d}");


            // Constraints

            // De eerste en de tweede jab valt altijd in de gegeven time interval
            for (int i = 0; i < jobCount; i++)
            {
                model.Add(T[i, 0] >= firstFeasibleIntervals[i].FirstIntervalStart);
                model.Add(T[i, 0] <= firstFeasibleIntervals[i].FirstIntervalEnd - global.TFirstDose + 1);

                model.Add(T[i, 1] >= T[i, 0] + global.TFirstDose + global.TGap + patientDelays[i]);
                model.Add(T[i, 1] <= T[i, 0] + global.TFirstDose + global.TGap + patientDelays[i] + secondIntervalLengths[i] - 1);
            }

            // Iedere patiënt krijgt exact 1 jab per dose
            int[] doses = new int[] { global.TFirstDose, global.TSecondDose };
            for (int i = 0; i < jobCount; i++)
            {
                for (int d = 0; d < JabCount; d++)
                {
                    IntVar[] x = new IntVar[machineCount * (maxTime + 1)];
                    for (int j = 0; j < machineCount; j++)
                    {
                        for (int t = 0; t <= maxTime; t++)
                        {
                            x[j * maxTime + t + j] = S[i, j, t, d];
                        }
                    }
                    model.Add(LinearExpr.Sum(x) == doses[d]);
                }
            }

            // Er kan hoogtens 1 job tegelijk plaatsvinden op een ziekenhuis
            for (int j = 0; j < machineCount; j++)
            {
                for (int t = 0; t <= maxTime; t++)
                {
                    IntVar[] x = new IntVar[jobCount * JabCount];
                    for (int i = 0; i < jobCount; i++)
                    {
                        for (int d = 0; d < JabCount; d++)
                        {
                            x[i * JabCount + d] = S[i, j, t, d];
                        }
                    }
                    model.Add(LinearExpr.Sum(x) <= 1);
                }
            }

            // Zieken huis j wordt gebruikt als het minstens 1 jab heeft
            for (int j = 0; j < machineCount; j++)
            {
                IntVar[] x = new IntVar[(maxTime + 1) * jobCount * JabCount];
                for (int i = 0; i < jobCount; i++)
                {
                    for (int d = 0; d < JabCount; d++)
                    {
                        for (int t = 0; t <= maxTime; t++)
                        {
                            x[i * JabCount * maxTime + i * JabCount + d * maxTime + t + d] = S[i, j, t, d];
                        }
                    }
                }
                model.Add(LinearExpr.Sum(x) <= G[j] * (maxTime + 1));
            }

            // Consecutive constraint 1

            for (int i = 0; i < jobCount; i++)
                for (int j = 0; j < machineCount; j++)
                    for (int d = 0; d < JabCount; d++)
                        for (int t = 1; t <= maxTime; t++)
                            model.Add(C[i, j, d] >= S[i, j, t, d] - S[i, j, t - 1, d]);


            // Consecutive constraint 2 only 1 consecutive sequences per dose
            for (int i = 0; i < jobCount; i++)
            {
                for (int d = 0; d < JabCount; d++)
                {
                    IntVar[] x = new IntVar[maxTime + 1];
                    for (int t = 0; t <= maxTime; t++)
                    {
                        x[t] = C[i, t, d];
                    }
                    model.Add(LinearExpr.Sum(x) == 1);
                }

            }

            // Consecutive constraint 3, jab starts at specified time for person i for jab d

            /*
            for (int i = 0; i < jobCount; i++)
            {
                LinearExpr[] x = new LinearExpr[maxTime + 1];
                for (int t = 0; t <= maxTime; t++)
                {
                    x[t] = LinearExpr.Prod(C[i, t, 0], t);
                }
                model.Add(LinearExpr.Sum(x) == T[i, 0]);
            }*/

            for (int i = 0; i < jobCount; i++)
            {
                for (int d = 0; d < JabCount; d++)
                {
                    for (int t = 0; t <= maxTime; t++)
                    {
                        // Declare our intermediate boolean variable.
                        IntVar b = model.NewBoolVar("b");

                        // Implement b == (C[i,t,d] == T[i,d)).
                        model.Add(T[i, d] == t).OnlyEnforceIf(b);
                        model.Add(T[i, d] != t).OnlyEnforceIf(b.Not());

                        // Create our two half-reified constraints.
                        // First, b implies (C[i,t,d] == 1).
                        model.Add(C[i, t, d] == 1).OnlyEnforceIf(b);
                        // Second, not(b) implies C[i,t,d] == 0.
                        model.Add(C[i, t, d] == 0).OnlyEnforceIf(b.Not());
                    }
                }
            }

            model.Minimize(LinearExpr.Sum(G));
            CpSolver solver = new CpSolver();
            solver.StringParameters = "search_branching:FIXED_SEARCH, enumerate_all_solutions:true";
            CpSolverStatus status = solver.Solve(model);
            Console.WriteLine($"Solve status: {status}");

            // Print solution.
            // Check that the problem has a feasible solution.
            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                Console.WriteLine($"Total cost: {solver.ObjectiveValue}\n");
            }

            Schedule[] schedules = new Schedule[jobCount];
            return schedules;
        }

    }
}
