using Aircadia.Services.Serializers;
using System;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ScalarFixedLineFixedColumnLocation : LocationBase, IFixedLine, IFixedColumn
	{
		[DeserializeConstructor]
		public ScalarFixedLineFixedColumnLocation(int rowIndex, int startColumn, int endColumn, Types type = Types.String, string format = "") : base(true, true, type, format)
		{
			RowIndex = rowIndex;
			StartColumn = startColumn;
			EndColumn = endColumn;
		}

		[Serialize]
		public int RowIndex { get; protected set; }
		[Serialize]
		public int StartColumn { get; protected set; }
		[Serialize]
		public int EndColumn { get; protected set; }

		public override object Read(TextFileManager tfm)
		{
			switch (Type)
			{
				case Types.Int:
					return tfm.ReadWord<int>(RowIndex, StartColumn, EndColumn);
				case Types.Double:
					return tfm.ReadWord<double>(RowIndex, StartColumn, EndColumn);
				case Types.String:
					return tfm.ReadWord<string>(RowIndex, StartColumn, EndColumn);
				case Types.Bool:
					return tfm.ReadWord<bool>(RowIndex, StartColumn, EndColumn);
				default:
					return tfm.ReadWord<object>(RowIndex, StartColumn, EndColumn);
			}
		}

		public override void Update(TextFileManager tfm, object input) => tfm.UpdateWord(RowIndex, StartColumn, EndColumn, input, Format);
	}
}
