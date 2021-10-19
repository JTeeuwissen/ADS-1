using System;
using Google.OrTools.Sat;
using VaccinationScheduling.Shared;
using System.Collections.Generic;

namespace VaccinationScheduling.Offline
{
    static class ILPSolver2
    {
        public static Schedule[] Solve(Global global, Job[] jobs)
        {
            (int jabCount, int iMax, int mMax, int tMax, int[]? r, int[]? d, int[]? x, int[]? l) =
                Constants.GetConstants(global, jobs);

            // Create the model
            CpModel model = new();

            // Create the variables

            // T[i,d] is an array of numeric integer variables,
            // which determines the start time of patient i's jab of dose d.
            Dictionary<(int, int), IntVar> T = new();

            for (int i = 0; i < iMax; i++)
            for (int j = 0; j < jabCount; j++)
                T.Add((i, j), model.NewIntVar(0, tMax, $"T_{i}_{j}"));


            // S[i,j,t,d] is an 3D array of booleans, which will be set to true
            // if patient i takes timeslot t in hospital j for dose d
            // ReSharper disable once InconsistentNaming
            Dictionary<(int, int, int, int), IntVar> S = new();

            for (int i = 0; i < iMax; i++)
            for (int m = 0; m < mMax; m++)
            for (int t = 0; t <= tMax; t++)
            for (int j = 0; j < jabCount; j++)
                S.Add((i, m, t, j), model.NewBoolVar($"S_{i}_{m}_{t}_{j}"));

            // G[i, j, d] is an array of booleans, which will be set to true if patient i
            // uses hospital j for dose d,
            // ReSharper disable once InconsistentNaming
            Dictionary<(int, int, int), IntVar> G = new();
            for (int i = 0; i < iMax; i++)
            for (int m = 0; m < mMax; m++)
            for (int j = 0; j < jabCount; j++)
                G.Add((i, m, j), model.NewBoolVar($"G_{i}_{m}_{j}"));

            // Hospital A[j] is used
            // ReSharper disable once InconsistentNaming
            IntVar[] A = new IntVar[mMax];
            for (int m = 0; m < mMax; m++)
                A[m] = model.NewBoolVar($"A_{m}");


            // C[i,j, t,d] is an array of booleans, determining consecutive elements for patient i for dose d
            // ReSharper disable once InconsistentNaming
            Dictionary<(int, int, int, int), IntVar> C = new();

            for (int i = 0; i < iMax; i++)
            for (int m = 0; m < mMax; m++)
            for (int t = 0; t <= tMax; t++)
            for (int j = 0; j < jabCount; j++)
                C.Add((i, m, t, j), model.NewBoolVar($"C_{i}_{m}_{t}_{j}"));


            for (int i = 0; i < iMax; i++)
            for (int m = 0; m < mMax; m++)
            for (int j = 0; j < jabCount; j++)
            for (int t = 0; t < 1; t++)
            {
                (int, int, int, int) key = (i, m, t, j);
                model.Add(S[key] == 0);
            }


            // Constraints

            // De eerste en de tweede jab valt altijd in de gegeven time interval
            for (int i = 0; i < iMax; i++)
            {
                (int, int) key1 = (i, 0);
                (int, int) key2 = (i, 1);
                model.Add(T[key1] >= r[i]);
                model.Add(T[key1] <= d[i] - global.TimeFirstDose + 1);
                //model.Add(T[key2] == 10);
                model.Add(T[key2] >= T[key1] + global.TimeFirstDose + global.TimeGap + x[i]);
                model.Add(T[key2] <= T[key1] + global.TimeFirstDose + global.TimeGap + x[i] + l[i] - 1);
            }

            // Ieder patient moet exact op 1 ziekenhuis zijn jab krijgen
            for (int i = 0; i < iMax; i++)
            {
                for (int j = 0; j < jabCount; j++)
                {
                    IntVar[] acc = new IntVar[mMax];
                    for (int m = 0; m < mMax; m++)
                    {
                        (int, int, int) key = (i, m, j);
                        acc[m] = G[key];
                    }

                    model.Add(LinearExpr.Sum(acc) == 1);
                }
            }

            // De time-slot lengte voor jab d wordt vervuld voor patient i

            int[] doses = { global.TimeFirstDose, global.TimeSecondDose };
            for (int i = 0; i < iMax; i++)
            for (int j = 0; j < jabCount; j++)
            for (int m = 0; m < mMax; m++)
            {
                IntVar[] acc = new IntVar[tMax + 1];
                for (int t = 0; t <= tMax; t++)
                {
                    (int, int, int, int) key1 = (i, m, t, j);
                    acc[t] = S[key1];
                }

                (int, int, int) key2 = (i, m, j);
                model.Add(LinearExpr.Sum(acc) == doses[j] * G[key2]);
            }


            // Er kan hoogstens 1 job tegelijk plaatsvinden op een ziekenhuis
            for (int m = 0; m < mMax; m++)
            for (int t = 0; t <= tMax; t++)
            {
                IntVar[] acc = new IntVar[iMax * jabCount];
                for (int i = 0; i < iMax; i++)
                for (int j = 0; j < jabCount; j++)
                {
                    (int, int, int, int) key = (i, m, t, j);
                    acc[i * jabCount + j] = S[key];
                }

                model.Add(LinearExpr.Sum(acc) <= 1);
            }


            // Zieken huis j wordt gebruikt als het minstens 1 jab heeft
            // ??
            for (int m = 0; m < mMax; m++)
            {
                IntVar[] acc = new IntVar[iMax * jabCount];
                for (int i = 0; i < iMax; i++)
                for (int j = 0; j < jabCount; j++)
                {
                    (int, int, int) key = (i, m, j);
                    acc[i * jabCount + j] = G[key];
                }

                model.Add(LinearExpr.Sum(acc) > 0).OnlyEnforceIf(A[m]);
                model.Add(LinearExpr.Sum(acc) <= 0).OnlyEnforceIf(A[m].Not());
                //model.Add(LinearExpr.Sum(x) <= (jobCount * JabCount) * A[j]);
            }


            // Consecutive constraint 1
            // Map de start van de achtereenvolgende eentjes van S naar C
            for (int i = 0; i < iMax; i++)
            for (int m = 0; m < mMax; m++)
            for (int j = 0; j < jabCount; j++)
            for (int t = 1; t <= tMax; t++)
            {
                (int, int, int, int) key1 = (i, m, t, j);
                (int, int, int, int) key2 = (i, m, t - 1, j);
                model.Add(C[key1] >= S[key1] - S[key2]);
            }


            // Consecutive constraint 2 exactly 1 consecutive sequences per dose
            for (int i = 0; i < iMax; i++)
            for (int j = 0; j < jabCount; j++)
            {
                IntVar[] acc = new IntVar[mMax * (tMax + 1)];
                for (int m = 0; m < mMax; m++)
                for (int t = 0; t <= tMax; t++)
                {
                    (int, int, int, int) key = (i, m, t, j);
                    acc[m * tMax + t + m] = C[key];
                }

                model.Add(LinearExpr.Sum(acc) == 1);
            }

            // Consecutive constraint 3, jab starts at specified time for person i for jab d


            for (int i = 0; i < iMax; i++)
            for (int j = 0; j < jabCount; j++)
            {
                LinearExpr[] exp = new LinearExpr[mMax * (tMax + 1)];
                for (int m = 0; m < mMax; m++)
                for (int t = 0; t <= tMax; t++)
                {
                    (int, int, int, int) key1 = (i, m, t, j);
                    exp[m * tMax + t + m] = LinearExpr.Prod(C[key1], t);
                }

                (int, int) key2 = (i, j);
                model.Add(LinearExpr.Sum(exp) == T[key2]);
            }

            model.Minimize(LinearExpr.Sum(A));
            CpSolver solver = new();
            //solver.StringParameters = "search_branching:FIXED_SEARCH, enumerate_all_solutions:true";
            CpSolverStatus status = solver.Solve(model);
            Console.WriteLine($"Solve status: {status}");

            // Print solution.
            // Check that the problem has a feasible solution.
            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                Console.WriteLine($"Total cost: {solver.ObjectiveValue}\n");


                for (int i = 0; i < iMax; i++)
                for (int j = 0; j < jabCount; j++)
                {
                    (int, int) key = (i, j);
                    Console.WriteLine($"C[{i},{j}] = " + solver.Value(T[key]));
                }


                for (int i = 0; i < iMax; i++)
                for (int j = 0; j < jabCount; j++)
                for (int m = 0; m < mMax; m++)
                {
                    string v = "";
                    string f = "";
                    for (int t = 0; t <= tMax; t++)
                    {
                        (int, int, int, int) key = (i, m, t, j);
                        v += solver.Value(C[key]).ToString();
                        f += solver.Value(S[key]).ToString();
                    }
                    //Console.WriteLine($"C[{i},{j},{d}] = " + v);
                    //Console.WriteLine($"S[{i},{j},{d}] = " + f);
                }
            }

            Schedule[] schedules = new Schedule[iMax];
            return schedules;
        }
    }
}