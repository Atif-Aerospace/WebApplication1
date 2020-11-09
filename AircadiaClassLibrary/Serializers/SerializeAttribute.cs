using System;

namespace Aircadia.Services.Serializers
{
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class SerializeAttribute : Attribute
	{

		// This is a positional argument
		public SerializeAttribute(string serializedName = default(string), SerializationType type = SerializationType.Value)
		{
			SerializedName = serializedName;
			Type = type;
		}

		public string SerializedName { get; set; }
		public SerializationType Type { get; set; }
		public object Default { get; set; }
		public bool ConstructorOnly { get; set; } = false;
	}

	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class SerializeEnumerableAttribute : SerializeAttribute
	{
		public SerializeEnumerableAttribute(string serializedName, SerializationType type = SerializationType.Value)
		{
			SerializedName = serializedName;
			SerializedChildrenName = String.Empty;
			Type = type;
		}

		// This is a positional argument
		public SerializeEnumerableAttribute(string serializedName, string serializedChildrenName, SerializationType type = SerializationType.Reference)
		{
			SerializedName = serializedName;
			SerializedChildrenName = serializedChildrenName;
			Type = type;
		}

		public string SerializedChildrenName { get; }
	}

	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class SerializeDictionaryAttribute : SerializeAttribute
	{
		public SerializeDictionaryAttribute(string serializedName, string serializedChildrenName, SerializationType type = SerializationType.Value)
		{
			SerializedName = serializedName;
			SerializedChildrenName = serializedChildrenName;
			Type = type;
		}

		public string SerializedChildrenName { get; }
	}

	[AttributeUsage(AttributeTargets.Constructor, Inherited = true, AllowMultiple = false)]
	public class DeserializeConstructorAttribute : Attribute
	{
		public DeserializeConstructorAttribute()
		{
		}
	}

	public enum SerializationType
	{
		Value,
		Path,
		Lines,
		Reference,
	}
}
