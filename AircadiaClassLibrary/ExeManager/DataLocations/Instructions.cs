using Aircadia.Services.Serializers;
using System;
using System.Collections.Generic;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class InstructionsSet
	{
		[DeserializeConstructor]
        public InstructionsSet(string filePath, Dictionary<string, LocationBase> instructions)
		{
			FilePath = filePath;
			Instructions = instructions;
		}

		[Serialize("FilePath", SerializationType.Path)]
		public string FilePath { get; protected set; }
		[SerializeDictionary("Instructions", "Instruction")]
		public Dictionary<string, LocationBase> Instructions { get; protected set; }
	}
}
