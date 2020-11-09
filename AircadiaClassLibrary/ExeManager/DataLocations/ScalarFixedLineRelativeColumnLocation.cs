using Aircadia.Services.Serializers;
using System;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ScalarFixedLineRelativeColumnLocation : LocationBase, IFixedLine, IRelativeColumn
	{
		[DeserializeConstructor]
		public ScalarFixedLineRelativeColumnLocation(int line, int position, char[] separators, Types type = Types.String, string format = "") : base(true, true, type, format)
		{
			RowIndex = line;
			Position = position;
			Separators = separators;
		}

		[Serialize]
		public int RowIndex { get; protected set; }
		[Serialize]
		public int Position { get; protected set; }
		[Serialize]
		public char[] Separators { get; protected set; }

		public override void Update(TextFileManager tfm, object input) => tfm.UpdateWord(RowIndex, Position, Separators, input, Format);

		public override object Read(TextFileManager tfm)
		{
			switch (Type)
			{
				case Types.Int:
					return tfm.ReadWord<int>(RowIndex, Position, Separators);
				case Types.Double:
					return tfm.ReadWord<double>(RowIndex, Position, Separators);
				case Types.String:
					return tfm.ReadWord<string>(RowIndex, Position, Separators);
				case Types.Bool:
					return tfm.ReadWord<bool>(RowIndex, Position, Separators);
				default:
					return tfm.ReadWord<object>(RowIndex, Position, Separators);
			}
		}
	}
}
