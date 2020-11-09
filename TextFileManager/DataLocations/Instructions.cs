using System;
using System.Collections.Generic;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class InstructionsSet
	{
        public InstructionsSet(string filePath, Dictionary<string, LocationBase> instructions)
		{
			FilePath = filePath;
			Instructions = instructions;
		}

		public string FilePath { get; protected set; }
		public Dictionary<string, LocationBase> Instructions { get; protected set; }
	}
}
