using System.Collections;
using System.Text;
using LearningScenarioBuilder.Modes;
using LearningScenarioBuilder.Steps;
using TMPro;
using UnityEngine;

namespace LearningScenarioBuilder.UI
{
    /// <summary>
    /// Экран прохождения: режим и группа, текущие задания, последнее нарушение.
    /// </summary>
    public class ScenarioHud : MonoBehaviour
    {
        private const string TITLE_FORMAT = "{0} — {1}";
        private const string TASK_FORMAT = "• {0}";

        [SerializeField]
        private TMP_Text _titleText;

        [SerializeField]
        private TMP_Text _tasksText;

        [SerializeField]
        private TMP_Text _violationText;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Сколько секунд показывается сообщение о нарушении.")]
        private float _violationDisplayDuration = 3f;

        private ScenarioMode _mode;
        private Coroutine _hideViolationCoroutine;

        private void OnDestroy()
        {
            UnsubscribeMode();
        }

        public void Show(ScenarioMode mode)
        {
            UnsubscribeMode();
            _mode = mode;
            _mode.StateChangedEvent += StateChangedEventHandler;
            _mode.ViolationRegisteredEvent += ViolationRegisteredEventHandler;

            gameObject.SetActive(true);
            _violationText.text = string.Empty;
            Refresh();
        }

        public void Hide()
        {
            UnsubscribeMode();
            _mode = null;
            gameObject.SetActive(false);
        }

        private void UnsubscribeMode()
        {
            if (_mode != null)
            {
                _mode.StateChangedEvent -= StateChangedEventHandler;
                _mode.ViolationRegisteredEvent -= ViolationRegisteredEventHandler;
            }
        }

        private void Refresh()
        {
            string groupTitle = _mode.CurrentGroupTitle;
            _titleText.text = string.IsNullOrWhiteSpace(groupTitle)
                ? _mode.DisplayName
                : string.Format(TITLE_FORMAT, _mode.DisplayName, groupTitle);

            StringBuilder tasks = new StringBuilder();

            foreach (ScenarioStep step in _mode.GetActiveSteps())
            {
                tasks.AppendLine(string.Format(TASK_FORMAT, step.Title));
            }

            _tasksText.text = tasks.ToString();
        }

        private IEnumerator HideViolationCoroutine()
        {
            yield return new WaitForSeconds(_violationDisplayDuration);
            _violationText.text = string.Empty;
            _hideViolationCoroutine = null;
        }

        private void StateChangedEventHandler()
        {
            Refresh();
        }

        private void ViolationRegisteredEventHandler(string message)
        {
            _violationText.text = message;

            if (_hideViolationCoroutine != null)
            {
                StopCoroutine(_hideViolationCoroutine);
            }

            _hideViolationCoroutine = StartCoroutine(HideViolationCoroutine());
        }
    }
}
