using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Writer
{
    using System.IO;
    using System.Linq;
    using System.Threading;

    using Importer.Pipe.Configuration;
    using Importer.Pipe.Parsers;

    public class CsvMultilineFileWriter:CsvFileWriter
    {
        public CsvMultilineFileWriter(Stream target, CsvMultilineFileConfiguration config)
            : base(target, config)
        {
            this.config = config;
            this.doFallback = !(this.config.Rows?.Count > 0);
        }

        protected override void PrepareTask(CancellationToken token)
        {
            if (this.doFallback)
            {
                base.PrepareTask(token);
            }

            while (!token.IsCancellationRequested || this.dictBuffer.Count > 0)
            {
                if (this.dictBuffer.TryTake(out Dictionary<string, IValue> values))
                {
                    foreach (var configRow in this.config.Rows)
                    {
                        
                    }
                    var strings = this.config.Columns.Select(x => values.TryGetValue(x.Source, out IValue value) ? value.ToString(x.Format) : "").ToList();
                    this.PrepareRecord(strings);
                }
                else
                {
                    Thread.Sleep(50);
                }
            }

        }

        private CsvMultilineFileConfiguration config;

        private bool doFallback = false;
    }
}
