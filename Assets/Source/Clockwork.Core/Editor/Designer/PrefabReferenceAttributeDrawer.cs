using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Clockwork.Collections;

#nullable enable

namespace Clockwork.Designer.Editor
{
    [CustomPropertyDrawer(typeof(PrefabReferenceAttribute))]
    public sealed class PrefabReferenceAttributeDrawer : PropertyDrawer
    {
        private static readonly string[] PrefabAssetDirs = { "Assets/Prefabs" };

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (!fieldInfo.FieldType.IsSubclassOf(typeof(Component)))
            {
                return new HelpBox("Invalid use of PrefabReferenceAttribute.", HelpBoxMessageType.Error);
            }

            PrefabData data = PrefabCache.Get(fieldInfo.FieldType);
            int index = data.Values.IndexOf((Component)property.objectReferenceValue);

            var dropdown = new PopupField<string>(
                property.displayName,
                data.Labels,
                Math.Max(0, index));

            dropdown.AddToClassList(PopupField<string>.alignedFieldUssClassName);

            dropdown.RegisterValueChangedCallback(_ =>
            {
                int index = dropdown.index;
                property.objectReferenceValue = data.Values[index];
                property.serializedObject.ApplyModifiedProperties();
            });

            dropdown.AddManipulator(new ContextualMenuManipulator(e =>
            {
                var value = property.objectReferenceValue;
                var status = value != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;

                e.menu.AppendAction("Open In Prefab Mode", _ =>
                {
                    AssetDatabase.OpenAsset(value);
                }, status);

                e.menu.AppendAction("Ping Prefab", _ =>
                {
                    EditorGUIUtility.PingObject(value);
                }, status);
            }));

            return dropdown;
        }

        private class PrefabData
        {
            public readonly List<Component?> Values;
            public readonly List<string> Labels;

            public PrefabData(List<Component?> values, List<string> labels)
            {
                Values = values;
                Labels = labels;
            }
        }

        private class PrefabCache : AssetPostprocessor
        {
            private static readonly HashMap<Type, PrefabData> s_data = new();

            public static PrefabData Get(Type prefabType)
            {
                if (!s_data.TryGetValue(prefabType, out PrefabData? result))
                {
                    s_data.Add(prefabType, result = Fetch(prefabType));
                }
                return result;
            }

            private static PrefabData Fetch(Type prefabType)
            {
                var values = new List<Component?> { null, null };
                var labels = new List<string> { "None", string.Empty };

                string[] guids = AssetDatabase.FindAssets("t:Prefab", PrefabAssetDirs);
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    if (prefab.TryGetComponent(prefabType, out Component component))
                    {
                        values.Add(component);
                        labels.Add(prefab.name);
                    }
                }

                return new PrefabData(values, labels);
            }

            private static void OnPostprocessAllAssets(
                string[] _,
                string[] __,
                string[] ___,
                string[] ____) => s_data.Clear();
        }
    }
}