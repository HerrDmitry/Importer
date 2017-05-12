using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Importer.Pipe.Reader;

namespace Importer.Pipe
{
    using System.Threading.Tasks;

    using Importer.Pipe.Configuration;

    public class DataFlow
    {
        public DataFlow(ImporterConfiguration configuration)
        {
            Logger.GetLogger().SetLogginLevel(Logger.LogLevel.Debug);

            this.LoadDictionaries(configuration.Files,configuration.Readers);
        }

        private void LoadDictionaries(Dictionary<string,string> files, IEnumerable<FileConfiguration> readers)
        {
            var dictionaries = new Dictionary<string, string>();
            readers.SelectMany(x => x.GetReferences()).ToList().ForEach(x => dictionaries[x.Key] = x.Value);
            var tasks=new List<Task>();
            foreach (var reader in readers.Where(x => dictionaries.Keys.Contains(x.Name) && !x.Disabled))
            {
                if (!files.TryGetValue(reader.Name, out string filePath) || !File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Data file \"{reader.Name}\" was not found");
                }

                tasks.Append(Task.Run(() => this.LoadDictionary(filePath, reader, dictionaries[reader.Name])));

            }

            Task.WaitAll(tasks.ToArray());
        }

        private void LoadDictionary(string filePath, FileConfiguration config, string keyFieldName)
        {
            using (var fileReader = FileReader.GetFileReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), config))
            {
                long counter = 0;
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var elapsedSeconds = 0D;
                //DataDictionary.LoadDictionary()
                foreach (var record in fileReader.ReadData())
                {
                    counter++;
                    if (stopWatch.Elapsed.TotalSeconds - elapsedSeconds > 10)
                    {
                        elapsedSeconds = stopWatch.Elapsed.TotalSeconds;
                        Logger.GetLogger().InfoAsync($"loaded {counter} records in {elapsedSeconds} seconds");
                    }
                }
                ;
            }
        }
    }
}
