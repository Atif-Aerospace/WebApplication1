using System;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Selection
{
	[Serializable()]
    public abstract class TournamentSelectionOpr : SelectionOpr
    {
        protected int tournamentSize;

        public TournamentSelectionOpr(OptimisationTemplate Template, GAParameters gaParameters, int tournamentSize)
            : base(Template, gaParameters)
        {
            this.tournamentSize = tournamentSize;
        }

        //shuffles an array containing indices (will be used by both tournament selection operators, i.e. one without replacement and other with replacement)
        //        protected void shuffleArray(int[] index)
        //        {
        //            int temp; //used for swapping
        //            for (int i = 0; i < index.Length - 2; i++)
        //            {
        //                int randomIndex = GARandom.BoundedRandomInteger(i, index.Length);
        //                //swapping
        //                if (i != randomIndex)
        //                {
        //                    temp = index[i];
        //                    index[i] = index[randomIndex];
        //                    index[randomIndex] = temp;
        //                }
        //            }
        //        }
        //same implementation as in C++ to get same results
        protected void shuffleArray(int[] index)
        {
            int i, temp, randomIndex, D1, D2;
            for (i = 1, D1 = 1, D2 = (index.Length - i + D1) / D1; D2 > 0; D2--, i += D1)
            {
                randomIndex = (int)((index.Length - i) * GARandom.random01() + i);
                temp = index[randomIndex - 1];
                index[randomIndex - 1] = index[i - 1];
                index[i - 1] = temp;
            }
        }

        public int TournamentSize
		{
			get => tournamentSize;
			set => tournamentSize = value;
		}
	}
}
