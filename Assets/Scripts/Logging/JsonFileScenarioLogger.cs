using System.IO;
using UnityEngine;

namespace LearningScenarioBuilder.Logging
{
    /// <summary>
    /// Журнал в памяти плюс файл в формате JSON Lines в Application.persistentDataPath.
    /// </summary>
    public class JsonFileScenarioLogger : ScenarioLogger
    {
        private const string LINE_SEPARATOR = "\n";

        [SerializeField]
        [Tooltip("Имя файла журнала в папке Application.persistentDataPath.")]
        private string _fileName = "scenario_log.jsonl";

        public string FilePath => Path.Combine(Application.persistentDataPath, _fileName);

        public override void Clear()
        {
            base.Clear();
            File.WriteAllText(FilePath, string.Empty);
        }

        public override void Write(ScenarioLogEntry entry)
        {
            base.Write(entry);
            File.AppendAllText(FilePath, JsonUtility.ToJson(entry) + LINE_SEPARATOR);
        }
    }
}
