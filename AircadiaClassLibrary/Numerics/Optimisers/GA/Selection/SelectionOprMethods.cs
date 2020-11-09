using System;

namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.Selection
{
	[Serializable()]
    public enum SelectionOprMethods
    {
        RouletteWheelSelection,
        TournamentSelectionWithoutReplacement,
        TournamentSelectionWithReplacement,
    }
}
