using System;
using System.Linq;
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
            int jobCount = jobs.Length;
            // ReSharper disable once InlineTemporaryVariable
            int machineCount = jobCount;
            int maxTime = jobs.Select(job => job.FirstIntervalEnd + job.ExtraDelay + job.SecondIntervalLength).Max() +
                          global.TGap;

            (int FirstIntervalStart, int FirstIntervalEnd)[] firstFeasibleIntervals =
                jobs.Select(job => (job.FirstIntervalStart, job.FirstIntervalEnd)).ToArray();
            int[] patientDelays = jobs.Select(job => job.ExtraDelay).ToArray();
            int[] secondIntervalLengths = jobs.Select(job => job.SecondIntervalLength).ToArray();

            // Create the linear solver with the SCIP backend.
            Solver solver = Solver.CreateSolver("SCIP");

            // Variable J[i,j,m,t] is 1 if jab j of job i is scheduled on machine m in timeslot t and 0 otherwise
            Variable[,,,] J = new Variable[jobCount, JabCount, machineCount, maxTime];

            for (int i = 0; i < jobCount; i++)
            for (int j = 0; j < JabCount; j++)
            for (int m = 0; m < machineCount; m++)
            for (int t = 0; t < maxTime; t++)
                J[i, j, m, t] = solver.MakeBoolVar($"J_{i}_{j}_{m}_{t}");

            // P1[i,t] is 1 if jab 1 of job i is allowed to be in slot t
            Variable[,] P1 = new Variable[jobCount, maxTime];
            for (int i = 0; i < jobCount; i++)
            for (int t = 0; t < maxTime; t++)
                P1[i, t] = solver.MakeBoolVar($"P1_{i}_{t}");

            // P2[i,t] is 1 if jab 2 of job i is allowed to be in slot t
            Variable[,] P2 = new Variable[jobCount, maxTime];
            for (int i = 0; i < jobCount; i++)
            for (int t = 0; t < maxTime; t++)
                P2[i, t] = solver.MakeBoolVar($"P2_{i}_{t}");

            // Variable which keeps track of if a machine is used
            Variable[] M = new Variable[machineCount];
            for (int k = 0; k < machineCount; k++) M[k] = solver.MakeBoolVar($"M_{k}");

            // M_k * t_max >= SUM(SUM(SUM(J_i_j_k_t, 0<=i<i_max), 0<j<=2),0<=t<=t_max) ∀k
            // We eisen dat M_k = 1 als er een jab plaatsvindt op de machine
            for (int m = 0; m < machineCount; m++)
            {
                Constraint constraint = solver.MakeConstraint(0, MaxValue, "M_k 1 if machine is used");
                constraint.SetCoefficient(M[m], maxTime);
                for (int i = 0; i < jobCount; i++)
                for (int j = 0; j < JabCount; j++)
                for (int t = 0; t < maxTime; t++)
                    constraint.SetCoefficient(J[i, j, m, t], -1);
            }

            // SUM(SUM(J_i_j_k_t, 0<=i<i_max), 0<j<=2) <= 1 ∀k,t
            // Er vindt maar 1 job tegelijk plaats op een machine k
            for (int m = 0; m < machineCount; m++)
            for (int t = 0; t < maxTime; t++)
            {
                Constraint constraint = solver.MakeConstraint(0, 1, "At any time at most 1 job on machine m");

                for (int i = 0; i < jobCount; i++)
                for (int j = 0; j < JabCount; j++)
                    constraint.SetCoefficient(J[i, j, m, t], 1);
            }

            // Iedere patient krijgt 1 eerste jab
            for (int i = 0; i < jobCount; i++)
            {
                Constraint constraint = solver.MakeConstraint(1, 1, "1 first jab for every patient");

                for (int m = 0; m < machineCount; m++)
                for (int t = 0; t < maxTime; t++)
                    constraint.SetCoefficient(J[i, (int)JabEnum.FirstJab, m, t], 1);
            }

            // Iedere patient krijgt 1 tweede jab
            for (int i = 0; i < jobCount; i++)
            {
                Constraint constraint = solver.MakeConstraint(1, 1, "1 second jab for every patient");

                for (int m = 0; m < machineCount; m++)
                for (int t = 0; t < maxTime; t++)
                    constraint.SetCoefficient(J[i, (int)JabEnum.SecondJab, m, t], 1);
            }

            // SUM(J_i_1_k_t', t<t'<=t+p1) + J_i_1_k_t >= 1 ∀i,k,t < t-p1
            // Na een jab wordt er in het ziekenhuis geen nieuwe jab geplanned voor een gegeven periode
            for (int i = 0; i < jobCount; i++)
            for (int m = 0; m < machineCount; m++)
            for (int t = 0; t < maxTime - global.TFirstDose; t++)
            {
                Constraint constraint = solver.MakeConstraint(0, 1, "No jabs for p1 after any first jab");
                constraint.SetCoefficient(J[i, (int)JabEnum.FirstJab, m, t], 1);

                for (int t1 = t; t1 < t + global.TFirstDose; t1++)
                    constraint.SetCoefficient(J[i, (int)JabEnum.FirstJab, m, t1], 1);
            }

            // P1_i_t * t >= r_i ∀i,t
            // Een eerste jab mag niet eerder dan aangegeven
            // P1_i_t * t + p_1 <= d_i ∀i,t
            // Een eerste jab mag niet later dan aangegeven
            for (int i = 0; i < jobCount; i++)
            for (int t = 0; t < maxTime; t++)
            {
                Constraint constraint = solver.MakeConstraint(
                    0,
                    MaxValue,
                    "Jab one not earlier or later than allowed."
                );
                constraint.SetCoefficient(P1[i, t], t - firstFeasibleIntervals[i].FirstIntervalStart);
            }

            //TODO werkt niet voor P1 = 0
            // P1_i_t * t + p_1 <= d_i ∀i,t
            // Een eerste jab mag niet later dan aangegeven
            for (int i = 0; i < jobCount; i++)
            for (int t = 0; t < maxTime; t++)
            {
                Constraint constraint = solver.MakeConstraint(0, MaxValue, "Jab one not later than allowed.");
                constraint.SetCoefficient(P1[i, t], firstFeasibleIntervals[i].FirstIntervalEnd - global.TFirstDose - t);
            }

            // J_i_1_k_t <= P1_i_t 	∀i,k,t
            // De eerste jab valt altijd in de gegeven time interval
            for (int i = 0; i < jobCount; i++)
            for (int m = 0; m < machineCount; m++)
            for (int t = 0; t < maxTime; t++)
            {
                Constraint constraint = solver.MakeConstraint(0, 0, "First jab in allowed period");
                constraint.SetCoefficient(P1[i, t], 1);
                constraint.SetCoefficient(J[i, (int)JabEnum.FirstJab, m, t], -1);
            }

            //TODO werkt niet voor P2 = 0
            // P2_i_t * t >= SUM(SUM(J_i_2_k_t' * t', 0<t'<=max_t), 0<k<=k_max) + p_1 + g + l_i	∀i,t
            // Een tweede jab start niet eerder dan mag
            for (int i = 0; i < jobCount; i++)
            for (int t = 0; t < maxTime; t++)
            {
                Constraint constraint = solver.MakeConstraint(
                    -maxTime,
                    MaxValue,
                    "Second jab not earlier than allowed"
                );
                constraint.SetCoefficient(P2[i, t], t - (global.TFirstDose + global.TGap + patientDelays[i]) - maxTime);

                for (int t1 = 0; t1 < maxTime; t1++)
                for (int m = 0; m < machineCount; m++)
                    constraint.SetCoefficient(J[i, (int)JabEnum.FirstJab, m, t1], -t1);
            }

            //TODO werkt niet voor P2 = 0
            // P2_i_t * t + p_2 <= SUM(SUM(J_i_2_k_t' * t', 0<t'<=max_t), 0<k<=k_max) + p_1 + g + l_i + I_i	∀i,t
            // Een tweede jab start niet later dan mag
            for (int i = 0; i < jobCount; i++)
            for (int t = 0; t < maxTime; t++)
            {
                Constraint constraint = solver.MakeConstraint(-maxTime, MaxValue, "Second jab not later than allowed");
                constraint.SetCoefficient(
                    P2[i, t],
                    t - (global.TFirstDose + global.TGap + patientDelays[i] + secondIntervalLengths[i] -
                         global.TSecondDose) - maxTime
                );

                for (int t1 = 0; t1 < maxTime; t1++)
                for (int m = 0; m < machineCount; m++)
                    constraint.SetCoefficient(J[i, (int)JabEnum.SecondJab, m, t1], t1);
            }

            // J_i_2_k_t <= P2_i_t ∀i,k,t
            // De tweede jab valt altijd in de gegeven time interval
            for (int i = 0; i < jobCount; i++)
            for (int m = 0; m < machineCount; m++)
            for (int t = 0; t < maxTime; t++)
            {
                Constraint constraint = solver.MakeConstraint(0, 0, "Second jab in allowed period");
                constraint.SetCoefficient(P2[i, t], 1);
                constraint.SetCoefficient(J[i, (int)JabEnum.SecondJab, m, t], -1);
            }

            // Minimize P = SUM(M_k, (0<=k<k_max))
            // Minimize the sum of all used machines.
            Objective objective = solver.Objective();
            for (int m = 0; m < machineCount; ++m) objective.SetCoefficient(M[m], 1);
            objective.SetMinimization();

            Solver.ResultStatus result = solver.Solve();

            // Check that the problem has an optimal solution.
            if (result != Solver.ResultStatus.OPTIMAL)
                throw new Exception("The problem does not have an optimal solution!");

            Console.WriteLine("Solution:");
            Console.WriteLine("Objective value = " + solver.Objective().Value());

            for (int i = 0; i < jobCount; i++)
            for (int j = 0; j < JabCount; j++)
            for (int m = 0; m < machineCount; m++)
            for (int t = 0; t < maxTime; t++)
                Console.WriteLine($"J_{i}_{j}_{m}_{t} {J[i, j, m, t].SolutionValue()}");

            for (int i = 0; i < jobCount; i++)
            for (int t = 0; t < maxTime; t++)
                Console.WriteLine($"P1_{i}_{t} {P1[i, t].SolutionValue()}");

            for (int i = 0; i < jobCount; i++)
            for (int t = 0; t < maxTime; t++)
                Console.WriteLine($"P2_{i}_{t} {P2[i, t].SolutionValue()}");

            for (int k = 0; k < machineCount; k++)
                Console.WriteLine($"M_{k} {M[k].SolutionValue()}");


            Schedule[] schedules = new Schedule[jobCount];

            (int m, int t) GetJab(int i, JabEnum j)
            {
                for (int m = 0; m < machineCount; m++)
                for (int t = 0; t < maxTime; t++)
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (J[i, (int)j, m, t].SolutionValue() == 1)
                        return (m + 1, t);
                throw new Exception($"Jab {j} from job {i} is missing");
            }

            for (int i = 0; i < jobCount; i++)
            {
                (int m1, int t1) = GetJab(i, JabEnum.FirstJab);
                (int m2, int t2) = GetJab(i, JabEnum.SecondJab);
                schedules[i] = new Schedule(t1, m1, t2, m2);
            }

            return schedules;
        }

        private const int JabCount = 2;
    }
}