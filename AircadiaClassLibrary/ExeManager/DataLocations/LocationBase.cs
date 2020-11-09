using Aircadia.Services.Serializers;
using System;
using System.Collections.Generic;

namespace ExeModelTextFileManager.DataLocations
{
	public interface IFixedLine
	{
		int RowIndex { get; }
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
		[Serialize]
		public Types Type { get; protected set; }
		[Serialize]
		public string Format { get; protected set; }

		public LocationBase(bool columnFixed, bool lineFixed, Types type = Types.String, string format = "")
		{
			FixedColumn = columnFixed;
			FixedLine = lineFixed;
			Type = type;
			Format = format;
		}

		public abstract void Update(TextFileManager tfm, object input);

		public abstract object Read(TextFileManager tfm);
	}
}
