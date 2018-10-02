/* 
 *
 * goertzel aglorithm, find the power of different
 * frequencies in an N point DFT.
 *
 * ftone/fsample = k/N   
 * k and N are integers.  fsample is 8000 (8khz)
 * this means the *maximum* frequency resolution
 * is fsample/N (each step in k corresponds to a
 * step of fsample/N hz in ftone)
 *
 * N was chosen to minimize the sum of the K errors for
 * all the tones detected...  here are the results :
 *
 * Best N is 240, with the sum of all errors = 3.030002
 * freq  freq actual   k     kactual  kerr
 * ---- ------------  ------ ------- -----
 *  350 (366.66667)   10.500 (11)    0.500
 *  440 (433.33333)   13.200 (13)    0.200
 *  480 (466.66667)   14.400 (14)    0.400
 *  620 (633.33333)   18.600 (19)    0.400
 *  697 (700.00000)   20.910 (21)    0.090
 *  700 (700.00000)   21.000 (21)    0.000
 *  770 (766.66667)   23.100 (23)    0.100
 *  852 (866.66667)   25.560 (26)    0.440
 *  900 (900.00000)   27.000 (27)    0.000
 *  941 (933.33333)   28.230 (28)    0.230
 * 1100 (1100.00000)  33.000 (33)    0.000
 * 1209 (1200.00000)  36.270 (36)    0.270
 * 1300 (1300.00000)  39.000 (39)    0.000
 * 1336 (1333.33333)  40.080 (40)    0.080
 **** I took out 1477.. too close to 1500
 * 1477 (1466.66667)  44.310 (44)    0.310
 ****
 * 1500 (1500.00000)  45.000 (45)    0.000
 * 1633 (1633.33333)  48.990 (49)    0.010
 * 1700 (1700.00000)  51.000 (51)    0.000
 * 2400 (2400.00000)  72.000 (72)    0.000
 * 2600 (2600.00000)  78.000 (78)    0.000
 *
 * notice, 697 and 700hz are indestinguishable (same K)
 * all other tones have a seperate k value.  
 * these two tones must be treated as identical for our
 * analysis.
 *
 * The worst tones to detect are 350 (error = 0.5, 
 * detet 367 hz) and 852 (error = 0.44, detect 867hz). 
 * all others are very close.
 *
 */
/* This program will detect MF tones and normal
 * dtmf tones as well as some other common tones such
 * as BUSY, DIALTONE and RING.
 * The program uses a goertzel algorithm to detect
 * the power of various frequency ranges.
 *
 * input is assumed to be 8 bit samples.  
 */

/*
 * calculate the power of each tone according
 * to a modified goertzel algorithm described in
 *  _digital signal processing applications using the
 *  ADSP-2100 family_ by Analog Devices
 *
 * input is 'data',  N sample values
 *
 * ouput is 'power', NUMTONES values
 *  corresponding to the power of each tone 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace SenApi.SensorInterfaces.DTMFDetector
{
    public class DTMFDetector
    {
        
        WaveInEvent aSource;
        bool detecting = false;
        int CurrentTone = -1;
        int TC = 0;

        public bool DetectSilence { get; set; }

        object _lockObj = new object();
        
        public event EventHandler<DTMFDetectedArgs> ToneDetected;
        public event EventHandler<EventArgs> EndRecording;

        public DTMFDetector()
        {         
            aSource = new WaveInEvent();
            aSource.WaveFormat = new WaveFormat(8000, 8, 1);
            aSource.RecordingStopped += aSource_RecordingStopped;
            DetectSilence = false;
        }

        void aSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            EndRecording.BeginInvoke(this, new EventArgs(), null, null);
        }

        public void StartDetect() {
            aSource.DataAvailable += aSource_DataAvailable;
            aSource.StartRecording();
        }

        public void Stop() {
            aSource.StopRecording();
            aSource.DataAvailable -= aSource_DataAvailable;            
        }

        void aSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            lock (_lockObj)
            {               
                //datachunks.Enqueue(e.Buffer);
                DetectformDataChunk(e.Buffer,e.BytesRecorded);
            }
        }

        void DetectformDataChunk(byte[] chunk,int realLen) {

            int iterations = realLen / DTMFConstanst.N;

            for (int i = 0; i < iterations; i++)
            {
                int x = decode(chunk.Skip(i * DTMFConstanst.N).Take(DTMFConstanst.N).ToArray());
                if (x >= 0)
                {
                   if (CurrentTone == x )
                    {
                        TC++;
                        if (TC >= DTMFConstanst.FLUSH_TIME && x != DTMFConstanst.DSIL)
                        {
                            ToneDetected.BeginInvoke(this, new DTMFDetectedArgs(CurrentTone, (double)(TC * DTMFConstanst.N) / 8000), null, null);
                            TC = 0;
                        }
                        else {
                            if (x == DTMFConstanst.DSIL && DetectSilence && TC >= DTMFConstanst.FLUSH_TIME)
                            {
                                ToneDetected.BeginInvoke(this, new DTMFDetectedArgs(CurrentTone, (double)(TC * DTMFConstanst.N) / 8000), null, null);
                                TC = 0;
                            }
                        }                                           
                    }
                    else {
                        TC++;
                        if (ToneDetected != null) {
                            ToneDetected.BeginInvoke(this, new DTMFDetectedArgs(CurrentTone, (double)(TC * DTMFConstanst.N) / 8000), null, null);
                        }
                        TC = 0;                      
                        CurrentTone = x;
                    }   
                }
            }
        }

        int calc_power(byte[] data, float[] power)
        {
            float[] u0 = new float[DTMFConstanst.NUMTONES];
            float[] u1 = new float[DTMFConstanst.NUMTONES];
            float t, inn;
            int i, j;

            for (j = 0; j < DTMFConstanst.NUMTONES; j++)
            {
                u0[j] = 0.0f;
                u1[j] = 0.0f;
            }
            for (i = 0; i < DTMFConstanst.N; i++)
            {   /* feedback */
                inn = (float)(data[i] / 128.0);
                for (j = 0; j < DTMFConstanst.NUMTONES; j++)
                {
                    t = u0[j];
                    u0[j] = inn + DTMFConstanst.coef[j] * u0[j] - u1[j];
                    u1[j] = t;
                }
            }
            for (j = 0; j < DTMFConstanst.NUMTONES; j++)   /* feedforward */
                power[j] = u0[j] * u0[j] + u1[j] * u1[j] - DTMFConstanst.coef[j] * u0[j] * u1[j];
            return (0);
        }

        /*
         * detect which signals are present.
         *
         * return values defined in the include file
         * note: DTMF 3 and MF 7 conflict.  To resolve
         * this the program only reports MF 7 between
         * a KP and an ST, otherwise DTMF 3 is returned
         */
        int decode(byte[] data)
        {
            float[] power = new float[DTMFConstanst.NUMTONES];
            float thresh;
            float maxpower;
            int[] on = new int[DTMFConstanst.NUMTONES];
            int on_count;
            int bcount, rcount, ccount;
            int row = 0, col = 0, b1 = 0, b2 = 0, i = 0;
            int[] r = new int[4];
            int[] c = new int[4];
            int[] b = new int[8];
            bool MFmode = false;

            calc_power(data, power);
          
            for (i = 0, maxpower = 0.0f; i < DTMFConstanst.NUMTONES; i++)
                if (power[i] > maxpower)
                    maxpower = power[i];

            if (maxpower < DTMFConstanst.THRESH)  /* silence? */
                return (DTMFConstanst.DSIL);
            thresh = (float)(DTMFConstanst.RANGE * maxpower);    /* allowable range of powers */
            for (i = 0, on_count = 0; i < DTMFConstanst.NUMTONES; i++)
            {
                if (power[i] > thresh)
                {
                    on[i] = 1;
                    on_count++;
                }
                else
                    on[i] = 0;
            }
       
            if (on_count == 1)
            {
                if (on[DTMFConstanst.B7] == 1)
                    return (DTMFConstanst.D24);
                if (on[DTMFConstanst.B8] == 1)
                    return (DTMFConstanst.D26);
                return (-1);
            }

            if (on_count == 2)
            {
                if (on[DTMFConstanst.X1] == 1 && on[DTMFConstanst.X2] == 1)
                    return (DTMFConstanst.DDT);
                if (on[DTMFConstanst.X2] == 1 && on[DTMFConstanst.X3] == 1)
                    return (DTMFConstanst.DRING);
                if (on[DTMFConstanst.X3] == 1 && on[DTMFConstanst.X4] == 1)
                    return (DTMFConstanst.DBUSY);

                b[0] = on[DTMFConstanst.B1]; b[1] = on[DTMFConstanst.B2]; b[2] = on[DTMFConstanst.B3]; b[3] = on[DTMFConstanst.B4];
                b[4] = on[DTMFConstanst.B5]; b[5] = on[DTMFConstanst.B6]; b[6] = on[DTMFConstanst.B7]; b[7] = on[DTMFConstanst.B8];
                c[0] = on[DTMFConstanst.C1]; c[1] = on[DTMFConstanst.C2]; c[2] = on[DTMFConstanst.C3]; c[3] = on[DTMFConstanst.C4];
                r[0] = on[DTMFConstanst.R1]; r[1] = on[DTMFConstanst.R2]; r[2] = on[DTMFConstanst.R3]; r[3] = on[DTMFConstanst.R4];

                for (i = 0, bcount = 0; i < 8; i++)
                {
                    if (b[i] == 1)
                    {
                        bcount++;
                        b2 = b1;
                        b1 = i;
                    }
                }
                for (i = 0, rcount = 0; i < 4; i++)
                {
                    if (r[i] == 1)
                    {
                        rcount++;
                        row = i;
                    }
                }
                for (i = 0, ccount = 0; i < 4; i++)
                {
                    if (c[i] == 1)
                    {
                        ccount++;
                        col = i;
                    }
                }

                if (rcount == 1 && ccount == 1)
                {   /* DTMF */
                    if (col == 3)  /* A,B,C,D */
                        return (DTMFConstanst.DA + row);
                    else
                    {
                        if (row == 3 && col == 0)
                            return (DTMFConstanst.DSTAR);
                        if (row == 3 && col == 2)
                            return (DTMFConstanst.DPND);
                        if (row == 3)
                            return (DTMFConstanst.D0);
                        if (row == 0 && col == 2)
                        {   /* DTMF 3 conflicts with MF 7 */
                            if (!MFmode)
                                return (DTMFConstanst.D3);
                        }
                        else
                            return (DTMFConstanst.D1 + col + row * 3);
                    }
                }

                if (bcount == 2)
                {       /* MF */
                    /* b1 has upper number, b2 has lower */
                    switch (b1)
                    {
                        case 7: return ((b2 == 6) ? DTMFConstanst.D2426 : -1);
                        case 6: return (-1);
                        case 5: if (b2 == 2 || b2 == 3)  /* KP */
                                MFmode = true;
                            if (b2 == 4)  /* ST */
                                MFmode = false;
                            return (DTMFConstanst.DC11 + b2);
                        /* MF 7 conflicts with DTMF 3, but if we made it
                         * here then DTMF 3 was already tested for 
                         */
                        case 4: return ((b2 == 3) ? DTMFConstanst.D0 : DTMFConstanst.D7 + b2);
                        case 3: return (DTMFConstanst.D4 + b2);
                        case 2: return (DTMFConstanst.D2 + b2);
                        case 1: return (DTMFConstanst.D1);
                    }
                }
                return (-1);
            }

            if (on_count == 0)
                return (DTMFConstanst.DSIL);
            return (-1);
        }
    }
}