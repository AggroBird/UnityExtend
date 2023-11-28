using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(NamedArrayElementAttribute))]
    internal class NamedArrayElementPropertyDrawer : PropertyDrawer
    {
        private static readonly LabelNameProviderCache<INamedArrayElementNameProvider> providerCache = new();
        private static readonly List<string> labelBuffer = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.LabelField(position, "NamedArrayElement attribute can only be used with int properties");
            }
            else if (providerCache.TryGetProvider(property, out INamedArrayElementNameProvider provider, out int index))
            {
                int selectedValue = property.intValue;

                labelBuffer.Clear();
                var namedArray = provider.GetNamedArray(index);
                int labelCount = namedArray.ElementCount;
                for (int i = 0; i < labelCount; i++)
                {
                    labelBuffer.Add(namedArray.GetElementName(i));
                }

                if ((uint)selectedValue >= (uint)labelCount)
                {
                    labelBuffer.Add($"{selectedValue} (missing)");
                    selectedValue = labelBuffer.Count - 1;
                }

                position = EditorGUI.PrefixLabel(position, label);
                int selection = EditorGUI.Popup(position, selectedValue, labelBuffer.ToArray());
                if (selection != selectedValue)
                {
                    property.intValue = selection;
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