using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Importer.Pipe.Reader;

namespace Importer.Pipe
{
    using Importer.Pipe.Configuration;

    public class DataFlow
    {
        public DataFlow(ImporterConfiguration configuration)
        {
            this.LoadDictionaries(configuration.Files,configuration.Readers);
        }

        private void LoadDictionaries(Dictionary<string,string> files, IEnumerable<FileConfiguration> readers)
        {
            var dictionaries = readers.SelectMany(x => x.GetReferences()).Distinct();
            foreach (var reader in readers.Where(x=>dictionaries.Contains(x.Name) && !x.Disabled))
            {
                if (!files.TryGetValue(reader.Name, out string filePath) || !File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Data file \"{reader.Name}\" was not found");
                }

                var fileReader = FileReader.GetFileReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), reader);
                var data = fileReader.ReadData().ToList();
                fileReader.Dispose();
            }
        }


    }
}
