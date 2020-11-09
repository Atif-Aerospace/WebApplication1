using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aircadia.ObjectModel.Treatments.MVDA
{
    [Serializable()]
    public abstract class MVDA : Treatment
    {
        public MVDA(string name, string description)
            : base(name, description)
        {
        }
    }
}
