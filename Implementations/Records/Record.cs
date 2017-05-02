using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Importer.Interfaces;

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
    }
}
