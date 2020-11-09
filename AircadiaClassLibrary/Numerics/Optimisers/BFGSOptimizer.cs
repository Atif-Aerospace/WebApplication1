/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using Aircadia.Services.Serializers;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using MathNet.Numerics.Optimization.ObjectiveFunctions;

namespace Aircadia.Numerics.Optimizers
{
	public class BFGSOptimizer : IMinimizer
	{
		[Serialize]
		[Option("RelativeIncrement")]
		public double RelativeIncrement { get; set; } = 1e-05;
		[Serialize]
		[Option("MinimumIncrement")]
		public double MinimumIncrement { get; set; } = 1e-08;
		[Serialize]
		[Option("GradientTolerance")]
		public double GradientTolerance { get; set; } = 1e-05;
		[Serialize]
		[Option("ParameterTolerance")]
		public double ParameterTolerance { get; set; } = 1e-05;
		[Serialize]
		[Option("FunctionProgressTolerance")]
		public double FunctionProgressTolerance { get; set; } = 1e-05;
		[Serialize]
		[Option("MaximumIterations")]
		public int MaximumIterations { get; set; } = 1000;

		public MinimizationResult FindMinimum(Func<Vector<double>, double> objectiveFunction, Vector<double> initialGuess)
		{
			var lowerBound = Vector<double>.Build.Dense(initialGuess.Count, Double.NegativeInfinity);
			var upperBound = Vector<double>.Build.Dense(initialGuess.Count, Double.PositiveInfinity);
			var objective = ObjectiveFunction.Value(objectiveFunction);
			var objectiveFunctionWithGradient = new ForwardDifferenceGradientObjectiveFunction(objective, lowerBound, upperBound, RelativeIncrement, MinimumIncrement);
			var bfgsMinimizer = new BfgsMinimizer(GradientTolerance, ParameterTolerance, FunctionProgressTolerance, MaximumIterations);
			return bfgsMinimizer.FindMinimum(objectiveFunctionWithGradient, initialGuess);
		}
	}
}


