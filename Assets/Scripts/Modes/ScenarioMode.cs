using System;
using System.Collections.Generic;
using LearningScenarioBuilder.Analysis;
using LearningScenarioBuilder.Logging;
using LearningScenarioBuilder.Steps;
using UnityEngine;

namespace LearningScenarioBuilder.Modes
{
    /// <summary>
    /// Режим прохождения: открывает стадии своей раскладки «группы → стадии → шаги» по очереди,
    /// обрабатывает действия шагов и пишет всё в журнал. Наследник задаёт раскладку и реакцию на нарушение.
    /// </summary>
    public abstract class ScenarioMode : MonoBehaviour
    {
        public event Action StateChangedEvent;
        public event Action<string> ViolationRegisteredEvent;
        public event Action FinishedEvent;

        private const string VIOLATION_TEXT_FORMAT = "{0}: {1}";

        private readonly Dictionary<ScenarioStep, StepStatus> _statuses = new Dictionary<ScenarioStep, StepStatus>();

        [SerializeField]
        [Tooltip("Название режима для пользователя и журнала. Если пусто — имя объекта.")]
        private string _displayName;

        [SerializeField]
        [Tooltip("Действие шага, который ещё не активирован: нарушение или игнорирование.")]
        private OutOfTurnPolicy _outOfTurnPolicy = OutOfTurnPolicy.RegisterViolation;

        [SerializeField]
        [Tooltip("Логгер, в который режим пишет журнал прохождения.")]
        private ScenarioLogger _logger;

        [SerializeField]
        [Tooltip("Анализатор, которым по умолчанию оценивается журнал этого режима.")]
        private ScenarioAnalyzer _analyzer;

        private float _startTime;
        private int _groupIndex;
        private int _stageIndex;
        private bool _isRunning;

        public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? name : _displayName;
        public ScenarioLogger Logger => _logger;
        public ScenarioAnalyzer Analyzer => _analyzer;
        public string CurrentGroupTitle => _isRunning ? CurrentGroup.Title : string.Empty;

        protected int CurrentGroupViolationCount { get; private set; }

        private StepGroup CurrentGroup => GetGroups()[_groupIndex];

        private void OnValidate()
        {
            // Prefab-ассет режима не знает объектов сцены — проверяется только экземпляр в сцене.
            if (!gameObject.scene.IsValid())
            {
                return;
            }

            if (_logger == null)
            {
                LogConfigurationWarning("не назначен логгер");
            }

            if (_analyzer == null)
            {
                LogConfigurationWarning("не назначен анализатор");
            }

            HashSet<ScenarioStep> seenSteps = new HashSet<ScenarioStep>();

            foreach (StepGroup group in GetGroups())
            {
                if (group.Stages.Count == 0)
                {
                    LogConfigurationWarning($"группа «{group.Title}» пуста");
                }

                foreach (StepStage stage in group.Stages)
                {
                    ValidateStage(group, stage, seenSteps);
                }
            }
        }

        private void OnDestroy()
        {
            UnsubscribeSteps();
        }

        public List<ScenarioStep> GetActiveSteps()
        {
            List<ScenarioStep> activeSteps = new List<ScenarioStep>();

            if (!_isRunning)
            {
                return activeSteps;
            }

            foreach (ScenarioStep step in GetCurrentStageSteps())
            {
                if (step != null && GetStatus(step) == StepStatus.Active)
                {
                    activeSteps.Add(step);
                }
            }

            return activeSteps;
        }

        public void StartScenario()
        {
            if (_logger != null)
            {
                _logger.Clear();
            }

            _startTime = Time.time;
            _isRunning = true;
            WriteEntry(ScenarioLogEntryTypes.SCENARIO_STARTED, message: DisplayName);
            RegisterSteps();

            _groupIndex = -1;
            StartNextGroup();
            TryAdvance();
        }

        protected abstract IReadOnlyList<StepGroup> GetGroups();

        protected abstract void ApplyViolation(ScenarioStep step);

        protected void FinishStep(ScenarioStep step, StepStatus status)
        {
            _statuses[step] = status;
            step.Deactivate();
            WriteEntry(ScenarioLogEntryTypes.STEP_FINISHED, step.Id, status.ToString());
        }

        protected void SkipCurrentGroup()
        {
            foreach (StepStage stage in CurrentGroup.Stages)
            {
                foreach (ScenarioStep step in stage.Steps)
                {
                    if (IsUnfinished(step))
                    {
                        FinishStep(step, StepStatus.Skipped);
                    }
                }
            }
        }

        protected void TryAdvance()
        {
            while (_isRunning && IsCurrentStageFinished())
            {
                MoveToNextStage();
            }

            NotifyStateChanged();
        }

        private void RegisterSteps()
        {
            UnsubscribeSteps();
            _statuses.Clear();

            foreach (StepGroup group in GetGroups())
            {
                foreach (StepStage stage in group.Stages)
                {
                    foreach (ScenarioStep step in stage.Steps)
                    {
                        if (step == null || _statuses.ContainsKey(step))
                        {
                            continue;
                        }

                        _statuses.Add(step, StepStatus.Pending);
                        step.ActionEvaluatedEvent += StepActionEvaluatedEventHandler;
                        WriteEntry(ScenarioLogEntryTypes.STEP_REGISTERED, step.Id, message: step.Title);
                    }
                }
            }
        }

        private void UnsubscribeSteps()
        {
            foreach (ScenarioStep step in _statuses.Keys)
            {
                if (step != null)
                {
                    step.ActionEvaluatedEvent -= StepActionEvaluatedEventHandler;
                }
            }
        }

        private void StartNextGroup()
        {
            _groupIndex++;

            if (_groupIndex >= GetGroups().Count)
            {
                FinishScenario();
                return;
            }

            _stageIndex = 0;
            CurrentGroupViolationCount = 0;
            WriteEntry(ScenarioLogEntryTypes.GROUP_STARTED, message: CurrentGroup.Title);
            ActivateCurrentStage();
        }

        private void MoveToNextStage()
        {
            _stageIndex++;

            if (_stageIndex >= CurrentGroup.Stages.Count)
            {
                StartNextGroup();
                return;
            }

            ActivateCurrentStage();
        }

        private void ActivateCurrentStage()
        {
            foreach (ScenarioStep step in GetCurrentStageSteps())
            {
                if (step != null && GetStatus(step) == StepStatus.Pending)
                {
                    _statuses[step] = StepStatus.Active;
                    step.Activate();
                    WriteEntry(ScenarioLogEntryTypes.STEP_ACTIVATED, step.Id);
                }
            }
        }

        private void FinishScenario()
        {
            _isRunning = false;

            foreach (ScenarioStep step in _statuses.Keys)
            {
                if (step != null)
                {
                    step.Deactivate();
                }
            }

            UnsubscribeSteps();
            WriteEntry(ScenarioLogEntryTypes.SCENARIO_FINISHED);
            FinishedEvent?.Invoke();
        }

        private void RegisterViolation(ScenarioStep step, string reason)
        {
            CurrentGroupViolationCount++;
            WriteEntry(ScenarioLogEntryTypes.STEP_VIOLATION, step.Id, message: reason);
            ViolationRegisteredEvent?.Invoke(string.Format(VIOLATION_TEXT_FORMAT, step.Title, reason));
            ApplyViolation(step);
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            if (_isRunning)
            {
                StateChangedEvent?.Invoke();
            }
        }

        private IReadOnlyList<ScenarioStep> GetCurrentStageSteps()
        {
            IReadOnlyList<StepStage> stages = CurrentGroup.Stages;
            return _stageIndex < stages.Count ? stages[_stageIndex].Steps : Array.Empty<ScenarioStep>();
        }

        private bool IsCurrentStageFinished()
        {
            foreach (ScenarioStep step in GetCurrentStageSteps())
            {
                if (IsUnfinished(step))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsUnfinished(ScenarioStep step)
        {
            if (step == null)
            {
                return false;
            }

            StepStatus status = GetStatus(step);
            return status == StepStatus.Pending || status == StepStatus.Active;
        }

        private StepStatus GetStatus(ScenarioStep step)
        {
            return _statuses.TryGetValue(step, out StepStatus status) ? status : StepStatus.Pending;
        }

        private void WriteEntry(string type, string stepId = "", string result = "", string message = "")
        {
            if (_logger != null)
            {
                _logger.Write(new ScenarioLogEntry(Time.time - _startTime, type, stepId, result, message));
            }
        }

        private void ValidateStage(StepGroup group, StepStage stage, HashSet<ScenarioStep> seenSteps)
        {
            if (stage.Steps.Count == 0)
            {
                LogConfigurationWarning($"в группе «{group.Title}» есть пустая стадия");
            }

            foreach (ScenarioStep step in stage.Steps)
            {
                if (step == null)
                {
                    LogConfigurationWarning($"в группе «{group.Title}» есть пустая ссылка на шаг");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(step.Id))
                {
                    LogConfigurationWarning($"у шага «{step.name}» не задан id");
                }

                if (!seenSteps.Add(step))
                {
                    LogConfigurationWarning($"шаг «{step.name}» встречается в режиме дважды");
                }
            }
        }

        private void LogConfigurationWarning(string message)
        {
            Debug.LogWarning($"{nameof(ScenarioMode)} «{DisplayName}»: {message}", this);
        }

        private void StepActionEvaluatedEventHandler(ScenarioStep step, StepActionResult result, string reason)
        {
            if (!_isRunning)
            {
                return;
            }

            StepStatus status = GetStatus(step);

            if (result == StepActionResult.Completed && status == StepStatus.Active)
            {
                FinishStep(step, StepStatus.Completed);
                TryAdvance();
                return;
            }

            bool isActiveViolation = result == StepActionResult.Violated && status == StepStatus.Active;
            bool isOutOfTurnViolation = result == StepActionResult.OutOfTurn
                && status == StepStatus.Pending
                && _outOfTurnPolicy == OutOfTurnPolicy.RegisterViolation;

            if (isActiveViolation || isOutOfTurnViolation)
            {
                RegisterViolation(step, reason);
            }
        }
    }
}
