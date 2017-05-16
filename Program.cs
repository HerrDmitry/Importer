using System;
using System.IO;
using System.Linq;
using System.Threading;
using Importer.Configuration;
using Importer.Interfaces;
using Importer.Pipe;
using Importer.Pipe.Configuration;
using Importer.Readers;
using Importer.Writers;

namespace Importer
{
    class Program
    {
        static int Main(string[] args)
        {
            bool hasError = false;
            var result = 0;
            try
            {
                Logger.GetLogger().MessageAsync("Hello world");
                var configPath = args.FirstOrDefault(x => x.ToUpper().StartsWith("/C:"))?.Substring(3);
                if (string.IsNullOrEmpty(configPath))
                {
                    PrintUsage();
                    Logger.GetLogger().Flush();
                    return -1;
                }

                var importerConfig = ImporterConfiguration.ReadConfiguration(configPath);
                var df = new DataFlow(importerConfig);

                return 0;


                var config = new Configuration.Configuration(configPath);
                var files = new System.Collections.Generic.Dictionary<string, string>(config.Files);
                foreach (var a in args)
                {
                    if (ExtractFile(a, out Tuple<string, string> file))
                    {
                        files[file.Item1] = file.Item2;
                    }
                }
                var readers = config.GetReaders();
                var writers = config.GetWriters();

                foreach (var reader in config.GetReaders())
                {
                    if (!files.TryGetValue(reader.Key, out string file) || string.IsNullOrWhiteSpace(file))
                    {
                        Logger.GetLogger().ErrorAsync($"No file defined for reader configuration {reader.Key}");
                        hasError = true;
                        break;
                    }

                    reader.Value.SetDataSource(new BufferedTextReader(File.OpenRead(file)));
                }

                foreach (var writer in config.GetWriters())
                {
                    if (!files.TryGetValue(writer.Key, out string file) || string.IsNullOrWhiteSpace(file))
                    {
                        Logger.GetLogger().ErrorAsync($"No file defined for writer configuration {writer.Key}");
                        hasError = true;
                        break;
                    }


                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }

                    FileWriter errorOutput = null;
                    if (files.TryGetValue(ERROR_OUTPUT_NAME, out string errorFile))
                    {
                        if (File.Exists(errorFile))
                        {
                            File.Delete(errorFile);
                        }

                        errorOutput = new FileWriter();
                        errorOutput.SetDataDestination(File.Open(errorFile, FileMode.Create, FileAccess.Write, FileShare.Read));
                    }

                    writer.Value.SetDataDestination(File.Open(file, FileMode.Create, FileAccess.Write, FileShare.Read), errorOutput);
                }

                var processor = new Processor(config);
                processor.ProcessAsync().Wait();
            }
            catch (Exception ex)
            {
                Logger.GetLogger().ErrorAsync(ex.Message);
                hasError = true;
            }
            finally
            {
                if (hasError)
                {
                    Logger.GetLogger().ErrorAsync("Execution failed");
                    result = -1;
                }
                Logger.GetLogger().Flush();
            }
            return result;
        }

        private static bool ExtractFile(string argument,out Tuple<string,string> file)
        {
            file=new Tuple<string, string>("","");
            if (!argument.ToUpper().StartsWith("/F:"))
            {
                return false;
            }

            var fileArg = argument.Substring(3);
            var index=fileArg.IndexOf(":");
            if (index > 0)
            {
                file=new Tuple<string, string>(fileArg.Substring(0,index),fileArg.Substring(index+1));
                if (!string.IsNullOrWhiteSpace(file.Item1) && !string.IsNullOrWhiteSpace(file.Item2))
                {
                    return true;
                }
            }

            return false;
        }

        private static void PrintUsage()
        {
            Logger.GetLogger().MessageAsync("Usage:");
            Logger.GetLogger().MessageAsync("/c:{configuration file name} [/f:{tableName}:{source file name} ...]");
        }

        private const string ERROR_OUTPUT_NAME = "Error_Output";
    }
}
