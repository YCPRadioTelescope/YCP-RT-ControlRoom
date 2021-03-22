﻿using ControlRoomApplication.Controllers.SensorNetwork;
using ControlRoomApplication.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlRoomApplicationTest.EntitiesTests
{
    [TestClass]
    public class SensorNetworkConfigTest
    {
        [TestMethod]
        public void TestInitialization()
        {
            int telescopeId = 5;

            // Create new SensorNetworkConfig with a telescope ID of 5
            SensorNetworkConfig config = new SensorNetworkConfig(telescopeId);

            // telescope ID 
            Assert.AreEqual(config.TelescopeId, telescopeId);

            // default constants
            Assert.AreEqual(config.TimeoutDataRetrieval, SensorNetworkConstants.DefaultDataRetrievalTimeout);
            Assert.AreEqual(config.TimeoutInitialization, SensorNetworkConstants.DefaultInitializationTimeout);

            // default initialization (all must default to true)
            Assert.AreEqual(config.ElevationTemp1Init, true);
            Assert.AreEqual(config.ElevationTemp2Init, true);
            Assert.AreEqual(config.AzimuthTemp1Init, true);
            Assert.AreEqual(config.AzimuthTemp2Init, true);
            Assert.AreEqual(config.ElevationAccelerometerInit, true);
            Assert.AreEqual(config.AzimuthAccelerometerInit, true);
            Assert.AreEqual(config.CounterbalanceAccelerometerInit, true);
            Assert.AreEqual(config.ElevationEncoderInit, true);
            Assert.AreEqual(config.AzimuthEncoderInit, true);
        }

        [TestMethod]
        public void TestEquals_Identical_Equal()
        {
            int telescopeId = 5;

            SensorNetworkConfig config = new SensorNetworkConfig(telescopeId);

            SensorNetworkConfig other = new SensorNetworkConfig(telescopeId);

            Assert.IsTrue(config.Equals(other));
        }

        [TestMethod]
        public void TestEquals_TelescopeIdDifferent_NotEqual()
        {
            SensorNetworkConfig config = new SensorNetworkConfig(5);
            SensorNetworkConfig other = new SensorNetworkConfig(6);

            Assert.IsFalse(config.Equals(other));
        }

        [TestMethod]
        public void TestEquals_ElevationTemp1InitDifferent_NotEqual()
        {
            int telescopeId = 5;

            SensorNetworkConfig config = new SensorNetworkConfig(telescopeId);
            SensorNetworkConfig other = new SensorNetworkConfig(telescopeId);

            other.ElevationTemp1Init = false;

            Assert.IsFalse(config.Equals(other));
        }

        [TestMethod]
        public void TestEquals_ElevationTemp2InitDifferent_NotEqual()
        {
            int telescopeId = 5;

            SensorNetworkConfig config = new SensorNetworkConfig(telescopeId);
            SensorNetworkConfig other = new SensorNetworkConfig(telescopeId);

            other.ElevationTemp2Init = false;

            Assert.IsFalse(config.Equals(other));
        }

        [TestMethod]
        public void TestEquals_AzimuthTemp1InitDifferent_NotEqual()
        {
            int telescopeId = 5;

            SensorNetworkConfig config = new SensorNetworkConfig(telescopeId);
            SensorNetworkConfig other = new SensorNetworkConfig(telescopeId);

            other.AzimuthTemp1Init = false;

            Assert.IsFalse(config.Equals(other));
        }

        [TestMethod]
        public void TestEquals_AzimuthTemp2InitDifferent_NotEqual()
        {
            int telescopeId = 5;

            SensorNetworkConfig config = new SensorNetworkConfig(telescopeId);
            SensorNetworkConfig other = new SensorNetworkConfig(telescopeId);

            other.AzimuthTemp2Init = false;

            Assert.IsFalse(config.Equals(other));
        }

        [TestMethod]
        public void TestEquals_AzimuthAccelerometerInitDifferent_NotEqual()
        {
            int telescopeId = 5;

            SensorNetworkConfig config = new SensorNetworkConfig(telescopeId);
            SensorNetworkConfig other = new SensorNetworkConfig(telescopeId);

            other.AzimuthAccelerometerInit = false;

            Assert.IsFalse(config.Equals(other));
        }

        [TestMethod]
        public void TestEquals_ElevationAccelerometerInitDifferent_NotEqual()
        {
            int telescopeId = 5;

            SensorNetworkConfig config = new SensorNetworkConfig(telescopeId);
            SensorNetworkConfig other = new SensorNetworkConfig(telescopeId);

            other.ElevationAccelerometerInit = false;

            Assert.IsFalse(config.Equals(other));
        }

        [TestMethod]
        public void TestEquals_CounterbalanceAccelerometerInitDifferent_NotEqual()
        {
            int telescopeId = 5;

            SensorNetworkConfig config = new SensorNetworkConfig(telescopeId);
            SensorNetworkConfig other = new SensorNetworkConfig(telescopeId);

            other.CounterbalanceAccelerometerInit = false;

            Assert.IsFalse(config.Equals(other));
        }

        [TestMethod]
        public void TestEquals_AzimuthEncoderInitDifferent_NotEqual()
        {
            int telescopeId = 5;

            SensorNetworkConfig config = new SensorNetworkConfig(telescopeId);
            SensorNetworkConfig other = new SensorNetworkConfig(telescopeId);

            other.AzimuthEncoderInit = false;

            Assert.IsFalse(config.Equals(other));
        }

        [TestMethod]
        public void TestEquals_ElevationEncoderInitDifferent_NotEqual()
        {
            int telescopeId = 5;

            SensorNetworkConfig config = new SensorNetworkConfig(telescopeId);
            SensorNetworkConfig other = new SensorNetworkConfig(telescopeId);

            other.ElevationEncoderInit = false;

            Assert.IsFalse(config.Equals(other));
        }

        [TestMethod]
        public void TestEquals_TimeoutDataRetrievalDifferent_NotEqual()
        {
            int telescopeId = 5;

            SensorNetworkConfig config = new SensorNetworkConfig(telescopeId);
            SensorNetworkConfig other = new SensorNetworkConfig(telescopeId);

            other.TimeoutDataRetrieval = 5;

            Assert.IsFalse(config.Equals(other));
        }

        [TestMethod]
        public void TestEquals_TimeoutInitializationDifferent_NotEqual()
        {
            int telescopeId = 5;

            SensorNetworkConfig config = new SensorNetworkConfig(telescopeId);
            SensorNetworkConfig other = new SensorNetworkConfig(telescopeId);

            other.TimeoutInitialization = 5;

            Assert.IsFalse(config.Equals(other));
        }

        [TestMethod]
        public void TestConvertSensorInitToBytes_AllTrue_AllBytesOne()
        {
            SensorNetworkConfig config = new SensorNetworkConfig(5);

            var bytes = config.ConvertSensorInitToBytes();

            // All bytes in the array should be 1
            for (int i = 0; i < bytes.Length; i++) Assert.IsTrue(bytes[i] == 1);
        }

        [TestMethod]
        public void TestConvertSensorInitToBytes_AllTrue_AllBytesZero()
        {
            SensorNetworkConfig config = new SensorNetworkConfig(5);

            config.ElevationTemp1Init = false;
            config.ElevationTemp2Init = false;
            config.AzimuthTemp1Init = false;
            config.AzimuthTemp2Init = false;
            config.ElevationAccelerometerInit = false;
            config.AzimuthAccelerometerInit = false;
            config.CounterbalanceAccelerometerInit = false;
            config.ElevationEncoderInit = false;
            config.AzimuthEncoderInit = false;

            var bytes = config.ConvertSensorInitToBytes();

            // All bytes in the array should be 1
            for (int i = 0; i < bytes.Length; i++) Assert.IsTrue(bytes[i] == 0);
        }
    }
}
