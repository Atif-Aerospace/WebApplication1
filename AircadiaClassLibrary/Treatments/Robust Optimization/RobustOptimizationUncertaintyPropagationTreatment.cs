/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections.Generic;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;
using Aircadia.ObjectModel.Distributions;
using System.Linq;

namespace Aircadia.ObjectModel.Treatments
{
	[Serializable()]
	public class RobustOptimizationUncertaintyPropagationTreatment : Treatment
	{
		public ExecutableComponent ExecutableComponent { get => template.ExecutableComponent; private set => template.ExecutableComponent = value; }
		public string[,] RobOutput { get; private set; }
		public string[,] RobEvaluation { get; private set; }
		public RobustTemplate template { get; }

		public RobustOptimizationUncertaintyPropagationTreatment(string name, RobustTemplate template) :
			 base(name, name)
		{
			this.template = template;
			ExecutableComponent = template.ExecutableComponent;
		}

		public RobustOptimizationUncertaintyPropagationTreatment(string name, List<RobustDesignVariable> designVariables, List<Parameter> parameters, List<Objective> objectives, List<Constraint> constraints, ExecutableComponent executableComponent) : this(name, new RobustTemplate(designVariables, parameters, objectives, constraints, executableComponent))
		{
		}

		public override bool ApplyOn(ExecutableComponent component)
		{
			if (component != null)
			{
				ExecutableComponent = component;
				return ApplyOn(); 
			}
			return false;
		}
		public override bool ApplyOn()
		{
			var allUncertainIn = template.DesignVariables.Select(dv => dv.Distribution)
				.Concat(template.Parameters.Where(c => c is RobustParameter).Select(c => (c as RobustParameter).Distribution))
				.ToList();
			var allNotUncertain = template.Parameters.Where(c => !(c is RobustParameter)).ToList();
			var allUncertainOut = ExecutableComponent.ModelDataOutputs.Select(o => new ProbabilityDistribution(o) as IProbabilityDistribution).ToList();

			foreach (Parameter parameter in allNotUncertain)
				parameter.Data.ValueAsString = parameter.Value;

			var urq = new UnivariateReducedQuadrature();
			urq.Propagate(allUncertainIn, allUncertainOut, ExecutableComponent as WorkflowComponent);

			int Nobjectives = template.Objectives.Count;
			int NConstraints = template.Constraints.Count;

			// Storage of the robust solution for the selected m objective(s) and n constraint(s). It is done via the RObOutput Matrix: the first column contains the names of the user-desired outputs, the second their corresponding value. The first m rows correspond to the m selected objectives, whereas the remaining n rows are related to the n selected constraints.
			RobOutput = new string[Nobjectives + NConstraints, 2]; // [Name, Value]
			RobEvaluation = new string[Nobjectives + NConstraints, 2]; // [Mean, Variance]

			var distributionsOut = allUncertainOut.ToDictionary(d => d.Name);
			for (int i = 0; i < template.Objectives.Count; i++)
			{
				Objective objective = template.Objectives[i];
				IProbabilityDistribution distribution = distributionsOut[objective.Name];
				if (objective is WeightLossFunctionObjective wlo)
				{
					objective.Data.Value = distribution.Mean + wlo.Sign * wlo.Weight * Math.Sqrt(distribution.Variance);
				}
				else
				{
					objective.Data.Value = distribution.Mean;
				}
				RobOutput[i, 0] = objective.Name;
				RobOutput[i, 1] = Convert.ToString(objective.Data.Value);
				RobEvaluation[i, 0] = Convert.ToString(distribution.Mean);
				RobEvaluation[i, 1] = Convert.ToString(distribution.Variance);
			}
			for (int i = 0; i < template.Constraints.Count; i++)
			{
				Constraint constraint = template.Constraints[i];
				IProbabilityDistribution distribution = distributionsOut[constraint.Name];
				int i2 = i + Nobjectives;
				if (constraint is WeightLossFunctionConstraint wlc)
				{
					constraint.Data.Value = distribution.Mean + wlc.Sign * wlc.Weight * Math.Sqrt(distribution.Variance);
				}
				else
				{
					constraint.Data.Value = distribution.Mean;
				}
				RobOutput[i2, 0] = constraint.Name;
				RobOutput[i2, 1] = Convert.ToString(constraint.Data.Value);
				RobEvaluation[i2, 0] = Convert.ToString(distribution.Mean);
				RobEvaluation[i2, 1] = Convert.ToString(distribution.Variance);
			}

			return true;
		}
	}
}
