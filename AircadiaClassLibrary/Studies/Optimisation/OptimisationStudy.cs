using System;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Treatments;
using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.NSGAII;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.SGA;
using Aircadia.Numerics.Optimizers;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Studies
{
	public class OptimisationStudy : Study
	{
		[Serialize]
		public OptimisationTemplateBase OptimisationTemplate { get; }

		[Serialize]
		public IMinimizer Minimizer { get; private set; }

		public OptimisationStudy(string name, string description, string parentName = "")
			: base(name, null, null, description, parentName)
		{
		}

		[DeserializeConstructor]
		public OptimisationStudy(string name, string description, WorkflowComponent studiedComponent, OptimisationTemplateBase optimisationTemplate, IMinimizer minimizer, string parentName = "")
			: this(name, description, parentName)
		{
			StudiedComponent = studiedComponent;
			Minimizer = minimizer;
			OptimisationTemplate = optimisationTemplate;

			Treatment = new SingleObjectiveUnboundedOptimizer("Optimiser", optimisationTemplate, Minimizer);

			Treatment.CreateFolder();
		}


		public override bool Execute()
		{
			bool status = true;

			if (Treatment is NSGAIIOptimiser || Treatment is SGAOptimizer)
			{
				throw new NotImplementedException("NSGAII or SGA  used a GUI to define some parameters, removing this dependencies needs to be implemented");

				//// GA setup
				//var gaSetup = new GASetup();
				//gaSetup.ShowDialog();
				//GAParameters gaParameters = gaSetup.GAParameters;
				//gaParameters.EvaluatedSolutionsFile = Path.Combine(AircadiaProject.ProjectPath, "evaluatedSolutions.txt");
				//((NSGAII)(Treatment)).GAParameters = gaParameters;
			}

			Treatment.Result = ActiveResult;

			status = Treatment.ApplyOn();

			return status;
		}

		public override Study Copy(string id, string name = null, string parentName = null)
		{
			return new OptimisationStudy(id, Description, StudiedComponent as WorkflowComponent, OptimisationTemplate, Minimizer, parentName ?? Name);
		}
	}
}
