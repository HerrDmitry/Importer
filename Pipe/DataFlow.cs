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
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using Importer.Pipe.Configuration;
    using Importer.Pipe.Parsers;

    public class DataFlow
    {
        public DataFlow(ImporterConfiguration configuration)
        {
            var stopwatch=new Stopwatch();
            stopwatch.Start();
            Logger.GetLogger().SetLogginLevel(Logger.LogLevel.Debug);

            this.LoadDictionaries(configuration.Files,configuration.Readers).Wait();

            this.writers = configuration.Writers.Where(x=>!x.Disabled).Select(x =>
            {
                if (configuration.Files.TryGetValue(x.Name, out string filePath))
                {
                    return FileWriter.GetFileWriter(File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read), x);
                }
                throw new ArgumentOutOfRangeException($"There is no file path defined for \"{x.Name}\"");
            }).ToList();

            var reader = configuration.Readers.FirstOrDefault(x => !x.Disabled && !DataDictionary.GetDictionaryNames().Contains(x.Name));
            if(reader!=null)
            {
                if (configuration.Files.TryGetValue(reader.Name, out string readerFilePath))
                {
                    if (!File.Exists(readerFilePath))
                    {
                        throw new FileNotFoundException($"File \"{readerFilePath}\" not found");
                    }
                    using (var fileReader = FileReader.GetFileReader(File.Open(readerFilePath, FileMode.Open, FileAccess.Read, FileShare.Read), reader))
                    {
                        var cancellationTokenSource=new CancellationTokenSource();
                        var tasks = new Task[Environment.ProcessorCount];
                        for (var i = 0; i < Environment.ProcessorCount; i++)
                        {
                            tasks[i] = Task.Run(() => this.WriterTask(cancellationTokenSource.Token));
                        }

                        foreach (var record in fileReader.ReadData())
                        {
                            if (this.buffer.Count > MAX_BUFFER_SIZE)
                            {
                                Logger.GetLogger().DebugAsync("Reached record buffer limit");
                                Thread.Sleep(50);
                            }

                            this.buffer.Add(record);
                        }

                        cancellationTokenSource.Cancel();
                        Task.WaitAll(tasks);

                        Logger.GetLogger().InfoAsync($"Successfully saved {this.successfulRecordCount} records");
                        Logger.GetLogger().InfoAsync($"Failed to process {this.exceptionRecordCount} records");
                    }
                }
            }

            stopwatch.Stop();
            Logger.GetLogger().InfoAsync($"Finished data processing in {stopwatch.Elapsed.Hours}:{stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds}.{stopwatch.Elapsed.Milliseconds}");
        }

        private void WriterTask(CancellationToken token)
        {
            while (!token.IsCancellationRequested || this.buffer.Count>0)
            {
                if (this.buffer.TryTake(out Dictionary<string, IValue> record))
                {
                    if (record.Values.Any(x => x.IsFailed))
                    {
                        lock (this.buffer)
                        {
                            this.exceptionRecordCount++;
                        }
                    }
                    else
                    {
                        foreach (var writer in this.writers)
                        {
                            writer.WriteLine(record);
                        }

                        lock (this.buffer)
                        {
                            this.successfulRecordCount++;
                        }
                    }

                    record.Clear();
                }
                else
                {
                    Thread.Sleep(50);
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

        private List<IFileWriter> writers;
        private ConcurrentBag<Dictionary<string, IValue>> buffer = new ConcurrentBag<Dictionary<string, IValue>>();
        private volatile int successfulRecordCount = 0;
        private volatile int exceptionRecordCount = 0;

        private CancellationTokenSource tokenSource;

        private const int MAX_BUFFER_SIZE = 10000;
    }
}
