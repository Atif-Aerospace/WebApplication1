using System;
using System.Collections.Generic;

namespace Aircadia.ObjectModel.DataObjects
{
	[Serializable]
    public abstract class MatrixData : Data
    {
		public List<string> Names { get; } = new List<string>();
		public List<string> Units { get; } = new List<string>();

		public MatrixData(string name, string description, bool isAux = false, string parentName = "", string displayName = "")
			: base(name, description, null, isAux: isAux, parentName: parentName, displayName: displayName)
        {
        }
    }
}
