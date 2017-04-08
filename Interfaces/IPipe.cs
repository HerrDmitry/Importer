using System;
using System.Collections.Generic;

namespace TestConsoleApp.Interfaces
{
	public interface IPipe
	{
	    void Configure(List<IParser> parsers);
	    void Input();
	}
}
