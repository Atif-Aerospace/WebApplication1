using System;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;
using MathNet.Numerics;
using static System.Math;

namespace Aircadia.ObjectModel.Distributions
{
	public class NormalDistribution : IProbabilityDistribution, IMCSDistribution, IFASTDistribution, IRoustOptimizationDistribution
	{
		[Serialize]
		[Parameter]
		public double StandarDeviation { get; protected set; }

		public NormalDistribution()
		{
		}

		public NormalDistribution(Data data)
		{
			Data = data;
		}

		[DeserializeConstructor]
		public NormalDistribution(Data data, double mean, double standarDeviation) : this(data)
		{
			Mean = mean;
			StandarDeviation = standarDeviation;
		}

		

		public int NParameters => 2;

		[Serialize]
		[Parameter]
		public double Mean { get; protected set; }

		public double Variance => StandarDeviation * StandarDeviation;

		public double Skewness => 0.0;

		public double Kurtosis => 3.0;

		[Serialize(Type = SerializationType.Reference)]
		public Data Data { get; protected set; }

		public string Name => Data.Name;

		public void Update(double[] paramerers)
		{
			int Nupdates = Math.Min(paramerers.Length, NParameters);
			if (Nupdates > 0)
				Mean = paramerers[0];
			if (Nupdates > 1)
				StandarDeviation = paramerers[1];
		}

		public double[] GetSamples(int N)
		{
			double[] samples = new double[N];
			MathNet.Numerics.Distributions.Normal.Samples(samples, Mean, StandarDeviation);
			return samples;
		}

		public double[] GetSamples(int N, double[] FASTSamples)
		{
			/*
			Approximation from
			author = {Dongbin Xiu and George E M Karniadakis},
			title = {The Wiener-Askey Polynomial Chaos for Stochastic Differential Equations},
			journal = {SIAM J. SCI. COMPUT},
			year = {2002},
			pages = {619--644}
			*/
			double c0 = 2.515517;
			double c1 = 0.802853;
			double c2 = 0.010328;
			double d1 = 1.432788;
			double d2 = 0.189269;
			double d3 = 0.001308;

			double[] T = Generate.Map(FASTSamples, s => Sqrt(-Log(Pow(Min(s, 1 - s), 2))));
			double[] X = Generate.Map2(FASTSamples, T, (s, t) => Sign(s - 0.5) * (t - (c0 + c1 * t + c2 * Pow(t, 2)) / (1 + d1 * t + d2 * Pow(t, 2) + d3 * Pow(t, 3))));
			return Generate.Map(X, x => Mean + StandarDeviation * x); 
		}

		public (double lower, double upper) GetBoundExtensions(double satisfactionProbability)
		{
			double upper = Sqrt(Variance) * GetK(satisfactionProbability);
			double lower = -upper;

			return (lower, upper);
		}

		public double GetK(double satisfactionProbability)
		{
			double a = 8 * (PI - 3) / (3 * PI * (4 - PI));
			double x = 2 * satisfactionProbability - 1;
			double erf_inv_apprx = x / Abs(x) * Sqrt((Sqrt(Pow(2 / (PI * a) + Log(1 - Pow(x, 2)) / 2, 2) - Log(1 - Pow(x, 2)) / a)) - (2 / (PI * a) + Log(1 - Pow(x, 2)) / 2));
			return Sqrt(2) * erf_inv_apprx;
		}

		private readonly Random random = new Random();
	}
}
