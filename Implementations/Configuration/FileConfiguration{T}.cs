using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Importer.Configuration
{
    public class FileConfiguration
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }

        [JsonProperty("references")]
        public List<FileReference> References { get; set; }
    }

    [DebuggerDisplay("{Name} - {Type}")]
    public class FileConfiguration<T>:FileConfiguration where T : ColumnInfo
    {

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

        private List<T> columns;
        private List<ColumnName> columnNames;
    }
}
