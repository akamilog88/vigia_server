using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SenApi.SensorInterfaces.ParallelPort
{
    internal class PortAccessWrapper
    {
        /// <summary>
        /// This method will be used to send the data out to the parallel port.
        /// </summary>
        /// <param name="adress">Address of the port to which the data needs to be sent.</param>
        /// <param name="value">Data that need to send out.</param>
        [DllImport("inpout32.dll", EntryPoint = "Out32")]
        public static extern void Output(int address, int value);

        /// <summary>
        /// This method will be used to receive any data from the parallel port.
        /// </summary>
        /// <param name="address">Address of the port from which the data should be received.</param>
        /// <returns>Returns Integer read from the given port.</returns>
        [DllImport("inpout32.dll", EntryPoint = "Inp32")]
        public static extern int Input(int address);
    }

    public class PortAccessAPI {

        private static PortAccessAPI _instance;
        private PortAccessAPI(){
            
        }
        public static PortAccessAPI GetInstance() {
            if (_instance == null)
                _instance = new PortAccessAPI();
            return _instance;
        }

        public void Output(int address, int value) {
            PortAccessWrapper.Output(address, value);
        }

        public int Input(int address) {
            return PortAccessWrapper.Input(address);
        }
    }
}
