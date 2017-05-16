using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;

namespace Importer.Pipe
{
    public static class Memory
    {
        public static char[] GetAvailableCharArray(int size)
        {

            var count = 0;
            while (memoryArrays.TryDequeue(out char[] array) && count++<100)
            {
                if (array.Length >= size)
                {
                    return array;
                }
                if (memoryArrays.Count < 100)
                {
                    StoreArray(array);
                }
            }


            return new char[size];
        }

        public static void StoreArray(char[] array)
        {

            if (array != null)
            {
                memoryArrays.Enqueue(array);
            }

        }

        public static StringBuilder GetAvailableStringBuilder()
        {
            return builders.TryDequeue(out StringBuilder builder)?builder:new StringBuilder();
        }

        public static void StoreStringBuilder(StringBuilder builder)
        {
            if (builder != null)
            {
                builder.Clear();
                builders.Enqueue(builder);
            }
        }

        private static ConcurrentQueue<char[]> memoryArrays = new ConcurrentQueue<char[]>();

        private static ConcurrentQueue<StringBuilder> builders = new ConcurrentQueue<StringBuilder>();

    }
}
