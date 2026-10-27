using System.Collections.Generic;
using UnityEngine;

namespace LearningScenarioBuilder.Modes
{
    /// <summary>
    /// Раскладка шагов экзамена: группы по порядку, внутри группы — стадии.
    /// </summary>
    [CreateAssetMenu(menuName = "Learning Scenario Builder/Exam Mode Config")]
    public class ExamModeConfig : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Группы экзамена по порядку. Внутри группы — стадии, открываемые последовательно.")]
        private List<StepGroup> _groups = new List<StepGroup>();

        public IReadOnlyList<StepGroup> Groups => _groups;
    }
}
