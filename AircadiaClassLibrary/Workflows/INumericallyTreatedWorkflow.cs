using System.Collections.Generic;
using Aircadia.Numerics.Solvers;

namespace Aircadia.ObjectModel.Workflows
{
	public interface INumericallyTreatedWorkflow
	{
		List<ISolver> Solvers { get; set; }
	}
}