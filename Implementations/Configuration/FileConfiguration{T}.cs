using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Importer.Configuration
{
    [DebuggerDisplay("{Name} - {Type}")]
    public class FileConfiguration<T> where T : ColumnInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }

        [JsonProperty("columns")]
        public List<T> Columns
        {
            get => this.columns;
            set
            {
                this.columns = value;
                this.columnNames = null;
            }
        }

        public List<ColumnName> GetColumnsWithFullNames()
        {
            return this.columnNames ?? (this.columnNames = this.Columns
                       .Select(x => new ColumnName(string.Concat(this.Name, ".", x.Name), x)).ToList());
        }

        private List<ColumnName> columnNames;
        private List<T> columns;

        public struct ColumnName
        {
            public ColumnName(string fullName, T column)
            {
                this.FullName = fullName;
                this.Column = column;
            }

            public string FullName { get; }

            public T Column { get; }
        }
    }
}
