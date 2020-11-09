using System;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;
using MathNet.Numerics.LinearAlgebra;

namespace Aircadia.Numerics.Solvers
{
	[Serializable()]
	public class NewtonSolver : Solver, IGradientSolver
	{

		NewtonOptions NewtonOptions;
		[Serialize]
		[Option("YTolerance")]
		public string YTolerance { get => NewtonOptions.YTolerance.ToString(); set => NewtonOptions.YTolerance = Convert.ToDouble(value); }
		[Serialize]
		[Option("XTolerance")]
		public string XTolerance { get => NewtonOptions.XTolerance.ToString(); set => NewtonOptions.XTolerance = Convert.ToDouble(value); }
		[Serialize]
		[Option("MaxIterations")]
		public string MaxIterations { get => NewtonOptions.MaxIterations.ToString(); set => NewtonOptions.MaxIterations = Convert.ToInt32(value); }
		[Serialize]
		[Option("StepLimits")]
		public string StepLimits { get => DoubleMatrixData.ValueToString(NewtonOptions.StepLimits); set => NewtonOptions.StepLimits = DoubleMatrixData.StringToValue(value); }
		[Serialize]
		[Option("DerivativeStep")]
		public string DerivativeStep { get => DoubleVectorData.ValueToString(NewtonOptions.DerivativeStep); set => NewtonOptions.DerivativeStep = DoubleVectorData.StringToValue(value); }

		

		private const double DefaulteDerivativeStep = 1e-6;

		public NewtonSolver(NewtonOptions options) => NewtonOptions = options;

		public NewtonSolver() => NewtonOptions = new NewtonOptions();


		public double[] Solve(Func<double[], double[]> f, double[] x0)
		{
			double[] x = x0;
			double rlx = 1.0;

			// Check Options are correct
			if (NewtonOptions.StepLimits == null || !(NewtonOptions.StepLimits.Rank == 2 && NewtonOptions.StepLimits.Length == 2 * x0.Length))
			{
				NewtonOptions.StepLimits = new double[x0.Length, 2];
				for (int i = 0; i < x0.Length; i++)
				{
					NewtonOptions.StepLimits[i, 0] = Double.NegativeInfinity;
					NewtonOptions.StepLimits[i, 1] = Double.PositiveInfinity;
				}
			}
			if (NewtonOptions.DerivativeStep == null || NewtonOptions.DerivativeStep.Length != x0.Length)
			{
				NewtonOptions.DerivativeStep = new double[x0.Length];
				for (int i = 0; i < x0.Length; i++) NewtonOptions.DerivativeStep[i] = DefaulteDerivativeStep;
			}


			for (int iter = 0; iter < NewtonOptions.MaxIterations; iter++)
			{
				double[] y = f(x);

				// Chek if y is close to 0
				double dyMax = 0;
				for (int i = 0; i < y.Length; i++)
					dyMax = (dyMax < Math.Abs(y[i])) ? Math.Abs(y[i]) : dyMax;
				if (Math.Abs(dyMax) < NewtonOptions.YTolerance)
				{
					IsSolved = true;
					return x;
				}

				// Calculate Jacobian
				double[,] A = new double[x0.Length, x0.Length];
				for (int j = 0; j < x.Length; j++)
				{
					x[j] += NewtonOptions.DerivativeStep[j];
					double[] ydy = f(x);
					for (int i = 0; i < x.Length; i++)
						A[i, j] = (ydy[i] - y[i]) / NewtonOptions.DerivativeStep[j];
					x[j] -= NewtonOptions.DerivativeStep[j];
				}

				linsolve(ref A, ref y, out double[] dx);

				// Update step and check if its length is close to zero
				double dxMax = 0;
				for (int i = 0; i < dx.Length; i++)
				{
					dx[i] = limit(dx[i], NewtonOptions.StepLimits[i, 0], NewtonOptions.StepLimits[i, 1]);
					dx[i] = Double.IsNaN(dx[i]) ? 0 : dx[i];
					dxMax = (dxMax < Math.Abs(dx[i])) ? Math.Abs(dx[i]) : dxMax;
					x[i] += rlx * dx[i];
				}

				if (dxMax < NewtonOptions.XTolerance)
				{
					IsSolved = true;
					return x;
				}
			}

			IsSolved = false;
			return x;

			double limit(double value, double lowerBound, double upperBound) => Math.Min(Math.Max(lowerBound, value), upperBound);

			void linsolve(ref double[,] A, ref double[] b, out double[] _x)
			{
				Matrix<double> _A = Matrix<double>.Build.DenseOfArray(A);
				Vector<double> _b = Vector<double>.Build.DenseOfArray(b);
				_x = _A.Solve(-_b).ToArray();
			}
		}


	}

}
