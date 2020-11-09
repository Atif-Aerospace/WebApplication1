using System;
using System.Collections.Generic;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Treatments.Optimisers.Formulation
{
	[Serializable()]
	public class OptimisationTemplateBase
	{
		public OptimisationTemplateBase()
		{
			DesignVariables = new List<DesignVariable>();
			Parameters = new List<Parameter>();
			Objectives = new List<Objective>();
			Constraints = new List<Constraint>();
		}

		public OptimisationTemplateBase(List<DesignVariable> designVariables, List<Parameter> parameters, List<Objective> objectives, List<Constraint> constraints, ExecutableComponent executableComponent)
        {
            DesignVariables = designVariables;
			Parameters = parameters;
            Objectives = objectives;
            Constraints = constraints;
            ExecutableComponent = executableComponent;
        }

		[SerializeEnumerable("Variables")]
		public List<DesignVariable> DesignVariables { get; set; }

		[SerializeEnumerable("Parameters")]
		public List<Parameter> Parameters { get; set; }

		[SerializeEnumerable("Objectives")]
		public List<Objective> Objectives { get; set; }

		[SerializeEnumerable("Constraints")]
		public List<Constraint> Constraints { get; set; }

		[Serialize("Component", SerializationType.Reference)]
		public ExecutableComponent ExecutableComponent { get; set; }
	}

	public class OptimisationTemplate
	{
		public OptimisationTemplate()
		{
			DesignVariables = new List<BoundedDesignVariableNoInital>();
			Parameters = new List<Parameter>();
			Objectives = new List<Objective>();
			Constraints = new List<Constraint>();
		}

		public OptimisationTemplate(List<BoundedDesignVariableNoInital> designVariables, List<Parameter> parameters, List<Objective> objectives, List<Constraint> constraints, ExecutableComponent executableComponent)
		{
			DesignVariables = designVariables;
			Parameters = parameters;
			Objectives = objectives;
			Constraints = constraints;
			ExecutableComponent = executableComponent;
		}

		public List<BoundedDesignVariableNoInital> DesignVariables { get; set; }

		public List<Parameter> Parameters { get; set; }

		public List<Objective> Objectives { get; set; }

		public List<Constraint> Constraints { get; set; }

		public ExecutableComponent ExecutableComponent { get; set; }
	}
}
