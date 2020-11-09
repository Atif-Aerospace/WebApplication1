using System;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Mutation
{
	[Serializable()]
    public abstract class MutationOpr
    {
        protected OptimisationTemplate Template;
        protected GAParameters gaParameters;

        protected double mutationProb;

        public MutationOpr(OptimisationTemplate Template, GAParameters gaParameters)
        {
            this.Template = Template;
            this.gaParameters = gaParameters;
        }

        public abstract void ApplyMutationOpr(Chromosome.Chromosome chromosome);
    }
}
