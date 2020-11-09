using System;

namespace Aircadia.ObjectModel.DataObjects
{
	[Serializable]
    public abstract class ScalarData : Data
    {
        public ScalarData(string name, string description, string unit, Dimension dimension = default(Dimension), bool isAux = false, string parentName = "", string displayName = "")
            : base(name, description, unit, dimension, isAux, parentName, displayName)
        {
        }
    }
}
