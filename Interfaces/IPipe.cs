using System;
using System.Collections.Generic;

namespace Importer.Interfaces
{
	public interface IPipe
	{
	    void Configure(List<IParser> parsers);
	    void Input(IInputRecord input);
        IEnumerable<IOutputRecord> Output();
	}
}
