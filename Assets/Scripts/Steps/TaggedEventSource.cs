using LearningScenarioBuilder.Attributes;
using UnityEngine;

namespace LearningScenarioBuilder.Steps
{
    /// <summary>
    /// Общая часть физических источников: срабатывание на объект с выбранным тегом (тег Anyone — на любой объект).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class TaggedEventSource : StepEventSource
    {
        private const string ANY_TAG = "Anyone";

        [SerializeField]
        [TagSelector]
        [Tooltip("Тег объекта, который вызывает действие. Anyone — любой объект.")]
        private string _requiredTag = ANY_TAG;

        protected void RaiseIfTagMatches(GameObject other)
        {
            if (_requiredTag == ANY_TAG || other.CompareTag(_requiredTag))
            {
                Raise();
            }
        }
    }
}
