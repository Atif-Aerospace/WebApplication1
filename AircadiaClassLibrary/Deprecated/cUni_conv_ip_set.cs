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
    public class Uni_conv_ip_set : InputSet
    {
        public double uFactor;
        public Uni_conv_ip_set(double uf)
    {
        uFactor=uf;
    }
            
    }
    
}
