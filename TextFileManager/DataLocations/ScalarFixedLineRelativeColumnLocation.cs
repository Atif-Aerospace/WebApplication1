using System;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ScalarFixedLineRelativeColumnLocation : LocationBase, IFixedLine, IRelativeColumn
	{
		public ScalarFixedLineRelativeColumnLocation(int line, int position, char[] separators, Types type = Types.String, string format = "") : base(true, true, type, format)
		{
			Line = line;
			Position = position;
			Separators = separators;
		}

		public int Line { get; protected set; }
		public int Position { get; protected set; }
		public char[] Separators { get; protected set; }

		public override void Update(TextFileManager tfm, object input) => tfm.UpdateWord(Line, Position, Separators, input, Format);

		protected override object Read_(TextFileManager tfm, Type t) => tfm.ReadWord<object>(t, Line, Position, Separators);
	}
}
