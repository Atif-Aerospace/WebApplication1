using Aircadia.Services.Serializers;
using ExeModelTextFileManager;
using ExeModelTextFileManager.DataLocations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExeModelTextFileManager.DataLocations
{
    [Serializable()]
    public class RowArrayLocation_AbsRowAbsColStartRelColEnd : LocationBase, IFixedLine
    {

        [DeserializeConstructor]
        public RowArrayLocation_AbsRowAbsColStartRelColEnd(int RowIndex, int ColumnStartIndex, char[] valueSeparators, int ColumnEndIndex, Types type = Types.String, string format = "") : base(true, true, type, format)
        {
            this.RowIndex = RowIndex;

            this.ColumnStartIndex = ColumnStartIndex;
            this.ColumnEndIndex = ColumnEndIndex;

            ValueSeparators = valueSeparators;

        }

        [Serialize]
        public int RowIndex { get; protected set; }
        [Serialize]
        public int ColumnStartIndex { get; protected set; }
        [Serialize]
        public int ColumnEndIndex { get; protected set; }
        [Serialize]
        public char[] ValueSeparators { get; protected set; }
        


        public override void Update(TextFileManager tfm, object input) { }// => tfm.UpdateArray(Line, StartColumn, input as object[]);

        public override object Read(TextFileManager tfm)
        {
            switch (Type)
            {
                case Types.Int:
                    return tfm.ReadRowArray<int>(RowIndex, ColumnStartIndex, ColumnEndIndex, ValueSeparators);
                case Types.Double:
                    return tfm.ReadRowArray<double>(RowIndex, ColumnStartIndex, ColumnEndIndex, ValueSeparators);
                case Types.String:
                    return tfm.ReadRowArray<string>(RowIndex, ColumnStartIndex, ColumnEndIndex, ValueSeparators);
                //case Types.Bool:
                //    return tfm.ReadArray<bool>(Line, StopText, StartColumn, EndColumn, Frequency, RemoveEmpty);
                default:
                    return tfm.ReadRowArray<object>(RowIndex, ColumnStartIndex, ColumnEndIndex, ValueSeparators);
            }
        }
    }
}
