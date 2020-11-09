using System;

namespace Aircadia.Numerics
{
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public class OptionAttribute : Attribute
	{

		// This is a positional argument
		public OptionAttribute(string displayName = "")
		{
			DisplayName = displayName;
		}

		public string DisplayName { get; }

	}
}
