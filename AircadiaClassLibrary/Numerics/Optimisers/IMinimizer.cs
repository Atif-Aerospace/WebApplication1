using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using System;

namespace Aircadia.Numerics.Optimizers
{
	public interface IMinimizer
	{
		MinimizationResult FindMinimum(Func<Vector<double>, double> objectiveFunction, Vector<double> initialGuess);
	}
}