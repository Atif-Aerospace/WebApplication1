using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;
using System;
using System.Collections.Generic;

namespace Aircadia.Numerics.Solvers
{
	[Serializable()]
	public class FixedPointSolver : Solver, IFixedPointSolver
	{
		private const int defaueltMaxEvals = 20;
		private const double defaultTolerance = 1e-6;
		// Options
		private double[] feedbackVarsTolerances;
		private int maxEvals = defaueltMaxEvals;

		[Serialize]
		[Option("VariablesTolerance")]
		public string FeedbackVarsTolerances
		{
			get => DoubleVectorData.ValueToString(feedbackVarsTolerances);
			set
			{
				if (value == null) return;
				feedbackVarsTolerances = DoubleVectorData.StringToValue(value);
				for (int i = 0; i < value.Length; i++)
					feedbackVarsTolerances[i] = (value[i] > Double.Epsilon) ? value[i] : defaultTolerance;
			}
		}
		[Serialize]
		[Option("MaxIterations")]
		public int MaxEvals { get => maxEvals; set => maxEvals = (value > 0) ? value : defaueltMaxEvals; }


		public double[] Solve(Func<double[]> f, double[] x0)
		{
			IsSolved = false;

			int Nvars = x0.Length;

			// Check Options are correct
			if (feedbackVarsTolerances == null || feedbackVarsTolerances.Length != Nvars)
			{
				feedbackVarsTolerances = new double[Nvars];
				for (int i = 0; i < x0.Length; i++) feedbackVarsTolerances[i] = defaultTolerance;
			}

			//Executes the subprocess based on fixed point iteration method.
			var FeedbackVarsConvergency = new List<double[]>();//List used for storing and monitoring the convergency (as difference between the current and requested values) of feedback variables throughout the FPI process //???
			double[] feedbackVariables = new double[Nvars];
			double[] updatedVariables = null;
			for (int ncount = 0; ncount < Nvars; ncount++)
				feedbackVariables[ncount] = x0[ncount];

			int iter = 0;
			bool StopSwitch = false; //Switch to stop the execution of the FPI method when given stopping criteria are met

			while (StopSwitch == false)
			{
				iter++;

				updatedVariables = f();

				// 1st STOPPING CRITERION CHECK: Checking if all feedback variables converge with a default/user-defined tolerance
				StopSwitch = true;
				IsSolved = true;
				double[] difference = new double[Nvars];
				for (int ncount = 0; ncount < Nvars; ncount++)
				{
					difference[ncount] = Math.Abs(feedbackVariables[ncount] - updatedVariables[ncount]);
					if (difference[ncount] > feedbackVarsTolerances[ncount] * Math.Max(Math.Abs(updatedVariables[ncount]), 1))
					{
						StopSwitch = false;
						IsSolved = false;
					}

					feedbackVariables[ncount] = updatedVariables[ncount];
				}
				FeedbackVarsConvergency.Add(difference);

				// 2nd STOPPING CRITERION CHECK: Checking if the maximum number of iterations has been reached
				if (iter > MaxEvals)
				{
					StopSwitch = true;
					IsSolved = IsSolved || false; // Stays the same
				}
			}

			return updatedVariables;
		}
	}
}
