using System;
using System.Collections.Generic;
using System.Linq;
using Aircadia.Numerics.Solvers;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Workflows
{
	[Serializable()]
	public class WorkflowReversedModel : Workflow, INumericallyTreatedWorkflow, IReversableWorkflow
	{
		public override string WorkflowType => "ReversedModel";

		[SerializeEnumerable("Solvers")]
		public List<ISolver> Solvers { get; set; }
		/// <summary>
		/// Reversed Inputs. Indices of the default workflow inputs that are also in this outputs.
		/// They need to be guessed
		/// </summary>
		public List<Data> ReversedOutputs { get; } = new List<Data>();
		/// <summary>
		/// Reversed Outputs. Indices of the default workflow outputs that are also in this inputs.
		/// They set target values in the outputs of the original model
		/// </summary>
		public List<Data> ReversedInputs { get; } = new List<Data>();

		[Serialize(Type = SerializationType.Reference)]
		public Model Model => ScheduledComponents.First() as Model;
		/// <summary>Newton Method Target Values</summary>
		private double[] targetValues;

		//public WorkflowSergioReversedModel(string name, string description, List<Data> inputs, List<Data> outputs, Model model) : this(name, description, inputs, outputs, model, null) { }
		[DeserializeConstructor]
		public WorkflowReversedModel(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, Model model, List<ISolver> solvers, string parentName = "")
			: base(name, description, modelDataInputs, modelDataOutputs, new List<WorkflowComponent>() { model }, new List<WorkflowComponent>() { model }, true, parentName)
		{
			//this.Model = model;

			var outputsHash = new HashSet<string>(ModelDataOutputs.Select(d => d.Id));
			foreach (Data dt in model.ModelDataInputs)
				if (outputsHash.Contains(dt.Id))
					ReversedInputs.Add(dt);

			var inputsHash = new HashSet<string>(ModelDataInputs.Select(d => d.Id));
			foreach (Data dt in model.ModelDataOutputs)
				if (inputsHash.Contains(dt.Id))
					ReversedOutputs.Add(dt);

			Solvers = solvers;
		}

		public override bool Execute()
		{
			bool status = true;

			if (ReversedOutputs.Count == 0) // default workflow (no need to use Newton method)
			{
				ExecuteInternal();
			}
			else // Reversed workflow (need to use Newton method)
			{
				// Get the target values
				targetValues = new double[ReversedOutputs.Count];
				for (int i = 0; i < ReversedOutputs.Count; i++)
					targetValues[i] = ReversedOutputs[i].GetValueAsDouble();

				// Solver Options?
				int n = ReversedInputs.Count;
				double[] x0 = new double[n];
				for (int i = 0; i < n; i++)
				{
					if (ReversedInputs[i] is ScalarData)
						x0[i] = ReversedInputs[i].GetValueAsDouble();
				}

				// Use Solvers
				foreach (ISolver solver in Solvers)
				{
					if (solver is IFixedPointSolver fps)
					{
						Console.WriteLine($"{Name} is a Reversed model, so it cannot be solved by a Fiexed Pint Solver such as {solver}. Trying next solver on the list");
						status = false;
						continue;
					}
					else if (solver is IGlobalSolver gs)
					{
						gs.Nvariables = x0.Length;
						gs.LowerBounds = new double[x0.Length];
						gs.UpperBounds = new double[x0.Length];
						for (int i = 0; i < ReversedOutputs.Count; i++)
						{
							gs.LowerBounds[i] = ReversedOutputs[i].Min;
							gs.UpperBounds[i] = ReversedOutputs[i].Max;
						}

						double[] x = gs.Solve(SolverFunction);
						status = gs.IsSolved;

					}
					else if (solver is IGradientSolver s)
					{
						double[] x = s.Solve(SolverFunction, x0);
						status = s.IsSolved;
					}

					if (status)
						break;
					else
						Console.WriteLine($"{Name} was NOT solved by {solver}. Trying next solver on the list");
				}
			}

			return status;
		}

		private double[] SolverFunction(double[] input)
		{
			// Set reversed inputs. Need to be guessed
			for (int i = 0; i < input.Length; i++)
				ReversedInputs[i].Value = input[i];


			// Execute the default workflow
			bool status = ExecuteInternal();


			// Get the difference (error): reversed outputs must match target value
			double[] residuals = new double[ReversedOutputs.Count];
			for (int i = 0; i < ReversedOutputs.Count; i++)
				residuals[i] = ReversedOutputs[i].GetValueAsDouble() - targetValues[i];

			return residuals;
		}

		private bool ExecuteInternal()
		{
			bool status = Model.Execute();

			//if (status == false)
			//{
			//	Console.WriteLine("Execution of model \"" + Model.Id + "\" in workflow \"" + Name + "\" has been failed.", "Workflow execution terminated");
			//}

			return status;
		}

		public override Workflow Copy(string id, string name = null, string parentName = null)
		{
			return new WorkflowReversedModel(id, Description, ModelDataInputs.ToList(), ModelDataOutputs.ToList(),
				Model, Solvers.ToList(), parentName ?? parentName);
		}
	}
}
