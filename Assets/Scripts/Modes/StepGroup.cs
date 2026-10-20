using System;
using System.Collections.Generic;
using UnityEngine;

namespace LearningScenarioBuilder.Modes
{
    /// <summary>
    /// Группа шагов: стадии внутри неё открываются последовательно.
    /// </summary>
    [Serializable]
    public class StepGroup
    {
        [SerializeField]
        [Tooltip("Название группы — показывается пользователю и пишется в журнал.")]
        private string _title;

        [SerializeField]
        [Tooltip("Стадии группы по порядку.")]
        private List<StepStage> _stages = new List<StepStage>();

        public string Title => _title;
        public IReadOnlyList<StepStage> Stages => _stages;
    }
}
