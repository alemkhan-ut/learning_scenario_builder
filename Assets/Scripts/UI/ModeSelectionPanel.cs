using System;
using System.Collections.Generic;
using LearningScenarioBuilder.Modes;
using UnityEngine;

namespace LearningScenarioBuilder.UI
{
    public class ModeSelectionPanel : MonoBehaviour
    {
        public event Action<ScenarioMode> ModeSelectedEvent;

        [SerializeField]
        private LabeledButton _buttonPrefab;

        [SerializeField]
        private Transform _buttonsRoot;

        private LabeledButtonList<ScenarioMode> _modeButtons;

        private void Awake()
        {
            _modeButtons = new LabeledButtonList<ScenarioMode>(_buttonPrefab, _buttonsRoot);
        }

        private void OnEnable()
        {
            _modeButtons.ItemSelectedEvent += ItemSelectedEventHandler;
        }

        private void OnDisable()
        {
            _modeButtons.ItemSelectedEvent -= ItemSelectedEventHandler;
        }

        public void Show(IReadOnlyList<ScenarioMode> modes)
        {
            gameObject.SetActive(true);
            _modeButtons.Show(modes, GetModeLabel);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private static string GetModeLabel(ScenarioMode mode)
        {
            return mode.DisplayName;
        }

        private void ItemSelectedEventHandler(ScenarioMode mode)
        {
            ModeSelectedEvent?.Invoke(mode);
        }
    }
}
