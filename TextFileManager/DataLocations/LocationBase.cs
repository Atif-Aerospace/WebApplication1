using System;
using System.Collections.Generic;

namespace ExeModelTextFileManager.DataLocations
{
	public interface IFixedLine
	{
		int Line { get; }
	}

	public interface IFixedColumn
	{
		int StartColumn { get; }
		int EndColumn { get; }
	}

	public interface IRelativeLine
	{
		List<string> AnchorTexts { get; }
		int SkipLines { get; }
	}

	public interface IRelativeColumn
	{
		int Position { get; }
		char[] Separators { get; }
	}

	public interface ITyped
	{
		Types Type { get; }
		string Format { get; }
	}

	public interface IArrayLocation
	{
		int Frequency { get; }
		bool RemoveEmpty { get; }
	}

	public interface IArrayStopLocation : IArrayLocation
	{
		string StopText { get; }
	}

	public interface IArrayCountLocation : IArrayLocation
	{
		int Count { get; }
	}

	public enum Types { Int, Double, String, Bool }

	[Serializable()]
	public abstract class LocationBase : ITyped
	{
		public bool FixedColumn { get; protected set; }
		public bool FixedLine { get; protected set; }
		public Types Type { get; protected set; }
		public string Format { get; protected set; }

		public LocationBase(bool columnFixed, bool lineFixed, Types type = Types.String, string format = "")
		{
			FixedColumn = columnFixed;
			FixedLine = lineFixed;
			Type = type;
			Format = format;
		}

		public abstract void Update(TextFileManager tfm, object input);

		public object Read(TextFileManager tfm)
		{
			Type t = GetTypeFromTypeEnum();
			return Read_(tfm, t);
		}

		protected abstract object Read_(TextFileManager tfm, Type t);

		protected Type GetTypeFromTypeEnum()
		{
			switch (Type)
			{
				case Types.Int:
					return typeof(int);
				case Types.Double:
					return typeof(double);
				case Types.String:
					return typeof(string);
				case Types.Bool:
					return typeof(bool);
				default:
					return typeof(object);
			}
		}
	}
}
