using Aircadia.Services.Serializers;
using System;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ArrayFixedLineRelativeColumnLocation : LocationBase, IFixedLine, IRelativeColumn, IArrayStopLocation
	{
		[DeserializeConstructor]
		public ArrayFixedLineRelativeColumnLocation(int line, string stopText, int position, char[] separators, int frequency = 1, Types type = Types.String, string format = "", bool removeEmpty = true) : base(true, true, type, format)
		{
			RowIndex = line;
			StopText = stopText;
			Position = position;
			Separators = separators;
			Frequency = frequency;
		}

		[Serialize]
		public int RowIndex { get; protected set; }
		[Serialize]
		public string StopText { get; protected set; }
		[Serialize]
		public int Position { get; protected set; }
		[Serialize]
		public char[] Separators { get; protected set; }
		[Serialize]
		public int Frequency { get; protected set; }
		[Serialize]
		public bool RemoveEmpty { get; protected set; }

		public override void Update(TextFileManager tfm, object input) => tfm.UpdateArray(RowIndex, Position, input as object[], Separators, Frequency, Format);

		public override object Read(TextFileManager tfm)
		{
			switch (Type)
			{
				case Types.Int:
					return tfm.ReadArray<int>(RowIndex, StopText, Position, Frequency, Separators, RemoveEmpty);
				case Types.Double:
					return tfm.ReadArray<double>(RowIndex, StopText, Position, Frequency, Separators, RemoveEmpty);
				case Types.String:
					return tfm.ReadArray<string>(RowIndex, StopText, Position, Frequency, Separators, RemoveEmpty);
				case Types.Bool:
					return tfm.ReadArray<bool>(RowIndex, StopText, Position, Frequency, Separators, RemoveEmpty);
				default:
					return tfm.ReadArray<object>(RowIndex, StopText, Position, Frequency, Separators, RemoveEmpty);
			}
		}
	}
}
