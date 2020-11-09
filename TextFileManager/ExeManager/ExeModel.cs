using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ExeModelTextFileManager.DataLocations;

namespace ExeModelTextFileManager.ExeManager
{
	[Serializable()]
	public class ExeModel
	{
		public ExeModel(string workingDirectory, string executableFilePath, string executableFileArguments, List<InstructionsSet> inputInstructions, List<InstructionsSet> outputInstructions)
		{
			WorkingDirectory = workingDirectory;
			ExecutableFilePath = executableFilePath;
			ExecutableFileArguments = executableFileArguments;
			InputInstructions = inputInstructions;
			OutputInstructions = outputInstructions;
		}

		public string WorkingDirectory { get; protected set; }
		public string ExecutableFilePath { get; protected set; }
		public string ExecutableFileArguments { get; protected set; }
		public List<InstructionsSet> InputInstructions { get; protected set; } = new List<InstructionsSet>();
		public List<InstructionsSet> OutputInstructions { get; protected set; } = new List<InstructionsSet>();

		public IDictionary<string, object> Run(IDictionary<string, object> inputs)
		{
			// Inputs
			ProcessInputInstructions(inputs);

			Process proc = new Process();
			proc.StartInfo.WorkingDirectory = WorkingDirectory; // @"C:\Users\s236995\Desktop\Aircadia Explorer\projects\GKN_VGK_STUDY\M3A2_TRUNK_V0.0"; // Directory.GetParent(ExecutableFilePath).FullName;
			proc.StartInfo.FileName = ExecutableFilePath;
			proc.StartInfo.Arguments = ExecutableFileArguments;
			proc.StartInfo.UseShellExecute = true;
			proc.StartInfo.RedirectStandardError = false;
			proc.StartInfo.CreateNoWindow = true;
			proc.Start();

			proc.WaitForExit();

			proc.Close();

			return ProcessOutputInstructions();
		}

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

	//switch (location)
	//{
	//	//case WordFixedLocation loc:
	//		//switch (loc.Type)
	//		//{
	//		//	case Types.Int:
	//		//		tfm.UpdateWord(loc.Line, loc.StartColumn, loc.EndColumn, (int)input, loc.Format);
	//		//		break;
	//		//	case Types.Double:
	//		//		tfm.UpdateWord(loc.Line, loc.StartColumn, loc.EndColumn, (double)input, loc.Format);
	//		//		break;
	//		//	case Types.String:
	//		//		tfm.UpdateWord(loc.Line, loc.StartColumn, loc.EndColumn, (string)input);
	//		//		break;
	//		//	case Types.Bool:
	//		//		tfm.UpdateWord(loc.Line, loc.StartColumn, loc.EndColumn, (bool)input);
	//		//		break;
	//		//	default:
	//		//		break;
	//		//}
	//		//break;
	//	//case WordFixedColumnLocation loc:
	//	//	switch (loc.Type)
	//	//	{
	//	//		case Types.Int:
	//	//			tfm.UpdateWord(loc.AnchorTexts, loc.SkipLines, loc.StartColumn, loc.EndColumn, (int)input, loc.Format);
	//	//			break;
	//	//		case Types.Double:
	//	//			tfm.UpdateWord(loc.AnchorTexts, loc.SkipLines, loc.StartColumn, loc.EndColumn, (double)input, loc.Format);
	//	//			break;
	//	//		case Types.String:
	//	//			tfm.UpdateWord(loc.AnchorTexts, loc.SkipLines, loc.StartColumn, loc.EndColumn, (string)input);
	//	//			break;
	//	//		case Types.Bool:
	//	//			tfm.UpdateWord(loc.AnchorTexts, loc.SkipLines, loc.StartColumn, loc.EndColumn, (bool)input);
	//	//			break;
	//	//		default:
	//	//			break;
	//	//	}
	//	//	break;
	//	//case WordFixedLineLocation loc:
	//	//	switch (loc.Type)
	//	//	{
	//	//		case Types.Int:
	//	//			tfm.UpdateWord(loc.Line, loc.Position, loc.Separators, (int)input, loc.Format);
	//	//			break;
	//	//		case Types.Double:
	//	//			tfm.UpdateWord(loc.Line, loc.Position, loc.Separators, (double)input, loc.Format);
	//	//			break;
	//	//		case Types.String:
	//	//			tfm.UpdateWord(loc.Line, loc.Position, loc.Separators, (string)input);
	//	//			break;
	//	//		case Types.Bool:
	//	//			tfm.UpdateWord(loc.Line, loc.Position, loc.Separators, (bool)input);
	//	//			break;
	//	//		default:
	//	//			break;
	//	//	}
	//	//	break;
	//	//case WordRelativeLocation loc:
	//	//	switch (loc.Type)
	//	//	{
	//	//		case Types.Int:
	//	//			tfm.UpdateWord(loc.AnchorTexts, loc.SkipLines, loc.Position, loc.Separators, (int)input, loc.Format);
	//	//			break;
	//	//		case Types.Double:
	//	//			tfm.UpdateWord(loc.AnchorTexts, loc.SkipLines, loc.Position, loc.Separators, (double)input, loc.Format);
	//	//			break;
	//	//		case Types.String:
	//	//			tfm.UpdateWord(loc.AnchorTexts, loc.SkipLines, loc.Position, loc.Separators, (string)input);
	//	//			break;
	//	//		case Types.Bool:
	//	//			tfm.UpdateWord(loc.AnchorTexts, loc.SkipLines, loc.Position, loc.Separators, (bool)input);
	//	//			break;
	//	//		default:
	//	//			break;
	//	//	}
	//	//	break;
	//	default:
	//		break;
	//}


	//switch (location)
	//{
	//	case WordFixedLocation loc:
	//		switch (loc.Type)
	//		{
	//			case Types.Int:
	//				outputs[variable] = tfm.ReadInt(loc.Line, loc.StartColumn, loc.EndColumn);
	//				break;
	//			case Types.Double:
	//				outputs[variable] = tfm.ReadDouble(loc.Line, loc.StartColumn, loc.EndColumn);
	//				break;
	//			case Types.String:
	//				outputs[variable] = tfm.ReadWord(loc.Line, loc.StartColumn, loc.EndColumn);
	//				break;
	//			case Types.Bool:
	//				outputs[variable] = tfm.ReadBool(loc.Line, loc.StartColumn, loc.EndColumn);
	//				break;
	//			default:
	//				break;
	//		}
	//		break;
	//	case WordFixedColumnLocation loc:
	//		switch (loc.Type)
	//		{
	//			case Types.Int:
	//				outputs[variable] = tfm.ReadInt(loc.AnchorTexts, loc.SkipLines, loc.StartColumn, loc.EndColumn);
	//				break;
	//			case Types.Double:
	//				outputs[variable] = tfm.ReadDouble(loc.AnchorTexts, loc.SkipLines, loc.StartColumn, loc.EndColumn);
	//				break;
	//			case Types.String:
	//				outputs[variable] = tfm.ReadWord(loc.AnchorTexts, loc.SkipLines, loc.StartColumn, loc.EndColumn);
	//				break;
	//			case Types.Bool:
	//				outputs[variable] = tfm.ReadBool(loc.AnchorTexts, loc.SkipLines, loc.StartColumn, loc.EndColumn);
	//				break;
	//			default:
	//				break;
	//		}
	//		break;
	//	case WordFixedLineLocation loc:
	//		switch (loc.Type)
	//		{
	//			case Types.Int:
	//				outputs[variable] = tfm.ReadInt(loc.Line, loc.Position, loc.Separators);
	//				break;
	//			case Types.Double:
	//				outputs[variable] = tfm.ReadDouble(loc.Line, loc.Position, loc.Separators);
	//				break;
	//			case Types.String:
	//				outputs[variable] = tfm.ReadWord(loc.Line, loc.Position, loc.Separators);
	//				break;
	//			case Types.Bool:
	//				outputs[variable] = tfm.ReadBool(loc.Line, loc.Position, loc.Separators);
	//				break;
	//			default:
	//				break;
	//		}
	//		break;
	//	case WordRelativeLocation loc:
	//		switch (loc.Type)
	//		{
	//			case Types.Int:
	//				outputs[variable] = tfm.ReadInt(loc.AnchorTexts, loc.SkipLines, loc.Position, loc.Separators);
	//				break;
	//			case Types.Double:
	//				outputs[variable] = tfm.ReadDouble(loc.AnchorTexts, loc.SkipLines, loc.Position, loc.Separators);
	//				break;
	//			case Types.String:
	//				outputs[variable] = tfm.ReadWord(loc.AnchorTexts, loc.SkipLines, loc.Position, loc.Separators);
	//				break;
	//			case Types.Bool:
	//				outputs[variable] = tfm.ReadBool(loc.AnchorTexts, loc.SkipLines, loc.Position, loc.Separators);
	//				break;
	//			default:
	//				break;
	//		}
	//		break;
	//	case ArrayFixedColumnLocation loc:
	//		outputs[variable] = tfm.ReadArray(loc.AnchorTexts, loc.StopText, loc.SkipLines, loc.StartColumn, loc.EndColumn, loc.Frequency).ConvertAll(s => Convert.ToDouble(s));
	//		break;
	//	default:
	//		break;
	//}
}
