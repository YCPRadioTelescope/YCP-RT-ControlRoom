﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ControlRoomApplication.Controllers.Communications;
using ControlRoomApplication.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ControlRoomApplicationTest.CommunicationTests
{
    [TestClass]
    public class PushNotificationTest
    {
        [TestInitialize]
        public void TestInit()
        {
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;
            string folderName = "PushNotificationTestResults";
            string pathString = Path.Combine(currentPath, folderName);
            Directory.CreateDirectory(pathString);
        }

        public static string testfileloc = $"test-out-{System.DateTime.Now.ToString("yyyyMMddHHmmss")}";


        [TestMethod]
        public void TestPushNotificationFilePresence()
        {
            Assert.IsTrue(pushNotification.send("TEST", "This should pass."));
        }

        [TestMethod]
        public void TestSendPushNotification()
        {
            // If sendPush is false, set it to true
            if (!pushNotification.getSendPush()) pushNotification.setSendPush(true);

            Assert.IsTrue(pushNotification.send("TEST", "This should pass."));
        }

        [TestMethod]
        public void TestSendingEmail()
        {
            string sender = "SystemTest@ycpradiotelescope.com";
            EmailFields.setSubject("Amazon SES Test");
            EmailFields.setText("AmazonSES Test (.NET)\r\nThis email was sent through AmazonSES using the AWS SDK for .NET.");

            User fakeUser = new User("Test", "User", "testradiotelescopeuser@ycp.edu", NotificationTypeEnum.ALL);

            Assert.IsTrue(EmailNotifications.sendToAllAdmins("test subject", "test body", sender, true));
            //Assert.IsTrue(EmailNotifications.sendToUser(fakeUser));
        }

        [TestMethod]
        public void TestSendingEmailWithAttachment()
        {
            string testpath = $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@".\PushNotificationTestResults\test-out-{System.DateTime.Now.ToString("yyyyMMddHHmmss")}")}";

            EmailFields.setSender("SystemTest@ycpradiotelescope.com");
            EmailFields.setSubject("Amazon SES Test WITH ATTACHMENT");
            EmailFields.setText("AmazonSES Test (.NET) with Attachment\r\nThis email and its attachment were sent through AmazonSES using the AWS SDK for .NET.");

            // If you want to actually get emails while testing, change the email address below to whatever one you want to receive at.
            // This was already done earlier.
            User fakeUser = new User("Test", "User", "testradiotelescope@gmail.com", NotificationTypeEnum.ALL);

            RFData junkdata = new RFData();
            junkdata.Id = 0;
            junkdata.appointment_id = 0;
            junkdata.TimeCaptured = System.DateTime.Now;
            junkdata.Intensity = 8675309;

            List<RFData> JunkRFData = new List<RFData>();
            JunkRFData.Add(junkdata);

            DataToCSV.ExportToCSV(JunkRFData, testpath);

            Assert.IsTrue(EmailNotifications.sendToUser(fakeUser, "test subject", "test body", $"{testpath}.csv"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;
            string folderName = "PushNotificationTestResults";
            string pathString = Path.Combine(currentPath, folderName);

            Directory.Delete(pathString, true);
        }
    }
}
