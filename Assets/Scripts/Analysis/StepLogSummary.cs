namespace LearningScenarioBuilder.Analysis
{
    /// <summary>
    /// Сводка по одному шагу, собранная из журнала.
    /// </summary>
    public class StepLogSummary
    {
        public StepLogSummary(string stepId)
        {
            StepId = stepId;
        }

        public string StepId { get; }
        public int ViolationCount { get; private set; }

        /// <summary>Результат первой записи StepFinished; null, если шаг не завершён.</summary>
        public string Result { get; private set; }

        public void AddViolation()
        {
            ViolationCount++;
        }

        public void SetResultIfMissing(string result)
        {
            if (Result == null)
            {
                Result = result;
            }
        }
    }
}
