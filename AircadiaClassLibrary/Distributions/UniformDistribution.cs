using System;
using static System.Math;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Distributions
{
	public class UniformDistribution : IProbabilityDistribution, IMCSDistribution, IFASTDistribution
	{
		[Serialize]
		[Parameter]
		public double LowerBound { get; protected set; }

		[Serialize]
		[Parameter]
		public double UpperBound { get; protected set; }

		public UniformDistribution()
		{
		}

		public UniformDistribution(Data data)
		{
			Data = data;
		}

		[DeserializeConstructor]
		public UniformDistribution(Data data, double lowerBound, double upperBound) : this(data)
		{
			LowerBound = lowerBound;
			UpperBound = upperBound;
		}

		

		public int NParameters => 2;

		public double Mean => (UpperBound + LowerBound) / 2.0;

		public double Variance => (UpperBound - LowerBound) * (UpperBound - LowerBound) / 12.0;

		public double Skewness => 0.0;

		public double Kurtosis => 9.0 / 5.0;

		[Serialize(Type = SerializationType.Reference)]
		public Data Data { get; protected set; }

		public string Name => Data.Name;

		public void Update(double[] paramerers)
		{
			int Nupdates = Min(paramerers.Length, NParameters);
			if (Nupdates > 0)
				LowerBound = paramerers[0];
			if (Nupdates > 1)
				UpperBound = paramerers[1];
		}

		public double[] GetSamples(int N)
		{
			double[] Samples = new double[N];

			double Δ = UpperBound - LowerBound;
			for (int i = 0; i < N; i++)
				Samples[i] = LowerBound + random.NextDouble() * Δ;

			return Samples;
		}

		public double[] GetSamples(int N, double[] FASTSamples)
		{
			double[] Samples = new double[N];

			double Δ = UpperBound - LowerBound;
			for (int i = 0; i < N; i++)
				Samples[i] = LowerBound + FASTSamples[i] * Δ;

			return Samples;
		}

		public (double lower, double upper) GetBoundExtensions(double satisfactionProbability) => throw new NotImplementedException();
		public double GetK(double satisfactionProbability) => throw new NotImplementedException();

		private readonly Random random = new Random();
	}
}
