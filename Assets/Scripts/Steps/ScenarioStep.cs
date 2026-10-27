using System;
using System.Collections.Generic;
using UnityEngine;

namespace LearningScenarioBuilder.Steps
{
    /// <summary>
    /// Шаг сценария: слушает источники действий и сообщает, выполнен он, нарушен или задет вне очереди.
    /// Новый тип шага — наследник с переопределением <see cref="TryEvaluate"/>.
    /// </summary>
    public class ScenarioStep : MonoBehaviour
    {
        public event Action<ScenarioStep, StepActionResult, string> ActionEvaluatedEvent;

        private const string WRONG_ACTION_MESSAGE = "Неверное действие: {0}";
        private const string WRONG_ORDER_MESSAGE = "Нарушен порядок действий";
        private const string OUT_OF_TURN_MESSAGE = "Действие вне очереди";

        private readonly HashSet<StepEventSource> _subscribedSources = new HashSet<StepEventSource>();
        private readonly HashSet<StepEventSource> _foreignSources = new HashSet<StepEventSource>();

        [SerializeField]
        [Tooltip("Уникальный идентификатор шага в журнале, например step_01.")]
        private string _id;

        [SerializeField]
        [Tooltip("Текст задания, который видит пользователь.")]
        private string _title;

        [SerializeField]
        [Tooltip("Действия для выполнения шага по порядку. Одно действие — простой шаг, несколько — «сначала A, затем B».")]
        private StepEventSource[] _successSequence = Array.Empty<StepEventSource>();

        [SerializeField]
        [Tooltip("Действия, которые считаются ошибкой, пока шаг активен. Если пусто — ошибка любое действие сценария, кроме действий шагов текущей стадии.")]
        private StepEventSource[] _violationSources = Array.Empty<StepEventSource>();

        [SerializeField]
        [Tooltip("Необязательно: объект, состояние которого проверяется при выполнении действия шага.")]
        private GameObject _requiredStateObject;

        [SerializeField]
        [Tooltip("Каким должен быть объект условия: включён (отмечено) или выключен.")]
        private bool _requiredActiveState = true;

        [SerializeField]
        [Tooltip("Сообщение, если условие по объекту не выполнено.")]
        private string _stateViolationMessage = "Не выполнено условие шага";

        private bool _isActive;
        private int _progress;

        public string Id => _id;
        public string Title => _title;
        public IReadOnlyList<StepEventSource> SuccessSequence => _successSequence;
        public IReadOnlyList<StepEventSource> ViolationSources => _violationSources;

        private void OnEnable()
        {
            SubscribeSources(_successSequence);
            SubscribeSources(_violationSources);
        }

        private void OnDisable()
        {
            foreach (StepEventSource source in _subscribedSources)
            {
                source.RaisedEvent -= SourceRaisedEventHandler;
            }

            _subscribedSources.Clear();
            _foreignSources.Clear();
        }

        /// <summary>
        /// Активирует шаг. <paramref name="foreignSources"/> — действия сценария, не относящиеся к текущей стадии:
        /// при пустом списке нарушений шаг считает ошибкой любое из них.
        /// </summary>
        public void Activate(IEnumerable<StepEventSource> foreignSources)
        {
            _isActive = true;
            _progress = 0;

            if (_violationSources.Length == 0)
            {
                _foreignSources.UnionWith(foreignSources);
                _foreignSources.ExceptWith(_subscribedSources);
                _foreignSources.Remove(null);
                SubscribeSources(_foreignSources);
            }
        }

        public void Deactivate()
        {
            _isActive = false;
            _progress = 0;

            foreach (StepEventSource source in _foreignSources)
            {
                source.RaisedEvent -= SourceRaisedEventHandler;
                _subscribedSources.Remove(source);
            }

            _foreignSources.Clear();
        }

        /// <summary>
        /// Оценивает действие активного шага. Возвращает false, если действие к шагу не относится.
        /// </summary>
        protected virtual bool TryEvaluate(StepEventSource source, out StepActionResult result, out string reason)
        {
            int sequenceIndex = Array.IndexOf(_successSequence, source);

            if (sequenceIndex == _progress)
            {
                if (!IsStateConditionMet())
                {
                    result = StepActionResult.Violated;
                    reason = _stateViolationMessage;
                    return true;
                }

                _progress++;
                result = _progress >= _successSequence.Length ? StepActionResult.Completed : StepActionResult.Progressed;
                reason = string.Empty;
                return true;
            }

            if (Array.IndexOf(_violationSources, source) >= 0 || _foreignSources.Contains(source))
            {
                result = StepActionResult.Violated;
                reason = string.Format(WRONG_ACTION_MESSAGE, source.DisplayName);
                return true;
            }

            if (sequenceIndex > _progress)
            {
                result = StepActionResult.Violated;
                reason = WRONG_ORDER_MESSAGE;
                return true;
            }

            result = default;
            reason = string.Empty;
            return false;
        }

        private bool IsStateConditionMet()
        {
            return _requiredStateObject == null || _requiredStateObject.activeSelf == _requiredActiveState;
        }

        private void SubscribeSources(IEnumerable<StepEventSource> sources)
        {
            foreach (StepEventSource source in sources)
            {
                if (source != null && _subscribedSources.Add(source))
                {
                    source.RaisedEvent += SourceRaisedEventHandler;
                }
            }
        }

        private void SourceRaisedEventHandler(StepEventSource source)
        {
            if (!_isActive)
            {
                ActionEvaluatedEvent?.Invoke(this, StepActionResult.OutOfTurn, OUT_OF_TURN_MESSAGE);
                return;
            }

            if (!TryEvaluate(source, out StepActionResult result, out string reason))
            {
                return;
            }

            if (result == StepActionResult.Violated)
            {
                _progress = 0;
            }

            ActionEvaluatedEvent?.Invoke(this, result, reason);
        }
    }
}
