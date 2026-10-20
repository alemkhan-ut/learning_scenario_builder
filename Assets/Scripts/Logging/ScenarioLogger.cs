using System.Collections.Generic;
using UnityEngine;

namespace LearningScenarioBuilder.Logging
{
    /// <summary>
    /// Журнал прохождения в памяти. Новый логгер — наследник с переопределением <see cref="Write"/> и <see cref="Clear"/>.
    /// </summary>
    public class ScenarioLogger : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Записи текущего прохождения (заполняются во время игры).")]
        private List<ScenarioLogEntry> _entries = new List<ScenarioLogEntry>();

        public virtual void Clear()
        {
            _entries.Clear();
        }

        public virtual void Write(ScenarioLogEntry entry)
        {
            _entries.Add(entry);
        }

        public IReadOnlyList<ScenarioLogEntry> GetEntries()
        {
            return _entries;
        }
    }
}
