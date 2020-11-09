using System;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;


namespace Aircadia.ObjectModel.Distributions
{
	public class GaussianMixtureDistribution : IProbabilityDistribution, IMCSDistribution
	{
		public double StandarDeviation { get; protected set; }

		public GaussianMixtureDistribution()
		{
		}

		public GaussianMixtureDistribution(Data data)
		{
			Data = data;
		}

		[DeserializeConstructor]
		public GaussianMixtureDistribution(Data data, double mean1, double standarDeviation1, double mean2, double standardDeviation2, double alpha1) : this(data)
		{
			Mean1 = mean1;
			StandardDeviation1 = standarDeviation1;
			Mean2 = mean2;
			StandardDeviation2 = standardDeviation2;
			Alpha1 = alpha1;
		}

		public int NParameters => 5;

		public double Mean => Alpha1 * Mean1 - Alpha2 * Mean2;

		public double Variance => StandarDeviation * StandarDeviation;

		public double Skewness => 0.0;

		public double Kurtosis => 3.0;


		[Serialize(Type = SerializationType.Reference)]
		public Data Data { get; protected set; }

		public string Name => Data.Name;


		[Serialize]
		[Parameter]
		public double Mean1 { get; protected set; }

		[Serialize]
		[Parameter]
		public double StandardDeviation1 { get; protected set; }

		[Serialize]
		[Parameter]
		public double Mean2 { get; protected set; }

		[Serialize]
		[Parameter]
		public double StandardDeviation2 { get; protected set; }

		[Serialize]
		[Parameter]
		public double Alpha1 { get; protected set; }

		public double Alpha2 => 1 - Alpha1;


		public void Update(double[] paramerers)
		{
			int Nupdates = Math.Min(paramerers.Length, NParameters);
			if (Nupdates > 0)
				Mean1 = paramerers[0];
			if (Nupdates > 1)
				StandardDeviation1 = paramerers[1];
			if (Nupdates > 2)
				Mean2 = paramerers[2];
			if (Nupdates > 3)
				StandardDeviation2 = paramerers[3];
			if (Nupdates > 4)
				Alpha1 = paramerers[4];
		}

		public double[] GetSamples(int N)
		{

			double c0 = 2.515517;
			double c1 = 0.802853;
			double c2 = 0.010328;
			double d1 = 1.432788;
			double d2 = 0.189269;
			double d3 = 0.001308;

			double mu1 = Mean1;
			double mu2 = Mean2;
			double sigma1 = StandardDeviation1;
			double sigma2 = StandardDeviation2;
			double[] Samples = new double[N];

			for (int i = 0; i < N; i++)
			{

				if (Samples[i] <= Alpha1)
				{
					double u = Samples[i] / Alpha1;
					double t = Math.Sqrt(-Math.Log(Math.Pow(Math.Min(u, 1 - u), 2)));
					double t2 = Math.Pow(t, 2);
					double t3 = Math.Pow(t, 3);
					double c = c0 + c1 * t + c2 * t2;
					double d = 1 + d1 * t + d2 * t2 + d3 * t3;
					double X = Math.Sign(u - 0.5) * (t - c / d);
					Samples[i] = mu1 + X * sigma1;
				}
				else
				{
					double u = (Samples[i] - (Alpha1 + 1e-8)) / Alpha2;
					double t = Math.Sqrt(-Math.Log(Math.Pow(Math.Min(u, 1 - u), 2)));
					double t2 = Math.Pow(t, 2);
					double t3 = Math.Pow(t, 3);
					double c = c0 + c1 * t + c2 * t2;
					double d = 1 + d1 * t + d2 * t2 + d3 * t3;
					double X = Math.Sign(u - 0.5) * (t - c / d);
					Samples[i] = mu2 + X * sigma2;
				}
			}

			return Samples;
		}

		public double[] GetSamples(int N, double[] FASTSamples) => throw new NotImplementedException();
		public (double lower, double upper) GetBoundExtensions(double satisfactionProbability) => throw new NotImplementedException();
		public double GetK(double satisfactionProbability) => throw new NotImplementedException();

		private readonly Random random = new Random();
	}
}
