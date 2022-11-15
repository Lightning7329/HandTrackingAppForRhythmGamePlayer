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

        private static int _frameRate = 30;
        public static int frameRate
        {
            get => _frameRate;
            set => _frameRate = value > 0 ? value : 30;
        }
        private static int period = (int)(1000f / _frameRate);

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

        public static void ChangeSpeed(float speedRatio)
        {
            if (timer != null) return;
            period = (int)(period / speedRatio);
            timer.Change(0, period);
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
