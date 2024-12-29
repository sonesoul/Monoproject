using System;
using System.Collections.Generic;

namespace InGame
{
    public class Grade
    {
        public static List<string> Grades { get; } = new()
        {
            "D",
            "C",
            "B",
            "A",
            "S",
            "P",
        };

        public float Value { get => _value; set => SetValue(value); }
        public int Index { get; private set; }
        public float PercentValue => Value * 100;

        public event Action<int, string> GradeChanged;
        public event Action<float> ValueChanged;

        private float _value;

        public Grade() : this(0) { }
        public Grade(float gradeValue) => Value = gradeValue;
        
        private void SetValue(float value)
        {
            float oldValue = _value;
            _value = value.Clamp01();

            if (_value != oldValue)
            {
                ValueChanged?.Invoke(_value - oldValue);
            }

            UpdateIndex();
        }
        private void UpdateIndex()
        {
            int oldIndex = Index;
            Index = GetLetterIndex(Value);

            if (Index != oldIndex)
            {
                GradeChanged?.Invoke(Index - oldIndex, ToString());
            }
        }
        public override string ToString() => GetLetter(Index);

        public static string GetLetter(float gradeValue) => GetLetter(GetLetterIndex(gradeValue));
        public static string GetLetter(int index) => Grades[index];
        public static int GetLetterIndex(float gradeValue) => (int)(gradeValue.Clamp01() * (Grades.Count - 1)).Rounded();
    }
}