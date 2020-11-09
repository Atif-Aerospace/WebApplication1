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
		[NonSerialized()]
		private static MLApp.MLApp matlab;

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
			var matlabType = Type.GetTypeFromProgID("Matlab.Autoserver");
            if (matlabType == null)
            {
                throw new Exception("MATLAB is not registered as Automation Server");
            }
			if (matlab == null)
			{
				matlab = Activator.CreateInstance(matlabType) as MLApp.MLApp;
				if (matlab == null)
				{
					throw new Exception("The MATLAB instance cannot be created");
				}
				matlab.Visible = ShowConsole ? 1 : 0;
			}

			try
			{
				matlab.Execute("clc");
			}
			catch (Exception)
			{
				matlab = Activator.CreateInstance(matlabType) as MLApp.MLApp;
				if (matlab == null || matlab?.Visible != 1)
				{
					throw new Exception("The MATLAB instance cannot be created");
				}
				matlab.Visible = ShowConsole ? 1 : 0;
			}
		}

		public override bool Execute()
		{
			matlab.Execute($"cd {Path.GetDirectoryName(FilePath)}");

			foreach (Data data in ModelDataInputs)
			{
				if (data.Value is double d)
					matlab.PutFullMatrix(data.Name, "base", new double[] { d }, new double[] { 0.0 });
				if (data.Value is int i)
					matlab.PutFullMatrix(data.Name, "base", new double[] { i }, new double[] { 0.0 });
				else if (data.Value is Array arr)
				{
					matlab.PutFullMatrix(data.Name, "base", arr as Array, new double[arr.GetLength(0), arr.GetLength(1)]);
				}
			}

			matlab.Execute($"[{outputString}] = {FunctionName}({inputString});");

			foreach (Data data in ModelDataOutputs)
			{
				data.Value = matlab.GetVariable(data.Name, "base");
			}

			return true;
		}
		public override string ModelType => "Matlab";

		public override Model Copy(string id, string name = null, string parentName = null)
			=> new ModelMatlab(id, Description, ModelDataInputs, ModelDataOutputs, FilePath, FunctionName, parentName ?? ParentName, name ?? Name);
	}
}
