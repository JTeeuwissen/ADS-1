using System;
using Google.OrTools.Sat;
using VaccinationScheduling.Shared;

namespace VaccinationScheduling.Offline
{
    static class ILPSolver2
    {
        public static Schedule[] Solve(Global global, Job[] jobs)
        {
            // Load in the constants.
            (int jabCount, int iMax, int mMax, int tMax, int[] r, int[] d, int[] x, int[] l) =
                Constants.GetConstants(global, jobs);

            int p1 = global.TimeFirstDose;
            int p2 = global.TimeSecondDose;
            int g = global.TimeGap;

            // Create a new model.
            CpModel model = new();

            // Create the variables.

            // T[i,j] is an array of numeric integer variables,
            // which determines the start time of each jab j of patient i.
            IntVar[,] T = new IntVar[iMax, jabCount];
            for (int i = 0; i < iMax; i++)
            for (int j = 0; j < jabCount; j++)
                T[i,j] = model.NewIntVar(0, tMax, $"T_{i}_{j}");

            // S[i,j,m,t] is an 4D array of booleans, which will be set to 1
            // if patient i takes timeslot t in hospital m for jab j.
            // ReSharper disable once InconsistentNaming
            IntVar[,,,] S = new IntVar[iMax, jabCount, mMax, tMax + 1];
            for (int i = 0; i < iMax; i++)
            for (int j = 0; j < jabCount; j++)
            for (int m = 0; m < mMax; m++)
            {
                S[i, j, m, 0] = model.NewIntVar(0 , 0, $"S_{i}_{j}_{m}_{0}");
                for (int t = 1; t <= tMax; t++)
                    S[i, j, m, t] = model.NewBoolVar($"S_{i}_{j}_{m}_{t}");
            }

            // C[i,j,m,t] is an 4D array of booleans,
            // which maps only the start of the sequence of 1's in S[i,j,m,t]
            // for each patient i for jab j in hospital m.
            // ReSharper disable once InconsistentNaming
            IntVar[,,,] C = new IntVar[iMax, jabCount, mMax, tMax + 1];
            for (int i = 0; i < iMax; i++)
            for (int j = 0; j < jabCount; j++)
            for (int m = 0; m < mMax; m++)
            for (int t = 0; t <= tMax; t++)
                C[i, j, m, t] = model.NewBoolVar($"C_{i}_{j}_{m}_{t}");

            // M[m] is an array of booleans,
            // which will be set to 1 if hospital m is used.
            // ReSharper disable once InconsistentNaming
            IntVar[] M = new IntVar[mMax];
            for (int m = 0; m < mMax; m++)
                M[m] = model.NewBoolVar($"M_{m}");

            // Minimize the sum of all used machines.
            model.Minimize(LinearExpr.Sum(M));

            // Set the constraints.

            // The start time of the first and the second jab
            // will be set within its feasible interval.
            for (int i = 0; i < iMax; i++)
            {
                model.Add(T[i,0] >= r[i]);
                model.Add(T[i,0] <= d[i] - p1 + 1);

                model.Add(T[i,1] >= T[i,0] + p1 + g + x[i]);
                model.Add(T[i,1] <= T[i,0] + p1 + g + x[i] + l[i] - p2);
            }

            // The processing time of each jab is met in schedule S.
            int[] doses = { p1, p2 };
            for (int i = 0; i < iMax; i++)
            for (int j = 0; j < jabCount; j++)
            {
                IntVar[] acc = new IntVar[mMax * (tMax + 1)];
                for (int m = 0; m < mMax; m++)
                for (int t = 0; t <= tMax; t++) acc[m * tMax + t + m] = S[i, j, m, t];

                model.Add(LinearExpr.Sum(acc) == doses[j]);
            }

            // There can only be 1 job performed at a time for each machine.
            for (int m = 0; m < mMax; m++)
            for (int t = 0; t <= tMax; t++)
            {
                IntVar[] acc = new IntVar[iMax * jabCount];
                for (int i = 0; i < iMax; i++)
                for (int j = 0; j < jabCount; j++) acc[i * jabCount + j] = S[i, j, m, t];

                model.Add(LinearExpr.Sum(acc) <= 1);
            }

            // Hospital m is used if it has to perform at least 1 jab.
            for (int m = 0; m < mMax; m++)
            {
                IntVar[] acc = new IntVar[iMax * jabCount * (tMax + 1)];
                for (int i = 0; i < iMax; i++)
                for (int j = 0; j < jabCount; j++)
                for (int t = 0; t <= tMax; t++)
                    acc[i * jabCount * tMax + j * tMax + t + j] = S[i, j, m, t];

                model.Add(LinearExpr.Sum(acc) <= (iMax * jabCount * (tMax + 1)) * M[m]);
            }

            // Maps the start of each jab of schedule S to C.
            for (int i = 0; i < iMax; i++)
            for (int j = 0; j < jabCount; j++)
            for (int m = 0; m < mMax; m++)
            for (int t = 1; t <= tMax; t++)
                model.Add(C[i, j, m, t] >= S[i, j, m, t] - S[i, j, m, t - 1]);

            // For each jab of each patient, only one sequence of 1's must exist.
            for (int i = 0; i < iMax; i++)
            for (int j = 0; j < jabCount; j++)
            {
                IntVar[] acc = new IntVar[mMax * (tMax + 1)];
                for (int m = 0; m < mMax; m++)
                for (int t = 0; t <= tMax; t++)
                    acc[m * tMax + t + m] = C[i, j, m, t];

                model.Add(LinearExpr.Sum(acc) == 1);
            }

            // Each jab j starts at the specified time T[i,j] in schedule S.
            for (int i = 0; i < iMax; i++)
            for (int j = 0; j < jabCount; j++)
            {
                LinearExpr[] exp = new LinearExpr[mMax * (tMax + 1)];
                for (int m = 0; m < mMax; m++)
                for (int t = 0; t <= tMax; t++)
                    exp[m * tMax + t + m] = LinearExpr.Prod(C[i, j, m, t], t);

                model.Add(LinearExpr.Sum(exp) == T[i, j]);
            }

            CpSolver solver = new();
            CpSolverStatus status = solver.Solve(model);
            Extensions.WriteDebugLine($"Solve status: {status}");

            // Print solution.
            // Check that the problem has a feasible solution.
            if (status is CpSolverStatus.Infeasible)
                throw new Exception("The problem does not have an optimal solution!");

            Extensions.WriteDebugLine("Solution:");
            Extensions.WriteDebugLine("Objective value = " + solver.ObjectiveValue);

            (int t1, int t2) GetJabTime(int i)
            {
                int t1 = (int)solver.Value(T[i,0]);
                int t2 = (int)solver.Value(T[i,1]);
                return (t1, t2);
            }

            (int m1, int m2) GetJabMachine(int i)
            {
                int? m1 = null;
                int? m2 = null;
                for (int m = 0; m < mMax; m++)
                for (int t = 0; t <= tMax; t++)
                {
                    if (solver.Value(S[i, 0, m, t]) == 1)
                        m1 = m;
                    if (solver.Value(S[i, 1, m, t]) == 1)
                        m2 = m;
                }
                return ((int)m1!, (int)m2!);
            }

            Schedule[] schedules = new Schedule[iMax];

            for (int i = 0; i < iMax; i++)
            {
                (int t1, int t2) = GetJabTime(i);
                (int m1, int m2) = GetJabMachine(i);
                schedules[i] = new Schedule(t1, m1, t2, m2);
            }

            return schedules;
        }
    }
}