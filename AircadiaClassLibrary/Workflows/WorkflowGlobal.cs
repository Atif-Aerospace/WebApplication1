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
	public class WorkflowGlobal : Workflow, IReversableWorkflow, INumericallyTreatedWorkflow
	{
		public override string WorkflowType => "WorkflowGlobal";

		[SerializeEnumerable("Solvers")]
		public List<ISolver> Solvers { get; set; }
		/// <summary>
		/// Reversed Inputs. Indices of the default workflow inputs that are also in this outputs.
		/// They set target values in the outputs of the original model
		/// </summary>
		public List<Data> ReversedOutputs { get; } = new List<Data>();
		/// <summary>
		/// Reversed Outputs. Indices of the default workflow outputs that are also in this inputs.
		/// They need to be guessed
		/// </summary>
		public List<Data> ReversedInputs { get; } = new List<Data>();

		/// <summary>
		/// Newton Method Target Values
		/// </summary>
		private double[] targetValues;

		/// <summary>
		/// Serialization Constructor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="description"></param>
		/// <param name="modelDataInputs"></param>
		/// <param name="modelDataOutputs"></param>
		/// <param name="components"></param>
		/// <param name="scheduledComponents"></param>
		/// <param name="solvers"></param>
		/// <param name="isAuxiliary"></param>
		/// <param name="scheduleMode"></param>
		[DeserializeConstructor]
		public WorkflowGlobal(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, List<WorkflowComponent> components, 
			List<WorkflowComponent> scheduledComponents, List<ISolver> solvers, bool isAuxiliary = true, string scheduleMode = "", string parentName = "")
			: base(name, description, modelDataInputs, modelDataOutputs, components, scheduledComponents, isAuxiliary, scheduleMode, parentName)
		{
			List<Data> allData = ScheduledComponents.GetAllData();
			ScheduledComponents.GetInputsOutputsStatus(allData, out List<Data> defaultInputs, out List<Data> defaultOutputs);

			var outputsHash = new HashSet<string>(ModelDataOutputs.Select(d => d.Id));
			foreach (Data dt in defaultInputs)
				if (outputsHash.Contains(dt.Id))
					ReversedInputs.Add(dt);

			var inputsHash = new HashSet<string>(ModelDataInputs.Select(d => d.Id));
			foreach (Data dt in defaultOutputs)
				if (inputsHash.Contains(dt.Id))
					ReversedOutputs.Add(dt);

			Solvers = solvers;
		}

		public override bool Execute()
		{
			bool status = true;

			if (ReversedOutputs.Count == 0) // default workflow (no need to use Newton method)
			{
				Execute_();
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
						Console.WriteLine($"{Name} is a Global Workflow model, so it cannot be solved by a Fiexed Pint Solver such as {solver}. Trying next solver on the list");
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
			bool status = Execute_();


			// Get the difference (error): reversed outputs must match target value
			double[] residuals = new double[ReversedOutputs.Count];
			for (int i = 0; i < ReversedOutputs.Count; i++)
				residuals[i] = ReversedOutputs[i].GetValueAsDouble() - targetValues[i];

			return residuals;
		}

		public override Workflow Copy(string id, string name = null, string parentName = null)
		{
			return new WorkflowGlobal(id, Description, ModelDataInputs.ToList(), ModelDataOutputs.ToList(),
				Components.ToList(), ScheduledComponents.ToList(), Solvers.ToList(), IsAuxiliary, ScheduleMode, parentName ?? parentName);
		}
	}
}
