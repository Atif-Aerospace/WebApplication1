/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aircadia
{
    [Serializable()]
    ////Subclass of the treatment output class. This class was designed specifically for optimiser.
    public class Treatment_InOut_OP_Output : Treatment_InOut
    {
        public Treatment_InOut_OP_Output(string ot)
        {
            output = ot;
        }
    }
}
