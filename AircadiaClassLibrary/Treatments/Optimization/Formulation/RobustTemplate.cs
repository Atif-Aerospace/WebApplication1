using System.Collections.Generic;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Treatments.Optimisers.Formulation
{
	public class RobustTemplate
	{
		public RobustTemplate()
		{
			DesignVariables = new List<RobustDesignVariable>();
			Parameters = new List<Parameter>();
			Objectives = new List<Objective>();
			Constraints = new List<Constraint>();
		}

		public RobustTemplate(List<RobustDesignVariable> designVariables, List<Parameter> parameters, List<Objective> objectives, List<Constraint> constraints, ExecutableComponent executableComponent)
		{
			DesignVariables = designVariables;
			Parameters = parameters;
			Objectives = objectives;
			Constraints = constraints;
			ExecutableComponent = executableComponent;
		}

		[SerializeEnumerable("Variables")]
		public List<RobustDesignVariable> DesignVariables { get; set; }

		[SerializeEnumerable("Parameters")]
		public List<Parameter> Parameters { get; set; }

		[SerializeEnumerable("Objectives")]
		public List<Objective> Objectives { get; set; }

		[SerializeEnumerable("Constraints")]
		public List<Constraint> Constraints { get; set; }

		[Serialize("Component", SerializationType.Reference)]
		public ExecutableComponent ExecutableComponent { get; set; }
	}
}
