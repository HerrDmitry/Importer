using System;
using System.Collections.Generic;
using System.Text;
using Importer.Configuration;
using Importer.Output;

namespace Importer.Input
{
    public interface IInput<out T>
    {
        void Receive(string source);
        void AddOutput(IOutput<T> output);
        void SetUp(ColumnInfo column);

        bool IsFailed { get; }
    }
}
