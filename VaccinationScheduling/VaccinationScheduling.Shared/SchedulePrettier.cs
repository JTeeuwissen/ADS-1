using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VaccinationScheduling.Shared
{
    public static class SchedulePrettier
    {
        /// <summary>
        /// Create a pretty schedule using global variables and a schedule enumerable.
        /// </summary>
        /// <param name="global">The global config.</param>
        /// <param name="schedules">A schedule list.</param>
        /// <returns>A string with a pretty schedule.</returns>
        public static string ToPrettyString(Global global, IEnumerable<Schedule> schedules)
        {
            (Schedule schedule, int index)[] indexedSchedules =
                schedules.Select((schedule, index) => (schedule, index)).ToArray();
            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine("Jab 1");
            foreach (IGrouping<int, (Schedule schedule, int index)> jab1Hospital in indexedSchedules.GroupBy(
                indexed => indexed.schedule.M1
            ))
            {
                stringBuilder.AppendLine($"Hospital {jab1Hospital.Key}".Pad());

                foreach ((var schedule, int index) in jab1Hospital)
                {
                    string job = $"Job {index}";
                    string line = ToLine(schedule.T1, global.P1);
                    stringBuilder.AppendLine($"{job.Pad()}{line}");
                }
            }

            stringBuilder.AppendLine();

            stringBuilder.AppendLine("Jab 2");
            foreach (IGrouping<int, (Schedule schedule, int index)> jab2Hospital in indexedSchedules.GroupBy(
                indexed => indexed.schedule.M2
            ))
            {
                stringBuilder.AppendLine($"Hospital {jab2Hospital.Key}".Pad());

                foreach ((var schedule, int index) in jab2Hospital)
                {
                    string job = $"Job {index}";
                    string line = ToLine(schedule.T2, global.P2);
                    stringBuilder.AppendLine($"{job.Pad()}{line}");
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Turn the timeslot and length into a nice box string.
        /// </summary>
        /// <param name="timeslot">The timeslot (offset + 1).</param>
        /// <param name="length">The jab length.</param>
        /// <returns>A pretty string.</returns>
        private static string ToLine(int timeslot, int length) =>
            $"{new string(' ', timeslot - 1)}{(length == 1 ? "║" : $"╠{new string('═', length - 2)}╣")}";

        /// <summary>
        /// Pad the string for a total length.
        /// </summary>
        /// <param name="value">The string to pad.</param>
        /// <returns>A padded string.</returns>
        private static string Pad(this string value) => value.PadRight(15);
    }
}