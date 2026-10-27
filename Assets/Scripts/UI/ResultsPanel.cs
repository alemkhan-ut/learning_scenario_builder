using System;
using System.Collections.Generic;
using System.Text;
using LearningScenarioBuilder.Analysis;
using LearningScenarioBuilder.Logging;
using TMPro;
using UnityEngine;

namespace LearningScenarioBuilder.UI
{
    /// <summary>
    /// Экран результатов: журнал прохождения и отчёт выбранного анализатора; любой анализатор переоценивает тот же журнал.
    /// </summary>
    public class ResultsPanel : MonoBehaviour
    {
        public event Action RestartRequestedEvent;

        private const string ANALYZER_FORMAT = "Анализатор: {0}";
        private const string SCORE_FORMAT = "Итог: {0} / {1}";
        private const string WARNINGS_HEADER = "Предупреждения:";
        private const string WARNING_FORMAT = "• {0}";
        private const string NO_ANALYZER_MESSAGE = "Анализатор не назначен";

        [SerializeField]
        private TMP_Text _reportText;

        [SerializeField]
        private TMP_Text _logText;

        [SerializeField]
        private Transform _analyzerButtonsRoot;

        [SerializeField]
        private LabeledButton _buttonPrefab;

        [SerializeField]
        private LabeledButton _restartButton;

        private LabeledButtonList<ScenarioAnalyzer> _analyzerButtons;
        private IReadOnlyList<ScenarioLogEntry> _entries = Array.Empty<ScenarioLogEntry>();

        private void Awake()
        {
            _analyzerButtons = new LabeledButtonList<ScenarioAnalyzer>(_buttonPrefab, _analyzerButtonsRoot);
        }

        private void OnEnable()
        {
            _analyzerButtons.ItemSelectedEvent += ItemSelectedEventHandler;
            _restartButton.ClickedEvent += ClickedEventHandler;
        }

        private void OnDisable()
        {
            _analyzerButtons.ItemSelectedEvent -= ItemSelectedEventHandler;
            _restartButton.ClickedEvent -= ClickedEventHandler;
        }

        public void Show(IReadOnlyList<ScenarioLogEntry> entries, ScenarioAnalyzer analyzer, IReadOnlyList<ScenarioAnalyzer> analyzers)
        {
            _entries = entries ?? Array.Empty<ScenarioLogEntry>();
            gameObject.SetActive(true);
            _analyzerButtons.Show(analyzers, GetAnalyzerLabel);
            _logText.text = GetLogText();
            ShowReport(analyzer);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private static string GetAnalyzerLabel(ScenarioAnalyzer analyzer)
        {
            return analyzer.DisplayName;
        }

        private string GetLogText()
        {
            StringBuilder builder = new StringBuilder();

            foreach (ScenarioLogEntry entry in _entries)
            {
                if (entry != null && !IsServiceEntry(entry))
                {
                    builder.AppendLine(entry.GetDisplayText());
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Служебные записи нужны анализатору, но не показываются пользователю.
        /// </summary>
        private static bool IsServiceEntry(ScenarioLogEntry entry)
        {
            return entry.Type == ScenarioLogEntryTypes.STEP_REGISTERED || entry.Type == ScenarioLogEntryTypes.STEP_ACTIVATED;
        }

        private void ShowReport(ScenarioAnalyzer analyzer)
        {
            if (analyzer == null)
            {
                _reportText.text = NO_ANALYZER_MESSAGE;
                return;
            }

            ScenarioReport report = analyzer.Analyze(_entries);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format(ANALYZER_FORMAT, analyzer.DisplayName));
            builder.AppendLine(string.Format(SCORE_FORMAT, report.Score, report.MaxScore));
            builder.AppendLine();

            foreach (string line in report.StepLines)
            {
                builder.AppendLine(line);
            }

            if (report.Warnings.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine(WARNINGS_HEADER);

                foreach (string warning in report.Warnings)
                {
                    builder.AppendLine(string.Format(WARNING_FORMAT, warning));
                }
            }

            _reportText.text = builder.ToString();
        }

        private void ItemSelectedEventHandler(ScenarioAnalyzer analyzer)
        {
            ShowReport(analyzer);
        }

        private void ClickedEventHandler(LabeledButton button)
        {
            RestartRequestedEvent?.Invoke();
        }
    }
}
