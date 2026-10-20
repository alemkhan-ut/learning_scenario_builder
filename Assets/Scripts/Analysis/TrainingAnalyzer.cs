using System.Collections.Generic;
using LearningScenarioBuilder.Logging;
using LearningScenarioBuilder.Modes;
using UnityEngine;

namespace LearningScenarioBuilder.Analysis
{
    /// <summary>
    /// Оценка по числу попыток: попытки шага = нарушения + 1; баллы только за выполненный шаг.
    /// </summary>
    [CreateAssetMenu(menuName = "Learning Scenario Builder/Training Analyzer")]
    public class TrainingAnalyzer : ScenarioAnalyzer
    {
        private const string COMPLETED_LINE_FORMAT = "{0}: попыток {1}, нарушений {2} → {3}";
        private const string NOT_COMPLETED_LINE_FORMAT = "{0}: {1}, нарушений {2} → 0";

        [SerializeField]
        [Min(0)]
        [Tooltip("Баллы за шаг, выполненный с первой попытки.")]
        private int _firstAttemptPoints = 10;

        [SerializeField]
        [Min(0)]
        [Tooltip("Баллы за шаг, выполненный со второй попытки.")]
        private int _secondAttemptPoints = 5;

        [SerializeField]
        [Min(0)]
        [Tooltip("Баллы за шаг, выполненный с третьей попытки и позже.")]
        private int _laterAttemptPoints = 0;

        public override ScenarioReport Analyze(IReadOnlyList<ScenarioLogEntry> entries)
        {
            ScenarioReport report = new ScenarioReport();
            List<StepLogSummary> summaries = GetStepSummaries(entries, report);

            foreach (StepLogSummary summary in summaries)
            {
                report.Score += GetStepPoints(summary, report);
            }

            report.MaxScore = _firstAttemptPoints * summaries.Count;
            return report;
        }

        private int GetStepPoints(StepLogSummary summary, ScenarioReport report)
        {
            if (summary.Result != nameof(StepStatus.Completed))
            {
                if (summary.Result == null)
                {
                    report.AddWarning(string.Format(STEP_NOT_FINISHED_WARNING, summary.StepId));
                }

                string result = summary.Result ?? NOT_FINISHED_RESULT;
                report.AddStepLine(string.Format(NOT_COMPLETED_LINE_FORMAT, summary.StepId, result, summary.ViolationCount));
                return 0;
            }

            int attempts = summary.ViolationCount + 1;
            int points = GetAttemptPoints(attempts);
            report.AddStepLine(string.Format(COMPLETED_LINE_FORMAT, summary.StepId, attempts, summary.ViolationCount, points));
            return points;
        }

        private int GetAttemptPoints(int attempts)
        {
            if (attempts == 1)
            {
                return _firstAttemptPoints;
            }

            return attempts == 2 ? _secondAttemptPoints : _laterAttemptPoints;
        }
    }
}
