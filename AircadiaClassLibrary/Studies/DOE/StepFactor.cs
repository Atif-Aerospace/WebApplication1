using System;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Studies
{
	public class BoundedDesignVariableNoInitial : Factor
	{
		[DeserializeConstructor]
		public BoundedDesignVariableNoInitial(Data data, decimal lowerBound, int levels, decimal upperBound) : base(data, lowerBound, upperBound)
		{
			Levels = levels;
		}
		[Serialize]
		public decimal Step => (UpperBound / (Levels - 1)) - (LowerBound / (Levels - 1)); 
		[Serialize]
		public int Levels { get; set; }
		public virtual decimal[] GetValues()
		{
			decimal[] values = new decimal[Levels];
			values[0] = LowerBound;
			for (int i = 1; i < Levels; i++)
				values[i] = values[i - 1] + Step;
			return values;
		}

        public override Factor Copy() => new BoundedDesignVariableNoInitial(Data, LowerBound, Levels, UpperBound);
    }
}
