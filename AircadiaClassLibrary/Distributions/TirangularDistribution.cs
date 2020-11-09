using System;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Distributions
{
    /// <summary>
    /// Triangluar distribution implemented using the formulae from 
    /// http://mathworld.wolfram.com/TriangularDistribution.html
    /// LowerBound = a, Upprebound = b, Mode = c
    /// </summary>
	public class TriangularDistribution : IProbabilityDistribution, IMCSDistribution, IRoustOptimizationDistribution
	{
		[Serialize]
		[Parameter]
		public double Mode { get; protected set; }
		[Serialize]
		[Parameter]
		public double LowerBound { get; protected set; }
		[Serialize]
		[Parameter]
		public double UpperBound { get; protected set; }

		public TriangularDistribution()
		{
		}

		public TriangularDistribution(Data data)
		{
			Data = data;
		}

		[DeserializeConstructor]
		public TriangularDistribution(Data data, double nominalValue, double lowerBound, double upperBound)
		{
			Data = data;
			Mode = nominalValue;
			LowerBound = lowerBound;
			UpperBound = upperBound;
		}


		public virtual int NParameters => 3;

		public double Mean => (LowerBound + Mode + UpperBound) / 3;

		public double Variance
		{
			get
			{
				double ul2 = Math.Pow(UpperBound - LowerBound, 2);
				double nl = Mode - LowerBound;
				double un = Mode - UpperBound;
				return (ul2 + nl * un) / 18;
			}
		}

		public double Skewness
		{
			get
			{
                double f1 = LowerBound + UpperBound - 2 * Mode; 
                double f2 = LowerBound + Mode - 2 * UpperBound; 
                double f3 = UpperBound + Mode - 2 * LowerBound; 
				return - f1 * f2 * f3 / 270;  // This one (mu3) 
                //return -f1 * f2 * f3 / 270 / Math.Pow(Variance, 3 / 2); // or mu3 / (sigma^3)
 			}
		}

        public double Kurtosis => 2.4;

        [Serialize(Type = SerializationType.Reference)]
		public Data Data { get; protected set; }

		public string Name => Data.Name;

		public virtual void Update(double[] paramerers)
		{
			int Nupdates = Math.Min(paramerers.Length, NParameters);
			if (Nupdates > 0)
				Mode = paramerers[0];
			if (Nupdates > 1)
				LowerBound = paramerers[1];
			if (Nupdates > 2)
				UpperBound = paramerers[2];
		}

		public double[] GetSamples(int N)
		{
			double[] samples = new double[N];
			MathNet.Numerics.Distributions.Triangular.Samples(samples, LowerBound, UpperBound, Mode);
			return samples;
		}

		public double[] GetSamples(int N, double[] FASTSamples) => throw new NotImplementedException();
		public (double lower, double upper) GetBoundExtensions(double satisfactionProbability)
		{
			double km = GetK(1 - satisfactionProbability);
			double kp = GetK(satisfactionProbability);

			double d1 = Mean - LowerBound;
			double d2 = UpperBound - Mean;

			double lower = (d1 - km * (d1 + d2));
			double upper = (kp * (d1 + d2) - d1);
			return (lower, upper);

			//double x_k_minus = StandardTriangularPDF_inv(1 - satisfactionProbability, d1, d2);
			//double x_k_plus = StandardTriangularPDF_inv(satisfactionProbability, d1, d2);

			//double StandardTriangularPDF_inv(double Prb, double d1, double d2)
			//{

			//double a = 0;
			//double b = 1;
			//double m = (2 * d1 - d2) / (d1 + d2);

			//double k = 0;
			//if (Prb <= (m - a) / (b - a) && Prb >= 0)
			//{
			//	k = a + Math.Sqrt(Prb * (m - a) * (b - a));
			//}
			//else if ((Prb > (m - a) / (b - a)) && (Prb <= 1))
			//{
			//	k = b - Math.Sqrt((1 - Prb) * (b - m) * (b - a));
			//}

			//}
		}

		public double GetK(double satisfactionProbability)
		{
			double mode = (Mode - LowerBound) / (UpperBound - LowerBound);
			return MathNet.Numerics.Distributions.Triangular.InvCDF(0, 1, mode, satisfactionProbability);
		}

		private readonly Random random = new Random();
	}

	public class TirangularDistribution2 : TriangularDistribution
	{
		[Serialize]
		[Parameter]
		public double Width { get; protected set; }

		public TirangularDistribution2() : base()
		{
		}

		public TirangularDistribution2(Data data) : base(data)
		{
		}

		[DeserializeConstructor]
		public TirangularDistribution2(Data data, double nominalValue, double lowerBound, double upperBound) : base(data, nominalValue, lowerBound, upperBound)
		{
			Width = upperBound - lowerBound;
		}


		public override int NParameters => 3;

		public override void Update(double[] paramerers)
		{
			int Nupdates = Math.Min(paramerers.Length, NParameters);
			if (Nupdates > 0)
				Mode = paramerers[0];
			if (Nupdates > 1)
			{
				double ratio = paramerers[1] / Width;
				UpperBound = Mode + ratio * (UpperBound - Mode);
				LowerBound = Mode + ratio * (LowerBound - Mode);
				Width = paramerers[1];
			}
		}

		private readonly Random random = new Random();
	}


}
