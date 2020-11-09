using Aircadia.Services.Serializers;
using System;
using System.Reflection;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ArrayFixedLineFixedColumnFixedCountLoaction : LocationBase, IFixedLine, IFixedColumn, IArrayCountLocation
	{
		[DeserializeConstructor]
		public ArrayFixedLineFixedColumnFixedCountLoaction(int rowIndex, int count, int startColumn, int endColumn, int frequency, Types type = Types.String, string format = "", bool removeEmpty = true) : base(true, true, type, format)
		{
			RowIndex = rowIndex;
			Count = count;
			StartColumn = startColumn;
			EndColumn = endColumn;
			Frequency = frequency;
			RemoveEmpty = removeEmpty;
		}

		[Serialize]
		public int RowIndex { get; protected set; }
		[Serialize]
		public int Count { get; protected set; }
		[Serialize]
		public int StartColumn { get; protected set; }
		[Serialize]
		public int EndColumn { get; protected set; }
		[Serialize]
		public int Frequency { get; protected set; }
		[Serialize]
		public bool RemoveEmpty { get; protected set; }

		public override void Update(TextFileManager tfm, object input) => tfm.UpdateArray(RowIndex, StartColumn, EndColumn, input as double[], Frequency, Format);

		public override object Read(TextFileManager tfm)
		{
			switch (Type)
			{
				case Types.Int:
					return tfm.ReadArray<int>(RowIndex, Count, StartColumn, EndColumn, Frequency, RemoveEmpty);
				case Types.Double:
					return tfm.ReadArray<double>(RowIndex, Count, StartColumn, EndColumn, Frequency, RemoveEmpty);
				case Types.String:
					return tfm.ReadArray<string>(RowIndex, Count, StartColumn, EndColumn, Frequency, RemoveEmpty);
				case Types.Bool:
					return tfm.ReadArray<bool>(RowIndex, Count, StartColumn, EndColumn, Frequency, RemoveEmpty);
				default:
					return tfm.ReadArray<object>(RowIndex, Count, StartColumn, EndColumn, Frequency, RemoveEmpty);
			}
		}
	}
}
