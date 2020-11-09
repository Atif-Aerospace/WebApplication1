using System;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Mutation
{
	[Serializable()]
    public class PolynomialMutationOpr : MutationOpr
    {
        public PolynomialMutationOpr(OptimisationTemplate Template, GAParameters gaParameters)
            : base(Template, gaParameters)
        {
        }

        public override void ApplyMutationOpr(Chromosome.Chromosome chromosome)
        {
            double minGeneVal, maxGeneVal; //minimum ans maximum values for a gene
            double geneVal; //for storing gene value of the chromosome
            double delta;
            double randomNo;
            double value, deltaq;
            double tempGene;
            int polynomialOrder = gaParameters.MutationPolynomialOrder;
            for (int i = 0; i < Template.DesignVariables.Count; i++)
            {
                geneVal = chromosome[i];
                // Flip a biased coin with mutation probability
                if (GARandom.Flip(gaParameters.MutationProbability))
                {
                    minGeneVal = Template.DesignVariables[i].LowerBound;
                    maxGeneVal = Template.DesignVariables[i].UpperBound;

                    if ((geneVal - minGeneVal) < (maxGeneVal - geneVal)) //if gene value is closer to the minimum gene value, then computing slope with gene value and minimum gene value
                        delta = (geneVal - minGeneVal) / (maxGeneVal - minGeneVal);
                    else //if gene value is closer to the maximum gene value (or in middle), then computing the slope with gene value and maximum gene value
                        delta = (maxGeneVal - geneVal) / (maxGeneVal - minGeneVal);

                    randomNo = GARandom.random01();
                    if (randomNo <= 0.5)
                    {
                        value = 2.0 * randomNo + (1.0 - 2.0 * randomNo) * Math.Pow(1.0 - delta, (double)(polynomialOrder + 1));
                        deltaq = Math.Pow(value, 1.0 / ((double)(polynomialOrder + 1))) - 1.0;
                    }
                    else
                    {
                        value = 2.0 * (1.0 - randomNo) + 2.0 * (randomNo - 0.5) * Math.Pow(1.0 - delta, (double)(polynomialOrder + 1));
                        deltaq = 1.0 - Math.Pow(value, 1.0 / ((double)(polynomialOrder + 1)));
                    }

                    tempGene = geneVal + (maxGeneVal - minGeneVal) * deltaq;
                    if (Template.DesignVariables[i].Type == DesignVariableType.Integer)
                        tempGene = Math.Round(tempGene);
                    if (tempGene < minGeneVal)
                        tempGene = minGeneVal;
                    else if (tempGene > maxGeneVal)
                        tempGene = maxGeneVal;

                    chromosome[i] = tempGene;

                }
            }
        }
    }
}
