﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Importer.Configuration;
using Importer.Interfaces;
using Importer.Readers;

namespace Importer.Records
{
    public abstract class Record : IRecord
    {
        public IEnumerable<IParser> GetValues()
        {
            return this.GetValuesInternal().Values.ToList();
        }

        public IParser this[string columnName] => this.GetValuesInternal()[columnName];

        protected abstract ImmutableDictionary<string, IParser> GetValuesInternal();

        public abstract void InitializeRecord<TCI>(FileConfiguration<TCI> config, StringBuilder source) where TCI : ColumnInfo;
        public abstract void ClearRecord();

        public abstract void Release();
    }

    public abstract class Record<T> : Record where T : Record, new()
    {
        public static class Factory
        {
            private static ConcurrentBag<T> recordPool = new ConcurrentBag<T>();

            public static T GetRecord<TCI>(FileConfiguration<TCI> config, StringBuilder source) where TCI : ColumnInfo
            {
                if (!recordPool.TryTake(out T record))
                {
                    record = new T();
                }

                record?.InitializeRecord(config, source);
                return record;
            }

            public static void ReleaseRecord(T record)
            {
                record.ClearRecord();
                recordPool.Add(record);
            }
        }

        public override void Release()
        {
            Factory.ReleaseRecord(this as T);
        }
    }
}
