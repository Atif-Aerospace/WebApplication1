using System;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA
{
	[Serializable()]
    public enum ConstraintHandlingMethods
    {
        LinearPenalty,
        QuadraticPenalty,
        Tournament,
    }
}
