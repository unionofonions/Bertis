using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

#nullable enable

namespace Clockwork.Collections.Editor
{
    [CustomPropertyDrawer(typeof(UniqueSampler<>), useForChildren: true)]
    public class UniqueSamplerDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var itemsProp = property.FindPropertyRelative("_items");
            var uniqueSamplesProp = property.FindPropertyRelative("_uniqueSamples");

            var rootElement = new VisualElement();
            var itemsField = new PropertyField(itemsProp, property.displayName);
            var uniqueSamplesField = new PropertyField(uniqueSamplesProp)
            {
                style = { marginLeft = 15f }
            };

            itemsField.RegisterCallback<GeometryChangedEvent>(_ => UpdateUniqueSamplesFieldVisibility());
            itemsField.RegisterValueChangeCallback(_ => UpdateUniqueSamplesFieldValue());
            uniqueSamplesField.RegisterValueChangeCallback(_ => UpdateUniqueSamplesFieldValue());

            rootElement.Add(itemsField);
            rootElement.Add(uniqueSamplesField);
            return rootElement;

            void UpdateUniqueSamplesFieldValue()
            {
                uniqueSamplesProp.intValue = Math.Max(0, Math.Min(
                    uniqueSamplesProp.intValue, itemsProp.arraySize - 1));

                uniqueSamplesProp.serializedObject.ApplyModifiedProperties();
            }

            void UpdateUniqueSamplesFieldVisibility()
            {
                bool visible = itemsProp is { isExpanded: true, arraySize: > 1 };
                uniqueSamplesField.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}