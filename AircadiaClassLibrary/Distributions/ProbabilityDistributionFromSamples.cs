using System;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Distributions
{
	public class ProbabilityDistributionFromSamples : IProbabilityDistribution, IMCSDistribution
	{
		public ProbabilityDistributionFromSamples()
		{
		}

		public ProbabilityDistributionFromSamples(Data data)
		{
			Data = data;
			Samples = new double[] { 0 };
		}

		[DeserializeConstructor]
		public ProbabilityDistributionFromSamples(Data data, double[] samples) : this(data)
		{

			Update(samples);
			Samples = samples ?? new double[] { 0 };
		}

		[Parameter]
		public double Mean { get; protected set; }

		[Parameter]
		public double Variance { get; protected set; }

		[Parameter]
		public double Skewness { get; protected set; }

		[Parameter]
		public double Kurtosis { get; protected set; }

		public int NParameters => 4;

		[Serialize(Type = SerializationType.Reference)]
		public Data Data { get; protected set; }

		public string Name => Data.Name;

		public string SamplesString => DoubleVectorData.ValueToString(Samples);
		[Serialize]
		public double[] Samples { get; }

		public (double lower, double upper) GetBoundExtensions(double satisfactionProbability) => throw new NotImplementedException();
		public double GetK(double satisfactionProbability) => throw new NotImplementedException();
		public double[] GetSamples(int N) => Samples;
		public double[] GetSamples(int N, double[] FASTSamples) => throw new NotImplementedException();

		public void Update(double[] paramerers)
		{
			if (paramerers.Length > 1)
			{

				double mean = paramerers[0];
				double M2 = 0;
				double M3 = 0;
				double M4 = 0;
				int n = 1;
				while (n < paramerers.Length)
				{
					double y = paramerers[n];
					n++;
					double δ1 = y - mean;
					double δ1_n = δ1 / n; 
					double n1δδ1_n = (n - 1) * δ1 * δ1_n;
					double δ1δ_nn = δ1_n * δ1_n; 

					M4   += n1δδ1_n * δ1δ_nn * (n * n - 3 * n + 3)	+ 6 * M2 * δ1δ_nn - 4 * M3 * δ1_n;
					M3   += n1δδ1_n * δ1_n   * (n - 2)				- 3 * M2 * δ1_n;
					M2   += n1δδ1_n;
					mean += δ1_n;
				}

				Mean = mean;
				Variance = M2 / n;
				Skewness = Math.Sqrt(n) * M3 / Math.Pow(M2, 3.0 / 2.0);
				Kurtosis = (n * M4) / (M2 * M2);// - 3
			}
			else
			{
				Mean = paramerers[0];
				Variance = 0;
				Skewness = 0;
				Kurtosis = 3;
			}
		}
	}
}
