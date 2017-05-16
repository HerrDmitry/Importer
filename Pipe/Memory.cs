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
            while (memoryArrays.TryTake(out char[] array))
            {
                if (array.Length >= size)
                {
                    return array;
                }
            }

            return new char[size];
        }

        public static void StoreArray(char[] array)
        {
            memoryArrays.Add(array);
        }

        private static ConcurrentBag<char[]> memoryArrays = new ConcurrentBag<char[]>();

    }
}
