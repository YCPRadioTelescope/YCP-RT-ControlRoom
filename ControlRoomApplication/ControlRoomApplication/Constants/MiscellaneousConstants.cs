﻿using ControlRoomApplication.Entities;

namespace ControlRoomApplication.Constants
{
    public sealed class MiscellaneousConstants
    {
        public const int MAX_USERNAME_LENGTH = 15;
        public const string LOCAL_DATABASE_NAME = "radio_telescope";

        // Can't create const instances, we have to use this as static readonly, sadly
        public static readonly Location JOHN_RUDY_PARK = new Location(76.7046, 40.0244, 395.0, "");

        public const double NEGLIGIBLE_POSITION_CHANGE_DEGREES = 0.1;

        public static readonly Orientation THERMAL_CALIBRATION_ORIENTATION = new Orientation(200, 20);

        public static readonly double THERMAL_CALIBRATION_OFFSET = 0.01;


        // constants used for user input validation
        public const int MAX_PORT_VALUE = 65535;

        public const int MIN_PORT_VALUE = 1; // 0 is reserved!

        public const double MAX_SPEED_RPM = 2.00;

        public const double MIN_SPEED_RPM = 0.00;

        // Constants for orientations that we use in various places

        /// <summary>
        /// The stow position of the telescope. This position has it pointing straight up in the air
        /// </summary>
        public static readonly Orientation Stow = new Orientation(0, 90);

        //private const




    }
}