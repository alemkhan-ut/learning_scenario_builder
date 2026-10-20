using System;
using System.Collections.Generic;
using LearningScenarioBuilder.Steps;
using UnityEngine;

namespace LearningScenarioBuilder.Modes
{
    /// <summary>
    /// Стадия — шаги, которые активируются одновременно и выполняются в любом порядке.
    /// </summary>
    [Serializable]
    public class StepStage
    {
        [SerializeField]
        [Tooltip("Шаги, активные одновременно. Следующая стадия откроется, когда завершатся все.")]
        private List<ScenarioStep> _steps = new List<ScenarioStep>();

        public IReadOnlyList<ScenarioStep> Steps => _steps;
    }
}
