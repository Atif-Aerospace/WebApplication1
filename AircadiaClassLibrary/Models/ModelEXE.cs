using Aircadia.ObjectModel.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using ExeModelTextFileManager.ExeManager;
using ExeModelTextFileManager.DataLocations;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Models
{
	[Serializable()]
    public class ModelExe : Model
	{
		private ExeTextBasedModel model;

		[Serialize(Type = SerializationType.Path)]
		public string WorkingDirectory => model.WorkingDirectory;
		[Serialize("ExecutableFilePath", Type = SerializationType.Path)]
		public string ExecutableFilePath => model.ExecutableFilePath;
		[Serialize("ExecutableFileArguments")]
		public string ExecutableFileArguments => model.ExecutableFileArguments;
		public IEnumerable<string> InputPaths => model.InputInstructions.Select(i => i.FilePath);
		public IEnumerable<string> OutputPaths => model.OutputInstructions.Select(i => i.FilePath);
		[SerializeEnumerable("InputFilesInstructionsSets")]
		public List<InstructionsSet> InputInstructions => model.InputInstructions;
		[SerializeEnumerable("OutputFilesInstructionsSets")]
		public List<InstructionsSet> OutputInstructions => model.OutputInstructions;

		public static bool ShowConsole { get => !ExeTextBasedModel.CreateNoWindow; set => ExeTextBasedModel.CreateNoWindow = !value; }

		[DeserializeConstructor]
        public ModelExe(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, string workingDirectory, string executableFilePath, string executableFileArguments, 
			List<InstructionsSet> inputInstructions, List<InstructionsSet> outputInstructions, string parentName = "", string displayName = "")
			: base(name?.Replace(" ", ""), description, modelDataInputs, modelDataOutputs, parentName: parentName, displayName: displayName)
        {
			model = new ExeTextBasedModel(workingDirectory, executableFilePath, executableFileArguments, inputInstructions, outputInstructions);
        }

		public override void PrepareForExecution() { }

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
			=> new ModelExe(id, Description, ModelDataInputs, ModelDataOutputs, WorkingDirectory, ExecutableFilePath, 
				ExecutableFileArguments, InputInstructions, OutputInstructions, parentName ?? ParentName, name ?? Name);
	}
}
