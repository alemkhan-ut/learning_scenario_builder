using System;
using System.Collections.Generic;
using LearningScenarioBuilder.Analysis;
using LearningScenarioBuilder.Logging;
using LearningScenarioBuilder.Modes;
using LearningScenarioBuilder.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningScenarioBuilder.Demo
{
    /// <summary>
    /// Связывает демо: выбор режима → прохождение → результаты → перезапуск сцены.
    /// </summary>
    public class ScenarioDemo : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Режимы, доступные в меню.")]
        private List<ScenarioMode> _modes = new List<ScenarioMode>();

        [SerializeField]
        [Tooltip("Анализаторы, которыми можно переоценить журнал на экране результатов.")]
        private List<ScenarioAnalyzer> _analyzers = new List<ScenarioAnalyzer>();

        [SerializeField]
        private ModeSelectionPanel _modeSelectionPanel;

        [SerializeField]
        private ScenarioHud _hud;

        [SerializeField]
        private ResultsPanel _resultsPanel;

        private ScenarioMode _activeMode;

        private void OnEnable()
        {
            _modeSelectionPanel.ModeSelectedEvent += ModeSelectedEventHandler;
            _resultsPanel.RestartRequestedEvent += RestartRequestedEventHandler;
        }

        private void OnDisable()
        {
            _modeSelectionPanel.ModeSelectedEvent -= ModeSelectedEventHandler;
            _resultsPanel.RestartRequestedEvent -= RestartRequestedEventHandler;

            if (_activeMode != null)
            {
                _activeMode.FinishedEvent -= FinishedEventHandler;
            }
        }

        private void Start()
        {
            _hud.Hide();
            _resultsPanel.Hide();
            _modeSelectionPanel.Show(_modes);
        }

        private void ModeSelectedEventHandler(ScenarioMode mode)
        {
            _modeSelectionPanel.Hide();
            _activeMode = mode;
            _activeMode.FinishedEvent += FinishedEventHandler;
            _hud.Show(_activeMode);
            _activeMode.StartScenario();
        }

        private void FinishedEventHandler()
        {
            _activeMode.FinishedEvent -= FinishedEventHandler;
            _hud.Hide();

            IReadOnlyList<ScenarioLogEntry> entries = _activeMode.Logger != null
                ? _activeMode.Logger.GetEntries()
                : Array.Empty<ScenarioLogEntry>();
            _resultsPanel.Show(entries, _activeMode.Analyzer, _analyzers);
        }

        private void RestartRequestedEventHandler()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
