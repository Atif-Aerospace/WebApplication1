using System;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Distributions
{
	public class ProbabilityDistribution : IProbabilityDistribution, IRoustOptimizationDistribution
	{
		public ProbabilityDistribution()
		{
		}

		public ProbabilityDistribution(Data data)
		{
			Data = data;
		}

		[DeserializeConstructor]
		public ProbabilityDistribution(Data data, double mean, double variance, double skewness, double kurtosis, Assumption distributionAssumption = Assumption.Normality, Metric distributionMetric = Metric.Quantile) : this(data)
		{
			Mean = mean;
			Variance = variance;
			Skewness = skewness;
			Kurtosis = kurtosis;
			DistributionAssumption = distributionAssumption;
			DistributionMetric = distributionMetric;
		}

		[Serialize]
		[Parameter]
		public double Mean { get; protected set; }

		[Serialize]
		[Parameter]
		public double Variance { get; protected set; }

		[Serialize]
		[Parameter]
		public double Skewness { get; protected set; }

		[Serialize]
		[Parameter]
		public double Kurtosis { get; protected set; }

		[Serialize]
		public Assumption DistributionAssumption { get; }

		[Serialize]
		public Metric DistributionMetric { get; }

		public int NParameters => 4;

		[Serialize(Type = SerializationType.Reference)]
		public Data Data { get; protected set; }

		public string Name => Data.Name;

		public (double lower, double upper) GetBoundExtensions(double satisfactionProbability)
		{
			double upper = Math.Sqrt(Variance) * GetK(satisfactionProbability);
			double lower = -upper;

			return (lower, upper);
		}

		public double GetK(double satisfactionProbability)
		{
			double k = 0;
			switch (DistributionAssumption)
			{
				case Assumption.None:
					k = Math.Sqrt(satisfactionProbability / (1 - satisfactionProbability));
					break;
				case Assumption.Normality:
					k = new NormalDistribution(Data, Mean, Math.Sqrt(Variance)).GetK(satisfactionProbability);
					break;
				case Assumption.Symmetry:
					if (satisfactionProbability >= 0.5)
						k = 1 / Math.Sqrt(2 * (1 - satisfactionProbability));
					break;
				case Assumption.Unimodality:
					if (satisfactionProbability >= 5 / 6)
						k = Math.Sqrt((9 * satisfactionProbability - 5) / (9 * (1 - satisfactionProbability)));
					else if (satisfactionProbability < 5 / 6)
						k = Math.Sqrt((3 * satisfactionProbability) / (4 - 3 * satisfactionProbability));
					break;
				case Assumption.SymmetryPlusUnimodlaity:
					if (satisfactionProbability >= 0.5)
						k = Math.Sqrt(2 / (9 * (1 - satisfactionProbability)));
					break;
			}
			return k;
		}

		public double[] GetSamples(int N) => new double[N];
		public double[] GetSamples(int N, double[] FASTSamples) => throw new NotImplementedException();

		public void Update(double[] paramerers)
		{
			int Nupdates = Math.Min(paramerers.Length, NParameters);
			if (Nupdates > 0)
				Mean = paramerers[0];
			if (Nupdates > 1)
				Variance = paramerers[1];
			if (Nupdates > 2)
				Skewness = paramerers[2];
			if (Nupdates > 3)
				Kurtosis = paramerers[3];
		}

		public enum Assumption { None, Normality, Symmetry, Unimodality, SymmetryPlusUnimodlaity };

		public enum Metric { Quantile, TailConditionalExpectation };
	}
}
