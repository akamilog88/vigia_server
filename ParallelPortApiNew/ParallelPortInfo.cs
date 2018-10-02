using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenApi.SensorInterfaces.ParallelPort
{
    
    public class ParallelPortInfo
    {
        public const short P378 = 888;
        public const short P379 = 889;
        public const short P37A = 890;

        private int port378;
        private int port379;
        private int port37A;

        public ParallelPortInfo(int port378, int port379, int port37A)
        {
            this.port378 = port378;
            this.port379 = port379;
            this.port37A = port37A;
        }

        public int getPortState(short port) {

            if (port == ParallelPortInfo.P378)
                return this.port378;
            if (port == ParallelPortInfo.P379)
                return this.port379;
            if (port == ParallelPortInfo.P37A)
                return this.port37A;
            return -1;
        }

        public bool IQualTo(ParallelPortInfo other) {
            if (other.getPortState(ParallelPortInfo.P378) != this.port378 || other.getPortState(ParallelPortInfo.P379) != this.port379 || other.getPortState(ParallelPortInfo.P37A) != this.port37A)
                return false;
            return true;            
        }
    }
}
