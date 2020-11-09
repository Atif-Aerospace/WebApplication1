using System;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Crossover
{
	[Serializable()]
    public class OnePointCrossoverOpr : CrossoverOpr
    {
        public OnePointCrossoverOpr(OptimisationTemplate Template, GAParameters gaParameters)
            : base(Template, gaParameters)
        {
        }

        public override void ApplyCrossoverOpr(Chromosome.Chromosome chromosome1, Chromosome.Chromosome chromosome2)
        {
            int crossoverPoint = GARandom.BoundedRandomInteger(0, Template.DesignVariables.Count - 1);
            for (int i = 0; i <= crossoverPoint; i++)
            {
                double tempGene = chromosome1[i];
                chromosome1[i] = chromosome2[i];
                chromosome2[i] = tempGene;
            }
        }
    }
}
