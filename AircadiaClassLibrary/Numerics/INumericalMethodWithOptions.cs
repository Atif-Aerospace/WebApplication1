using System.Collections.Generic;

namespace Aircadia.Numerics
{
	public interface INumericalMethodOptions
	{
		List<string> Names { get; }
		string this[string key] { get; set; }
	}
}