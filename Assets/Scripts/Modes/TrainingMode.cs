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
        [Tooltip("Общая группа всех шагов: стадии открываются по очереди, шаги стадии — одновременно.")]
        private StepGroup _group = new StepGroup();

        protected override IReadOnlyList<StepGroup> GetGroups()
        {
            return new StepGroup[] { _group };
        }

        protected override void ApplyViolation(ScenarioStep step)
        {
            // Шаг остаётся активным: его прогресс уже сброшен, остальные активные шаги не затрагиваются.
        }
    }
}
