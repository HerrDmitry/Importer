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
            foreach (var a in args)
            {
                try
                {
                    if (ExtractFile(a, out Tuple<string, string> file))
                    {
                        IReader reader;
                        if (config.GetReaders().TryGetValue(file.Item1, out reader))
                        {
                            if (!File.Exists(file.Item2))
                            {
                                Logger.GetLogger().ErrorAsync($"Source file \"{file.Item2}\" not found");
                                hasError = true;
                            }
                            reader.ReadFromStream(File.OpenRead(file.Item2));
                        }
                        else
                        {
                            Logger.GetLogger().ErrorAsync(
                                $"There is no definition for \"{file.Item1}\" in configuration file \"{configPath}\"");
                            hasError = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.GetLogger().ErrorAsync(ex.Message);
                    hasError = true;
                }
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
