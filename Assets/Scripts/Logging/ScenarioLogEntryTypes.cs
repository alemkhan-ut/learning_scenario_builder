namespace LearningScenarioBuilder.Logging
{
    /// <summary>
    /// Типы записей журнала. Неизвестные типы потребители игнорируют.
    /// </summary>
    public static class ScenarioLogEntryTypes
    {
        /// <summary>Message — имя режима.</summary>
        public const string SCENARIO_STARTED = "ScenarioStarted";

        /// <summary>StepId — шаг, участвующий в прохождении; Message — текст задания.</summary>
        public const string STEP_REGISTERED = "StepRegistered";

        /// <summary>Message — название группы.</summary>
        public const string GROUP_STARTED = "GroupStarted";

        public const string STEP_ACTIVATED = "StepActivated";

        /// <summary>Message — причина нарушения.</summary>
        public const string STEP_VIOLATION = "StepViolation";

        /// <summary>Result — Completed, Failed или Skipped.</summary>
        public const string STEP_FINISHED = "StepFinished";

        public const string SCENARIO_FINISHED = "ScenarioFinished";
    }
}
