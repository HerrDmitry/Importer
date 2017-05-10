using System;
using System.Collections.Generic;
using System.Text;
using Importer.Configuration;
using Newtonsoft.Json.Linq;

namespace Importer.Output
{
    public interface IOutput<in T>
    {
        void SetUp(JObject column);
        void Receive(T value);
        string ToString();
        string ToString(string format);

        bool IsFailed { get; }
    }
}
