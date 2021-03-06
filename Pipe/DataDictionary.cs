﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe
{
    using Importer.Pipe.Parsers;

    public static class DataDictionary
    {

        private static readonly Dictionary<string, Dictionary<string, Dictionary<string, IValue>>> dictionary = new Dictionary<string, Dictionary<string, Dictionary<string, IValue>>>();

        public static Dictionary<string, Dictionary<string, IValue>> Get(string reference)
        {
            if (dictionary.TryGetValue(reference, out Dictionary<string, Dictionary<string, IValue>> dict))
            {
                return dict;
            }

            return new Dictionary<string, Dictionary<string, IValue>>();
        }

        public static Dictionary<string, IValue> Get(string reference, string keyValue)
        {
            var dict=Get(reference);
            return dict.TryGetValue(keyValue, out Dictionary<string, IValue> value) ? value : null;
        }

        public static IValue Get(string reference, string keyValue, string field)
        {
            var dict = Get(reference, keyValue);
            if (dict != null && dict.TryGetValue(field, out IValue value))
            {
                return value;
            }

            return null;
        }

        public static void Set(string reference, string keyField, Dictionary<string, IValue> record)
        {
            if (!dictionary.TryGetValue(reference, out Dictionary<string, Dictionary<string, IValue>> dict))
            {
                dict = new Dictionary<string, Dictionary<string, IValue>>();
                dictionary[reference] = dict;
            }

            if (record.TryGetValue(keyField, out IValue keyValue) && !keyValue.IsFailed)
            {
                dict[keyValue.ToString()] = record;
            }
        }

        public static IEnumerable<string> GetDictionaryNames()
        {
            return dictionary.Keys;
        }
    }
}
