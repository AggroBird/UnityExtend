using System;
using System.Text;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    public static class FormattedTagUtility
    {
        // Format a tag according to tag formatting rules (e.g. 'Test Tag' => 'Test_Tag')
        public static string FormatTag(string str, int maxLength = 32)
        {
            if (maxLength <= 0) return "Empty";
            StringBuilder tagBuilder = new();
            bool allowNumber = false;
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                {
                    tagBuilder.Append(c);
                    allowNumber = true;
                }
                else if (c >= '0' && c <= '9')
                {
                    // Dont allow starting with a number (most programming languages dont support this)
                    if (allowNumber)
                    {
                        tagBuilder.Append(c);
                    }
                }
                else if (c == ' ' || c == '_')
                {
                    tagBuilder.Append('_');
                    allowNumber = true;
                }
                if (tagBuilder.Length == maxLength)
                {
                    break;
                }
            }
            return tagBuilder.Length == 0 ? "Empty" : tagBuilder.ToString();
        }

        // If the tag is already in use, append a followup number (Foo becomes Foo_1, Foo_2, etc.)
        public static string GenerateUniqueFormattedTag(string str, Func<string, bool> predicate, int maxLength = 32)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            str = FormatTag(str, maxLength);
            if (predicate(str))
            {
                int idx = 0;
                int lastUnderscore = str.LastIndexOf('_');
                if (lastUnderscore > 0 && lastUnderscore != str.Length - 1)
                {
                    bool validFollowUpNumber = true;
                    if (str[lastUnderscore + 1] != '0')
                    {
                        for (int i = lastUnderscore + 1; i < str.Length; i++)
                        {
                            if (str[i] < '0' || str[i] > '9')
                            {
                                validFollowUpNumber = false;
                                break;
                            }
                        }
                    }
                    if (validFollowUpNumber)
                    {
                        int.TryParse(str.Substring(lastUnderscore + 1), out idx);
                        str = str.Substring(0, lastUnderscore);
                    }
                }
                string result;
                do
                {
                    result = str + $"_{++idx}";
                }
                while (predicate(result));
                return result;
            }
            return str;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FormattedTagAttribute : PropertyAttribute
    {
        public int MaxLength { get; set; } = 32;
    }
}
