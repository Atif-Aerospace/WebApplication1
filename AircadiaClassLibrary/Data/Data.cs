using System;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.DataObjects
{
	[Serializable]
	public abstract class Data : AircadiaComponent
	{
		public abstract string GetDataType();

		[Serialize]
		public double Min { get; set; } = Double.NegativeInfinity;
		[Serialize]
		public double Max { get; set; } = Double.PositiveInfinity;
		[Serialize]
		public bool IsAuxiliary { get; set; }
		[Serialize]
		public object Value { get; set; }
		[Serialize]
		public string Unit { get; set; }
        [Serialize]
        public Dimension Dimension { get; set; }

		public abstract string ValueAsString
		{
			get;
			set;
		}

		public Data(string name, string description, string unit, Dimension dimension = default(Dimension), 
			bool isAux = false, string parentName = "", string displayName = "")
			: base(name, description, parentName)
		{
			Unit = unit?.Trim() ?? String.Empty;
			Dimension = dimension;
			IsAuxiliary = isAux;
			if (!String.IsNullOrWhiteSpace(displayName))
			{
				Rename(displayName);
			}
		}

		public string TypeName => Value.GetType().Name;

		public abstract bool IsValidSerialization(string serializedData, out string reason);
		public abstract Data Copy(string id, string name = null, string parentName = null);
	}
}
