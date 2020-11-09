/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aircadia
{
    [Serializable()]
    ////Subclass of the treatment input class. This class was designed specifically for trade-study.
    public class Treatment_InOut_TS_Input :Treatment_InOut
    {
        public Treatment_InOut_TS_Input(ArrayList sl)
        {
            setuplist.AddRange(sl.ToArray());
        }
    }
}
