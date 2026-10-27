using System;
using System.Collections.Generic;
using LearningScenarioBuilder.Steps;
using UnityEngine;

namespace LearningScenarioBuilder.Modes
{
    /// <summary>
    /// Тренировка: все шаги в одной группе, нарушение не завершает шаг — пользователь пробует снова.
    /// </summary>
    public class TrainingMode : ScenarioMode
    {
        [SerializeField]
        [Tooltip("Конфиг с раскладкой шагов тренировки.")]
        private TrainingModeConfig _config;

        protected override IReadOnlyList<StepGroup> GetGroups()
        {
            return _config != null ? new StepGroup[] { _config.Group } : Array.Empty<StepGroup>();
        }

        protected override void ApplyViolation(ScenarioStep step)
        {
            // Шаг остаётся активным: его прогресс уже сброшен, остальные активные шаги не затрагиваются.
        }
    }
}
