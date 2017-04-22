using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Importer.Implementations.Parsers;
using Importer.Interfaces;

namespace Importer.Implementations
{
    public class DataDictionary
    {
        private static Dictionary<string, DataDictionary> dictionaries = new Dictionary<string, DataDictionary>();

        public static DataDictionary GetDictionary(string referencePath, IEnumerable<IInputRecord> records)
        {
            lock (dictionaries)
            {
                if (!dictionaries.TryGetValue(referencePath, out DataDictionary dict))
                {
                    dict = new DataDictionary(referencePath, records);
                    dictionaries[referencePath] = dict;
                    new Task(() =>
                    {
                        lock (dict.locker)
                        {
                            dict.LoadItems().Wait();
                        }
                    }).Start();
                }

                return dict;
            }
        }

        public static DataDictionary GetDictionary(string referencePath){
            return dictionaries.TryGetValue(referencePath, out DataDictionary dict) ? dict : null;
        }

        private DataDictionary(string referencePath, IEnumerable<IInputRecord> records)
        {
            this.referencePath = referencePath;
            this.records = records;
        }

        public IImmutableDictionary<string, IInputRecord> Items
        {
            get
            {
                lock (this.locker)
                {
                    if (this.dictionaryItems == null)
                    {
                        this.LoadItems().Wait();
                    }
                }

                return this.dictionaryItems;
            }
        }

        public IInputRecord this[string key] => this.Items[key];

        public IInputRecord TryGetValueDefault(string key, IInputRecord defaultValue = null)
        {
            return this.Items.TryGetValue(key, out IInputRecord value) ? value : defaultValue;
        }

        private async Task LoadItems()
        {
            var task = new Task(() =>
              {
                  try
                  {
                      var stopwatch = Stopwatch.StartNew();
                      var dictionary = new Dictionary<string, IInputRecord>();
                      var referenceParts = referencePath.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                      if (referenceParts.Length != 2)
                      {
                          this.dictionaryItems = new Dictionary<string, IInputRecord>().ToImmutableDictionary();
                          var message = $"Dictionary reference \"{this.referencePath}\" is in incorrect format.";
                          Logger.GetLogger().ErrorAsync(message);
                          throw new FormatException(message);
                      }
                      Logger.GetLogger().InfoAsync($"Loading dictionary {referenceParts[0]}...");
                      foreach (var inputRecord in records)
                      {
                          var key = inputRecord[referenceParts[1]].Parse();
                          dictionary[key] = inputRecord;
                      }
                      Logger.GetLogger().InfoAsync($"Loaded dictionary {referenceParts[0]} - {dictionary.Count()} records.");

                      this.dictionaryItems = dictionary.ToImmutableDictionary();
                      stopwatch.Stop();
                      Logger.GetLogger().DebugAsync($"Loading dictionary reference \"{referencePath}\" completed in {stopwatch.ElapsedMilliseconds}ms");
                  }
                  catch (Exception ex)
                  {
                      Logger.GetLogger().ErrorAsync($"Failed to load reference dictionary \"{this.referencePath}\"\n{ex.Message}");
                  }
              });
            task.Start();
            await task;
        }

        private ImmutableDictionary<string, IInputRecord> dictionaryItems;
        private string referencePath;
        private IEnumerable<IInputRecord> records;
        private object locker = new object();
    }
}
