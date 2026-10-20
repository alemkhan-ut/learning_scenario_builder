using System.Collections.Generic;
using LearningScenarioBuilder.Logging;
using UnityEngine;

namespace LearningScenarioBuilder.Analysis
{
    /// <summary>
    /// Анализатор считает оценку только по журналу, поэтому применим к журналу любого режима.
    /// Новый анализатор — наследник с реализацией <see cref="Analyze"/>.
    /// </summary>
    public abstract class ScenarioAnalyzer : ScriptableObject
    {
        protected const string STEP_NOT_FINISHED_WARNING = "Шаг {0} не завершён";
        protected const string NOT_FINISHED_RESULT = "не завершён";

        private const string EMPTY_LOG_WARNING = "Журнал пуст";
        private const string NO_STEP_LIST_WARNING = "В журнале нет списка шагов — учитываются только встреченные шаги";
        private const string SCENARIO_NOT_FINISHED_WARNING = "Прохождение не завершено";

        [SerializeField]
        [Tooltip("Название анализатора для пользователя. Если пусто — имя ассета.")]
        private string _displayName;

        public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? name : _displayName;

        public abstract ScenarioReport Analyze(IReadOnlyList<ScenarioLogEntry> entries);

        /// <summary>
        /// Сводки по шагам в порядке появления в журнале. Недостающие данные — предупреждение в отчёте, не исключение.
        /// </summary>
        protected List<StepLogSummary> GetStepSummaries(IReadOnlyList<ScenarioLogEntry> entries, ScenarioReport report)
        {
            List<StepLogSummary> summaries = new List<StepLogSummary>();

            if (entries == null || entries.Count == 0)
            {
                report.AddWarning(EMPTY_LOG_WARNING);
                return summaries;
            }

            Dictionary<string, StepLogSummary> summariesById = new Dictionary<string, StepLogSummary>();
            bool hasStepList = false;
            bool isScenarioFinished = false;

            foreach (ScenarioLogEntry entry in entries)
            {
                if (entry == null)
                {
                    continue;
                }

                if (entry.Type == ScenarioLogEntryTypes.SCENARIO_FINISHED)
                {
                    isScenarioFinished = true;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.StepId))
                {
                    continue;
                }

                switch (entry.Type)
                {
                    case ScenarioLogEntryTypes.STEP_REGISTERED:
                        hasStepList = true;
                        GetOrAddSummary(entry.StepId, summariesById, summaries);
                        break;
                    case ScenarioLogEntryTypes.STEP_ACTIVATED:
                        GetOrAddSummary(entry.StepId, summariesById, summaries);
                        break;
                    case ScenarioLogEntryTypes.STEP_VIOLATION:
                        GetOrAddSummary(entry.StepId, summariesById, summaries).AddViolation();
                        break;
                    case ScenarioLogEntryTypes.STEP_FINISHED:
                        GetOrAddSummary(entry.StepId, summariesById, summaries).SetResultIfMissing(entry.Result);
                        break;
                }
            }

            if (!hasStepList)
            {
                report.AddWarning(NO_STEP_LIST_WARNING);
            }

            if (!isScenarioFinished)
            {
                report.AddWarning(SCENARIO_NOT_FINISHED_WARNING);
            }

            return summaries;
        }

        private static StepLogSummary GetOrAddSummary(string stepId, Dictionary<string, StepLogSummary> summariesById, List<StepLogSummary> summaries)
        {
            if (!summariesById.TryGetValue(stepId, out StepLogSummary summary))
            {
                summary = new StepLogSummary(stepId);
                summariesById.Add(stepId, summary);
                summaries.Add(summary);
            }

            return summary;
        }
    }
}
