using Aircadia.Services.Serializers;
using System;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ArrayFixedLineFixedColumnLocation : LocationBase, IFixedLine, IFixedColumn, IArrayStopLocation
	{
		[DeserializeConstructor]
		public ArrayFixedLineFixedColumnLocation(int line, string stopText, int startColumn, int endColumn, int frequency, Types type = Types.String, string format = "", bool removeEmpty = true) : base(true, true, type, format)
		{
			RowIndex = line;
			StopText = stopText;
			StartColumn = startColumn;
			EndColumn = endColumn;
			Frequency = frequency;
			RemoveEmpty = removeEmpty;
		}

		[Serialize]
		public int RowIndex { get; protected set; }
		[Serialize]
		public string StopText { get; protected set; }
		[Serialize]
		public int StartColumn { get; protected set; }
		[Serialize]
		public int EndColumn { get; protected set; }
		[Serialize]
		public int Frequency { get; protected set; }
		[Serialize]
		public bool RemoveEmpty { get; protected set; }

		public override void Update(TextFileManager tfm, object input) => tfm.UpdateArray(RowIndex, StartColumn, EndColumn, input as object[], Frequency, Format);

		public override object Read(TextFileManager tfm)
		{
			switch (Type)
			{
				case Types.Int:
					return tfm.ReadArray<int>(RowIndex, StopText, StartColumn, EndColumn, Frequency, RemoveEmpty);
				case Types.Double:
					return tfm.ReadArray<double>(RowIndex, StopText, StartColumn, EndColumn, Frequency, RemoveEmpty);
				case Types.String:
					return tfm.ReadArray<string>(RowIndex, StopText, StartColumn, EndColumn, Frequency, RemoveEmpty);
				case Types.Bool:
					return tfm.ReadArray<bool>(RowIndex, StopText, StartColumn, EndColumn, Frequency, RemoveEmpty);
				default:
					return tfm.ReadArray<object>(RowIndex, StopText, StartColumn, EndColumn, Frequency, RemoveEmpty);
			}
		}
	}
}
