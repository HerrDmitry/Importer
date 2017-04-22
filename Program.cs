using System;
using System.IO;
using System.Linq;
using System.Threading;
using Importer.Implementations;
using Importer.Interfaces;

namespace Importer
{
    class Program
    {
        static int Main(string[] args)
        {
            Logger.GetLogger().MessageAsync("Hello world");
            var configPath = args.FirstOrDefault(x => x.ToUpper().StartsWith("/C:"))?.Substring(3);
            if (string.IsNullOrEmpty(configPath))
            {
                PrintUsage();
                Logger.GetLogger().Flush();
                return -1;
            }

            var config=new Configuration(configPath);
            bool hasError = false;
            var files = new System.Collections.Generic.Dictionary<string, string>(config.Files);
            try
            {
                foreach (var a in args)
                {
                    if (ExtractFile(a, out Tuple<string, string> file))
                    {
                        files[file.Item1] = file.Item2;
                    }
                }
                foreach (var file in files)
                {
                    if (config.GetReaders().TryGetValue(file.Key, out IReader reader))
                    {
                        if (!File.Exists(file.Value))
                        {
                            Logger.GetLogger().ErrorAsync($"Source file \"{file.Value}\" not found");
                            hasError = true;
                        }
                        reader.SetDataSource(File.OpenRead(file.Value));
                    }
                    else
                    {
                        Logger.GetLogger().ErrorAsync(
                            $"There is no definition for \"{file.Key}\" in configuration file \"{configPath}\"");
                        hasError = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().ErrorAsync(ex.Message);
                hasError = true;
            }
            var result = 0;
            if (hasError)
            {
                Logger.GetLogger().ErrorAsync("Execution failed");
                result = -1;
            }
            Logger.GetLogger().Flush();
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
    }
}
