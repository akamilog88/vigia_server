using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenApi.SensorInterfaces.DTMFDetector
{
    public class DTMFDetectedArgs: EventArgs
    {
        int TonConstant;
        public double Duration { get; private set; }
        public String ToneAscii
        {
            get
            {
                return DTMFConstanst.dtran[TonConstant];
            }
        }
        public DTMFDetectedArgs(int ToneCodeConstant,double duration) {
            this.TonConstant = ToneCodeConstant;
            this.Duration = duration;
        }
    }
}
