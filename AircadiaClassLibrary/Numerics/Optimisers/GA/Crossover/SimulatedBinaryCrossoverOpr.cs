using System;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Crossover
{
	[Serializable()]
    public class SimulatedBinaryCrossoverOpr : CrossoverOpr
    {
        private readonly double genewiseSwapProb;
        private readonly int polynomialOrder;

        public SimulatedBinaryCrossoverOpr(OptimisationTemplate Template, GAParameters gaParameters)
            : this(Template, gaParameters, 0.5, 10)
        {
        }

        public SimulatedBinaryCrossoverOpr(OptimisationTemplate Template, GAParameters gaParameters, double genewiseSwapProb, int polynomialOrder)
            : base(Template, gaParameters)
        {
            this.genewiseSwapProb = genewiseSwapProb;
            this.polynomialOrder = polynomialOrder;
        }

        public override void ApplyCrossoverOpr(Chromosome.Chromosome chromosome1, Chromosome.Chromosome chromosome2)
        {
            double minGene, maxGene;
            double gene1, gene2;
            double beta, betaq;
            double alpha;
            double tempGene;
            //***********************************need to delete, just for comparison***********************************
            var c1 = new Chromosome.MultiObjectiveChromosome(Template, gaParameters, false);
            var c2 = new Chromosome.MultiObjectiveChromosome(Template, gaParameters, false);
            //***********************************need to delete, just for comparison***********************************
            if (GARandom.Flip(gaParameters.CrossoverProbability))
            {
                for (int i = 0; i < Template.DesignVariables.Count; i++)
                {
                    if (GARandom.Flip(genewiseSwapProb) && Math.Abs(chromosome1[i] - chromosome2[i]) >= 1.0E-10)
                    {
                        minGene = Template.DesignVariables[i].LowerBound;
                        maxGene = Template.DesignVariables[i].UpperBound;

                        if (chromosome2[i] > chromosome1[i])
                        {
                            gene1 = chromosome1[i];
                            gene2 = chromosome2[i];
                        }
                        else
                        {
                            gene1 = chromosome2[i];
                            gene2 = chromosome1[i];
                        }

                        if ((gene1 - minGene) > (maxGene - gene2))
                            beta = (gene2 - gene1) / (2.0 * maxGene - gene2 - gene1);
                        else
                            beta = (gene2 - gene1) / (gene2 + gene1 - 2.0 * minGene);

                        alpha = GARandom.random01() * (2.0 - Math.Pow(beta, (polynomialOrder + 1)));
                        if (alpha <= 1.0)
                            betaq = Math.Pow(alpha, 1.0 / ((double)(polynomialOrder + 1)));
                        else
                            betaq = Math.Pow(1.0 / (2.0 - alpha), 1.0 / ((double)(polynomialOrder + 1)));

                        tempGene = 0.5 * ((gene1 + gene2) - betaq * (gene2 - gene1));
                        if (Template.DesignVariables[i].Type == DesignVariableType.Integer)
                            tempGene = (int)(tempGene + 0.5);
                        chromosome1[i] = tempGene;
                        tempGene = 0.5 * ((gene1 + gene2) + betaq * (gene2 - gene1));
                        if (Template.DesignVariables[i].Type == DesignVariableType.Integer)
                            tempGene = (int)(tempGene + 0.5);
                        chromosome2[i] = tempGene;
                    }
                }
            }
        }
    }
}
