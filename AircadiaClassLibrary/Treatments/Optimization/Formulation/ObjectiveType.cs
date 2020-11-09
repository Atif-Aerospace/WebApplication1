using System;

namespace Aircadia.ObjectModel.Treatments.Optimisers.Formulation
{
	[Serializable()]
    public enum ObjectiveType
    {
        Minimise,
        Maximise,
        Target,
        Reduce,
        Increase,
        Achieve,
        Eliminate,
    }
}
