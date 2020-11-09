using System.Collections.Generic;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;

namespace Aircadia.WorkflowManagement
{
	public interface IDependencyAnalysis
	{
		HashSet<Data> BackwardTrace(Data data);
		HashSet<Data> BackwardTrace(Data data, IEnumerable<Data> independentVars);
		HashSet<Data> ForwardTrace(Data data);

		HashSet<WorkflowComponent> BackwardTrace(WorkflowComponent component);
		HashSet<WorkflowComponent> ForwardTrace(WorkflowComponent component);

		WorkflowComponent Provider(Data data);
	}
}