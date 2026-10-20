using UnityEngine;
using UnityEngine.UI;

namespace LearningScenarioBuilder.Steps
{
    [RequireComponent(typeof(Button))]
    public class ButtonEventSource : StepEventSource
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(ButtonClickHandler);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(ButtonClickHandler);
        }

        private void ButtonClickHandler()
        {
            Raise();
        }
    }
}
