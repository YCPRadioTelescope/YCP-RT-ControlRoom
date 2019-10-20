﻿using System;
using System.Threading;
using ControlRoomApplication.Constants;

namespace ControlRoomApplication.Entities
{
    public abstract class AbstractWeatherStation : HeartbeatInterface
    {
        protected Mutex OperatingMutex;
        protected Thread OperatingThread;
        protected bool KeepOperatingThreadAlive;

        private double _CurrentWindSpeedMPH;

        public double CurrentWindSpeedMPH
        {
            get
            {
                OperatingMutex.WaitOne();
                double read = GetWindSpeed();
                OperatingMutex.ReleaseMutex();

                return read;
            }
        }

        public int CurrentWindSpeedScanDelayMS { get; }

        public bool CurrentWindSpeedIsAllowable
        {
            get
            {
                return CurrentWindSpeedMPH <= MiscellaneousHardwareConstants.WEATHER_STATION_MAXIMUM_ALLOWABLE_WIND_SPEED_MPH;
            }
        }

        public AbstractWeatherStation(int currentWindSpeedScanDelayMS)
        {
            CurrentWindSpeedScanDelayMS = currentWindSpeedScanDelayMS;
            _CurrentWindSpeedMPH = 0.0;
            OperatingMutex = new Mutex();
            OperatingThread = new Thread(new ThreadStart(OperationLoop));
            KeepOperatingThreadAlive = false;
        }

        public bool Start()
        {
            KeepOperatingThreadAlive = true;

            try
            {
                OperatingThread.Start();
            }
            catch (Exception e)
            {
                if ((e is ThreadStateException) || (e is OutOfMemoryException))
                {
                    return false;
                }
                else
                {
                    // Unexpected exception
                    throw e;
                }
            }

            return true;
        }

        public bool RequestKillAndJoin()
        {
            try
            {
                OperatingMutex.WaitOne();
                KeepOperatingThreadAlive = false;
                OperatingMutex.ReleaseMutex();

                OperatingThread.Join();
            }
            catch (Exception e)
            {
                if ((e is ObjectDisposedException) || (e is AbandonedMutexException) || (e is InvalidOperationException)
                    || (e is ApplicationException) || (e is ThreadStateException) | (e is ThreadInterruptedException))
                {
                    return false;
                }
                else
                {
                    // Unexpected exception
                    throw e;
                }
            }

            return true;
        }

        public void OperationLoop()
        {
            OperatingMutex.WaitOne();
            bool KeepAlive = KeepOperatingThreadAlive;
            OperatingMutex.ReleaseMutex();

            while (KeepAlive)
            {
                OperatingMutex.WaitOne();
                _CurrentWindSpeedMPH = GetWindSpeed();
                KeepAlive = KeepOperatingThreadAlive;
                OperatingMutex.ReleaseMutex();

                Thread.Sleep(CurrentWindSpeedScanDelayMS);
            }
        }

        protected override bool KillHeartbeatComponent()
        {
            return RequestKillAndJoin();
        }


        protected abstract void InitializeStation();
        
        protected abstract float GetBarometricPressure();
        protected abstract float GetOutsideTemp();
        protected abstract float GetDewPoint();
        protected abstract float GetWindChill();
        protected abstract int GetHumidity();
        protected abstract float GetTotalRain();
        protected abstract float GetDailyRain();
        protected abstract float GetMonthlyRain();
        public abstract float GetWindSpeed();
        protected abstract char GetWindDirection();
        protected abstract float GetRainRate();
        protected abstract int GetHeatIndex();

        // An abstract method that will get all of the information
        // to be able to put into the database cleanly
        //protected abstract short GetAllRecords();
    }
}
