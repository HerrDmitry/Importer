using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Importer.Interfaces;

namespace Importer
{
    using System.Collections.Concurrent;

    public class DataDictionary
    {
        private static ConcurrentDictionary<string, DataDictionary> dictionaries = new ConcurrentDictionary<string, DataDictionary>();

        public static async Task<DataDictionary> LoadDictionary(string referencePath, IEnumerable<IRecord> records)
        {
            return await Task.Run(
                () =>
                    {
                        if (dictionaries.TryGetValue(referencePath, out DataDictionary alreadyLoaded))
                        {
                            return alreadyLoaded;
                        }
                        
                        var dict = new DataDictionary(referencePath, records);
                        dictionaries[referencePath] = dict;
                        return dict;
                    });
        }

        public static DataDictionary GetDictionary(string referencePath){
            return dictionaries.TryGetValue(referencePath, out DataDictionary dict) ? dict : throw new ArgumentException($"Dictionary \"{referencePath}\" was not loaded");
        }

        private DataDictionary(string referencePath, IEnumerable<IRecord> records)
        {
            this.referencePath = referencePath;
            this.records = records;
            this.LoadItems();
        }

        public ConcurrentDictionary<string, IRecord> Items
        {
            get
            {
                if (this.dictionaryItems == null)
                {
                    this.LoadItems();
                }

                return this.dictionaryItems;
            }
        }

        public IRecord this[string key] => this.Items[key];

        public IRecord TryGetValueDefault(string key, IRecord defaultValue = null)
        {
            return this.Items.TryGetValue(key, out IRecord value) ? value : defaultValue;
        }

        private void LoadItems()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var referenceParts = this.referencePath.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (referenceParts.Length != 2)
                {
                    var message = $"Dictionary reference \"{this.referencePath}\" is in incorrect format.";
                    Logger.GetLogger().ErrorAsync(message);
                    throw new FormatException(message);
                }
                Logger.GetLogger().InfoAsync($"Loading dictionary {referenceParts[0]}...");
                foreach (var inputRecord in this.records)
                {

                    var key = inputRecord[this.referencePath];
                    if (!key.IsFailed)
                    {
                        this.dictionaryItems[key.ToString()] = inputRecord;
                    }
                }
                Logger.GetLogger().InfoAsync($"Loaded dictionary {referenceParts[0]} - {this.dictionaryItems.Count} records.");

                stopwatch.Stop();
                Logger.GetLogger().DebugAsync($"Loading dictionary reference \"{this.referencePath}\" completed in {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                Logger.GetLogger().ErrorAsync($"Failed to load reference dictionary \"{this.referencePath}\"\n{ex.Message}");
            }
        }

        private ConcurrentDictionary<string, IRecord> dictionaryItems=new ConcurrentDictionary<string, IRecord>();
        private string referencePath;
        private IEnumerable<IRecord> records;
    }
}
