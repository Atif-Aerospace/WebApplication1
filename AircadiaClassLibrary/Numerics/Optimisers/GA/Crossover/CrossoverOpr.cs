using System;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Crossover
{
	[Serializable()]
    public abstract class CrossoverOpr
    {
        protected OptimisationTemplate Template;
        protected GAParameters gaParameters;

        public CrossoverOpr(OptimisationTemplate Template, GAParameters gaParameters)
        {
            this.Template = Template;
            this.gaParameters = gaParameters;
        }

        public abstract void ApplyCrossoverOpr(Chromosome.Chromosome chromosome1, Chromosome.Chromosome chromosome2);
    }
}
