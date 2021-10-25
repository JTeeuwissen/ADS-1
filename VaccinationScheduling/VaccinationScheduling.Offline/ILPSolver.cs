using System;
using Google.OrTools.LinearSolver;
using VaccinationScheduling.Shared;
using static System.Double;

namespace VaccinationScheduling.Offline
{
    /// <summary>
    /// See <see href="https://developers.google.cn/optimization/assignment/assignment_example"/> for more information.
    /// </summary>
    static class ILPSolver
    {
        public static Schedule[] Solve(Global global, Job[] jobs)
        {
            (int jabCount, int iMax, int mMax, int tMax, int[]? r, int[]? d, int[]? x, int[]? l) =
                Constants.GetConstants(global, jobs);

            int p1 = global.TimeFirstDose;
            int p2 = global.TimeSecondDose;
            int g = global.TimeGap;

            // Create the linear solver with the SCIP backend.
            Solver solver = Solver.CreateSolver("SAT");

            // Variable J[i,j,m,t] is 1 if jab j of job i is scheduled on machine m in timeslot t and 0 otherwise
            // ReSharper disable once InconsistentNaming
            Variable[,,,] J = new Variable[iMax, jabCount, mMax, tMax];

            for (int i = 0; i < iMax; i++)
            for (int j = 0; j < jabCount; j++)
            for (int m = 0; m < mMax; m++)
            for (int t = 0; t < tMax; t++)
                J[i, j, m, t] = solver.MakeBoolVar($"J_{i}_{j}_{m}_{t}");

            // P2[i,t] is 1 if jab 2 of job i is allowed to be in slot t
            // ReSharper disable once InconsistentNaming
            Variable[,] P2 = new Variable[iMax, tMax];
            for (int i = 0; i < iMax; i++)
            for (int t = 0; t < tMax; t++)
                P2[i, t] = solver.MakeBoolVar($"P2_{i}_{t}");

            // Variable which keeps track of if a machine is used
            // ReSharper disable once InconsistentNaming
            Variable[] M = new Variable[mMax];
            for (int m = 0; m < mMax; m++) M[m] = solver.MakeBoolVar($"M_{m}");

            // Minimize the sum of all used machines.
            Objective objective = solver.Objective();
            for (int m = 0; m < mMax; m++) objective.SetCoefficient(M[m], 1);
            objective.SetMinimization();

            // M_m must be 1 if any jab is planned in hospital m
            for (int m = 0; m < mMax; m++)
            {
                Constraint constraint = solver.MakeConstraint(0, MaxValue, "M_m 1 if machine is used");
                constraint.SetCoefficient(M[m], tMax);

                for (int i = 0; i < iMax; i++)
                for (int j = 0; j < jabCount; j++)
                for (int t = 0; t < tMax; t++)
                    constraint.SetCoefficient(J[i, j, m, t], -1);
            }

            // A maximum of 1 job is planned at any time on any machine
            for (int m = 0; m < mMax; m++)
            for (int t = 0; t < tMax; t++)
            {
                Constraint constraint = solver.MakeConstraint(0, 1, "At any time at most 1 job on machine m");

                for (int i = 0; i < iMax; i++)
                for (int j = 0; j < jabCount; j++)
                    constraint.SetCoefficient(J[i, j, m, t], 1);
            }

            // Every patient gets exactly 1 first jab
            for (int i = 0; i < iMax; i++)
            {
                Constraint constraint = solver.MakeConstraint(1, 1, "1 first jab for every patient");

                for (int m = 0; m < mMax; m++)
                for (int t = 0; t < tMax; t++)
                    constraint.SetCoefficient(J[i, (int)JabEnum.FirstJab, m, t], 1);
            }

            // Every patient gets exactly 1 second jab
            for (int i = 0; i < iMax; i++)
            {
                Constraint constraint = solver.MakeConstraint(1, 1, "1 second jab for every patient");

                for (int m = 0; m < mMax; m++)
                for (int t = 0; t < tMax; t++)
                    constraint.SetCoefficient(J[i, (int)JabEnum.SecondJab, m, t], 1);
            }

            // After a first jab in a hospital, no new jab is planned in that hospital for p1 slots
            for (int m = 0; m < mMax; m++)
            for (int t = 0; t < tMax - p1; t++)
            {
                Constraint constraint = solver.MakeConstraint(
                    0,
                    iMax * jabCount * (p1 - 1),
                    "No jabs for p1 after any first jab"
                );

                for (int i = 0; i < iMax; i++)
                {
                    constraint.SetCoefficient(J[i, (int)JabEnum.FirstJab, m, t], iMax * jabCount * (p1 - 1));

                    for (int j = 0; j < jabCount; j++)
                    for (int t1 = t + 1; t1 < t + p1; t1++)
                        constraint.SetCoefficient(J[i, j, m, t1], 1);
                }
            }

            // After a second jab in a hospital, no new jab is planned in that hospital for p2 slots
            for (int m = 0; m < mMax; m++)
            for (int t = 0; t < tMax - p2; t++)
            {
                Constraint constraint = solver.MakeConstraint(
                    0,
                    iMax * jabCount * (p2 - 1),
                    "No jabs for p2 after any second jab"
                );

                for (int i = 0; i < iMax; i++)
                {
                    constraint.SetCoefficient(J[i, (int)JabEnum.SecondJab, m, t], iMax * jabCount * (p2 - 1));

                    for (int j = 0; j < jabCount; j++)
                    for (int t1 = t + 1; t1 < t + p2; t1++)
                        constraint.SetCoefficient(J[i, j, m, t1], 1);
                }
            }

            // A first jab is only planned in allowed time slots
            for (int i = 0; i < iMax; i++)
            for (int m = 0; m < mMax; m++)
            for (int t = 0; t < tMax; t++)
            {
                if (t >= r[i] - 1 && t <= d[i] - p1) continue;
                Constraint constraint = solver.MakeConstraint(0, 0, "First jab in allowed period");
                constraint.SetCoefficient(J[i, (int)JabEnum.FirstJab, m, t], 1);
            }

            // A second jab is not allowed earlier than indicated by the patient
            for (int i = 0; i < iMax; i++)
            for (int t = 0; t < tMax; t++)
            {
                Constraint constraint = solver.MakeConstraint(-tMax, MaxValue, "Second jab not earlier than allowed");
                constraint.SetCoefficient(P2[i, t], t - p1 - g - x[i] - tMax);

                for (int t1 = 0; t1 < tMax; t1++)
                for (int m = 0; m < mMax; m++)
                    constraint.SetCoefficient(J[i, (int)JabEnum.FirstJab, m, t1], -t1);
            }

            // A second jab is not allowed later than indicated by the patient
            for (int i = 0; i < iMax; i++)
            for (int t = 0; t < tMax; t++)
            {
                Constraint constraint = solver.MakeConstraint(
                    MinValue,
                    p1 + g + x[i] + l[i],
                    "Second jab not later than allowed"
                );
                constraint.SetCoefficient(P2[i, t], t + p2);

                for (int t1 = 0; t1 < t; t1++)
                for (int m = 0; m < mMax; m++)
                    constraint.SetCoefficient(J[i, (int)JabEnum.FirstJab, m, t1], -t1);
            }

            // A second jab is only planned in allowed time slots
            for (int i = 0; i < iMax; i++)
            for (int m = 0; m < mMax; m++)
            for (int t = 0; t < tMax; t++)
            {
                Constraint constraint = solver.MakeConstraint(-1, 0, "Second jab in allowed period");
                constraint.SetCoefficient(P2[i, t], -1);
                constraint.SetCoefficient(J[i, (int)JabEnum.SecondJab, m, t], 1);
            }

            Solver.ResultStatus result = solver.Solve();

            // Check that the problem has an optimal solution.
            if (result != Solver.ResultStatus.OPTIMAL)
                throw new Exception("The problem does not have an optimal solution!");

            Extensions.WriteDebugLine("Solution:");
            Extensions.WriteDebugLine("Objective value = " + solver.Objective().Value());
            
            Schedule[] schedules = new Schedule[iMax];

            (int m, int t) GetJab(int i, JabEnum j)
            {
                for (int m = 0; m < mMax; m++)
                for (int t = 0; t < tMax; t++)
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (J[i, (int)j, m, t].SolutionValue() == 1)
                        return (m + 1, t + 1);
                throw new Exception($"Jab {j} from job {i} is missing");
            }

            for (int i = 0; i < iMax; i++)
            {
                (int m1, int t1) = GetJab(i, JabEnum.FirstJab);
                (int m2, int t2) = GetJab(i, JabEnum.SecondJab);
                schedules[i] = new Schedule(t1, m1, t2, m2);
            }

            return schedules;
        }
    }
}