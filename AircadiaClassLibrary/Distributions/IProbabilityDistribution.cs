using Aircadia.ObjectModel.DataObjects;

namespace Aircadia.ObjectModel.Distributions
{
	public interface IProbabilityDistribution : INamedComponent
	{
		Data Data { get; }

		void Update(double[] paramerers);
	
		double Mean { get; }
		double Variance { get; }
		double Skewness { get; }
		double Kurtosis { get; }
	}

	public interface IMCSDistribution : IProbabilityDistribution
	{
		double[] GetSamples(int N);
	}

	public interface IFASTDistribution : IProbabilityDistribution
	{
		double[] GetSamples(int N, double[] FASTSamples);
	}

	public interface IRoustOptimizationDistribution : IProbabilityDistribution
	{
		(double lower, double upper) GetBoundExtensions(double satisfactionProbability);

		double GetK(double satisfactionProbability);
	}
}
