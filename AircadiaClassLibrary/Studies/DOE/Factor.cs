using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Studies
{
	public class Factor
	{
		[DeserializeConstructor]
		public Factor(Data data, decimal lowerBound, decimal upperBound)
		{
			Data = data;
			LowerBound = lowerBound;
			UpperBound = upperBound;
		}

		public override string ToString() => Data.Name;

		[Serialize("Name", SerializationType.Reference)]
		public Data Data { get; set; }
		public string Name => Data.Name;
		[Serialize]
		public decimal LowerBound { get; set; }
		[Serialize]
		public decimal UpperBound { get; set; }

        public virtual Factor Copy() => new Factor(Data, LowerBound, UpperBound);
    }
}
