using System;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ScalarFixedLineFixedColumnLocation : LocationBase, IFixedLine, IFixedColumn
	{
		public ScalarFixedLineFixedColumnLocation(int line, int startColumn, int endColumn, Types type = Types.String, string format = "") : base(true, true, type, format)
		{
			Line = line;
			StartColumn = startColumn;
			EndColumn = endColumn;
		}

		public int Line { get; protected set; }
		public int StartColumn { get; protected set; }
		public int EndColumn { get; protected set; }

		protected override object Read_(TextFileManager tfm, Type t) => tfm.ReadWord<object>(t, Line, StartColumn, EndColumn);

		public override void Update(TextFileManager tfm, object input) => tfm.UpdateWord(Line, StartColumn, EndColumn, input, Format);
	}
}
