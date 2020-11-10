using Aircadia.ObjectModel.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using ExeModelTextFileManager.ExeManager;
using ExeModelTextFileManager.DataLocations;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Models
{
	public class MatlabTModel : Model
	{
		public MatlabTextBasedModel model;


		[Serialize(Type = SerializationType.Path)]
		public string FilePath => model.FilePath;
		[Serialize()]
		public string FunctionName => model.FunctionName;
		public IEnumerable<string> InputPaths => model.InputInstructions.Select(i => i.FilePath);
		public IEnumerable<string> OutputPaths => model.OutputInstructions.Select(i => i.FilePath);
		[SerializeEnumerable("InputFilesInstructionsSets")]
		public List<InstructionsSet> InputInstructions => model.InputInstructions;
		[SerializeEnumerable("OutputFilesInstructionsSets")]
		public List<InstructionsSet> OutputInstructions => model.OutputInstructions;

		public MatlabTModel(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, MatlabTextBasedModel model, string parentName = "", string displayName = "") 
			: base(name?.Replace(" ", ""), description, modelDataInputs, modelDataOutputs, parentName: parentName, displayName: displayName)
		{
			this.model = model;
		}

		public override void PrepareForExecution()
		{
			
		}

		public override bool Execute()
		{
			var inputs = ModelDataInputs.ToDictionary(d => d.Name, d => d.Value);
			IDictionary<string, object> outputs = model.Run(inputs);
			for (int i = 0; i < ModelDataOutputs.Count; i++)
				ModelDataOutputs[i].Value = outputs[ModelDataOutputs[i].Name];

			return true;
		}

		public override string ModelType => "Exe";

		public override void RenameVariable(string oldName, string newName)
		{
			foreach (InstructionsSet instruction in InputInstructions.Concat(OutputInstructions))
			{
				if (instruction.Instructions.ContainsKey(oldName))
				{
					LocationBase loc = instruction.Instructions[oldName];
					instruction.Instructions[newName] = loc;
					instruction.Instructions.Remove(oldName);
				}
			}
		}

		public override Model Copy(string id, string name = null, string parentName = null)
			=> new MatlabTModel(id, Description, ModelDataInputs, ModelDataOutputs, model, parentName ?? ParentName, name ?? Name);
	}
}
