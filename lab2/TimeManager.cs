using System;
using System.Diagnostics;

namespace SendingFiles
{
    class TimeManager
    {
        string nameFile;
        DateTime waitTime;
        int wait;
        Stopwatch sw;
        static long freq = Stopwatch.Frequency;
        const int countNumbersAfterDot = 3;
        long time;
        long countAllBytes;
        long countBytesNow;
        long countTicksNow;
        long allBytes;
        bool isEnd;
        public TimeManager(int wait, string nameFile, long allBytes)
        {
            this.wait = wait;
            this.waitTime = DateTime.Now;
            this.allBytes = allBytes;
            this.nameFile = nameFile;
            this.sw = new Stopwatch();
            this.time = 0;
            countAllBytes = 0;
            countBytesNow = 0;
            countTicksNow = 0;
            isEnd = false;
        }

        public void Start() 
        { 
            sw.Start(); 
        }
        public void Restart() 
        { 
            sw.Restart(); 
        }

        public void Stop()
        {
            sw.Stop();
            time += sw.ElapsedTicks;
            countTicksNow += sw.ElapsedTicks;
        }

        public void AddBytes(long bytes)
        {
            countAllBytes += bytes;
            countBytesNow += bytes;
        }

        private void PrintSpeedInformation(string typeSpeed, double bytes, double ticks)
        {
            double speed = Math.Round(
                                    bytes * freq / ticks / 1_000_000,
                                    countNumbersAfterDot);
            Console.WriteLine(nameFile + $": {typeSpeed} speed is " + speed.ToString() + " MB/s");
        }

        public void PrintSpeed(bool periodic) 
        {

            if ((DateTime.Now.CompareTo(waitTime) >= 0) || !periodic && !isEnd) 
            {
                waitTime = waitTime.AddSeconds(wait);

                PrintSpeedInformation("moment", countBytesNow, countTicksNow);

                PrintSpeedInformation("average", countAllBytes, time);

                Console.WriteLine(nameFile + $": is {(countAllBytes * 100) / allBytes}% complete");
                Console.WriteLine(nameFile + $": spend {time / freq} seconds");
                
                IsEnd();
                RestartCountNow();
            }
        }

        private void RestartCountNow()
        {
            countBytesNow = 0;
            countTicksNow = 0;
        }

        private void IsEnd()
        {
            if (countAllBytes == allBytes)
            {
                isEnd = true;
            }
        }
    }
}