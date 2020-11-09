using System;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Elitism
{
	[Serializable()]
    public abstract class ElitismOpr
    {
        protected OptimisationTemplate Template;
        protected GAParameters gaParameters;
        protected Population.Population parentPopulation;
        protected Population.Population childPopulation;

        public ElitismOpr(OptimisationTemplate Template, GAParameters gaParameters, Population.Population parentPopulation, Population.Population childPopulation)
        {
            this.Template = Template;
            this.gaParameters = gaParameters;
            this.parentPopulation = parentPopulation;
            this.childPopulation = childPopulation;
        }
    }
}
