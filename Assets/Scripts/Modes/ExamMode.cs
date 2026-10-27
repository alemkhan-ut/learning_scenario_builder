using System;
using System.Collections.Generic;
using LearningScenarioBuilder.Steps;
using UnityEngine;

namespace LearningScenarioBuilder.Modes
{
    /// <summary>
    /// Экзамен: нарушение завершает шаг как Failed; при достижении порога нарушений оставшиеся шаги группы пропускаются.
    /// </summary>
    public class ExamMode : ScenarioMode
    {
        [SerializeField]
        [Tooltip("Конфиг с группами экзамена.")]
        private ExamModeConfig _config;

        [SerializeField]
        [Min(1)]
        [Tooltip("Сколько нарушений в группе приводит к пропуску оставшихся шагов группы.")]
        private int _violationThreshold = 2;

        protected override IReadOnlyList<StepGroup> GetGroups()
        {
            return _config != null ? _config.Groups : Array.Empty<StepGroup>();
        }

        protected override void ApplyViolation(ScenarioStep step)
        {
            FinishStep(step, StepStatus.Failed);

            if (CurrentGroupViolationCount >= _violationThreshold)
            {
                SkipCurrentGroup();
            }

            TryAdvance();
        }
    }
}
