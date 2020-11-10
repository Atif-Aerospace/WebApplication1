using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ExeModelTextFileManager.DataLocations;

namespace ExeModelTextFileManager.ExeManager
{
	[Serializable()]
	public class ExeTextBasedModel : TextBasedModel
	{
		public ExeTextBasedModel(string workingDirectory, string executableFilePath, string executableFileArguments, List<InstructionsSet> inputInstructions, List<InstructionsSet> outputInstructions) 
			: base (inputInstructions, outputInstructions) 
		{
			WorkingDirectory = workingDirectory;
			ExecutableFilePath = executableFilePath;
			ExecutableFileArguments = executableFileArguments;
		}

		public string WorkingDirectory { get; protected set; }
		public string ExecutableFilePath { get; protected set; }
		public string ExecutableFileArguments { get; protected set; }
		public static bool CreateNoWindow { get; set; }

		protected override void Execute()
		{
			Process proc = new Process();
			proc.StartInfo.WorkingDirectory = WorkingDirectory; // @"C:\Users\s236995\Desktop\Aircadia Explorer\projects\GKN_VGK_STUDY\M3A2_TRUNK_V0.0"; // Directory.GetParent(ExecutableFilePath).FullName;
			proc.StartInfo.FileName = ExecutableFilePath;
			proc.StartInfo.Arguments = ExecutableFileArguments;
			proc.StartInfo.UseShellExecute = true;
			proc.StartInfo.RedirectStandardError = false;
			proc.StartInfo.CreateNoWindow = CreateNoWindow;
			proc.Start();

			proc.WaitForExit();

			proc.Close();
		}
	}

	public class MatlabTextBasedModel : TextBasedModel
	{
		public string FilePath { get; }
		public string FunctionName { get; }

		

		public MatlabTextBasedModel(string filePath, string functionName, List<InstructionsSet> inputInstructions, List<InstructionsSet> outputInstructions)
			: base(inputInstructions, outputInstructions)
		{
			FilePath = filePath;
			FunctionName = functionName;
		}


		protected override void Execute()
		{
			
		}
	}

	public abstract class TextBasedModel
	{
		public TextBasedModel(List<InstructionsSet> inputInstructions, List<InstructionsSet> outputInstructions)
		{
			InputInstructions = inputInstructions;
			OutputInstructions = outputInstructions;
		}

		public List<InstructionsSet> InputInstructions { get; protected set; } = new List<InstructionsSet>();
		public List<InstructionsSet> OutputInstructions { get; protected set; } = new List<InstructionsSet>();

		public IDictionary<string, object> Run(IDictionary<string, object> inputs)
		{
			// Inputs
			ProcessInputInstructions(inputs);

			Execute();

			return ProcessOutputInstructions();
		}

		protected abstract void Execute();

		private void ProcessInputInstructions(IDictionary<string, object> inputs)
		{
			foreach (var instruction in InputInstructions)
			{
				var tfm = new TextFileManager(File.ReadAllLines(instruction.FilePath));
				foreach (var variable in instruction.Instructions.Keys)
				{
					var input = inputs[variable];
					var location = instruction.Instructions[variable];
					location.Update(tfm, input);
				}
				File.WriteAllLines(instruction.FilePath, tfm.FileContents);
			}
		}

		private Dictionary<string, object> ProcessOutputInstructions()
		{
			var outputs = new Dictionary<string, object>();
			foreach (var instruction in OutputInstructions)
			{
				var tfm = new TextFileManager(File.ReadAllLines(instruction.FilePath));
				foreach (var variable in instruction.Instructions.Keys)
				{
					var location = instruction.Instructions[variable];
					outputs[variable] = location.Read(tfm);

				}
			}
			return outputs;
		}

	}
}
