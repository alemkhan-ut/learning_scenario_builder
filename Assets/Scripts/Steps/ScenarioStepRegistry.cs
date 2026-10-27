using System;
using System.Collections.Generic;
using UnityEngine;

namespace LearningScenarioBuilder.Steps
{
    /// <summary>
    /// Реестр шагов сцены: по id из конфигов режимов находит компоненты шагов.
    /// </summary>
    public class ScenarioStepRegistry : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Все шаги сценария. Режимы находят здесь шаги по id из своих конфигов.")]
        private List<ScenarioStep> _steps = new List<ScenarioStep>();

        private void OnValidate()
        {
            HashSet<string> ids = new HashSet<string>();

            foreach (ScenarioStep step in _steps)
            {
                if (step == null)
                {
                    LogConfigurationWarning("есть пустая ссылка на шаг");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(step.Id))
                {
                    LogConfigurationWarning($"у шага «{step.name}» не задан id");
                    continue;
                }

                if (!ids.Add(step.Id))
                {
                    LogConfigurationWarning($"id «{step.Id}» используется несколькими шагами");
                }
            }
        }

        public bool TryGetStep(string id, out ScenarioStep step)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                foreach (ScenarioStep candidate in _steps)
                {
                    if (candidate != null && string.Equals(candidate.Id, id, StringComparison.Ordinal))
                    {
                        step = candidate;
                        return true;
                    }
                }
            }

            step = null;
            return false;
        }

        /// <summary>
        /// Все источники действий шагов реестра: и действия выполнения, и действия-нарушения.
        /// </summary>
        public HashSet<StepEventSource> GetActionSources()
        {
            HashSet<StepEventSource> sources = new HashSet<StepEventSource>();

            foreach (ScenarioStep step in _steps)
            {
                if (step != null)
                {
                    sources.UnionWith(step.SuccessSequence);
                    sources.UnionWith(step.ViolationSources);
                }
            }

            sources.Remove(null);
            return sources;
        }

        private void LogConfigurationWarning(string message)
        {
            Debug.LogWarning($"{nameof(ScenarioStepRegistry)} «{name}»: {message}", this);
        }
    }
}
