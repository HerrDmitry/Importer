using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Importer.Configuration;
using Importer.Interfaces;
using Importer.Parsers;

namespace Importer.Records
{
    public abstract class Record : IRecord
    {
        public Dictionary<string,IParser> GetValues()
        {
            return this.GetValuesInternal();
        }

        public IParser this[string columnName]
        {
            get
            {
                var value =  this.GetValuesInternal().TryGetValue(columnName, out IParser parser) ? parser : null;
                if (value == null)
                {
                    this.references?.TryGetValue(columnName, out value);
                }

                return value ?? new NotFoundParser();
            }
        }

        protected Dictionary<string, IParser> references = null;

        protected abstract Dictionary<string, IParser> GetValuesInternal();

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
