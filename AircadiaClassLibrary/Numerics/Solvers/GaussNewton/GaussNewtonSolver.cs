using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;

namespace Aircadia.Numerics.Solvers
{
	[Serializable()]
	public class GaussNewtonSolver : Solver, IGradientSolver
	{
		private const double defaultTolerance = 1e-6;
		private const double defaultDiffPerc = 1e-8;
		private const int defaultMaxEvals = 20;

		private double[] varsTolerances = new double[0];
		private double diffperc = defaultDiffPerc;
		private int maxEvals = defaultMaxEvals;

		[Serialize]
		[Option("YTolerance")]
		public string VarsTolerances
		{
			get => DoubleVectorData.ValueToString(varsTolerances); set
			{
				if (value == null) return;
				varsTolerances = DoubleVectorData.StringToValue(value);
				for (int i = 0; i < varsTolerances.Length; i++)
					varsTolerances[i] = (varsTolerances[i] > Double.Epsilon) ? varsTolerances[i] : defaultTolerance;
			}
		}
		[Serialize]
		[Option("DerivativePercentage")]
		public double Diffperc { get => diffperc; set => diffperc = (value > 0) ? value : defaultDiffPerc; }
		[Serialize]
		[Option("MaxIterations")]
		public int MaxEvals { get => maxEvals; set => maxEvals = (value > 0) ? value : defaultMaxEvals; }

		public double[] Solve(Func<double[], double[]> f, double[] x0)
		{
			IsSolved = false;

			int Nvars = x0.Length;

			// Check Options are correct
			if (varsTolerances == null || varsTolerances.Length != Nvars)
			{
				varsTolerances = new double[Nvars];
				for (int i = 0; i < x0.Length; i++) varsTolerances[i] = defaultTolerance;
			}


			double[] outp = f(x0);
			if (outp == null)
				return outp; // false

			double[] diff = Diffcalculator(Diffperc, x0);

			int iter = 0;
			bool StopSwitch = false; //Switch to stop the execution of the FPI method when given stopping criteria are met
			var VarsHistory = new List<double[]>(); //Matrix in which the computation history of feedback variables is stored
			var DifferencesHistory = new List<double[]>(); //Matrix in which the computation history of differences is stored

			try
			{
				while (StopSwitch == false)
				{
					Matrix<double> Jacob = FindJacob(f, x0, diff);
					if (Jacob == null)
						return outp; // false
					Matrix<double> Jacob_T = Jacob.Transpose();
					Matrix<double> Jacob_Mult = Jacob_T * Jacob;
					Matrix<double> Jacob_Inv = Jacob_Mult.Inverse();
					Matrix<double> Jacob_Fin = Jacob_Inv * Jacob_T;
					Vector<double> vecto = Vector<double>.Build.DenseOfArray(outp);
					Vector<double> inpnext = Jacob_Fin * vecto;
					for (int ninp = 0; ninp < x0.Length; ninp++)
					{
						x0[ninp] = x0[ninp] - inpnext[ninp];
					}
					outp = f(x0);
					if (outp == null)
						return outp; // false
					iter++;

					//Storing the values of the feedback variables at the current iteration
					VarsHistory.Add(x0);

					// 1st STOPPING CRITERION CHECK: Checking if all feedback variables converge with a default/user-defined tolerance
					StopSwitch = true;
					IsSolved = true;
					int IndexCounter = 0;
					double[] ABS_dDifference = new double[x0.Length];
					for (int i = 0; i < outp.Length; i++)
					{
						ABS_dDifference[IndexCounter] = Math.Abs(outp[IndexCounter]);
						if (ABS_dDifference[IndexCounter] > varsTolerances[IndexCounter])
						{
							StopSwitch = false;
							IsSolved = false;
							break;
						}
						IndexCounter++;
					}

					//Storing the values of the feedback variables at the current iteration
					DifferencesHistory.Add(ABS_dDifference);

					// 2nd STOPPING CRITERION CHECK: Checking if the maximum number of iterations has been reached
					if (iter > MaxEvals)
					{
						Console.WriteLine($"Reached {MaxEvals} iterations in Guass Newton Method. Solver failed.");
						IsSolved = false;
						return outp; // false
					}
				}

				return outp; // true?
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				IsSolved = false;
				return outp; // false
			}
		}

		/// <summary>
		/// Calculates the perturbations to be introduced inorder to calculate the finite differences
		/// </summary>
		/// <param name="diffperc"></param>
		/// <param name="inp"></param>
		/// <returns></returns>
		private double[] Diffcalculator(double diffperc, double[] inp)
		{
			//Calculates the diff for the inp
			double[] diff = new double[inp.Length];
			for (int ninp = 0; ninp < inp.Length; ninp++)
			{
				if (inp[ninp] == 0)
					diff[ninp] = 1e-8;
				else
					diff[ninp] = inp[ninp] * diffperc;
			}
			return diff;
		}

		/// <summary>
		/// Calculates jacobian for 'testfunc'
		/// </summary>
		/// <param name="f"></param>
		/// <param name="x"></param>
		/// <param name="diff"></param>
		/// <returns></returns>
		private Matrix<double> FindJacob(Func<double[], double[]> f, double[] x, double[] diff)
		{
			int ndes = x.Length;
			Matrix<double> Jacob = Matrix<double>.Build.Dense(x.Length, x.Length);
			double? tmpJacob;
			for (int nrow = 0; nrow < x.Length; nrow++)
			{
				for (int ncol = 0; ncol < x.Length; ncol++)
				{
					tmpJacob = FindGradient(f, x, nrow, ncol, diff[ncol]);
					if (!tmpJacob.HasValue)
						return null;
					else
						Jacob[nrow, ncol] = tmpJacob.Value;
				}
			}
			return Jacob;
		}


		/// <summary>
		/// Calculates Gradient for the 'testfunc'
		/// </summary>
		/// <param name="f"></param>
		/// <param name="x"></param>
		/// <param name="nrow"></param>
		/// <param name="ncol"></param>
		/// <param name="diff"></param>
		/// <returns></returns>
		private double? FindGradient(Func<double[], double[]> f, double[] x, int nrow, int ncol, double diff)
		{
			double grad;
			double[] outp;
			double tmpin = x[ncol];
			x[ncol] = tmpin + diff;
			outp = f(x);
			if (outp == null)
				return null;
			grad = outp[nrow];
			x[ncol] = tmpin - diff;
			outp = f(x);
			grad = grad - outp[nrow];
			grad = grad / (diff + diff);
			x[ncol] = tmpin;
			return grad;
		}
	}
}
