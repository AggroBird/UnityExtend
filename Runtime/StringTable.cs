using System;
using System.Collections.Generic;
using UnityEngine;

namespace AggroBird.UnityExtend
{
    [Serializable]
    public sealed class StringTable<T> : ISerializationCallbackReceiver, IStringLabelList
    {
        [Serializable]
        private struct ListValue
        {
            [FormattedTag] public string key;
            public T value;
            [HideInInspector] public int order;
        }

        [SerializeField] private List<ListValue> list;
        private readonly Dictionary<string, (T value, int order)> dictionary = new();

        int IStringLabelList.StringLabelCount => list.Count;
        string IStringLabelList.GetStringLabelName(int index) => list[index].key;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            list ??= new();
            list.Clear();
            foreach (var kv in dictionary)
            {
                list.Add(new ListValue { key = kv.Key, value = kv.Value.value, order = kv.Value.order });
            }
            list.Sort((a, b) => a.order.CompareTo(b.order));
            dictionary.Clear();
        }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            dictionary.Clear();
            if (list != null && list.Count > 0)
            {
                int order = 1;
                foreach (var kv in list)
                {
                    int idx = 0;
                    string originalKey = kv.key;
                    if (dictionary.ContainsKey(originalKey))
                    {
                        int lastUnderscore = originalKey.LastIndexOf('_');
                        if (lastUnderscore > 0)
                        {
                            bool validFollowUpNumber = true;
                            for (int i = lastUnderscore + 1; i < originalKey.Length; i++)
                            {
                                if (originalKey[i] == '0' && i == 0)
                                {
                                    validFollowUpNumber = false;
                                    break;
                                }
                                if (originalKey[i] < '0' || originalKey[i] > '9')
                                {
                                    validFollowUpNumber = false;
                                    break;
                                }
                            }
                            if (validFollowUpNumber)
                            {
                                int.TryParse(originalKey.Substring(lastUnderscore + 1), out idx);
                                originalKey = originalKey.Substring(0, lastUnderscore);
                            }
                        }
                        if (string.IsNullOrEmpty(originalKey)) originalKey = "Empty";
                        string newKey = originalKey;
                        do
                        {
                            newKey = originalKey + $"_{++idx}";
                        }
                        while (dictionary.ContainsKey(newKey));
                        dictionary[newKey] = (kv.value, order++);
                    }
                    else
                    {
                        dictionary[originalKey] = (kv.value, order++);
                    }
                }
            }
        }

        public bool ContainsKey(string key) => dictionary.ContainsKey(key);
        public bool TryGetValue(string key, out T value)
        {
            if (dictionary.TryGetValue(key, out var kv))
            {
                value = kv.value;
                return true;
            }
            value = default;
            return false;
        }
    }
}