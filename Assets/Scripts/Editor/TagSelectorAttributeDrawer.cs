using LearningScenarioBuilder.Attributes;
using UnityEditor;
using UnityEngine;

namespace LearningScenarioBuilder.EditorTools
{
    [CustomPropertyDrawer(typeof(TagSelectorAttribute))]
    public class TagSelectorAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
            EditorGUI.EndProperty();
        }
    }
}
