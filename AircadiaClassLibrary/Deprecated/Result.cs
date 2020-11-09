using System;
using System.Collections.Generic;

using Aircadia.ObjectModel;

namespace Aircadia
{
	[Serializable]
    public class Result : AircadiaComponent
    {
        public string DatabasePath
        {
            get;
            set;
        }


        public bool IsComplete = false;


        public int BatchSize
        {
            get;
            set;
        }
        public int NoOfBatches
        {
            get;
            set;
        }
		public List<double> MinValues { get; } = new List<double>();
		public List<double> MaxValues { get; } = new List<double>();

		public Result(string name) : base(name, null, null)
        {
        }


    }
}
