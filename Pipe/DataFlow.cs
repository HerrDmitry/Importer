using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Importer.Interfaces;
using Importer.Pipe.Reader;
using Importer.Pipe.Writer;

namespace Importer.Pipe
{
    using System.Threading.Tasks;

    using Importer.Pipe.Configuration;

    public class DataFlow
    {
        public DataFlow(ImporterConfiguration configuration)
        {
            Logger.GetLogger().SetLogginLevel(Logger.LogLevel.Debug);

            this.LoadDictionaries(configuration.Files,configuration.Readers).Wait();

            var writers = configuration.Writers.Select(x =>
            {
                if (configuration.Files.TryGetValue(x.Name, out string filePath))
                {
                    return FileWriter.GetFileWriter(File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read), x);
                }
                throw new ArgumentOutOfRangeException($"There is no file path defined for \"{x.Name}\"");
            });
            foreach (var reader in configuration.Readers.Where(x=>!DataDictionary.GetDictionaryNames().Contains(x.Name)))
            {
                if (configuration.Files.TryGetValue(reader.Name, out string readerFilePath))
                {
                    if (!File.Exists(readerFilePath))
                    {
                        throw new FileNotFoundException($"File \"{readerFilePath}\" not found");
                    }
                    using (var fileReader = FileReader.GetFileReader(File.Open(readerFilePath, FileMode.Open, FileAccess.Read, FileShare.Read), reader))
                    {
                        foreach (var record in fileReader.ReadData())
                        {
                            foreach (var writer in writers)
                            {
                                writer.WriteLine(record);
                            }
                        }
                    }
                }
            }
        }

        private async Task LoadDictionaries(Dictionary<string,string> files, IEnumerable<FileConfiguration> readers)
        {
            var dictionaries = new Dictionary<string, string>();
            readers.SelectMany(x => x.GetReferences()).ToList().ForEach(x => dictionaries[x.Key] = x.Value);
            foreach (var reader in readers.Where(x => dictionaries.Keys.Contains(x.Name) && !x.Disabled))
            {
                if (!files.TryGetValue(reader.Name, out string filePath) || !File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Data file \"{reader.Name}\" was not found");
                }

                await this.LoadDictionary(filePath, reader, dictionaries[reader.Name]);
            }
        }

        private async Task LoadDictionary(string filePath, FileConfiguration config, string keyFieldName)
        {
            await Task.Run(() =>
            {
                using (var fileReader = FileReader.GetFileReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), config))
                {
                    long counter = 0;
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    var elapsedSeconds = 0D;
                    var reference = config.Name + "." + keyFieldName;
                    foreach (var record in fileReader.ReadData())
                    {
                        DataDictionary.Set(reference, keyFieldName, record);
                        counter++;
                    }
                    stopWatch.Stop();
                    elapsedSeconds = stopWatch.Elapsed.TotalSeconds;
                    Logger.GetLogger().InfoAsync($"Loaded dictionary {config.Name},  {counter} records in {elapsedSeconds} seconds");
                }
            });
        }
    }
}
