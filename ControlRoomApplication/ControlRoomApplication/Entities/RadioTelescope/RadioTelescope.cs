﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ControlRoomApplication.Controllers;
using System;
using ControlRoomApplication.Controllers.SensorNetwork;

namespace ControlRoomApplication.Entities
{
    [Table("radio_telescope")]
    public class RadioTelescope
    {

        public RadioTelescope()
        {

        }

        public RadioTelescope(AbstractSpectraCyberController spectraCyberController, AbstractPLCDriver plcCommsHandler, Location location, Orientation calibrationOrientation)
        {
            PLCDriver = plcCommsHandler;
            SpectraCyberController = spectraCyberController;
            CalibrationOrientation = calibrationOrientation;
            Location = location;
            CurrentOrientation = new Orientation();
        }

        public RadioTelescope(AbstractSpectraCyberController spectraCyberController, AbstractPLCDriver plcCommsHandler, Location location, Orientation calibrationOrientation, int localDBID) {
            PLCDriver = plcCommsHandler;
            SpectraCyberController = spectraCyberController;
            CalibrationOrientation = calibrationOrientation;
            Location = location;
            CurrentOrientation = new Orientation();
            Id = localDBID;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("online")]
        public int online { get; set; }
        
        public int current_orientation_id { get; set; }
        [ForeignKey("current_orientation_id")]
        public Orientation CurrentOrientation { get; set; }
        
        public int calibration_orientation_id { get; set; }
        [ForeignKey("calibration_orientation_id")]
        public Orientation CalibrationOrientation { get; set; }

        public int location_id { get; set; }
        [ForeignKey("location_id")]
        public virtual Location Location { get; set; }

        [NotMapped]
        public RadioTelescopeTypeEnum _TeleType
        {
            get
            {
                return (RadioTelescopeTypeEnum)Enum.Parse(typeof(RadioTelescopeTypeEnum), teleType);
            }
            set
            {
                this.teleType = value.ToString();
            }
        }

        private string backingTeleType { get; set; }

        [Required]
        [Column("telescope_type")]
        public string teleType
        {
            get
            {
                return this.backingTeleType;
            }
            set
            {
                if (value == null || Enum.IsDefined(typeof(RadioTelescopeTypeEnum), value))
                {
                    this.backingTeleType = value;
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
        }

        [NotMapped]
        public AbstractPLCDriver PLCDriver { get; set; }

        [NotMapped]
        public SensorNetworkServer SensorNetworkServer { get; set; }

        [NotMapped]
        public AbstractSpectraCyberController SpectraCyberController { get; set; }

        [NotMapped]
        protected RadioTelescopeController Parent;

        public RadioTelescopeController GetParent()
        {
            return Parent;
        }

        public void SetParent(RadioTelescopeController rt)
        {
            Parent = rt;
        }

        [NotMapped]
        public AbstractWeatherStation WeatherStation { get; set; }
    }
}