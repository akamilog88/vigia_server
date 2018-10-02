using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace SenApi.SensorInterfaces.DTMFDetector
{
    class Program
    {
        static int count = 0;
        static DTMFDetector d = new DTMFDetector();
        static void Main(string[] args)
        {
            object S = new object();
            Timer t = new Timer(Increment, S, 0, 1000);

            d.ToneDetected += d_ToneDetected;
            d.EndRecording += d_EndRecording;
            d.DetectSilence = true;
            d.StartDetect();

            //Thread.Sleep(200);
            //d.Stop();
            Console.WriteLine("***Detecting DTMF***");
            Console.WriteLine("***Press <ENTER> To EXIT***");
            Console.ReadKey();
        }

        static void d_EndRecording(object sender, EventArgs e)
        {
            Console.WriteLine("End of Recording!!!");
        }
        static void Increment(object t)
        {
            lock (t)
            {
                count++;
            }
            if (count > 30)
                d.Stop();
        }
        static void d_ToneDetected(object sender, DTMFDetectedArgs e)
        {
            string code = e.ToneAscii;
            Console.WriteLine("Detected code:{0}--->duration:{1}", code, e.Duration);
            count = 0;
        }
    }
}
