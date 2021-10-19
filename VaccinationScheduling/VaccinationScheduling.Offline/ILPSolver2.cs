using System;
using System.Linq;
using Google.OrTools.Sat;
using VaccinationScheduling.Shared;
using System.Collections.Generic;

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
                          global.TimeGap;

            (int FirstIntervalStart, int FirstIntervalEnd)[] firstFeasibleIntervals =
                jobs.Select(job => (job.FirstIntervalStart, job.FirstIntervalEnd)).ToArray();
            int[] patientDelays = jobs.Select(job => job.ExtraDelay).ToArray();
            int[] secondIntervalLengths = jobs.Select(job => job.SecondIntervalLength).ToArray();

            // Create the model
            CpModel model = new CpModel();

            // Create the variables

            // T[i,d] is an array of numeric integer variables,
            // which determines the start time of patient i's jab of dose d.
            Dictionary<Tuple<int, int>, IntVar> T = new Dictionary<Tuple<int, int>, IntVar>();

            for (int i = 0; i < jobCount; i++)
                for (int d = 0; d < JabCount; d++)
                    T.Add(Tuple.Create(i, d), model.NewIntVar(0, maxTime, $"T_{i}_{d}"));


            // S[i,j,t,d] is an 3D array of booleans, which will be set to true
            // if patient i takes timeslot t in hospital j for dose d
            Dictionary<Tuple<int, int, int, int>, IntVar> S = new Dictionary<Tuple<int, int, int, int>, IntVar>();

            for (int i = 0; i < jobCount; i++)
                for (int j = 0; j < machineCount; j++)
                    for (int t = 0; t <= maxTime; t++)
                        for (int d = 0; d < JabCount; d++)
                            S.Add(Tuple.Create(i, j, t, d), model.NewBoolVar($"S_{i}_{j}_{t}_{d}"));

            // G[i, j, d] is an array of booleans, which will be set to true if patient i
            // uses hospital j for dose d,
            Dictionary<Tuple<int, int, int>, IntVar> G = new Dictionary<Tuple<int, int, int>, IntVar>();

            for (int i = 0; i < jobCount; i++)
                for (int j = 0; j < machineCount; j++)
                    for (int d = 0; d < JabCount; d++)
                        G.Add(Tuple.Create(i, j, d), model.NewBoolVar($"G_{i}_{j}_{d}"));

            // Hostpital A[j] is used
            IntVar[] A = new IntVar[machineCount];

            for (int j = 0; j < machineCount; j++)
                A[j] = model.NewBoolVar($"A_{j}");


            // C[i,j, t,d] is an array of booleans, determining consecutive elements for patient i for dose d
            Dictionary<Tuple<int, int, int, int>, IntVar> C = new Dictionary<Tuple<int, int, int, int>, IntVar>();

            for (int i = 0; i < jobCount; i++)
                for (int j = 0; j < machineCount; j++)
                    for (int t = 0; t <= maxTime; t++)
                        for (int d = 0; d < JabCount; d++)
                            C.Add(Tuple.Create(i, j, t, d), model.NewBoolVar($"C_{i}_{j}_{t}_{d}"));


            for (int i = 0; i < jobCount; i++)
                for (int j = 0; j < machineCount; j++)
                    for (int d = 0; d < JabCount; d++)
                        for (int t = 0; t < 1; t++)
                        {
                            var key = Tuple.Create(i, j, t, d);
                            model.Add(S[key] == 0);
                        }


            // Constraints

            // De eerste en de tweede jab valt altijd in de gegeven time interval
            for (int i = 0; i < jobCount; i++)
            {
                var key1 = Tuple.Create(i, 0);
                var key2 = Tuple.Create(i, 1);
                model.Add(T[key1] >= firstFeasibleIntervals[i].FirstIntervalStart);
                model.Add(T[key1] <= firstFeasibleIntervals[i].FirstIntervalEnd - global.TimeFirstDose + 1);
                //model.Add(T[key2] == 10);
                model.Add(T[key2] >= T[key1] + global.TimeFirstDose + global.TimeGap + patientDelays[i]);
                model.Add(T[key2] <= T[key1] + global.TimeFirstDose + global.TimeGap + patientDelays[i] + secondIntervalLengths[i] - 1);

            }

            // Ieder patient moet exact op 1 ziekenhuis zijn jab krijgen
            for (int i = 0; i < jobCount; i++)
            {
                for (int d = 0; d < JabCount; d++)
                {
                    IntVar[] x = new IntVar[machineCount];
                    for (int j = 0; j < machineCount; j++)
                    {
                        var key = Tuple.Create(i, j, d);
                        x[j] = G[key];
                    }
                    model.Add(LinearExpr.Sum(x) == 1);
                }
            }

            // De time-slot duratie voor jab d wordt vervuld voor patient i

            int[] doses = new int[] { global.TimeFirstDose, global.TimeSecondDose };
            for (int i = 0; i < jobCount; i++)
            {
                for (int d = 0; d < JabCount; d++)
                {
                    for (int j = 0; j < machineCount; j++)
                    {
                        IntVar[] x = new IntVar[maxTime + 1];
                        for (int t = 0; t <= maxTime; t++)
                        {
                            var key1 = Tuple.Create(i, j, t, d);
                            x[t] = S[key1];
                        }

                        var key2 = Tuple.Create(i, j, d);
                        model.Add(LinearExpr.Sum(x) == doses[d] * G[key2]);
                    }
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
                            var key = Tuple.Create(i, j, t, d);
                            x[i * JabCount + d] = S[key];
                        }
                    }
                    model.Add(LinearExpr.Sum(x) <= 1);
                }
            }


            // Zieken huis j wordt gebruikt als het minstens 1 jab heeft
            // ??
            for (int j = 0; j < machineCount; j++)
            {

                IntVar[] x = new IntVar[jobCount * JabCount];
                for (int i = 0; i < jobCount; i++)
                {
                    for (int d = 0; d < JabCount; d++)
                    {
                        var key = Tuple.Create(i, j, d);
                        x[i * JabCount + d] = G[key];
                    }
                }

                model.Add(LinearExpr.Sum(x) > 0).OnlyEnforceIf(A[j]);
                model.Add(LinearExpr.Sum(x) <= 0).OnlyEnforceIf(A[j].Not());
                //model.Add(LinearExpr.Sum(x) <= (jobCount * JabCount) * A[j]);
            }


            // Consecutive constraint 1
            // // Mapt de start van de achtereenvolgende eenen van S naar C

            for (int i = 0; i < jobCount; i++)
                for (int j = 0; j < machineCount; j++)
                    for (int d = 0; d < JabCount; d++)
                    {
                        for (int t = 1; t <= maxTime; t++)
                        {
                            var key1 = Tuple.Create(i, j, t, d);
                            var key2 = Tuple.Create(i, j, t - 1, d);
                            model.Add(C[key1] >= S[key1] - S[key2]);
                        }
                    }


            // Consecutive constraint 2 exactly 1 consecutive sequences per dose
            for (int i = 0; i < jobCount; i++)
            {
                for (int d = 0; d < JabCount; d++)
                {
                    IntVar[] x = new IntVar[machineCount * (maxTime + 1)];
                    for (int j = 0; j < machineCount; j++)
                    {
                        for (int t = 0; t <= maxTime; t++)
                        {
                            var key = Tuple.Create(i, j, t, d);
                            x[j * maxTime + t + j] = C[key];
                        }
                    }
                    model.Add(LinearExpr.Sum(x) == 1);
                }

            }

            // Consecutive constraint 3, jab starts at specified time for person i for jab d


            for (int i = 0; i < jobCount; i++)
            {
                for (int d = 0; d < JabCount; d++)
                {
                    LinearExpr[] x = new LinearExpr[machineCount * (maxTime + 1)];
                    for (int j = 0; j < machineCount; j++)
                    {
                        for (int t = 0; t <= maxTime; t++)
                        {
                            var key1 = Tuple.Create(i, j, t, d);
                            x[j * maxTime + t + j] = LinearExpr.Prod(C[key1], t);
                        }
                    }
                    var key2 = Tuple.Create(i, d);
                    model.Add(LinearExpr.Sum(x) == T[key2]);
                }

            }

            model.Minimize(LinearExpr.Sum(A));
            CpSolver solver = new CpSolver();
            //solver.StringParameters = "search_branching:FIXED_SEARCH, enumerate_all_solutions:true";
            CpSolverStatus status = solver.Solve(model);
            Console.WriteLine($"Solve status: {status}");

            // Print solution.
            // Check that the problem has a feasible solution.
            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                Console.WriteLine($"Total cost: {solver.ObjectiveValue}\n");


                for (int i = 0; i < jobCount; i++)
                {
                    for (int d = 0; d < JabCount; d++)
                    {
                        var key = Tuple.Create(i, d);
                        Console.WriteLine($"C[{i},{d}] = " + solver.Value(T[key]));
                    }
                }



                for (int i = 0; i < jobCount; i++)
                {
                    for (int d = 0; d < JabCount; d++)
                    {
                        for (int j = 0; j < machineCount; j++)
                        {
                            string v = "";
                            string f = "";
                            for (int t = 0; t <= maxTime; t++)
                            {
                                var key = Tuple.Create(i, j, t, d);
                                v += solver.Value(C[key]).ToString();
                                f += solver.Value(S[key]).ToString();
                            }
                            //Console.WriteLine($"C[{i},{j},{d}] = " + v);
                            //Console.WriteLine($"S[{i},{j},{d}] = " + f);
                        }
                    }
                }

            }

            Schedule[] schedules = new Schedule[jobCount];
            return schedules;
        }

    }
}
