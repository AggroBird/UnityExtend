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
                EditorGUI.PropertyField(position, property, label);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Evaluate(property) ? EditorGUI.GetPropertyHeight(property) : 0;
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
                        Type objType = obj.GetType();
                        object lhsValue;
                        FieldInfo compareField = objType.GetField(conditionalFieldAttribute.fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (compareField != null)
                        {
                            lhsValue = compareField.GetValue(obj);
                        }
                        else
                        {
                            PropertyInfo compareProperty = objType.GetProperty(conditionalFieldAttribute.fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (compareProperty.CanRead)
                            {
                                lhsValue = compareProperty.GetValue(obj);
                            }
                            else
                            {
                                throw new Exception($"Failed to find conditional field {conditionalFieldAttribute.fieldName}");
                            }
                        }

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