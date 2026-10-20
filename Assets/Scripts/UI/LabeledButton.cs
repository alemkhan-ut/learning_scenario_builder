using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningScenarioBuilder.UI
{
    [RequireComponent(typeof(Button))]
    public class LabeledButton : MonoBehaviour
    {
        public event Action<LabeledButton> ClickedEvent;

        [SerializeField]
        private TMP_Text _label;

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

        public void SetLabel(string text)
        {
            _label.text = text;
        }

        private void ButtonClickHandler()
        {
            ClickedEvent?.Invoke(this);
        }
    }
}
