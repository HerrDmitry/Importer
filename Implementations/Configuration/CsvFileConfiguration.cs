using System;
using Newtonsoft.Json;

namespace Importer.Configuration
{
    public class CsvFileConfiguration<T>:FileConfiguration<T> where T:ColumnInfo
    {
        [JsonProperty("delimiter")]
        public string Delimiter
        {
            get
            {
                return this.DelimiterChar.ToString();
            }

            set
            {
                this.DelimiterChar = !string.IsNullOrWhiteSpace(value) ? value[0] : ',';
            }
        }

        [JsonIgnore]
        public char DelimiterChar { get; set; }
        [JsonProperty("textQualifier")]
        public string TextQualifier
        {
            get
            {
                return this.TextQualifierChar.ToString();
            }

            set
            {
                this.TextQualifierChar = !string.IsNullOrEmpty(value) ? value[0] : '"';
            }
        }

        [JsonIgnore]
        public char TextQualifierChar { get; private set; }

        [JsonProperty("footer")]
        public int? Footer { get; set; }

        [JsonProperty("header")]
        public int? Header { get; set; }
    }
}
