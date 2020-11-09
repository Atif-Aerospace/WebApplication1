using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Distributions;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Treatments.Optimisers.Formulation
{
	public class RobustDesignVariable : BoundedDesignVariableNoInital
	{
		[Serialize]
		public double Probability { get; }
		[Serialize]
		public IRoustOptimizationDistribution Distribution { get; }

		[DeserializeConstructor]
		public RobustDesignVariable(ScalarData data, double lowerBound, double upperBound, double probability, IRoustOptimizationDistribution distribution) 
			: base(data)
		{
			(double lower, double upper) = distribution.GetBoundExtensions(probability);
			LowerBound = lowerBound + lower;
			UpperBound = upperBound + upper;
			Probability = probability;
			Distribution = distribution;
		}
	}
}
