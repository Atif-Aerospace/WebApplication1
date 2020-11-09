/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections.Generic;

using System.Linq;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.Numerics.Solvers;
using Aircadia.ObjectModel.Workflows;

namespace Aircadia.ObjectModel.Models
{
	[Serializable()]
	////Parent class of models(cModel) and subprocess(cSubprocess)
	public abstract class Model : WorkflowComponent
	{
		protected Model(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, bool isAuxiliary = false, string parentName = "", string displayName = "") 
			: base(name, description, modelDataInputs, modelDataOutputs, isAuxiliary, parentName)
		{
			if (!String.IsNullOrWhiteSpace(displayName))
			{
				Rename(displayName);
			}
		}

		/// <summary>
		/// Specifies the model type for serialization
		/// </summary>
		public abstract string ModelType { get; }

		/// <summary>
		/// Makes sure than the model is ready to be executed
		/// </summary>
		public abstract void PrepareForExecution();

		/// <summary>
		/// Provides the revesed model, according to the specified variables
		/// </summary>
		public WorkflowReversedModel Reverse(List<Data> inputs, List<Data> outputs)
		{
			var opts = new NewtonOptions() { MaxIterations = 20, DerivativeStep = new double[] { 0.01 } };
			var solver = new NewtonSolver(opts);
			string reversedName = GetReversedName(inputs, outputs);
			var reversedModel = new WorkflowReversedModel(reversedName, Description, inputs, outputs, this, new List<ISolver>() { solver });
			reversedModel.Rename(FullName + new string(reversedName.SkipWhile(c => c != ':').ToArray()));
			return reversedModel;
		}


		public string GetReversedName(List<Data> inputs, List<Data> outputs)
		{
			var reversedIN = inputs.GetNames().Except(ModelDataInputs.GetNames());
			var reversedOUT = outputs.GetNames().Except(ModelDataOutputs.GetNames());
			var reversed = new HashSet<string>(reversedIN.Union(reversedOUT));
			string reversalID = ModelDataInputs.Aggregate(String.Empty, (t, d) => t += reversed.Contains(d.Name) ? '1' : '0', t => t += ':');
			reversalID = ModelDataOutputs.Aggregate(reversalID, (t, d) => t += reversed.Contains(d.Name) ? '1' : '0');
			return $"{Id}:Reversed:{reversalID}";
		}

		public virtual void RenameVariable(string oldName, string newName) { }

		public abstract Model Copy(string id, string name = null, string parentName = null);
	}
}
