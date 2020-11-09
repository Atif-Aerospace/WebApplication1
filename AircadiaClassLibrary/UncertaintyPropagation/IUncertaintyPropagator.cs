using System.Collections.Generic;
using Aircadia.ObjectModel.Distributions;
using Aircadia.ObjectModel.Models;
using MathNet.Numerics.LinearAlgebra;

namespace Aircadia
{
	public interface IUncertaintyPropagator
	{
		void Propagate(List<IProbabilityDistribution> inputDistributions, List<IProbabilityDistribution> outputDistributions, WorkflowComponent innerWorkflow);
	}

	public interface IUncertaintyPropagatorBySamples : IUncertaintyPropagator
	{
		Matrix<double> Samples { get; }
	}
}