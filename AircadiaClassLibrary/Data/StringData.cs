using Aircadia.Services.Serializers;
using System;

namespace Aircadia.ObjectModel.DataObjects
{
	[Serializable]
    public class StringData : Data
    {
		public override string GetDataType() => "String";

		public override bool IsValidSerialization(string serializedData, out string reason)
		{
			reason = String.Empty;
			return true;
		}

		public override string ValueAsString
		{
			get => Convert.ToString(Value);
			set => Value = value ?? String.Empty;
		}

		public StringData(string name)
            : this(name, null, String.Empty)
        {
        }

        public StringData(string name, string value)
            : this(name, null, value)
        {
        }

		[DeserializeConstructor]
		public StringData(string name, string description, string value, bool isAux = false, string parentName = "", string displayName = "")
            : base(name, description, null, isAux: isAux, parentName: parentName, displayName: displayName)
        {
			Value = value ?? String.Empty;
        }

		public override Data Copy(string id, string name = null, string parentName = null) 
			=> new StringData(id, Description, ValueAsString, IsAuxiliary, parentName ?? ParentName, name ?? Name);


		//public override string PropertiesSummaryText()
		//{
		//	string output = "NAME: " + Name + "\r\n";
		//	output = output + "TYPE: " + "Double" + "\r\n";
		//	output = output + "VALUE: " + ValueAsString + "\r\n";
		//	return output;
		//}
	}
}
