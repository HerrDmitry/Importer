using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Importer.Interfaces;

namespace Importer
{
    public class Processor
    {
        public Processor(Importer.Configuration.Configuration config)
        {
            this.config = config;
            this.processingTasks=new Task[Environment.ProcessorCount];
            for (var i = 0; i < Environment.ProcessorCount; i++)
            {
                this.processingTasks[i] = Task.Run(() => this.HandleRecord());
            }
        }

        public async Task ProcessAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    this.FindAndLoadDictionaries();
                    var writers = this.config.GetWriters().ToList();
                    var reader = this.config.GetReaders().First().Value;
                    var lastSeconds = 0;
                    Logger.GetLogger().DebugAsync("Start loading data");
                    foreach (var record in reader.ReadData())
                    {
                        while (this.pendingRecords.Count > 10000)
                        {
                            Thread.Sleep(1);
                        }
                        this.pendingRecords.Enqueue(record);

                        if (lastSeconds+9 < (int) stopwatch.Elapsed.TotalSeconds)
                        {
                            Logger.GetLogger()
                                .InfoAsync($"Loaded {(int) (reader.Percentage * 100)}% in {(int) stopwatch.Elapsed.TotalSeconds} seconds.");
                            lastSeconds = (int) stopwatch.Elapsed.TotalSeconds;
                        }
                    }
                    this.isDone = true;
                    Logger.GetLogger().DebugAsync($"All data loaded in {stopwatch.Elapsed.TotalSeconds} seconds, waiting for write threads...");
                    Task.WaitAll(this.processingTasks);
                    writers.ForEach(x => x.Value.Close());
                    stopwatch.Stop();
                    Logger.GetLogger().InfoAsync($"done in - {(int)stopwatch.Elapsed.TotalMinutes}:{stopwatch.Elapsed.Seconds}.{stopwatch.Elapsed.Milliseconds}");
                }
                catch (Exception ex)
                {
                    Logger.GetLogger().ErrorAsync(ex.Message);
                }
            });
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

        private void HandleRecord()
        {
            var writers = this.config.GetWriters().ToList();
            while (!this.isDone)
            {
                if (this.pendingRecords.TryDequeue(out IRecord record))
                {
                    writers.ForEach(x => x.Value.WriteAsync(record).Wait());
                    record.Release();
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        private Importer.Configuration.Configuration config;

        private Task[] processingTasks;

        private ConcurrentQueue<IRecord> pendingRecords=new ConcurrentQueue<IRecord>();

        private volatile bool isDone;
    }
}
