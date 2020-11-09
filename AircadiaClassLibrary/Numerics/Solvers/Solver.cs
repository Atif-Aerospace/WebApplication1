using System.Linq;

namespace Aircadia.Numerics.Solvers
{
	public abstract class Solver : ISolver
	{
		public string Name { get => GetType().Name.Split('.').Last(); set => value.ToString(); }
		public string Description { get; set; }

		public bool IsSolved { get; protected set; }

		public override string ToString() => Name;
	}
}