using System;
using System.Collections.Generic;
using System.Linq;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel
{
	[Serializable]
    public abstract class ExecutableComponent : AircadiaComponent
    {
       
		[SerializeEnumerable("Inputs", "Input", ConstructorOnly = true)]
		public List<Data> ModelDataInputs { get; set; }

		[SerializeEnumerable("Outputs", "Output", ConstructorOnly = true)]
		public List<Data> ModelDataOutputs { get; set; }

		public List<Data> GetAllData() => ModelDataInputs.Concat(ModelDataOutputs).ToList();

		public string GetInputsNamesString()
        {
            string s = "";
            foreach (Data data in ModelDataInputs)
            {
                s += (data.Name + ",");
            }
            s = s.TrimEnd(',');
            return s;
        }
		public string GetOutputsNamesString()
        {
            string s = "";
            foreach (Data data in ModelDataOutputs)
            {
                s += (data.Name + ",");
            }
            s = s.TrimEnd(',');
            return s;
        }
		public string ExecutableCode { get; set; }

		public abstract bool Execute();

		public ExecutableComponent(string name, string description, string parentName = "")
            : base(name, description, parentName)
        {
        }

    }
}
