using System.Collections.Generic;
using UnityEngine;

namespace LearningScenarioBuilder.Analysis
{
    /// <summary>
    /// Результат анализа журнала: итог, строки по шагам и предупреждения о недостающих данных.
    /// </summary>
    public class ScenarioReport
    {
        private readonly List<string> _stepLines = new List<string>();
        private readonly List<string> _warnings = new List<string>();

        public int Score { get; set; }
        public int MaxScore { get; set; }
        public IReadOnlyList<string> StepLines => _stepLines;
        public IReadOnlyList<string> Warnings => _warnings;

        public void AddStepLine(string line)
        {
            _stepLines.Add(line);
        }

        public void AddWarning(string message)
        {
            _warnings.Add(message);
            Debug.LogWarning(message);
        }
    }
}
