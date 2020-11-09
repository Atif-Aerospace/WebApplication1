using Aircadia.ObjectModel.Distributions;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Treatments.Optimisers.Formulation
{
	public class RobustParameter : Parameter
	{
		[Serialize]
		public IProbabilityDistribution Distribution { get; }

		public RobustParameter(IProbabilityDistribution distribution) : base(distribution.Data)
		{
			Distribution = distribution;
		}
	}
}
