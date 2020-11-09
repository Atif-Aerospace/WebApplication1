using System;

namespace Aircadia.Numerics.Solvers
{
	[Serializable()]
	public class NewtonOptions
    {
		private const double defaultTolerance = 1e-6;
		private const int defaultMaxIterations = 20;
		private const double defaultStepLimiy = Double.MaxValue;

		private int maxIterations = defaultMaxIterations;
        private double xTolerance = defaultTolerance;
        private double yTolerance = defaultTolerance;
		private double[,] stepLimits;
		private double[] derivativeStep;

		public double YTolerance
		{
			get => yTolerance;
			set => yTolerance = (value > 0) ? value : defaultTolerance;
		}

		public double XTolerance
		{
			get => xTolerance;
			set => xTolerance = (value > 0) ? value : defaultTolerance;
		}

		public int MaxIterations
		{
			get => maxIterations;
			set => maxIterations = (value > 0) ? value : 10;
		}

		public double[,] StepLimits { get => stepLimits; set
			{
				if (value == null) return;
				stepLimits = new double[value.GetLength(0), value.GetLength(1)];
				for (int i = 0; i < value.GetLength(0); i++)
				{
					stepLimits[i, 0] = (value[i, 0] < Double.Epsilon) ? value[i, 0] : defaultStepLimiy;
					stepLimits[i, 1] = (value[i, 1] > -Double.Epsilon) ? value[i, 1] : -defaultStepLimiy;
				}
			}
		}

		public double[] DerivativeStep { get => derivativeStep; set
			{
				if (value == null) return;
				derivativeStep = new double[value.Length];
				for (int i = 0; i < value.Length; i++)
					derivativeStep[i] = (value[i] > Double.Epsilon) ? value[i] : defaultTolerance;
			}
		}
	}
}
