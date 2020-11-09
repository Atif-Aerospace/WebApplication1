using System;

namespace Aircadia.ObjectModel.DataObjects
{
	[Serializable]
    public abstract class VectorData : Data
    {
        public VectorData(string name, string description, string unit, bool isAux = false, string parentName = "", string displayName = "")
            : base(name, description, unit, isAux: isAux, parentName: parentName, displayName: displayName)
        {
        }
    }
}
