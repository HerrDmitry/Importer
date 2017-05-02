using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Importer
{
    public class Processor
    {
        public Processor(Importer.Configuration.Configuration config)
        {
            this.config = config;
        }

        public async Task<int> ProcessAsync()
        {
            this.FindAndLoadDictionaries();
            var writers = this.config.GetWriters().ToList();
            Parallel.ForEach(this.config.GetReaders().First().Value.ReadData(), r =>
            {
                writers.ForEach( w => w.Value.WriteAsync(r));
            });
            writers.ForEach(x => x.Value.FlushAsync());
            writers.ForEach(x => x.Value.Close());
            return await Task.FromResult(-1);
        }

        private void FindAndLoadDictionaries()
        {
            var references = this.config.GetReaders().SelectMany(x => x.Value.Columns.Where(c => !string.IsNullOrWhiteSpace(c.Reference)).Select(c => c.Reference)).Distinct();
            foreach (var reference in references)
            {
                var referenceParts = reference.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (referenceParts.Length == 2)
                {
                    var reader = this.config.GetReader(referenceParts[0]);
                    if (reader == null)
                    {
                        var message = $"Could not find reference reader {reference}, reader name \"{referenceParts[0]}\" is incorrect.";
                        Logger.GetLogger().ErrorAsync(message);
                        throw new ArgumentException(message);
                    }

                    DataDictionary.GetDictionary(reference, reader.ReadData());
                }
            }
        }

        private Importer.Configuration.Configuration config;
    }
}
