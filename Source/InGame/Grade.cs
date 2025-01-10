using System;
using System.Collections.Generic;
using System.Linq;

namespace InGame
{
    public class Grade
    {
        public static Dictionary<float, string> RangeRankPairs { get; } = new()
        {
            { RankStep * 0, "D" },
            { RankStep * 1, "C" },
            { RankStep * 2, "B" },
            { RankStep * 3, "A" },
            { RankStep * 4, "S" },
        };
        private static List<float> Ranges { get; } = RangeRankPairs.Keys.ToList();
        private static List<string> Ranks { get; } = RangeRankPairs.Values.ToList();

        public static float RankStep => 0.2f;

        public float Value { get => _value; set => SetValue(value); }
        public string Rank { get; private set; }
        
        public event Action<string> RankChanged;
        public event Action<float> ValueChanged;

        private float _value;
        private int lastLessRangeIndex = 0;

        public Grade() : this(0) { }
        public Grade(float gradeValue) => Value = gradeValue;
        
        public float DistanceToNext()
        {
            float range = 1;
            int index = lastLessRangeIndex + 1;
            
            if (index < Ranges.Count)
            {
                range = Ranges[index];
            }
            
            return range - Value;
        }
        public float DistanceToPrevious()
        {
            return Value - Ranges[lastLessRangeIndex];
        }

        private void SetValue(float value)
        {
            float oldValue = _value;
            _value = value.Clamp01();

            if (_value != oldValue)
            {
                ValueChanged?.Invoke(_value - oldValue);
            }

            string newRank = RangeRankPairs[GetRange(_value, out lastLessRangeIndex)];
            
            if (newRank != Rank)
                RankChanged?.Invoke(newRank);

            Rank = newRank;
        }
        public override string ToString() => Rank;

        public static string ToRank(float value) => RangeRankPairs[GetRange(value)];

        public static float GetRange(float value) => GetRange(value, out _);
        public static float GetRange(float value, out int index)
        {
            value = value.Clamp01();
            index = 0;

            var ranges = Ranges;
            for (int i = ranges.Count - 1; i >= 0; i--)
            {
                float range = ranges[i];
            
                if (range <= value) 
                {
                    index = i;
                    return range;
                }
            }

            return 0;
        }
    }
}