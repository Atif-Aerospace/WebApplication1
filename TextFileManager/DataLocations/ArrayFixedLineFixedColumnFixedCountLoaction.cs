using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExeModelTextFileManager.DataLocations
{
	[Serializable()]
	public class ArrayFixedLineFixedColumnFixedCountLoaction : LocationBase, IFixedLine, IFixedColumn, IArrayCountLocation
	{
		public ArrayFixedLineFixedColumnFixedCountLoaction(int line, int count, int startColumn, int endColumn, int frequency, Types type = Types.String, string format = "", bool removeEmpty = true) : base(true, true, type, format)
		{
			Line = line;
			Count = count;
			StartColumn = startColumn;
			EndColumn = endColumn;
			Frequency = frequency;
			RemoveEmpty = removeEmpty;
		}

		public int Line { get; protected set; }
		public int Count { get; protected set; }
		public int StartColumn { get; protected set; }
		public int EndColumn { get; protected set; }
		public int Frequency { get; protected set; }
		public bool RemoveEmpty { get; protected set; }

		private MethodInfo mi;

		protected override object Read_(TextFileManager tfm, Type t)
		{
			if (mi == null)
			{
				mi = typeof(TextFileManagerExtensions).GetMethod("ReadArray", new Type[] { typeof(TextFileManager), typeof(Type), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool) }).MakeGenericMethod(new Type[] { t });
			}
			return mi.Invoke(null, new object[] { tfm, t, Line, Count, StartColumn, EndColumn, Frequency, RemoveEmpty });
		}

        public override void Update(TextFileManager tfm, object input)
        {
            //Type t = GetTypeFromTypeEnum();
            //var arrayType = t.MakeArrayType();
            //mi = typeof(TextFileManagerExtensions).GetMethod("UpdateArray", new Type[] { typeof(TextFileManager), typeof(int), typeof(int), typeof(int), arrayType, typeof(int), typeof(string) }).MakeGenericMethod(new Type[] { t });
            
            tfm.UpdateArray(Line, StartColumn, EndColumn, input as double[], Frequency, Format);
        }
    }
}
