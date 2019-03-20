﻿using System;
using System.Collections.Generic;
using ControlRoomApplication.Controllers.PLCCommunication;
using ControlRoomApplication.Controllers.SpectraCyberController;
using ControlRoomApplication.Database.Operations;
using ControlRoomApplication.Entities;
using ControlRoomApplication.Entities.RadioTelescope;

namespace ControlRoomApplication.Main
{
    public static class ConfigurationManager
    {
        private static int NumLocalDBRTInstancesCreated = 1;

        /// <summary>
        /// Configures the type of SpectraCyberController to be used in the rest of the 
        /// application based on the second program argument passed in.
        /// </summary>
        /// <param name="arg1"> The second program argument passed in. </param>
        /// <returns> A concrete instance of a SpectraCyberController. </returns>
        public static AbstractSpectraCyberController ConfigureSpectraCyberController(string arg1)
        {
            switch (arg1.ToUpper())
            {
                case "/PS":
                    return new SpectraCyberController(new SpectraCyber());

                case "/TS":
                    return new SpectraCyberTestController(new SpectraCyberSimulator());

                default:
                    // If none of the switches match or there wasn't one declared
                    // for the spectraCyber, assume we are using the simulated/testing one.
                    return new SpectraCyberSimulatorController(new SpectraCyberSimulator());
            }
        }

        /// <summary>
        /// Configures the type of radiotelescope that will be used in the rest
        /// of the application based on the third command line argument passed in.
        /// </summary>
        /// <returns> A concrete instance of a radio telescope. </returns>
        public static RadioTelescope ConfigureRadioTelescope(AbstractSpectraCyberController spectraCyberController, string ip, string port, bool usingLocalDB)
        {
            PLCClientCommunicationHandler PLCCommsHandler = new PLCClientCommunicationHandler(ip, int.Parse(port));

            // Create Radio Telescope Location
            Location location = new Location(76.7046, 40.0244, 395.0); // John Rudy Park hardcoded for now

            // Return Radio Telescope
            if (usingLocalDB)
            {
                return new RadioTelescope(spectraCyberController, PLCCommsHandler, location, NumLocalDBRTInstancesCreated++);
            }
            else
            {
                return new RadioTelescope(spectraCyberController, PLCCommsHandler, location);
            }
        }

        /// <summary>
        /// Configures the type of PLC driver to simulate, based on the type of radio telescope being used.
        /// </summary>
        /// <returns> An instance of the proper PLC driver. </returns>
        public static AbstractPLCDriver ConfigureSimulatedPLCDriver(string arg2, string ip, string port)
        {
            switch (arg2.ToUpper())
            {
                case "/PR":
                    // The production telescope
                    throw new NotImplementedException("There is not yet communication for the real PLC.");

                case "/SR":
                    // Case for the test/simulated radiotelescope.
                    return new ScaleModelPLCDriver(ip, int.Parse(port));

                case "/TR":
                default:
                    // Should be changed once we have a simulated radiotelescope class implemented
                    return new TestPLCDriver(ip, int.Parse(port));
            }
        }

        /// <summary>
        /// Constructs a series of radio telescopes, given input parameters.
        /// </summary>
        /// <returns> A list of built instances of a radio telescope. </returns>
        public static List<KeyValuePair<RadioTelescope, AbstractPLCDriver>> BuildRadioTelescopeSeries(string[] args, bool usingLocalDB)
        {
            int NumRTs;
            try
            {
                NumRTs = int.Parse(args[0]);
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid first input argument, an int was expected.");
                return null;
            }

            if (NumRTs != args.Length - 1)
            {
                Console.WriteLine("The requested number of RTs doesn't match the number of input arguments.");
                return null;
            }

            List<KeyValuePair<RadioTelescope, AbstractPLCDriver>> RTDriverPairList = new List<KeyValuePair<RadioTelescope, AbstractPLCDriver>>();
            for (int i = 0; i < NumRTs; i++)
            {
                string[] RTArgs = args[i + 1].Split(',');

                if (RTArgs.Length != 4)
                {
                    Console.WriteLine("Unexpected format for input #" + i.ToString() + "[" + args[i + 1] + "], skipping...");
                    continue;
                }

                RadioTelescope ARadioTelescope = ConfigureRadioTelescope(ConfigureSpectraCyberController(RTArgs[1]), RTArgs[2], RTArgs[3], usingLocalDB);
                AbstractPLCDriver APLCDriver = ConfigureSimulatedPLCDriver(RTArgs[0], RTArgs[2], RTArgs[3]);

                RTDriverPairList.Add(new KeyValuePair<RadioTelescope, AbstractPLCDriver>(ARadioTelescope, APLCDriver));
            }

            return RTDriverPairList;
        }

        public static void ConfigureLocalDatabase(int NumRTInstances)
        {
            DatabaseOperations.DeleteLocalDatabase();
            DatabaseOperations.PopulateLocalDatabase(NumRTInstances);
        }
    }
}
