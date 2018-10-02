using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenApi.SensorInterfaces.DTMFDetector
{
    class DTMFConstanst
    {
        internal const int FSAMPLE = 8000;
        internal const int N = 240;
        
        internal readonly int[] k = new int[] { 11, 13, 14, 19, 21, 23, 26, 27, 28, 33, 36, 39, 40,
 /*44,*/ 45, 49, 51, 72, 78, };

        /* coefficients for above k's as:
         *   2 * cos( 2*pi* k/N )
         */
        internal static readonly float[] coef = new float[]{
1.917639f, 1.885283f, 1.867161f, 1.757634f, 
1.705280f, 1.648252f, 1.554292f, 1.520812f, 1.486290f, 
1.298896f, 1.175571f, 1.044997f, 1.000000f, /* 0.813473,*/ 
0.765367f, 0.568031f, 0.466891f, -0.618034f, -0.907981f,  };

        internal const int X1 = 0;    /* 350 dialtone */
        internal const int X2 = 1;    /* 440 ring, dialtone */
        internal const int X3 = 2;    /* 480 ring, busy */
        internal const int X4 = 3;    /* 620 busy */

        internal const int R1 = 4;    /* 697, dtmf row 1 */
        internal const int R2 = 5;    /* 770, dtmf row 2 */
        internal const int R3 = 6;    /* 852, dtmf row 3 */
        internal const int R4 = 8;    /* 941, dtmf row 4 */
        internal const int C1 = 10;    /* 1209, dtmf col 1 */
        internal const int C2 = 12;    /* 1336, dtmf col 2 */
        internal const int C3 = 13;    /* 1477, dtmf col 3 */
        internal const int C4 = 14;    /* 1633, dtmf col 4 */

        internal const int B1 = 4;    /* 700, blue box 1 */
        internal const int B2 = 7;    /* 900, bb 2 */
        internal const int B3 = 9;    /* 1100, bb 3 */
        internal const int B4 = 11;    /* 1300, bb4 */
        internal const int B5 = 13;    /* 1500, bb5 */
        internal const int B6 = 15;    /* 1700, bb6 */
        internal const int B7 = 16;    /* 2400, bb7 */
        internal const int B8 = 17;    /* 2600, bb8 */

        internal const int NUMTONES = 18;

        /* values returned by detect 
         *  0-9     DTMF 0 through 9 or MF 0-9
         *  10-11   DTMF *, #
         *  12-15   DTMF A,B,C,D
         *  16-20   MF last column: C11, C12, KP1, KP2, ST
         *  21      2400
         *  22      2600
         *  23      2400 + 2600
         *  24      DIALTONE
         *  25      RING
         *  26      BUSY
         *  27      silence
         *  -1      invalid
         */
        public const int D0 = 0;
        public const int D1 = 1;
        public const int D2 = 2;
        public const int D3 = 3;
        public const int D4 = 4;
        public const int D5 = 5;
        public const int D6 = 6;
        public const int D7 = 7;
        public const int D8 = 8;
        public const int D9 = 9;
        public const int DSTAR = 10;
        public const int DPND = 11;
        public const int DA = 12;
        public const int DB = 13;
        public const int DC = 14;
        public const int DD = 15;
        public const int DC11 = 16;
        public const int DC12 = 17;
        public const int DKP1 = 18;
        public const int DKP2 = 19;
        public const int DST = 20;
        public const int D24 = 21;
        public const int D26 = 22;
        public const int D2426 = 23;
        public const int DDT = 24;
        public const int DRING = 25;
        public const int DBUSY = 26;
        public const int DSIL = 27;

        /* translation of above codes into text */
        internal static readonly String[] dtran = {
  "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
  "*", "#", "A", "B", "C", "D", 
  "+C11", "+C12", "KP1+", "KP2+", "+ST",
  "2400", "2600", "2400+2600",
  "DIALTONE", "RING", "BUSY","SILENCE" };
        internal const double RANGE = 0.1;           /* any thing higher than RANGE*peak is "on" */
        internal const double THRESH = 100;         /* minimum level for the loudest tone */
        //internal const double FLUSH_TIME = 100;       /* 100 frames = 3 seconds */
        internal const int FLUSH_TIME = 8;   /* (8 * 240 = 2000) ---- 8000 = 1 second  2000/8000 = 1/4 second  */
    }
}
