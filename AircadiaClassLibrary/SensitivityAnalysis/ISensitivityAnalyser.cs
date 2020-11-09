using System.Collections.Generic;
using Aircadia.ObjectModel.Distributions;
using Aircadia.ObjectModel.Models;

namespace Aircadia.ObjectModel
{
	public interface ISensitivityAnalyser
	{
		void Analyse(List<IProbabilityDistribution> inputDistributions, List<IProbabilityDistribution> outputDistributions, WorkflowComponent innerWorkflow);
	}
}