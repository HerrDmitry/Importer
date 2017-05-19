using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Importer.Pipe.Reader;
using Importer.Pipe.Writer;

namespace Importer.Pipe
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using Configuration;
    using Record;

    public class DataFlow
    {
        public DataFlow(ImporterConfiguration configuration)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                Logger.GetLogger().SetLogginLevel(Logger.LogLevel.Debug);

                if (configuration.Files.TryGetValue("Error_Output", out string errorPath))
                {
                    ErrorOutput.SetOutputTarget(File.Open(errorPath, FileMode.Create, FileAccess.Write, FileShare.Read));
                }

                this.LoadDictionaries(configuration.Files, configuration.Readers).Wait();

                this.writers = configuration.Writers.Where(x => !x.Disabled)
                    .Select(
                        x =>
                            {
                                if (configuration.Files.TryGetValue(x.Name, out string filePath))
                                {
                                    return FileWriter.GetFileWriter(File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read), x);
                                }
                                throw new ArgumentOutOfRangeException($"There is no file path defined for \"{x.Name}\"");
                            })
                    .ToList();

                var reader = configuration.Readers.FirstOrDefault(x => !x.Disabled && !DataDictionary.GetDictionaryNames().Contains(x.Name));
                if (reader != null)
                {
                    if (configuration.Files.TryGetValue(reader.Name, out string readerFilePath))
                    {
                        if (!File.Exists(readerFilePath))
                        {
                            throw new FileNotFoundException($"File \"{readerFilePath}\" not found");
                        }
                        using (var fileReader = FileReader.GetFileReader(File.Open(readerFilePath, FileMode.Open, FileAccess.Read, FileShare.Read), reader))
                        {
                            var cancellationTokenSource = new CancellationTokenSource();
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
                Logger.GetLogger()
                    .InfoAsync($"Finished data processing in {stopwatch.Elapsed.Hours:00}:{stopwatch.Elapsed.Minutes:00}:{stopwatch.Elapsed.Seconds:00}.{stopwatch.Elapsed.Milliseconds}");
            }
            finally
            {
                ErrorOutput.Flush();
            }
        }

        private void WriterTask(CancellationToken token)
        {
            while (!token.IsCancellationRequested || this.buffer.Count>0)
            {
                if (this.buffer.TryTake(out DataRecord record))
                {
                    var failed = record.Parsed.Values.FirstOrDefault(x => x.IsFailed);
                    if (failed!=null)
                    {
                        lock (this.buffer)
                        {
                            this.exceptionRecordCount++;
                            ErrorOutput.Write($"Line {record.RecordNumber} - Column {failed.Column.Name} - ");
                            ErrorOutput.WriteLine(string.Join(", ", record.Source));
                        }
                    }
                    else
                    {
                        foreach (var writer in this.writers)
                        {
                            writer.WriteLine(record.Parsed);
                        }

                        lock (this.buffer)
                        {
                            this.successfulRecordCount++;
                        }
                    }
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
                        DataDictionary.Set(reference, reference, record.Parsed);
                        counter++;
                    }
                    stopWatch.Stop();
                    elapsedSeconds = stopWatch.Elapsed.TotalSeconds;
                    Logger.GetLogger().InfoAsync($"Loaded dictionary {config.Name},  {counter} records in {elapsedSeconds} seconds");
                }
            });
        }

        private List<IFileWriter> writers;
        private ConcurrentBag<DataRecord> buffer = new ConcurrentBag<DataRecord>();
        private volatile int successfulRecordCount = 0;
        private volatile int exceptionRecordCount = 0;

        private const int MAX_BUFFER_SIZE = 10000;
    }
}
