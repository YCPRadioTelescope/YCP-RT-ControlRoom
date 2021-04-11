﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControlRoomApplication.Constants;
using ControlRoomApplication.Entities;
using System.Threading;
using ControlRoomApplication.Controllers.Sensors;
using ControlRoomApplication.Database;
using ControlRoomApplication.Controllers.Communications;
using ControlRoomApplication.Util;


namespace ControlRoomApplication.Controllers
{
    public class RadioTelescopeController
    {
        public RadioTelescope RadioTelescope { get; set; }
        public CoordinateCalculationController CoordinateController { get; set; }
        public OverrideSwitchData overrides;

        // Thread that monitors database current temperature
        private Thread SensorMonitoringThread;
        private bool MonitoringSensors;
        private bool AllSensorsSafe;

        private double MaxElTempThreshold;
        private double MaxAzTempThreshold;

        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructor that takes an AbstractRadioTelescope object and sets the
        /// corresponding field.
        /// </summary>
        /// <param name="radioTelescope"></param>
        public RadioTelescopeController(RadioTelescope radioTelescope)
        {
            RadioTelescope = radioTelescope;
            CoordinateController = new CoordinateCalculationController(radioTelescope.Location);

            overrides = new OverrideSwitchData(radioTelescope);

            SensorMonitoringThread = new Thread(SensorMonitor);
            SensorMonitoringThread.Start();
            MonitoringSensors = true;
            AllSensorsSafe = true;

            MaxAzTempThreshold = DatabaseOperations.GetThresholdForSensor(SensorItemEnum.AZ_MOTOR_TEMP);
            MaxElTempThreshold = DatabaseOperations.GetThresholdForSensor(SensorItemEnum.ELEV_MOTOR_TEMP);
        }

        /// <summary>
        /// Gets the status of whether this RT is responding.
        /// 
        /// The implementation of this functionality is on a "per-RT" basis, as
        /// in this may or may not work, it depends on if the derived
        /// AbstractRadioTelescope class has implemented it.
        /// </summary>
        /// <returns> Whether or not the RT responded. </returns>
        public bool TestCommunication()
        {
            return RadioTelescope.PLCDriver.Test_Connection();
        }

        /// <summary>
        /// Gets the current orientation of the radiotelescope in azimuth and elevation.
        /// 
        /// The implementation of this functionality is on a "per-RT" basis, as
        /// in this may or may not work, it depends on if the derived
        /// AbstractRadioTelescope class has implemented it.
        /// </summary>
        /// <returns> An orientation object that holds the current azimuth/elevation of the scale model. </returns>
        public Orientation GetCurrentOrientation()
        {
            return RadioTelescope.PLCDriver.read_Position();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Orientation GetAbsoluteOrientation()
        {
            return RadioTelescope.SensorNetworkServer.CurrentAbsoluteOrientation;
        }

        /// <summary>
        /// Gets the status of the interlock system associated with this Radio Telescope.
        /// 
        /// The implementation of this functionality is on a "per-RT" basis, as
        /// in this may or may not work, it depends on if the derived
        /// AbstractRadioTelescope class has implemented it.
        /// </summary>
        /// <returns> Returns true if the safety interlock system is still secured, false otherwise. </returns>
        public bool GetCurrentSafetyInterlockStatus()
        {
            return RadioTelescope.PLCDriver.Get_interlock_status();
        }

        /// <summary>
        /// Method used to cancel this Radio Telescope's current attempt to change orientation.
        /// 
        /// The implementation of this functionality is on a "per-RT" basis, as
        /// in this may or may not work, it depends on if the derived
        /// AbstractRadioTelescope class has implemented it.
        /// </summary>
        public bool CancelCurrentMoveCommand()
        {
            return RadioTelescope.PLCDriver.Cancel_move();
        }

        /// <summary>
        /// Method used to shutdown the Radio Telescope in the case of inclement
        /// weather, maintenance, etcetera.
        /// 
        /// The implementation of this functionality is on a "per-RT" basis, as
        /// in this may or may not work, it depends on if the derived
        /// AbstractRadioTelescope class has implemented it.
        /// </summary>
        public bool ShutdownRadioTelescope()
        {
            return RadioTelescope.PLCDriver.Shutdown_PLC_MCU();
        }

        /// <summary>
        /// Method used to calibrate the Radio Telescope before each observation.
        /// 
        /// The implementation of this functionality is on a "per-RT" basis, as
        /// in this may or may not work, it depends on if the derived
        /// AbstractRadioTelescope class has implemented it.
        /// </summary>
        public Task<bool> ThermalCalibrateRadioTelescope()
        {
            if (!AllSensorsSafe) return Task.FromResult(false);
            return RadioTelescope.PLCDriver.Thermal_Calibrate(); // MOVE
        }

        /// <summary>
        /// Method used to request to set configuration of elements of the RT.
        /// takes the starting speed of the motor in RPM (speed of tellescope after gearing)
        /// </summary>
        /// <param name="startSpeedAzimuth">RPM</param>
        /// <param name="startSpeedElevation">RPM</param>
        /// <param name="homeTimeoutAzimuth">SEC</param>
        /// <param name="homeTimeoutElevation">SEC</param>
        /// <returns></returns>
        public bool ConfigureRadioTelescope(double startSpeedAzimuth, double startSpeedElevation, int homeTimeoutAzimuth, int homeTimeoutElevation)
        {
            return RadioTelescope.PLCDriver.Configure_MCU(startSpeedAzimuth, startSpeedElevation, homeTimeoutAzimuth, homeTimeoutElevation); // NO MOVE
        }

        /// <summary>
        /// Method used to request to move the Radio Telescope to an objective
        /// azimuth/elevation orientation.
        /// 
        /// The implementation of this functionality is on a "per-RT" basis, as
        /// in this may or may not work, it depends on if the derived
        /// AbstractRadioTelescope class has implemented it.
        /// <see cref="Controllers.BlkHeadUcontroler.EncoderReader"/>
        /// </summary>
        public Task<bool> MoveRadioTelescopeToOrientation(Orientation orientation)//TODO: once its intagrated use the microcontrole to get the current opsition 
        {
            if (!AllSensorsSafe) return Task.FromResult(false);
            return RadioTelescope.PLCDriver.Move_to_orientation(orientation, RadioTelescope.PLCDriver.read_Position()); // MOVE
        }

        /// <summary>
        /// Method used to request to move the Radio Telescope to an objective
        /// right ascension/declination coordinate pair.
        /// 
        /// The implementation of this functionality is on a "per-RT" basis, as
        /// in this may or may not work, it depends on if the derived
        /// AbstractRadioTelescope class has implemented it.
        /// </summary>
        public Task<bool> MoveRadioTelescopeToCoordinate(Coordinate coordinate)
        {
            if (!AllSensorsSafe) return Task.FromResult(false);
            return MoveRadioTelescopeToOrientation(CoordinateController.CoordinateToOrientation(coordinate, DateTime.UtcNow)); // MOVE
        }


        /// <summary>
        /// Method used to request to start jogging the Radio Telescope's azimuth
        /// at a speed (in RPM), in either the clockwise or counter-clockwise direction.
        /// 
        /// The implementation of this functionality is on a "per-RT" basis, as
        /// in this may or may not work, it depends on if the derived
        /// AbstractRadioTelescope class has implemented it.
        /// </summary>
        public bool StartRadioTelescopeAzimuthJog(double speed, bool PositiveDIR)
        {
            if (!AllSensorsSafe) return false;
            return RadioTelescope.PLCDriver.Start_jog( speed, PositiveDIR, 0,false );// MOVE
        }

        /// <summary>
        /// Method used to request to start jogging the Radio Telescope's elevation
        /// at a speed (in RPM), in either the clockwise or counter-clockwise direction.
        /// 
        /// The implementation of this functionality is on a "per-RT" basis, as
        /// in this may or may not work, it depends on if the derived
        /// AbstractRadioTelescope class has implemented it.
        /// </summary>
        public bool StartRadioTelescopeElevationJog(double speed, bool PositiveDIR)
        {
            if (!AllSensorsSafe) return false;
            return RadioTelescope.PLCDriver.Start_jog( 0,false,speed, PositiveDIR);// MOVE
        }


        /// <summary>
        /// send a clear move to the MCU to stop a jog
        /// </summary>
        public bool ExecuteRadioTelescopeStopJog() {
            return RadioTelescope.PLCDriver.Stop_Jog();
        }

        /// <summary>
        /// Method used to request that all of the Radio Telescope's movement comes
        /// to a controlled stop. this will not work for jog moves use 
        /// 
        /// The implementation of this functionality is on a "per-RT" basis, as
        /// in this may or may not work, it depends on if the derived
        /// AbstractRadioTelescope class has implemented it.
        /// </summary>
        public bool ExecuteRadioTelescopeControlledStop()
        {
            return RadioTelescope.PLCDriver.ControlledStop(); // NO MOVE
        }

        /// <summary>
        /// Method used to request that all of the Radio Telescope's movement comes
        /// to an immediate stop.
        /// 
        /// The implementation of this functionality is on a "per-RT" basis, as
        /// in this may or may not work, it depends on if the derived
        /// AbstractRadioTelescope class has implemented it.
        /// </summary>
        public bool ExecuteRadioTelescopeImmediateStop()
        {
            return RadioTelescope.PLCDriver.ImmediateStop(); // NO MOVE
        }


        /// <summary>
        /// return true if the RT has finished the previous move comand
        /// </summary>
        public bool finished_exicuting_move( RadioTelescopeAxisEnum axis )//[7]
        {
             
            var Taz = RadioTelescope.PLCDriver.GET_MCU_Status( RadioTelescopeAxisEnum.AZIMUTH );  //Task.Run( async () => { await  } );
            var Tel = RadioTelescope.PLCDriver.GET_MCU_Status( RadioTelescopeAxisEnum.ELEVATION );

            Taz.Wait();
            bool azFin = Taz.Result[(int)MCUConstants.MCUStatusBitsMSW.Move_Complete];
            bool elFin = Tel.GetAwaiter().GetResult()[(int)MCUConstants.MCUStatusBitsMSW.Move_Complete];
            if(axis == RadioTelescopeAxisEnum.BOTH) {
                return elFin && azFin;
            } else if(axis == RadioTelescopeAxisEnum.AZIMUTH) {
                return azFin;
            } else if(axis == RadioTelescopeAxisEnum.ELEVATION) {
                return elFin;
            }
            return false;
        }


        private static bool ResponseMetBasicExpectations(byte[] ResponseBytes, int ExpectedSize)
        {
            return ((ResponseBytes[0] + (ResponseBytes[1] * 256)) == ExpectedSize) && (ResponseBytes[2] == 0x1);
            //TODO: throws object is not instance of object when the  PLCClientCommunicationHandler.ReadResponse() retuns null usually due to time out

         }

        private static bool MinorResponseIsValid(byte[] MinorResponseBytes)
        {
            
            System.Diagnostics.Debug.WriteLine(MinorResponseBytes);
            return ResponseMetBasicExpectations(MinorResponseBytes, 0x3);
        }

        // Checks the motor temperatures against acceptable ranges every second
        private void SensorMonitor()
        {
            // Getting initial current temperatures
            Temperature currAzTemp = RadioTelescope.SensorNetworkServer.CurrentAzimuthMotorTemp[RadioTelescope.SensorNetworkServer.CurrentAzimuthMotorTemp.Length - 1];
            Temperature currElTemp = RadioTelescope.SensorNetworkServer.CurrentElevationMotorTemp[RadioTelescope.SensorNetworkServer.CurrentElevationMotorTemp.Length - 1];
            bool elTempSafe = checkTemp(currElTemp, true);
            bool azTempSafe = checkTemp(currAzTemp, true);

            // Sensor overrides must be taken into account
            bool currentAZOveride = overrides.overrideAzimuthMotTemp;
            bool currentELOveride = overrides.overrideElevatMotTemp;

            // Loop through every one second to get new temperatures. If the temperature has changed, notify the user
            while (MonitoringSensors)
            {
                azTempSafe = checkTemp(RadioTelescope.SensorNetworkServer.CurrentAzimuthMotorTemp[RadioTelescope.SensorNetworkServer.CurrentAzimuthMotorTemp.Length - 1], azTempSafe);
                elTempSafe = checkTemp(RadioTelescope.SensorNetworkServer.CurrentElevationMotorTemp[RadioTelescope.SensorNetworkServer.CurrentElevationMotorTemp.Length - 1], elTempSafe);
                
                // Determines if the telescope is in a safe state
                if (azTempSafe && elTempSafe) AllSensorsSafe = true;
                else AllSensorsSafe = false;
                
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        ///  Checks that the motor temperatures are within acceptable ranges. If the temperature exceeds
        ///  the corresponding value in SimulationConstants.cs, it will return false, otherwise
        ///  it will return true if everything is good.
        ///  Tl;dr:
        ///  False - bad
        ///  True - good
        /// </summary>
        /// <returns>override bool</returns>
        public bool checkTemp(Temperature t, bool lastIsSafe)
        {
            // get maximum temperature threshold
            double max;

            // Determine whether azimuth or elevation
            String s;
            bool isOverridden;
            if (t.location_ID == (int)SensorLocationEnum.AZ_MOTOR)
            {
                s = "Azimuth";
                isOverridden = overrides.overrideAzimuthMotTemp;
                max = MaxAzTempThreshold;
            }
            else
            {
                s = "Elevation";
                isOverridden = overrides.overrideElevatMotTemp;
                max = MaxElTempThreshold;
            }

            // Check temperatures
            if (t.temp < SimulationConstants.MIN_MOTOR_TEMP)
            {
                if (lastIsSafe)
                {
                    logger.Info(Utilities.GetTimeStamp() + ": " + s + " motor temperature BELOW stable temperature by " + Math.Truncate(SimulationConstants.STABLE_MOTOR_TEMP - t.temp) + " degrees Fahrenheit.");

                    pushNotification.sendToAllAdmins("MOTOR TEMPERATURE", s + " motor temperature BELOW stable temperature by " + Math.Truncate(SimulationConstants.STABLE_MOTOR_TEMP - t.temp) + " degrees Fahrenheit.");
                    EmailNotifications.sendToAllAdmins("MOTOR TEMPERATURE", s + " motor temperature BELOW stable temperature by " + Math.Truncate(SimulationConstants.STABLE_MOTOR_TEMP - t.temp) + " degrees Fahrenheit.");
                }
                    
                // Only overrides if switch is true
                if (!isOverridden) return false;
                else return true;
            }
            else if (t.temp > SimulationConstants.OVERHEAT_MOTOR_TEMP)
            {
                if (lastIsSafe)
                {
                    logger.Info(Utilities.GetTimeStamp() + ": " + s + " motor temperature OVERHEATING by " + Math.Truncate(t.temp - max) + " degrees Fahrenheit.");

                    pushNotification.sendToAllAdmins("MOTOR TEMPERATURE", s + " motor temperature OVERHEATING by " + Math.Truncate(t.temp - max) + " degrees Fahrenheit.");
                    EmailNotifications.sendToAllAdmins("MOTOR TEMPERATURE", s + " motor temperature OVERHEATING by " + Math.Truncate(t.temp - max) + " degrees Fahrenheit.");
                }

                // Only overrides if switch is true
                if (!isOverridden) return false;
                else return true;
            }
            else if (t.temp <= SimulationConstants.MAX_MOTOR_TEMP && t.temp >= SimulationConstants.MIN_MOTOR_TEMP && !lastIsSafe) {
                logger.Info(Utilities.GetTimeStamp() + ": " + s + " motor temperature stable.");

                pushNotification.sendToAllAdmins("MOTOR TEMPERATURE", s + " motor temperature stable.");
                EmailNotifications.sendToAllAdmins("MOTOR TEMPERATURE", s + " motor temperature stable.");
            }

            return true;
        }

        /// <summary>
        /// This will set the overrides based on input. Takes in the sensor that it will be changing,
        /// and then the status, true or false.
        /// true = overriding
        /// false = enabled
        /// </summary>
        /// <param name="sensor"></param>
        /// <param name="set"></param>
        public void setOverride(String sensor, bool set)
        {
            if      (sensor.Equals("azimuth motor temperature"))    overrides.setAzimuthMotTemp(set);
            else if (sensor.Equals("elevation motor temperature"))  overrides.setElevationMotTemp(set);
            else if (sensor.Equals("main gate"))                    overrides.setGatesOverride(set);
            else if (sensor.Equals("elevation proximity (1)"))      overrides.setElProx0Override(set);
            else if (sensor.Equals("elevation proximity (2)"))      overrides.setElProx90Override(set);
            else if (sensor.Equals("azimuth absolute encoder")) overrides.setAzimuthAbsEncoder(set);
            else if (sensor.Equals("elevation absolute encoder")) overrides.setElevationAbsEncoder(set);
            else if (sensor.Equals("azimuth motor accelerometer")) overrides.setAzimuthAccelerometer(set);
            else if (sensor.Equals("elevation motor accelerometer")) overrides.setElevationAccelerometer(set);
            else if (sensor.Equals("counterbalance accelerometer")) overrides.setCounterbalanceAccelerometer(set);


            if (set)
            {
                logger.Info(Utilities.GetTimeStamp() + ": Overriding " + sensor + " sensor.");

                pushNotification.sendToAllAdmins("SENSOR OVERRIDES", "Overriding " + sensor + " sensor.");
                EmailNotifications.sendToAllAdmins("SENSOR OVERRIDES", "Overriding " + sensor + " sensor.");
            }
            else
            {
                logger.Info(Utilities.GetTimeStamp() + ": Enabled " + sensor + " sensor.");

                pushNotification.sendToAllAdmins("SENSOR OVERRIDES", "Enabled " + sensor + " sensor.");
                EmailNotifications.sendToAllAdmins("SENSOR OVERRIDES", "Enabled " + sensor + " sensor.");
            }
        }
    }
}