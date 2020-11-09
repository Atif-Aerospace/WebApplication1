using System;

namespace Aircadia.ObjectModel.Studies
{
	[Serializable()]
    public class MVDAStudy : Study
    {
        public string DatabaseFileName
        {
            get;
            set;
        }
        public MVDAStudy(string name, string description)
            : base(name, description)
        {       
        }

        public override bool Execute()
        {
            bool status = true;



            //this.Treatment.Result = this.ActiveResult;

            //status = this.Treatment.ApplyOn();



            return status;
        }
    }
}