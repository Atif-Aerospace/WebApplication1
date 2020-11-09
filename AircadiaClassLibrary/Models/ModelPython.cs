using System;
using System.Collections.Generic;
using System.Linq;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace Aircadia.ObjectModel.Models
{
	[Serializable()]
	public class ModelPython : Model
	{
		[Serialize]
		public string Code { get; set; }

		private readonly string outputString = "";
		private readonly string inputString = "";
		private ScriptEngine engine;
		private ScriptScope scope;

		[DeserializeConstructor]
		public ModelPython(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, string code, string parentName = "", string displayName = "") 
			: base(name, description, modelDataInputs, modelDataOutputs, parentName: parentName, displayName: displayName)
		{
			Code = code;

			inputString = modelDataInputs.GetNames().Aggregate(String.Empty, (t, s) => t += s + ", ", s => s.TrimEnd(',', ' '));
			outputString = modelDataOutputs.GetNames().Aggregate(String.Empty, (t, s) => t += s + ", ", s => s.TrimEnd(',', ' '));

			// Generate model code
			PrepareForExecution();
		}

		protected ModelPython(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, string parentName = "", string displayName = "") 
			: base(name, description, modelDataInputs, modelDataOutputs, parentName: parentName, displayName: displayName)
		{
			inputString = modelDataInputs.GetNames().Aggregate(String.Empty, (t, s) => t += s + ", ", s => s.TrimEnd(',', ' '));
			outputString = modelDataOutputs.GetNames().Aggregate(String.Empty, (t, s) => t += s + ", ", s => s.TrimEnd(',', ' '));
		}

		public override void PrepareForExecution()
		{
			try
			{
				engine = engine ?? Python.CreateEngine();
				scope = engine.CreateScope();
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to start IronPython", ex);
			}

			try
			{
				foreach (Data data in ModelDataInputs)
				{
					scope.SetVariable(data.Name, data.Value);
				}

				engine.Execute(Code, scope);
			}
			catch (Exception ex)
			{
				throw new Exception("Somethinhg is wrong with the python code", ex);
			}
		}

		public static string GenerateSignature(string name, List<Data> inputs, List<Data> outputs)
		{
			string inputString = inputs.GetNames().Aggregate(String.Empty, (t, s) => t += s + ", ", s => s.TrimEnd(',', ' '));
			string outputString = outputs.GetNames().Aggregate(String.Empty, (t, s) => t += s + ", ", s => s.TrimEnd(',', ' '));
			return $"[{outputString}] = {name}({inputString})";
		}

		public override bool Execute()
		{
			foreach (Data data in ModelDataInputs)
			{
				scope.SetVariable(data.Name, data.Value);
			}

			engine.Execute(Code, scope);

			foreach (Data data in ModelDataOutputs)
			{
				data.Value = scope.GetVariable(data.Name);
			}

			return true;
		}

		public override string ModelType => "Python";

		public override Model Copy(string id, string name = null, string parentName = null)
			=> new ModelPython(id, Description, ModelDataInputs, ModelDataOutputs, Code, parentName ?? ParentName, name ?? Name);
	}
}
