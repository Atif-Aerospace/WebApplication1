using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Models
{
	[Serializable()]
	public class ModelMatlab : Model
	{
		[Serialize(Type = SerializationType.Path)]
		public string FilePath { get; set; }
		[Serialize]
		public string FunctionName { get; set; }

		private string outputString = "";
		private string inputString = "";

		

		public static bool ShowConsole {get; set;}

		[DeserializeConstructor]
		public ModelMatlab(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, string filePath, string functionName, string parentName = "", string displayName = "")
			: base(name?.Replace(" ", ""), description, modelDataInputs, modelDataOutputs, parentName: parentName, displayName: displayName)
		{
			FilePath = filePath;
			FunctionName = functionName;

			foreach (string s in modelDataInputs.Select(i => i.Name)) inputString += s + ", ";
			inputString = inputString.Remove(inputString.Length - 2);

			foreach (string s in modelDataOutputs.Select(o => o.Name)) outputString += s + ", ";
			outputString = outputString.Remove(outputString.Length - 2);

			// Generate model code
			PrepareForExecution();
		}

		public override void PrepareForExecution()
		{
			
		}

		public override bool Execute()
		{
			

			return true;
		}
		public override string ModelType => "Matlab";

		public override Model Copy(string id, string name = null, string parentName = null)
			=> new ModelMatlab(id, Description, ModelDataInputs, ModelDataOutputs, FilePath, FunctionName, parentName ?? ParentName, name ?? Name);
	}
}
