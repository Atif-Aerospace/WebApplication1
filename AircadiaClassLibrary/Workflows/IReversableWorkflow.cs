using System.Collections.Generic;
using System.Threading.Tasks;
using Aircadia.ObjectModel.DataObjects;

namespace Aircadia.ObjectModel.Workflows
{
	public interface IReversableWorkflow
	{
		List<Data> ReversedInputs { get; }
		List<Data> ReversedOutputs { get; }

		bool Execute();
	}
}