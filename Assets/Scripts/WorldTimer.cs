using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KW_Mocap
{
    public static class WorldTimer
    {
        private static Timer timer = null;

        public static int frameCount { get; private set; } = 0;

        private static int _frameRate = 30;
        public static int frameRate
        {
            get => _frameRate;
            set => _frameRate = value > 0 ? value : 30;
        }

        public static Action CountUp;

        public static void Run()
        {
            if (timer != null) return;

            int dT = (int)(1000f / _frameRate);
            timer = new Timer(state => { frameCount++; CountUp(); }, null, 0, dT);
        }

        public static void Stop()
        {
            if (timer == null) return;
            
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            timer.Dispose();
            timer = null;
        }

        public static void PlyCountReset()
        {
            frameCount = 0;
        }

        public static void DisplayFrameCount()
        {
            Debug.Log("World Timer: " + frameCount.ToString());
        }
    }
}
