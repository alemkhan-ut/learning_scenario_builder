namespace LearningScenarioBuilder.Modes
{
    /// <summary>
    /// Реакция режима на действие шага, который ещё не активирован.
    /// </summary>
    public enum OutOfTurnPolicy
    {
        RegisterViolation,
        IgnoreAction
    }
}
