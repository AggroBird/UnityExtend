using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AggroBird.UnityExtend.Editor
{
    [CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
    internal sealed class ConditionalFieldAttributeDrawer : PropertyDrawer
    {
        private static readonly List<object> values = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Evaluate(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
            else if (attribute is ConditionalFieldAttribute conditionalFieldAttribute && conditionalFieldAttribute.style == ConditionalFieldStyle.Disable)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool disable = attribute is ConditionalFieldAttribute conditionalFieldAttribute && conditionalFieldAttribute.style == ConditionalFieldStyle.Disable;
            return disable || Evaluate(property) ? EditorGUI.GetPropertyHeight(property, true) : 0;
        }

        private bool Evaluate(SerializedProperty property)
        {
            static bool IsSupportedType(TypeCode type)
            {
                switch (type)
                {
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        return true;
                    default:
                        return false;
                }
            }

            static int Compare(object lhs, object rhs)
            {
                // Check for null
                if (lhs == null)
                {
                    return rhs == null ? 0 : 1;
                }
                else if (rhs == null)
                {
                    return -1;
                }

                Type lhsType = lhs.GetType();
                Type rhsType = rhs.GetType();

                // Use IComparable
                if (lhsType.Equals(rhsType) && lhs is IComparable lhsComparable)
                {
                    return lhsComparable.CompareTo(rhs);
                }

                TypeCode lhsTypeCode = Type.GetTypeCode(lhsType);
                TypeCode rhsTypeCode = Type.GetTypeCode(rhsType);

                // Default implicit casts
                if (IsSupportedType(lhsTypeCode) && IsSupportedType(rhsTypeCode))
                {
                    static int DoCompare(object lhs, object rhs)
                    {
                        switch (lhs)
                        {
                            case int lhsCasted:
                                switch (rhs)
                                {
                                    case int rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                }
                                break;
                            case uint lhsCasted:
                                switch (rhs)
                                {
                                    case uint rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                }
                                break;
                            case long lhsCasted:
                                switch (rhs)
                                {
                                    case int rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                    case uint rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                    case long rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                }
                                break;
                            case ulong lhsCasted:
                                switch (rhs)
                                {
                                    case uint rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                    case ulong rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                }
                                break;
                            case float lhsCasted:
                                switch (rhs)
                                {
                                    case int rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                    case uint rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                    case long rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                    case ulong rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                    case float rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                }
                                break;
                            case double lhsCasted:
                                switch (rhs)
                                {
                                    case int rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                    case uint rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                    case long rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                    case ulong rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                    case float rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                    case double rhsCasted: return lhsCasted.CompareTo(rhsCasted);
                                }
                                break;
                        }
                        return 0;
                    }

                    if (lhsTypeCode < rhsTypeCode)
                    {
                        // If operand is the largest type, swap the comparison and invert the result
                        return -Mathf.Clamp(DoCompare(rhs, lhs), -1, 1);
                    }
                    else
                    {
                        return Mathf.Clamp(DoCompare(lhs, rhs), -1, 1);
                    }
                }

                throw new Exception($"Failed to perform implicit conditional field comparison between types '{lhs.GetType()}' and '{rhs.GetType()}'");
            }

            try
            {
                if (attribute is ConditionalFieldAttribute conditionalFieldAttribute)
                {
                    if (EditorExtendUtility.TryGetFieldInfo(property, out _, out _, values))
                    {
                        object obj = values[^1];

                        static bool TryFindField(string fieldName, object obj, out object value)
                        {
                            Type objType = obj.GetType();
                            while (objType != typeof(object))
                            {
                                FieldInfo field = objType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                if (field != null)
                                {
                                    value = field.GetValue(obj);
                                    return true;
                                }

                                PropertyInfo property = objType.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                if (property != null && property.CanRead)
                                {
                                    value = property.GetValue(obj);
                                    return true;
                                }

                                objType = objType.BaseType;
                            }

                            value = null;
                            return false;
                        }

                        if (TryFindField(conditionalFieldAttribute.fieldName, obj, out object lhsValue))
                        {
                            object rhsValue = conditionalFieldAttribute.operand;
                            switch (conditionalFieldAttribute.op)
                            {
                                case ConditionalFieldOperator.Equal:
                                    return Equals(lhsValue, rhsValue);
                                case ConditionalFieldOperator.NotEqual:
                                    return !Equals(lhsValue, rhsValue);
                                case ConditionalFieldOperator.LessThan:
                                    return Compare(lhsValue, rhsValue) < 0;
                                case ConditionalFieldOperator.LessEqual:
                                    return Compare(lhsValue, rhsValue) <= 0;
                                case ConditionalFieldOperator.GreaterThan:
                                    return Compare(lhsValue, rhsValue) > 0;
                                case ConditionalFieldOperator.GreaterEqual:
                                    return Compare(lhsValue, rhsValue) >= 0;
                            }
                        }
                        else
                        {
                            throw new Exception($"Failed to find conditional field {conditionalFieldAttribute.fieldName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return true;
        }
    }
}