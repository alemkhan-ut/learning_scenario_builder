using System;
using System.Collections.Generic;
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
        [Tooltip("Id шагов (поле Id компонента Scenario Step), активных одновременно. Следующая стадия откроется, когда завершатся все.")]
        private List<string> _stepIds = new List<string>();

        public IReadOnlyList<string> StepIds => _stepIds;
    }
}
