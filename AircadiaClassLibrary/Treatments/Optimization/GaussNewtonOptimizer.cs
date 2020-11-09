/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

using Aircadia.ObjectModel;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Treatments;
using MathNet.Numerics.LinearAlgebra;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

namespace Aircadia
{
	[Serializable()]
	//// Subclass of the treatment class. This is the treatment class for Gauss Newton Optimiser. This treatment still needs testing and is not well implemented.
	public class GaussNewtonOptimizer : Treatment
	{
		public WorkflowComponent Component { get; private set; }

		// Optimizer Options
		private const int MaxIterations = 5000;
		private const double PercentageOfDifference = 1e-8;

		private readonly List<UnboundedDesignVariable> designVariables;
		private readonly List<Parameter> parameters;
		private readonly List<Objective> objectives;

		/// <summary>
		/// Constructer
		/// </summary>
		/// <param name="name"></param>
		/// <param name="input_opt"></param>
		/// <param name="outp"></param>
		public GaussNewtonOptimizer(string name, List<UnboundedDesignVariable> designVariables, List<Parameter> parameters, List<Objective> objectives, WorkflowComponent component) :
			base(name, "")
		{
			this.designVariables = designVariables;
			this.parameters = parameters;
			this.objectives = objectives;
			Component = component;
		}

		/// <summary>
		/// exectues the optimiser study
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		public override bool ApplyOn(ExecutableComponent component)
		{
			Component = component as WorkflowComponent;
			return ApplyOn();
		}
		public override bool ApplyOn()
		{
			//string outputString = GetOutputString(d => d.Name);

			int ndes = designVariables.Count;
			int mobj = objectives.Count;

			bool status = (mobj + ndes) < ndes ? false : true;
			Vector<double> inputs = Vector<double>.Build.Dense(ndes);
			Vector<double> outputs = Vector<double>.Build.Dense(mobj);

			// Obtain initial values and parameters
			SetInitialConditionsAndparameters(inputs);

			using (var csvFile = new CSVFiler(CsvPath))
			{
				// Initial evaluation
				ExecuteComponent(inputs, outputs);

				double newNorm = outputs.L2Norm();
				//outputString += GetOutputString(d => d.ValueAsString);
				WriteCsvRow(csvFile);

				int iterationsWithoutImprovement = 0;
				int iterations = 0;
				for (; iterations < MaxIterations; iterations++)
				{
					Matrix<double> J = JacobianCalculator(inputs, outputs, PercentageOfDifference);
					Vector<double> step = J.TransposeThisAndMultiply(J).Cholesky().Solve(J.TransposeThisAndMultiply(outputs));

					/* Vector<double> nextInput = inputs - step;
					if (checkforbounds(nextInput, (input_options as cTreatment_InOut_OP_Input).setuplist, ndescount) == false)
					{
						//diffperc = 0.01;
						inp[0] = inp[0] + 1;
						diff = diffcalculator(diffperc, inp, ndescount);
						continue;
					}*/

					// Take the step
					inputs -= step;

					// Function evaluation
					ExecuteComponent(inputs, outputs);
					//outputString += GetOutputString(d => d.ValueAsString);
					WriteCsvRow(csvFile);

					// Check if the objective function has been improved
					double oldNorm = newNorm;
					newNorm = outputs.L2Norm();
					if (newNorm < oldNorm)
					{
						iterationsWithoutImprovement = 0;
					}
					else
					{
						iterationsWithoutImprovement++;
						if (iterationsWithoutImprovement >= 10)
						{
							Console.WriteLine("10 consecutive iterations without improvig the objective function");
							break;
						}
					}
				}

				if (iterations >= MaxIterations - 1)
					Console.WriteLine("Reached 4999 iterations in Gauss-Newton Method");
			}

			return status;
		}

		private void ExecuteComponent(Vector<double> inputs, Vector<double> outputs)
		{
			// Set Inputs
			for (int i = 0; i < designVariables.Count; i++)
				designVariables[i].Data.Value = inputs[i];

			//Execute
			Component.Execute();

			// Get Outputs
			for (int i = 0; i < objectives.Count; i++)
				outputs[i] = objectives[i].Data.GetValueAsDouble();
		}

		private void SetInitialConditionsAndparameters(Vector<double> inputs)
		{
			// Initial Values
			for (int i = 0; i < designVariables.Count; i++)
				inputs[i] = designVariables[i].InitialValue;

			//Parameters
			foreach (Parameter parameter in parameters)
				parameter.Data.ValueAsString = parameter.Value;
		}

		private string GetOutputString(Func<Data, string> infoExtractor) => Component.GetAllData().Aggregate(String.Empty, (t, d) => t += infoExtractor(d) + " ") + "\n";
		private void WriteCsvRow(CSVFiler csv)
		{
			csv.NewRow();

			IEnumerable<Data> allData = parameters.Select(p => p.Data)
				.Concat(designVariables.Select(dv => dv.Data))
				.Concat(objectives.Select(o => o.Data));

			foreach (Data data in allData)
				csv.AddToRow(data);

			csv.WriteRow();
		}

		/// <summary>
		/// Calculates the perturbations to be introduced inorder to calculate the finite differences
		/// </summary>
		/// <param name="differencePercentage"></param>
		/// <param name="inp"></param>
		/// <param name="ndescount"></param>
		/// <returns></returns>
		private Vector<double> DifferencesCalculator(Vector<double> inputs, double differencePercentage) => inputs.Map(e => e * differencePercentage);
		/// <summary>
		/// Calculates jacobian 
		/// </summary>
		/// <param name="inputs"></param>
		/// <param name="mobj"></param>
		/// <param name="Component"></param>
		/// <param name="ndescount"></param>
		/// <param name="mobjcount"></param>
		/// <param name="diff"></param>
		/// <returns></returns>
		private Matrix<double> JacobianCalculator(Vector<double> inputs, Vector<double> outputs, double differencesPercentage)
		{
			Vector<double> differences = DifferencesCalculator(inputs, differencesPercentage);

			int ndes = designVariables.Count;
			int mobj = objectives.Count;
			Matrix<double> Jacob = Matrix<double>.Build.Dense(mobj, ndes);
			Vector<double> gradient = Vector<double>.Build.Dense(ndes);

			for (int ncol = 0; ncol < ndes; ncol++)
				SetGradinetinJacobian(ncol, differences[ncol]);

			return Jacob;

			void SetGradinetinJacobian(int ncol, double difference)
			{
				double temp = inputs[ncol];
				inputs[ncol] = temp + difference;

				ExecuteComponent(inputs, outputs);

				//inp[ncol] = tmpin - diff;
				//Data.SetValuesDouble(Component.ModelDataInputs, inp);
				//Component.Execute();
				//outp = Data.GetValuesDouble(Component.ModelDataOutputs);
				//grad = grad - outp[nrow];
				//grad = grad / (diff + diff);
				inputs[ncol] = temp;

				Jacob.SetColumn(ncol, (gradient - outputs) / difference);
			}
		}

		public override void CreateFolder()
		{
			base.CreateFolder();

			// Add custom metadata for Latin Hypercube
			metadata.AddAttribute("Type", "DesignsStudy");
			metadata.AddAttribute("WorkflowName", Component.Name);

			metadata.AddTag("Name", "Optimization");
			metadata.AddTag("Algorithm", "Gauss-Newton");

			metadata.AddParameter(new IntegerData("ID"));

			foreach (Parameter parameter in parameters)
				metadata.AddParameter(parameter.Data, "OptimizationParameter",
					("Value", parameter.Value));

			foreach (UnboundedDesignVariable variable in designVariables)
				metadata.AddParameter(variable.Data, "OptimizationDesignVariable",
					("InitialValue", variable.InitialValue));

			foreach (Objective objective in objectives)
				metadata.AddParameter(objective.Data, "OptimizationObjective",
					("Type", objective.Type));

			metadata.Save();
		}
	}
}


