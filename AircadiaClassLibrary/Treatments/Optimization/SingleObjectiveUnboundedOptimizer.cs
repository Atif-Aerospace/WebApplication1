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
using MathNet.Numerics.Optimization;
using Aircadia.Numerics.Optimizers;

namespace Aircadia
{
	[Serializable()]
	public class SingleObjectiveUnboundedOptimizer : Treatment
	{
		private WorkflowComponent component;

		private readonly List<UnboundedDesignVariable> designVariables;
		private readonly List<Parameter> parameters;
		private readonly List<SingleObjective> objectives;

		private readonly IMinimizer minimizer;


		/// <summary>
		/// Constructer
		/// </summary>
		/// <param name="name"></param>
		/// <param name="input_opt"></param>
		/// <param name="outp"></param>
		public SingleObjectiveUnboundedOptimizer(string name, OptimisationTemplateBase template, IMinimizer minimizer) : base(name, "")
		{
			component = template.ExecutableComponent as WorkflowComponent;

			designVariables = template.DesignVariables.Cast<UnboundedDesignVariable>().ToList();
			parameters = template.Parameters;
			objectives = template.Objectives.Cast<SingleObjective>().ToList();

			this.minimizer = minimizer;
		}

		/// <summary>
		/// exectues the optimiser study
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		public override bool ApplyOn(ExecutableComponent component)
		{
			this.component = component as WorkflowComponent;
			return ApplyOn();
		}
		public override bool ApplyOn()
		{
			//string outputString = GetOutputString(d => d.Name);
			using (var csvFile = new CSVFiler(CsvPath))
			{
				Vector<double> x0 = SetInitialConditionsAndparameters();
				WriteCsvRow(csvFile);

				//MinimizationResult result = bfgsMinimizer.FindMinimum(objectiveFunctionWithGradient, x0);
				MinimizationResult result = minimizer.FindMinimum(ObjectiveFunction, x0);

				SetOptimum(result.MinimizingPoint);
				WriteCsvRow(csvFile);

				return result.ReasonForExit == ExitCondition.Converged; 
			}
		}

		private Vector<double> SetInitialConditionsAndparameters()
		{
			//Parameters
			foreach (Parameter parameter in parameters)
				parameter.Data.ValueAsString = parameter.Value;

			// Initial Values
			Vector<double> inputs = Vector<double>.Build.DenseOfEnumerable(designVariables.Select(dv => dv.InitialValue));

			return inputs;
		}

		private void SetOptimum(Vector<double> optimum)
		{
			for (int i = 0; i < designVariables.Count; i++)
				designVariables[i].Value = optimum[i];
		}

		public double ObjectiveFunction(Vector<double> inputs)
		{
			// Set Inputs
			for (int i = 0; i < designVariables.Count; i++)
				designVariables[i].Data.Value = inputs[i];

			//Execute
			component.Execute();

			// Get Outputs
			double output = 0;
			foreach (SingleObjective objective in objectives)
			{
				double sign = objective.Type == ObjectiveType.Minimise ? 1.0 : -1.0;
				output += sign * objective.Weight * objective.Data.GetValueAsDouble();
			}
			return output;
		}


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

		public override void CreateFolder()
		{
			base.CreateFolder();

			// Add custom metadata for Latin Hypercube
			metadata.AddAttribute("Type", "DesignsStudy");
			metadata.AddAttribute("WorkflowName", component.Name);

			metadata.AddTag("Name", "Optimization");
			metadata.AddTag("Algorithm", "BFGS");

			metadata.AddParameter(new IntegerData("ID"));

			foreach (Parameter parameter in parameters)
				metadata.AddParameter(parameter.Data, "OptimizationParameter", 
					("Value", parameter.Value));

			foreach (UnboundedDesignVariable variable in designVariables)
				metadata.AddParameter(variable.Data, "OptimizationDesignVariable",
					("InitialValue", variable.InitialValue));

			foreach (SingleObjective objective in objectives)
				metadata.AddParameter(objective.Data, "OptimizationObjective",
					("Type", objective.Type),
					("Weight", objective.Weight));

			metadata.Save();
		}
	}
}


