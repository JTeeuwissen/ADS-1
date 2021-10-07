using Google.OrTools.LinearSolver;

namespace VaccinationScheduling.Offline
{
    /// <summary>
    /// See <see href="https://developers.google.cn/optimization/assignment/assignment_example"/> for more information.
    /// </summary>
    static class ILPRienk
    {
        private const int jabCount = 2;

        public static Solver CreateSolver(
            int jobCount,
            int machineCount,
            int maxTime,
            int g,
            (int r1, int d1)[] firstFeasibleIntervals,
            int[] patientDelays,
            int[] secondFeasibleIntervalLengths,
            int p1,
            int p2
        )
        {
            // Create the linear solver with the SCIP backend.
            Solver solver = Solver.CreateSolver("SCIP");

            // Variable J[i,j,m,t] is 1 if jab j of job i is scheduled on machine m in timeslot t and 0 otherwise
            Variable[,,,] J = new Variable[jobCount, jabCount, machineCount, maxTime];

            for (int i = 0; i < jobCount; i++)
            for (int j = 0; j < jabCount; j++)
            for (int m = 0; m < machineCount; m++)
            for (int t = 0; t < maxTime; t++)
                J[i, j, m, t] = solver.MakeBoolVar( $"J_{i}_{j}_{m}_{t}");

            // P1[i,t] is 1 if jab 1 of job i is allowed to be in slot t
            Variable[,] P1 = new Variable[jobCount, maxTime];
            for (int i = 0; i < jobCount; i++)
            for (int t = 0; t < maxTime; t++)
                P1[i, t] = solver.MakeBoolVar($"P1_{i}_{t}");

            // P2[i,t] is 1 if jab 2 of job i is allowed to be in slot t
            Variable[,] P2 = new Variable[jobCount, maxTime];
            for (int i = 0; i < jobCount; i++)
            for (int t = 0; t < maxTime; t++)
                P2[i, t] = solver.MakeBoolVar( $"P2_{i}_{t}");

            // Variable which keeps track of if a machine is used
            Variable[] M = new Variable[machineCount];
            for (int k = 0; k < machineCount; k++) M[k] = solver.MakeIntVar(0, double.MaxValue, $"M_{k}");
            //TODO waarom is dit double.maxvalue? als het gaat om of de machine wel of niet gebruikt word. ik zou bool verwachten.

            // M_k * t_max >= SUM(SUM(SUM(J_i_j_k_t, 0<=i<i_max), 0<j<=2),0<=t<=t_max) ∀k
            // We eisen dat M_k = 1 als er een jab plaatsvindt op de machine
            for (int m = 0; m < machineCount; m++)
            {
                //TODO waarom 0 en 1 meegeven aan de constraint?
                Constraint constraint = solver.MakeConstraint(0, 1, "M_k 1 if machine is used");
                constraint.SetCoefficient(M[m], maxTime);
                for (int i = 0; i < jobCount; i++)
                for (int j = 0; j < jabCount; j++)
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
                for (int j = 0; j < jabCount; j++)
                    constraint.SetCoefficient(J[i, j, m, t], 1);
            }

            // SUM(SUM(SUM(J_i_j_k_t, 0<j<=2), 0<=k<k_max), 0<=t<t_max) = 2	∀i
            // Iedere patient krijgt twee jabs
            for (int i = 0; i < jobCount; i++)
            {
                Constraint constraint = solver.MakeConstraint(jabCount, jabCount, "2 Jabs for every patient");

                for (int m = 0; m < machineCount; m++)
                for (int t = 0; t < maxTime; t++)
                for (int j = 0; j < jabCount; j++)
                    constraint.SetCoefficient(J[i, j, m, t], 1);
            }

            // SUM(J_i_1_k_t', t<t'<=t+p1) + J_i_1_k_t >= 1 ∀i,k,t < t-p1
            // Na een jab wordt er in het ziekenhuis geen nieuwe jab geplanned voor een gegeven periode
            for (int i = 0; i < jobCount; i++)
            for (int m = 0; m < machineCount; m++)
            for (int t = 0; t < maxTime - p1; t++)
            {
                Constraint constraint = solver.MakeConstraint(0, 1, "No jabs for p1 after any first jab");
                constraint.SetCoefficient(J[i, 1, m, t], 1);

                for (int t1 = 0; t < t1 && t1 < t + p1; t1++) constraint.SetCoefficient(J[i, 1, m, t1], 1);
            }

            // P1_i_t * t >= r_i ∀i,t
            // Een eerste jab mag niet eerder dan aangegeven
            for (int i = 0; i < jobCount; i++)
            for (int t = 0; t < maxTime; t++)
            {
                Constraint constraint = solver.MakeConstraint(
                    firstFeasibleIntervals[i].r1,
                    double.MaxValue,
                    "Jab one not earlier than allowed."
                );
                constraint.SetCoefficient(P1[i, t], t);
            }

            // P1_i_t * t + p_1 <= d_i ∀i,t
            // Een eerste jab mag niet later dan aangegeven
            for (int i = 0; i < jobCount; i++)
            for (int t = 0; t < maxTime; t++)
            {
                Constraint constraint = solver.MakeConstraint(
                    0,
                    firstFeasibleIntervals[i].d1 - p1,
                    "Jab one not later than allowed."
                );
                constraint.SetCoefficient(P1[i, t], t);
            }

            // J_i_1_k_t <= P1_i_t 	∀i,k,t
            // De eerste jab valt altijd in de gegeven time interval
            for (int i = 0; i < jobCount; i++)
            for (int m = 0; m < machineCount; m++)
            for (int t = 0; t < maxTime; t++)
            {
                Constraint constraint = solver.MakeConstraint(0, 0, "First jab in allowed period");
                constraint.SetCoefficient(P1[i, t], -1);
                constraint.SetCoefficient(J[i, 1, m, t], 1);
            }

            // P2_i_t * t >= SUM(SUM(J_i_2_k_t' * t', 0<t'<=max_t), 0<k<=k_max) + p_1 + g + l_i	∀i,t
            // Een tweede jab start niet eerder dan mag
            for (int i = 0; i < jobCount; i++)
            for (int t = 0; t < maxTime; t++)
            {
                Constraint constraint = solver.MakeConstraint(
                    p1 + g + patientDelays[i],
                    double.MaxValue,
                    "Second jab not earlier than allowed"
                );
                constraint.SetCoefficient(P2[i, t], t);

                for (int t1 = 0; t1 < maxTime; t1++)
                for (int m = 0; m < machineCount; m++)
                    constraint.SetCoefficient(J[i, jabCount, m, t1], -t1);
            }

            // P2_i_t * t + p_2 <= SUM(SUM(J_i_2_k_t' * t', 0<t'<=max_t), 0<k<=k_max) + p_1 + g + l_i + I_i	∀i,t
            // Een tweede jab start niet later dan mag
            for (int i = 0; i < jobCount; i++)
            for (int t = 0; t < maxTime; t++)
            {
                Constraint constraint = solver.MakeConstraint(
                    0,
                    p1 + g + patientDelays[i] + secondFeasibleIntervalLengths[i] - p2,
                    "Second jab not later than allowed"
                );
                constraint.SetCoefficient(P2[i, t], t);

                for (int t1 = 0; t1 < maxTime; t1++)
                for (int m = 0; m < machineCount; m++)
                    constraint.SetCoefficient(J[i, 2, m, t1], t1);
            }

            // J_i_2_k_t <= P2_i_t ∀i,k,t
            // De tweede jab valt altijd in de gegeven time interval
            for (int i = 0; i < jobCount; i++)
            for (int m = 0; m < machineCount; m++)
            for (int t = 0; t < maxTime; t++)
            {
                Constraint constraint = solver.MakeConstraint(double.MinValue, 0);
                constraint.SetCoefficient(J[i, 2, m, t], 1);
                constraint.SetCoefficient(P2[i, t], -1);
            }

            // Minimize P = SUM(M_k, (0<=k<k_max))
            // Minimize the sum of all used machines.
            Objective objective = solver.Objective();
            for (int m = 0; m < machineCount; ++m) objective.SetCoefficient(M[m], 1);
            objective.SetMinimization();

            return solver;
        }
    }
}