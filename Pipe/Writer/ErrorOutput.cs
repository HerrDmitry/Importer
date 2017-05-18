namespace Importer.Pipe.Writer
{
    using System.IO;

    public static class ErrorOutput
    {
        public static void SetOutputTarget(Stream target)
        {
            writer=new StreamWriter(target);
        }

        public static void Write(string line)
        {
            writer.Write(line);
        }

        public static void WriteLine(string line)
        {
            Write(line);
            writer.WriteLine();
        }

        private static StreamWriter writer;
    }
}