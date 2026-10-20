using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LearningScenarioBuilder.UI
{
    /// <summary>
    /// Список кнопок «по кнопке на элемент»: создаёт кнопки из prefab-а и сообщает, какой элемент выбран.
    /// </summary>
    public class LabeledButtonList<T>
    {
        public event Action<T> ItemSelectedEvent;

        private readonly Dictionary<LabeledButton, T> _itemsByButton = new Dictionary<LabeledButton, T>();
        private readonly LabeledButton _buttonPrefab;
        private readonly Transform _buttonsRoot;

        public LabeledButtonList(LabeledButton buttonPrefab, Transform buttonsRoot)
        {
            _buttonPrefab = buttonPrefab;
            _buttonsRoot = buttonsRoot;
        }

        public void Show(IReadOnlyList<T> items, Func<T, string> getLabel)
        {
            Clear();

            foreach (T item in items)
            {
                if (item == null)
                {
                    continue;
                }

                LabeledButton button = Object.Instantiate(_buttonPrefab, _buttonsRoot);
                button.SetLabel(getLabel(item));
                button.ClickedEvent += ButtonClickedEventHandler;
                _itemsByButton.Add(button, item);
            }
        }

        public void Clear()
        {
            foreach (LabeledButton button in _itemsByButton.Keys)
            {
                button.ClickedEvent -= ButtonClickedEventHandler;
                Object.Destroy(button.gameObject);
            }

            _itemsByButton.Clear();
        }

        private void ButtonClickedEventHandler(LabeledButton button)
        {
            if (_itemsByButton.TryGetValue(button, out T item))
            {
                ItemSelectedEvent?.Invoke(item);
            }
        }
    }
}
