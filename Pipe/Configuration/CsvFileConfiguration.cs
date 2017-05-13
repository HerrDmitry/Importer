using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Importer.Pipe.Configuration
{
    using Newtonsoft.Json;

    public class CsvFileConfiguration : FileConfiguration
    {
        [JsonProperty("columns")]
        public List<Column> Columns { get; set; }

        [JsonProperty("delimiter")]
        public string Delimiter { get; set; }

        [JsonProperty("textQualifier")]
        public string TextQualifier { get; set; }

        public override List<KeyValuePair<string,string>> GetReferences()
        {
            return Columns.Where(x => !string.IsNullOrWhiteSpace(x.Reference)).Select(x =>
            {
                var parts = x.Reference.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 1 ? new KeyValuePair<string,string>(parts[0],parts[1]) : new KeyValuePair<string, string>("","");
            }).Where(x => !string.IsNullOrWhiteSpace(x.Key)).Distinct().ToList();
        }

        protected override FileConfiguration NormalizeColumnNames()
        {
            var prefix = this.Name + ".";
            this.Columns?.ForEach(x =>
            {
                var name = x.Name.Replace(" ", "");
                if (!name.StartsWith(prefix))
                {
                    name = prefix + name;
                }
                if (x.Name != name)
                {
                    x.Name = name;
                }
            });

            return this;
        }
    }
}
 