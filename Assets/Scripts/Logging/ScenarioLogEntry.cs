using System;
using System.Globalization;
using System.Text;

namespace LearningScenarioBuilder.Logging
{
    /// <summary>
    /// Запись журнала прохождения. Новые поля добавляются как необязательные — существующая обработка их не читает.
    /// </summary>
    [Serializable]
    public class ScenarioLogEntry
    {
        private const string TIME_FORMAT = "[{0:0.0} c]";
        private const char PART_SEPARATOR = ' ';

        public float Time;
        public string Type;
        public string StepId;
        public string Result;
        public string Message;

        public ScenarioLogEntry(float time, string type, string stepId, string result, string message)
        {
            Time = time;
            Type = type;
            StepId = stepId;
            Result = result;
            Message = message;
        }

        public string GetDisplayText()
        {
            StringBuilder builder = new StringBuilder(string.Format(CultureInfo.InvariantCulture, TIME_FORMAT, Time));
            AppendPart(builder, Type);
            AppendPart(builder, StepId);
            AppendPart(builder, Result);
            AppendPart(builder, Message);
            return builder.ToString();
        }

        private static void AppendPart(StringBuilder builder, string part)
        {
            if (!string.IsNullOrWhiteSpace(part))
            {
                builder.Append(PART_SEPARATOR).Append(part);
            }
        }
    }
}
