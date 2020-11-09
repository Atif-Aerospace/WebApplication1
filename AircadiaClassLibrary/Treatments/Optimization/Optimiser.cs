using System;
using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

namespace Aircadia.ObjectModel.Treatments.Optimisers
{
	[Serializable]
    public abstract class Optimiser : Treatment
    {
		public OptimisationTemplate OptimisationTemplate { get; set; }

		public Optimiser(string name, string description)
            : base(name, description)
        {
        }
    }
}
