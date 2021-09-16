﻿using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace VaccinationScheduling.Shared.JabSchedule
{
    public class JabSchedule
    {
        private readonly Global _global;
        private readonly HashSet<Slot> _startjabOne = new();
        private readonly HashSet<Slot> _endjabOne = new();
        private readonly HashSet<Slot> _startjabTwo = new();
        private readonly HashSet<Slot> _endjabTwo = new();

        public JabSchedule(Global global)
        {
            _global = global;
        }

        private IEnumerable<HashSet<Slot>> HashSets()
        {
            yield return _startjabOne;
            yield return _endjabOne;
            yield return _startjabTwo;
            yield return _endjabTwo;
        }

        private IEnumerable<Slot> SlotsFromRange((BigInteger start, BigInteger end) range)
        {
            yield return new Slot(range.start / _global.TFirstDose, range);
            yield return new Slot(range.end / _global.TFirstDose, range);
            yield return new Slot(range.start / _global.TSecondDose, range);
            yield return new Slot(range.end / _global.TSecondDose, range);
        }

        public bool Contains((BigInteger start, BigInteger end) range) =>
            HashSets().Zip(SlotsFromRange(range)).Any(tuple => tuple.First.Contains(tuple.Second));

        public void Add(JabEnum jabEnum, (BigInteger start, BigInteger end) range, Job job)
        {
            if (jabEnum == JabEnum.JabOne)
            {
                _startjabOne.Add(new SlotWithJob(range.start / _global.TFirstDose, range, job));
                _endjabOne.Add(new SlotWithJob(range.end / _global.TFirstDose, range, job));
            }
            else
            {
                _startjabTwo.Add(new SlotWithJob(range.start / _global.TSecondDose, range, job));
                _endjabTwo.Add(new SlotWithJob(range.end / _global.TSecondDose, range, job));
            }
        }

        public IEnumerable<Job> Get((BigInteger start, BigInteger end) range)
        {
            return HashSets().Zip(SlotsFromRange(range)).Select(
                tuple =>
                {
                    tuple.First.TryGetValue(tuple.Second, out var slotMaybe);
                    return (slotMaybe as SlotWithJob)?.Job;
                }
            ).Where(jobMaybe => jobMaybe != null)!;
        }
    }
}