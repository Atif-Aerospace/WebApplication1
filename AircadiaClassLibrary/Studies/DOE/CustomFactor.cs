using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;
using System.Linq;

namespace Aircadia.ObjectModel.Studies
{
	public class CustomFactor : BoundedDesignVariableNoInitial
	{
		[DeserializeConstructor]
		public CustomFactor(Data data, decimal[] values) : base(data, values.Min(), values.Count(), values.Max())
		{
			Values = values.ToArray();
		}

		[Serialize]
		private decimal[] Values { get; }

		public override decimal[] GetValues() => Values;

        public override Factor Copy() => new CustomFactor(Data, Values);
    }
}
