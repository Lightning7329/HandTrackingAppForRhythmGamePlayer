using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KW_Mocap
{
    public static class WorldTimer
    {
        public static Action CountUp;
        private static Timer timer = null;

        public static int frameCount { get; private set; } = 0;

        private static float _frameRate = 40.0f;
        public static float frameRate
        {
            get => _frameRate;
            set => _frameRate = value > 0.0f ? value : 30.0f;
        }
        private static int period;

        public static void Run()
        {
            if (timer != null) return;
            period = (int)(1000f / _frameRate);
            timer = new Timer(state => { frameCount++; CountUp(); }, null, 0, period);
        }

        public static void Stop()
        {
            if (timer == null) return;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            timer.Dispose();
            timer = null;
        }

        public static void FrameCountReset()
        {
            frameCount = 0;
        }

        public static void DisplayFrameCount()
        {
            Debug.Log("World Timer: " + frameCount.ToString());
        }
    }
}
