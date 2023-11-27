using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(StringLabel))]
    internal sealed class StringLabelPropertyDrawer : PropertyDrawer
    {
        private static readonly LabelNameProviderCache<IStringLabelNameProvider> providerCache = new();
        private static readonly List<string> labelBuffer = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (providerCache.TryGetProvider(property, out IStringLabelNameProvider provider, out int index))
            {
                SerializedProperty valueProperty = property.FindPropertyRelative((StringLabel def) => def.Value);

                string selectedValue = valueProperty.stringValue;

                int selectedIndex = -1;
                labelBuffer.Clear();
                int labelCount = provider.StringLabelCount;
                for (int i = 0; i < labelCount; i++)
                {
                    string name = provider.GetStringLabelName(i);
                    if (!labelBuffer.Contains(name))
                    {
                        labelBuffer.Add(name);
                        if (selectedIndex == -1 && name.Equals(selectedValue, System.StringComparison.Ordinal))
                        {
                            selectedIndex = i;
                        }
                    }
                }

                if (selectedIndex == -1 && !string.IsNullOrEmpty(selectedValue))
                {
                    labelBuffer.Add($"{selectedValue} (missing)");
                    selectedIndex = labelBuffer.Count - 1;
                }

                position = EditorGUI.PrefixLabel(position, label);
                int selection = EditorGUI.Popup(position, selectedIndex, labelBuffer.ToArray());
                if (selection != selectedIndex)
                {
                    valueProperty.stringValue = labelBuffer[selection];
                }
            }
            else
            {
                EditorGUI.LabelField(position, "Failed to find label name provider asset");
            }

            EditorGUI.EndProperty();
        }
    }
}