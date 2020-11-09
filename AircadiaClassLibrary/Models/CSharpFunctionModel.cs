using Aircadia.ObjectModel.DataObjects;
using System;
using System.Collections.Generic;

namespace Aircadia.ObjectModel.Models
{
	public class CSharpFunctionModel : Model
	{
		public CSharpFunctionModel(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs,
			Func<List<Data>, List<Data>, bool> function, bool isAuxiliary = false, string parentName = "", string displayName = "") 
			: base(name, description, modelDataInputs, modelDataOutputs, isAuxiliary, parentName, displayName)
		{
			Function = function;
		}

		Func<List<Data>, List<Data>, bool> Function { get; }

		public override string ModelType => "CSharpFunctionModel";

		public override bool Execute() => Function(ModelDataInputs, ModelDataOutputs);

		public override void PrepareForExecution() { }

		public override Model Copy(string id, string name = null, string parentName = null)
			=> new CSharpFunctionModel(id, Description, ModelDataInputs, ModelDataOutputs, Function, IsAuxiliary, parentName ?? ParentName, name ?? Name);
	}
}
