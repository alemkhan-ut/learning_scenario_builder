using System.Collections.Generic;
using LearningScenarioBuilder.Logging;
using LearningScenarioBuilder.Modes;
using UnityEngine;

namespace LearningScenarioBuilder.Analysis
{
    /// <summary>
    /// Оценка штрафами: итог = базовый балл − штрафы за Failed и Skipped, не меньше нуля.
    /// </summary>
    [CreateAssetMenu(menuName = "Learning Scenario Builder/Exam Analyzer")]
    public class ExamAnalyzer : ScenarioAnalyzer
    {
        private const string STEP_LINE_FORMAT = "{0}: {1} → {2}";
        private const string PENALTY_FORMAT = "−{0}";

        [SerializeField]
        [Min(0)]
        [Tooltip("Баллы до вычета штрафов — максимальный результат.")]
        private int _baseScore = 100;

        [SerializeField]
        [Min(0)]
        [Tooltip("Штраф за проваленный шаг (Failed).")]
        private int _failedPenalty = 10;

        [SerializeField]
        [Min(0)]
        [Tooltip("Штраф за пропущенный шаг (Skipped). Незавершённый шаг штрафуется так же.")]
        private int _skippedPenalty = 5;

        public override ScenarioReport Analyze(IReadOnlyList<ScenarioLogEntry> entries)
        {
            ScenarioReport report = new ScenarioReport();
            int totalPenalty = 0;

            foreach (StepLogSummary summary in GetStepSummaries(entries, report))
            {
                int penalty = GetStepPenalty(summary, report);
                string result = summary.Result ?? NOT_FINISHED_RESULT;
                string penaltyText = penalty > 0 ? string.Format(PENALTY_FORMAT, penalty) : "0";
                report.AddStepLine(string.Format(STEP_LINE_FORMAT, summary.StepId, result, penaltyText));
                totalPenalty += penalty;
            }

            report.MaxScore = _baseScore;
            report.Score = Mathf.Max(0, _baseScore - totalPenalty);
            return report;
        }

        private int GetStepPenalty(StepLogSummary summary, ScenarioReport report)
        {
            switch (summary.Result)
            {
                case nameof(StepStatus.Completed):
                    return 0;
                case nameof(StepStatus.Failed):
                    return _failedPenalty;
                case nameof(StepStatus.Skipped):
                    return _skippedPenalty;
                default:
                    report.AddWarning(string.Format(STEP_NOT_FINISHED_WARNING, summary.StepId));
                    return _skippedPenalty;
            }
        }
    }
}
