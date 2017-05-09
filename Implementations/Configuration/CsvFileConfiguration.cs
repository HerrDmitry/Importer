using System;
using Newtonsoft.Json;

namespace Importer.Configuration
{
    using System.Runtime.InteropServices;

    public class CsvFileConfiguration<T>:FileConfiguration<T> where T:ColumnInfo
    {
        public CsvFileConfiguration() {
            this.DelimiterChar = ',';
            this.TextQualifierChar = '\"';
            this.textQualifier = "\"";
            this.textQualifierDouble = "\"\"";
        }

        [JsonProperty("delimiter")]
        public string Delimiter
        {
            get => this.DelimiterChar.ToString();

            set => this.DelimiterChar = !string.IsNullOrWhiteSpace(value) ? value[0] : ',';
        }

        [JsonIgnore]
        public char DelimiterChar { get; set; }

        [JsonProperty("textQualifier")]
        public string TextQualifier
        {
            get => this.textQualifier;

            set
            {
                this.textQualifier = !string.IsNullOrEmpty(value) ? value[0].ToString() : "\"";
                this.textQualifierDouble = this.textQualifier + this.textQualifier;
                this.TextQualifierChar = this.textQualifier[0];
            } 
        }

        public string TextQualifierDouble => this.textQualifierDouble;

        [JsonIgnore]
        public char TextQualifierChar { get; private set; }

        [JsonProperty("footer")]
        public int? Footer { get; set; }

        [JsonProperty("header")]
        public int? Header { get; set; }

        [JsonProperty("readBuffer")]
        public int? ReadBuffer { get; set; }

        private string textQualifier;
        private string textQualifierDouble;
    }
}
