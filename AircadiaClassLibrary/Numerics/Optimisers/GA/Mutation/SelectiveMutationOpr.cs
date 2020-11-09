using System;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Mutation
{
	[Serializable()]
    public class SelectiveMutationOpr : MutationOpr
    {
        public SelectiveMutationOpr(OptimisationTemplate Template, GAParameters gaParameters)
            : base(Template, gaParameters)
        {
        }

        public override void ApplyMutationOpr(Chromosome.Chromosome chromosome)
        {
            int mutationPosition = GARandom.BoundedRandomInteger(0, Template.DesignVariables.Count);

            double gene = 0;
            if (Template.DesignVariables[mutationPosition].Type == DesignVariableType.Integer)
            {
                gene = GARandom.BoundedRandomInteger((int)Template.DesignVariables[mutationPosition].LowerBound, (int)Template.DesignVariables[mutationPosition].UpperBound);
            }
            else if (Template.DesignVariables[mutationPosition].Type == DesignVariableType.Double)
            {
                gene = GARandom.BoundedRandomDouble(Template.DesignVariables[mutationPosition].LowerBound, Template.DesignVariables[mutationPosition].UpperBound);
            }
            else
            {
                //System.out.println("Invalid Design Variable Type");
                //System.exit(1);
            }

            chromosome[mutationPosition] = gene;

        }

    }
}
