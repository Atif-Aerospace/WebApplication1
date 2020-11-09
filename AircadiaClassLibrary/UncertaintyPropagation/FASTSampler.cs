using System.Linq;
using MathNet.Numerics;
using static System.Math;

namespace Aircadia.Treatments.Uncertainty
{
	class FASTSampler
	{
		public int N { get; }
		public int Ns { get; private set; }
		public int M { get; } = 4; //free to interference up up to M order
		public double[] Samples { get; private set; }
		public int[] Omega { get; private set; }

		public FASTSampler(int n)
		{
			N = n;
			GenerateOmega();
			GenerateSamples();
		}

		private void GenerateOmega()
		{
			//generate frequencies    based on Flavio Cannavo's code and ‘A computational implementation of FAST’ [McRae et al.]
			if (N == 2)
			{
				Omega = new int[] { 5, 9 };
			}
			else if (N == 3)
			{
				Omega = new int[] { 1, 9, 15 };
			}
			else
			{
				int[] F = { 0, 3, 1, 5, 11, 1, 17, 23, 19, 25, 41, 31,
							  23, 87, 67, 73, 85, 143, 149, 99, 119, 237,
							  267, 283, 151, 385, 157, 215, 449, 163, 337,
							  253, 375, 441, 673, 773, 875, 873, 587, 849,
							  623, 637, 891, 943, 1171, 1225, 1335, 1725, 1663, 2019 };

				int[] DN = { 4,8,6,10,20,22,32,40,38,26,56,62,46,76,96,60,
							   86,126,134,112,92,128,154,196,34,416,106,
							   208,328,198,382,88,348,186,140,170,284,
							   568,302,438,410,248,448,388,596,216,100,488,166,0};

				Omega = new int[N];
				Omega[0] = F[N - 1];

				for (int i = 1; i < N; i++)   // the ith input following nature numbering, when i = 1, the Omiga has already been chosen
					Omega[i] = Omega[i - 1] + DN[N - 1 - i];   // ‘A computational implementation of FAST’ [McRae et al.]
			}
		}

		private void GenerateSamples()
		{
			Ns = 2 * M * Omega.Max() + 1;   //number of runs
			Samples = Generate.LinearSpacedMap(Ns, -Ns + 1, Ns - 1, d => d * PI / Ns);
		}

		public double[] GetSamplesForVariable(int index)
		{
			int o = Omega[index];
			return Generate.Map(Samples, s => 0.5 + Asin(Sin(o * s)) / PI);
		}
	}
}
