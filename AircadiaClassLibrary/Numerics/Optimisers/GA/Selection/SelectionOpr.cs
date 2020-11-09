using System;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Selection
{
	[Serializable()]
    public abstract class SelectionOpr
    {
        //template for optimisation problem
        protected OptimisationTemplate Template;
        //genetic algorithm parameters
        protected GAParameters gaParameters;

        protected SelectionOpr(OptimisationTemplate Template, GAParameters gaParameters)
        {
            this.Template = Template;
            this.gaParameters = gaParameters;
        }

        public abstract int[] ApplySelectionOpr(Population.Population population);
    }
}
