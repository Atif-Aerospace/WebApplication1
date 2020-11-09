using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;


namespace Aircadia.ObjectModel.Util
{
	class PolyFit
	{
		public static void FitAndGetPoints(double[] xdata, double[] ydata, int order, int fitPoints, double[] xfit, out double[] yfit)
		{
			double[] p = FitPoly(xdata, ydata, order);
			yfit = EvaluatePoly(p, xfit); 
		}

		private static double[] FitPoly(double[] x, double[] fx, int order)
		{
			int Ncoefficients = order + 1;
			Matrix<double> A = Matrix<double>.Build.Dense(x.Length, Ncoefficients);
			for (int i = 0; i < x.Length; i++)
			{
				A[i, 0] = 1;
				for (int j = 1; j < Ncoefficients; j++)
					A[i, j] = A[i, j - 1] * x[i];
			}

			return MultipleRegression.DirectMethod<double>(A, Vector<double>.Build.Dense(fx), DirectRegressionMethod.QR).AsArray();
			//return MultipleRegression.DirectMethod<double>(A.ToRowArrays(), fx, true, DirectRegressionMethod.QR);
			//return Fit.Polynomial(x, fx, order);
		}

		private static double[] EvaluatePoly(double[] p, double[] x)
		{
			if (x.Length == 0)
				return new double[0];

			if (p.Length == 0)
				return Vector<double>.Build.Dense(x.Length, 0).AsArray();

			Matrix<double> A = Matrix<double>.Build.Dense(x.Length, p.Length);
			for (int i = 0; i < x.Length; i++)
			{
				A[i, 0] = 1;
				for (int j = 1; j < p.Length; j++)
					A[i, j] = A[i, j - 1] * x[i];
			}

			return (A * Vector<double>.Build.Dense(p)).AsArray();
		}
	}
}
