using Aircadia.ObjectModel;
using System;

namespace Aircadia.Numerics.Solvers
{
	public interface ISolver : IAircadiaComponent
	{
		bool IsSolved { get; }
		//Dictionary<string, string> Options { get; set; }
		//List<string> OptionFields { get; }
	}

	public interface IGradientSolver : ISolver
	{
		double[] Solve(Func<double[], double[]> f, double[] x0);
	}

	public interface IFixedPointSolver : ISolver
	{
		double[] Solve(Func<double[]> f, double[] x0);
	}

	public interface IGlobalSolver : ISolver
	{
		double[] Solve(Func<double[], double[]> f);
		int Nvariables { get; set; }
		double[] LowerBounds { get; set; }
		double[] UpperBounds { get; set; }
	}
}