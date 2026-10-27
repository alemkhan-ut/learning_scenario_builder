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
        [Tooltip("Реестр шагов сцены: по нему находятся шаги по id из конфига режима.")]
        private ScenarioStepRegistry _stepRegistry;

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

            if (_stepRegistry == null)
            {
                LogConfigurationWarning("не назначен реестр шагов");
            }

            if (_logger == null)
            {
                LogConfigurationWarning("не назначен логгер");
            }

            if (_analyzer == null)
            {
                LogConfigurationWarning("не назначен анализатор");
            }

            if (GetGroups().Count == 0)
            {
                LogConfigurationWarning("не назначен конфиг или в нём нет групп");
            }

            HashSet<string> seenStepIds = new HashSet<string>();

            foreach (StepGroup group in GetGroups())
            {
                if (group.Stages.Count == 0)
                {
                    LogConfigurationWarning($"группа «{group.Title}» пуста");
                }

                foreach (StepStage stage in group.Stages)
                {
                    ValidateStage(group, stage, seenStepIds);
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
                if (GetStatus(step) == StepStatus.Active)
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
            WriteEntry(ScenarioLogEntryTypes.STEP_FINISHED, step.Id, status.ToString(), step.Title);
        }

        protected void SkipCurrentGroup()
        {
            foreach (StepStage stage in CurrentGroup.Stages)
            {
                foreach (ScenarioStep step in GetStageSteps(stage))
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
                    foreach (ScenarioStep step in GetStageSteps(stage))
                    {
                        if (_statuses.ContainsKey(step))
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
            List<ScenarioStep> stageSteps = GetCurrentStageSteps();

            if (stageSteps.Count == 0)
            {
                return;
            }

            HashSet<StepEventSource> foreignSources = GetForeignSources(stageSteps);

            foreach (ScenarioStep step in stageSteps)
            {
                if (GetStatus(step) == StepStatus.Pending)
                {
                    _statuses[step] = StepStatus.Active;
                    step.Activate(foreignSources);
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

        private List<ScenarioStep> GetCurrentStageSteps()
        {
            IReadOnlyList<StepStage> stages = CurrentGroup.Stages;
            return _stageIndex < stages.Count ? GetStageSteps(stages[_stageIndex]) : new List<ScenarioStep>();
        }

        /// <summary>
        /// Шаги стадии по id из конфига; id, которых нет в реестре, пропускаются (предупреждение — в OnValidate).
        /// </summary>
        private List<ScenarioStep> GetStageSteps(StepStage stage)
        {
            List<ScenarioStep> steps = new List<ScenarioStep>();

            if (_stepRegistry == null)
            {
                return steps;
            }

            foreach (string stepId in stage.StepIds)
            {
                if (_stepRegistry.TryGetStep(stepId, out ScenarioStep step))
                {
                    steps.Add(step);
                }
            }

            return steps;
        }

        /// <summary>
        /// Действия сценария, не выполняющие ни один шаг стадии: шаги одной стадии не мешают друг другу.
        /// </summary>
        private HashSet<StepEventSource> GetForeignSources(List<ScenarioStep> stageSteps)
        {
            HashSet<StepEventSource> sources = _stepRegistry.GetActionSources();

            foreach (ScenarioStep step in stageSteps)
            {
                sources.ExceptWith(step.SuccessSequence);
            }

            return sources;
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

        private void ValidateStage(StepGroup group, StepStage stage, HashSet<string> seenStepIds)
        {
            if (stage.StepIds.Count == 0)
            {
                LogConfigurationWarning($"в группе «{group.Title}» есть пустая стадия");
            }

            foreach (string stepId in stage.StepIds)
            {
                if (string.IsNullOrWhiteSpace(stepId))
                {
                    LogConfigurationWarning($"в группе «{group.Title}» есть пустой id шага");
                    continue;
                }

                if (_stepRegistry != null && !_stepRegistry.TryGetStep(stepId, out ScenarioStep _))
                {
                    LogConfigurationWarning($"шаг «{stepId}» из группы «{group.Title}» не найден в реестре шагов");
                }

                if (!seenStepIds.Add(stepId))
                {
                    LogConfigurationWarning($"шаг «{stepId}» встречается в режиме дважды");
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
