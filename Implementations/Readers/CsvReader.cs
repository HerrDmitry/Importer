using System.Collections.Generic;
using System.Linq;
using System.Text;
using Importer.Records;
using Importer.Configuration;
using Importer.Interfaces;
using Newtonsoft.Json.Linq;

namespace Importer.Readers
{
    using System.Collections.Concurrent;

    public class CsvReader : IReader
    {
        protected CsvReader()
        {
        }

        public CsvReader(JObject configuration)
        {
            this.configuration = Configuration.Configuration.ParseConfiguration<CsvReaderConfiguration>(configuration);
        }

        protected IEnumerable<StringBuilder> ReadLines()
        {
            long counter = 0;
            var qualifier = this.configuration.TextQualifierChar;
            using (var readerEnumerator = this.reader.GetLines().GetEnumerator())
            {
                var sourceLine = new StringBuilder();
                var qualifierCount = 0;
                while (readerEnumerator.MoveNext())
                {
                    if (sourceLine.Length > 0)
                    {
                        sourceLine.AppendLine();
                    }

                    var line = readerEnumerator.Current;
                    sourceLine.Append(line);

                    var index = line.IndexOf(qualifier);
                    while (index >= 0)
                    {
                        qualifierCount++;
                        index++;
                        if (index >= line.Length)
                        {
                            break;
                        }

                        index = line.IndexOf(qualifier, index);
                    }

                    if (qualifierCount == 0 || qualifierCount % 2 == 0)
                    {
                        counter++;
                        yield return sourceLine;
                        sourceLine=new StringBuilder();
                    }
                }

                Logger.GetLogger().DebugAsync($"Loaded {counter} records.");
            }
        }

        public void SetDataSource(ITextReader reader)
        {
            this.reader = reader;
        }

        public virtual IEnumerable<IRecord> ReadData()
        {
            return this.ReadLines().Select(sourceLine => Record<CsvRecord>.Factory.GetRecord(this.configuration, sourceLine));
        }

        public List<ColumnInfo> Columns => new List<ColumnInfo>(this.configuration.Columns??new List<ColumnInfo>());
        public List<FileReference> References => new List<FileReference>(this.configuration.References??new List<FileReference>());

        public long LoadedBytes => this.reader.LoadedBytes;
        public long TotalBytes => this.reader.TotalBytes;

        protected CsvReaderConfiguration configuration;

        private ITextReader reader;
        private const int LINES_BUFFER = 100000;

        private ConcurrentQueue<StringBuilder> csvLines=new ConcurrentQueue<StringBuilder>();
        public class CsvReaderConfiguration:CsvFileConfiguration<ColumnInfo>{}
    }
}
