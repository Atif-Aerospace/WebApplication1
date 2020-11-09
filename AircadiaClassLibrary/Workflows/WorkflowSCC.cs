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
	public class WorkflowSCC : Workflow, INumericallyTreatedWorkflow, IReversableWorkflow
	{
		public override string WorkflowType => "StronglyConnectedComponent";

		public List<Data> FeedbackVariables { get; }
		/// <summary>
		/// Reversed Inputs. Indices of the default workflow inputs that are also in this outputs.
		/// They need to be guessed
		/// </summary>
		public List<Data> ReversedInputs { get; }
		/// <summary>
		/// Reversed Outputs. Indices of the default workflow outputs that are also in this inputs.
		/// They set target values in the outputs of the original model
		/// </summary>
		public List<Data> ReversedOutputs { get; }
		[SerializeEnumerable("Solvers")]
		public List<ISolver> Solvers { get; set; }

		private List<Data> allGuessVariables = new List<Data>();
		private List<Data> allTergetVariables = new List<Data>();
		private double[] targetValues;


		//private List<Data> fdbvariables_mod_in = new List<Data>();
		//private List<Data> fdbvariables_mod_out = new List<Data>();

		//public WorkflowSergioSCC(string name, string description, List<Data> inputs, List<Data> outputs, List<WorkflowComponent> components) : this(name, description, inputs, outputs, components, null) { }
		[DeserializeConstructor]
		public WorkflowSCC(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, List<WorkflowComponent> components, 
			List<WorkflowComponent> scheduledComponents, List<ISolver> solvers, string parentName = "")
			: base(name, description, modelDataInputs, modelDataOutputs, components, scheduledComponents, true, parentName)
		{
			//ScheduledComponents = GetAllModels();
			List<Data> allData = ScheduledComponents.GetAllData();
			var defaultOutputs = new HashSet<Data>(ScheduledComponents.SelectMany(c => c.ModelDataOutputs));
			var defaultInputs = new HashSet<Data>(allData.Where(d => !defaultOutputs.Contains(d)));

			FeedbackVariables = GetFeedbackVars();
			ReversedInputs = GetReversedInputs(defaultInputs);
			ReversedOutputs = GetReversedOutputs(defaultOutputs);

			allGuessVariables.AddRange(FeedbackVariables);
			allGuessVariables.AddRange(ReversedInputs);
			allTergetVariables.AddRange(FeedbackVariables);
			allTergetVariables.AddRange(ReversedOutputs);

			Solvers = solvers;
		}

		private List<Data> GetReversedOutputs(HashSet<Data> defaultOutputs)
		{
			var reversedOutputs = new List<Data>();
			foreach (WorkflowComponent component in Components)
			{
				if (component is WorkflowReversedModel reversedModel)
					reversedOutputs.AddRange(reversedModel.ReversedOutputs);
			}

			foreach (Data input in ModelDataInputs)
				if (defaultOutputs.Contains(input))
					reversedOutputs.Add(input); // Input set a target on default workflow

			return reversedOutputs;
		}

		private List<Data> GetReversedInputs(HashSet<Data> defaultInputs)
		{
			var reversedInputs = new List<Data>();
			foreach (WorkflowComponent component in Components)
			{
				if (component is WorkflowReversedModel reversedModel)
					reversedInputs.AddRange(reversedModel.ReversedInputs);
			}

			foreach (Data output in ModelDataOutputs)
				if (defaultInputs.Contains(output))
					reversedInputs.Add(output); // Gused value will be output

			return reversedInputs;
		}

		/// <summary>
		/// Find the feedback variables in an SCC
		/// </summary>
		/// <returns></returns>
		private List<Data> GetFeedbackVars()
		{
			var feedbackVariables = new List<Data>();

			// Relates data with comonents
			List<Data> allData = ScheduledComponents.GetAllData();
			var dataComponentDictionary = new Dictionary<string, List<string>>(allData.Count);
			foreach (Data data in allData)
				dataComponentDictionary.Add(data.Id, new List<string>(ScheduledComponents.Count));
			foreach (WorkflowComponent component in ScheduledComponents)
				foreach (Data input in component.ModelDataInputs)
					dataComponentDictionary[input.Id].Add(component.Id);

			// Hash for the visited component, if model.Id is in Hash component has been visited
			var componentHash = new HashSet<string>();
			foreach (WorkflowComponent component in ScheduledComponents)
			{
				foreach (Data output in component.ModelDataOutputs)
				{
					foreach (string outputComponentName in dataComponentDictionary[output.Id])
					{
						// If the model has been visited
						if (componentHash.Contains(outputComponentName))
							feedbackVariables.Add(output);
					}
				}
				componentHash.Add(component.Id);
			}

			return feedbackVariables;
		}

		public override bool Execute()
		{
			//Checking that the default/preferred sequence of solvers is avaliable
			if (Solvers?.Count == 0)
			{
				throw new ArgumentException($"No solver has been associated with the SCC(s) involved in the execution of the selected object: {Name}");
			}

			//object[] initin = Data.GetValues(ModelDataInputs);
			//object[] initout = Data.GetValues(ModelDataOutputs);
			bool status = true;

			targetValues = new double[allTergetVariables.Count];

			double[] x0 = GetStartingPoint();

			// Use solvers
			foreach (ISolver solver in Solvers)
			{
				if (solver is IFixedPointSolver fps)
				{
					x0 = GetStartingPointForFixedPoint();

					if (x0.Length > FeedbackVariables.Count)
						continue;

					// In case they have been modified by another solver
					for (int i = 0; i < FeedbackVariables.Count; i++)
					{
						if (FeedbackVariables[i] is ScalarData)
							FeedbackVariables[i].Value = x0[i];
					}

					double[] x = fps.Solve(SolverFunctionFPI, x0);
					status = fps.IsSolved;
				}
				else if (solver is IGlobalSolver gs)
				{
					gs.Nvariables = x0.Length;
					gs.LowerBounds = new double[x0.Length];
					gs.UpperBounds = new double[x0.Length];
					for (int i = 0; i < allGuessVariables.Count; i++)
					{
						gs.LowerBounds[i] = allGuessVariables[i].Min;
						gs.UpperBounds[i] = allGuessVariables[i].Max;
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
				//status = ExecuteGaussNewton(startPt);
				//if (status == false)
				//{
				//	Data.SetValues(ModelDataInputs, initin);
				//	Data.SetValues(ModelDataOutputs, initout);
				//	Console.WriteLine("\r\n Gauss Newton method failed.Retrying after changing starting points \r\n");
				//	startPt = mGetStartingPoint_Allones();
				//	status = ExecuteGaussNewton(startPt);
				//	/*This section is yet to be implemented. Global level solving of SCCs with modified models in it
				//                if (status == false)
				//                {
				//                    MessageBox.Show("SCC not solved at local level; global approach will be tried now!");
				//                    //call Particle Swarm Optimization
				//                    //status = particleval();
				//                }*/
				//}
			}

			if (status == false)
			{
				Console.WriteLine("Values of data reset to initial in model " + Name + "\r\n");
				//Data.SetValues(ModelDataInputs, initin);
				//Data.SetValues(ModelDataOutputs, initout);
			}
			return status;
		}

		private double[] GetStartingPointForFixedPoint()
		{
			int n = FeedbackVariables.Count;
			double[] x0 = new double[n];
			for (int i = 0; i < n; i++)
			{
				if (FeedbackVariables[i] is ScalarData)
					x0[i] = FeedbackVariables[i].GetValueAsDouble();
			}
			return x0;
		}

		/// <summary>
		/// Get the starting point for the feedback variables of the SCC. This is normally the current values 
		/// of the feedback varaibles.
		/// </summary>
		/// <returns></returns>
		private double[] GetStartingPoint()
		{
			int n = allGuessVariables.Count;
			double[] x0 = new double[n];
			for (int i = 0; i < n; i++)
			{
				if (allGuessVariables[i] is ScalarData)
					x0[i] = allGuessVariables[i].GetValueAsDouble();
			}
			return x0;
		}

		private double[] SolverFunction(double[] input)
		{
			// Set reversed inputs. Need to be guessed
			for (int i = 0; i < input.Length; i++)
				allGuessVariables[i].Value = input[i];

			// Unlike revesed models target change every iteration
			for (int i = 0; i < allTergetVariables.Count; i++)
				targetValues[i] = allTergetVariables[i].GetValueAsDouble();


			// Execute the default workflow
			bool status = Execute_();


			// Get the difference (error): reversed outputs must match target value
			double[] residuals = new double[allTergetVariables.Count];
			for (int i = 0; i < allTergetVariables.Count; i++)
				residuals[i] = allTergetVariables[i].GetValueAsDouble() - targetValues[i];

			return residuals;
		}

		private double[] SolverFunctionFPI()
		{
			// Done automatically when executing all components
			//// Set reversed inputs. Need to be guessed
			//for (int i = 0; i < input.Length; i++)
			//	allGuessVariables[i].ValueAsDouble = input[i];

			// Execute the default workflow
			bool status = Execute_();


			// Get the difference (error): reversed outputs must match target value
			double[] feedbackVariables = new double[FeedbackVariables.Count];
			for (int i = 0; i < FeedbackVariables.Count; i++)
				feedbackVariables[i] = FeedbackVariables[i].GetValueAsDouble();

			return feedbackVariables;
		}

		public override Workflow Copy(string id, string name = null, string parentName = null)
		{
			return new WorkflowSCC(id, Description, ModelDataInputs.ToList(), ModelDataOutputs.ToList(),
				Components.ToList(), ScheduledComponents.ToList(), Solvers.ToList(), parentName ?? parentName);
		}
	}
}
