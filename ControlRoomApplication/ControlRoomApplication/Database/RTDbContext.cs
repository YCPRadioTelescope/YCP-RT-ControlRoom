﻿using System.Data.Entity;
using ControlRoomApplication.Constants;
using ControlRoomApplication.Entities;

namespace ControlRoomApplication.Main
{
    public class RTDbContext : DbContext
    {
        public RTDbContext() : base(MiscellaneousConstants.LOCAL_DATABASE_NAME)
        {
            
        }

        public RTDbContext(string connectionString) : base(connectionString)
        {
            Database.Connection.ConnectionString = connectionString;
            Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        { 
            modelBuilder.Entity<Appointment>().HasRequired(t => t.CelestialBody).WithMany().Map(d => d.MapKey("celestial_body_id"));
            modelBuilder.Entity<Appointment>().HasRequired(t => t.SpectraCyberConfig).WithMany().Map(d => d.MapKey("spectracyber_config_id"));
            modelBuilder.Entity<Appointment>().HasRequired(t => t.Orientation).WithMany().Map(d => d.MapKey("orientation_id"));
            modelBuilder.Entity<Appointment>().HasRequired(t => t.Telescope).WithMany().Map(d => d.MapKey("telescope_id"));
            modelBuilder.Entity<Appointment>().HasRequired(t => t.User).WithMany().Map(d => d.MapKey("user_id"));

            modelBuilder.Entity<RFData>().HasRequired(t => t.Appointment).WithMany().Map(d => d.MapKey("appointment_id"));

        }

        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<RFData> RFDatas { get; set; }
        public DbSet<Orientation> Orientations { get; set; }
        public DbSet<Coordinate> Coordinates { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Temperature> Temperatures { get; set; }
        public DbSet<Acceleration> Accelerations { get; set; }
        public DbSet<CelestialBody> CelestialBodies { get; set; }
        public DbSet<SpectraCyberConfig> SpectraCyberConfigs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<WeatherData> Weather { get; set; }
        public DbSet<SensorStatus> SensorStatus { get; set; }
        public DbSet<RadioTelescope> RadioTelescope { get; set; }
        public DbSet<ThresholdValues> ThresholdValues { get; set; }
        public DbSet<Override> Override { get; set; }
    }
}