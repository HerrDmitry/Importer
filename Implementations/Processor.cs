using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Importer.Interfaces;

namespace Importer
{
    using System.Collections.Generic;

    public class Processor
    {
        public Processor(Configuration.Configuration config)
        {
            this.config = config;
            var processingThreads = 1;// Environment.ProcessorCount;
            this.processingTasks=new Task[processingThreads];
            for (var i = 0; i < processingThreads; i++)
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
                    this.handledRecords = 0;
                    this.enqueuedRecords = 0;
                    foreach (var record in reader.ReadData())
                    {

                        while (this.pendingRecords.Count > 10000)
                        {
                            //Logger.GetLogger().DebugAsync("Reached record limit");
                            Thread.Sleep(50);
                        }
                        this.pendingRecords.Add(record);
                        this.enqueuedRecords++;

                        if (lastSeconds+9 < (int) stopwatch.Elapsed.TotalSeconds)
                        {
                            Logger.GetLogger()
                                .InfoAsync($"Loaded {(int) (reader.LoadedBytes/(double)reader.TotalBytes * 100)}% in {(int) stopwatch.Elapsed.TotalSeconds} seconds.");
                            lastSeconds = (int) stopwatch.Elapsed.TotalSeconds;
                        }
                    }

                    Logger.GetLogger().DebugAsync($"All data {this.enqueuedRecords} records, loaded in {stopwatch.Elapsed.TotalSeconds} seconds, waiting for write threads...");
                    this.isDone = true;
                    Task.WaitAll(this.processingTasks);
                    Logger.GetLogger().DebugAsync($"{this.handledRecords} records enqueued for parsing.");
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
            var references = this.config.GetReaders().SelectMany(x => x.Value.References?.Select(c => c.Reference)).Distinct();
            var dictionaryTask = new List<Task>();
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

                    dictionaryTask.Add(DataDictionary.LoadDictionary(reference, reader.ReadData()));
                }
            }

            Task.WaitAll(dictionaryTask.ToArray());
        }

        private void HandleRecord()
        {
            var writers = this.config.GetWriters().ToList();
            long recordCount = 0;
            while (!this.isDone || this.handledRecords<this.enqueuedRecords)
            {
                while (this.pendingRecords.TryTake(out IRecord record))
                {
                    try
                    {
                        writers.ForEach(x => x.Value.Write(record));
                        record.Release();
                        recordCount++;
                    }
                    catch(Exception ex){
                        Logger.GetLogger().ErrorAsync($"HandleRecord - {ex.Message}");
                    }
                }

                lock (this.pendingRecords)
                {
                    this.handledRecords+=recordCount;
                    recordCount = 0;
                }

                Thread.Sleep(1);
            }
        }

        private Importer.Configuration.Configuration config;

        private Task[] processingTasks;

        private ConcurrentBag<IRecord> pendingRecords=new ConcurrentBag<IRecord>();

        private volatile bool isDone;

        private long handledRecords;

        private long enqueuedRecords;
    }
}
