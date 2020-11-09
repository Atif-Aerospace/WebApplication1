using System;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Treatments.Optimisers.Formulation
{
	[Serializable()]
    public class Constant
    {
        //name of the constant
        public string Name { get; set; }

		public Data Data { get; }

		//value of the constant
		protected double value;
        public double Value { get; set; }


		public Constant(string name, Data wrappedData)
        {
            Name = name;
			Data = wrappedData;
        }





    }

	[Serializable()]
	public class Parameter : INamedComponent
	{
		[Serialize(Type = SerializationType.Reference)]
		public Data Data { get; }
		public string Name => Data.Name;

		//value of the parameter
		[Serialize]
		public string Value { get; }

		[DeserializeConstructor]
		public Parameter(Data data)
		{
			Data = data;
			Value = Data.ValueAsString;
		}
	}
}
