using UnityEngine;

namespace LearningScenarioBuilder.Modes
{
    /// <summary>
    /// Раскладка шагов тренировки: одна общая группа, стадии открываются по очереди.
    /// </summary>
    [CreateAssetMenu(menuName = "Learning Scenario Builder/Training Mode Config")]
    public class TrainingModeConfig : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Общая группа всех шагов: стадии открываются по очереди, шаги стадии — одновременно.")]
        private StepGroup _group = new StepGroup();

        public StepGroup Group => _group;
    }
}
