using System;
using UnityEngine;

namespace LearningScenarioBuilder.Steps
{
    /// <summary>
    /// Источник действия для шагов сценария. Сам по себе — источник «UnityEvent»:
    /// метод <see cref="Raise"/> вызывается из любого UnityEvent в Inspector.
    /// Новый тип источника — наследник, вызывающий <see cref="Raise"/>.
    /// </summary>
    public class StepEventSource : MonoBehaviour
    {
        public event Action<StepEventSource> RaisedEvent;

        [SerializeField]
        [Tooltip("Название действия в сообщениях. Если пусто — имя объекта.")]
        private string _displayName;

        public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? name : _displayName;

        public void Raise()
        {
            RaisedEvent?.Invoke(this);
        }
    }
}
