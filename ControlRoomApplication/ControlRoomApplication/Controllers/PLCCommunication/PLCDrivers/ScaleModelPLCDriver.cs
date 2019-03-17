﻿using System;
using System.Net;
using System.Net.Sockets;
using ControlRoomApplication.Entities;

namespace ControlRoomApplication.Controllers.PLCCommunication
{
    public class ScaleModelPLCDriver : AbstractPLCDriver
    {
        public ScaleModelPLCDriver(IPAddress ip_address, int port) : base(ip_address, port) { }

        public ScaleModelPLCDriver(string ip, int port) : this(IPAddress.Parse(ip), port) { }

        protected override bool ProcessRequest(NetworkStream ActiveClientStream, byte[] query)
        {
            int ExpectedSize = query[0] + (16 * query[1]);
            if (query.Length != ExpectedSize)
            {
                throw new ArgumentException(
                    "ScaleModelPLCDriverController read a package specifying a size [" + ExpectedSize.ToString() + "], but the actual size was different [" + query.Length + "]."
                );
            }

            byte CommandQueryTypeAndExpectedResponseStatus = query[2];
            byte CommandQueryTypeByte = (byte)(CommandQueryTypeAndExpectedResponseStatus & 0x3F);
            byte ExpectedResponseStatusByte = (byte)(CommandQueryTypeAndExpectedResponseStatus >> 6);

            PLCCommandAndQueryTypeEnum CommandQueryTypeEnum = PLCCommandAndQueryTypeConversionHelper.GetFromByte(CommandQueryTypeByte);
            PLCCommandResponseExpectationEnum ExpectedResponseStatusEnum = PLCCommandResponseExpectationConversionHelper.GetFromByte(ExpectedResponseStatusByte);

            byte[] FinalResponseContainer;

            if (ExpectedResponseStatusEnum == PLCCommandResponseExpectationEnum.FULL_RESPONSE)
            {
                FinalResponseContainer = new byte[]
                {
                    0x13, 0x0,
                    0x0,
                    0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0
                };

                switch (CommandQueryTypeEnum)
                {
                    case PLCCommandAndQueryTypeEnum.TEST_CONNECTION:
                        {
                            FinalResponseContainer[3] = 0x1;
                            FinalResponseContainer[2] = 0x1;
                            break;
                        }

                    case PLCCommandAndQueryTypeEnum.GET_CURRENT_AZEL_POSITIONS:
                        {
                            double TestAzimuth = 180.0;
                            double TestElevation = 42.0;
                            
                            Array.Copy(BitConverter.GetBytes(TestAzimuth), 0, FinalResponseContainer, 3, 8);
                            Array.Copy(BitConverter.GetBytes(TestElevation), 0, FinalResponseContainer, 11, 8);

                            FinalResponseContainer[2] = 0x1;

                            break;
                        }

                    case PLCCommandAndQueryTypeEnum.GET_CURRENT_LIMIT_SWITCH_STATUSES:
                        {
                            PLCLimitSwitchStatusEnum StatusAzimuthUnderRotation = PLCLimitSwitchStatusEnum.WITHIN_SAFE_LIMITS;
                            PLCLimitSwitchStatusEnum StatusAzimuthOverRotation = PLCLimitSwitchStatusEnum.WITHIN_SAFE_LIMITS;
                            PLCLimitSwitchStatusEnum StatusElevationUnderRotation = PLCLimitSwitchStatusEnum.WITHIN_SAFE_LIMITS;
                            PLCLimitSwitchStatusEnum StatusElevationOverRotation = PLCLimitSwitchStatusEnum.WITHIN_SAFE_LIMITS;

                            int PacketSum =
                                PLCLimitSwitchStatusConversionHelper.ConvertToByte(StatusElevationOverRotation)
                                 + (PLCLimitSwitchStatusConversionHelper.ConvertToByte(StatusElevationUnderRotation) * 0x4)
                                 + (PLCLimitSwitchStatusConversionHelper.ConvertToByte(StatusAzimuthOverRotation) * 0x10)
                                 + (PLCLimitSwitchStatusConversionHelper.ConvertToByte(StatusAzimuthUnderRotation) * 0x40)
                            ;

                            FinalResponseContainer[3] = (byte)PacketSum;
                            FinalResponseContainer[2] = 0x1;

                            break;
                        }

                    case PLCCommandAndQueryTypeEnum.GET_CURRENT_SAFETY_INTERLOCK_STATUS:
                        {
                            FinalResponseContainer[3] = PLCSafetyInterlockStatusConversionHelper.ConvertToByte(PLCSafetyInterlockStatusEnum.LOCKED);
                            FinalResponseContainer[2] = 0x1;
                            break;
                        }

                    default:
                        {
                            throw new ArgumentException("Invalid PLCCommandAndQueryTypeEnum value seen while expecting a response: " + CommandQueryTypeEnum.ToString());
                        }
                }
            }
            else if (ExpectedResponseStatusEnum == PLCCommandResponseExpectationEnum.MINOR_RESPONSE)
            {
                FinalResponseContainer = new byte[]
                {
                    0x3, 0x0, 0x0
                };

                switch (CommandQueryTypeEnum)
                {
                    case PLCCommandAndQueryTypeEnum.CANCEL_ACTIVE_OBJECTIVE_AZEL_POSITION:
                    case PLCCommandAndQueryTypeEnum.SHUTDOWN:
                    case PLCCommandAndQueryTypeEnum.CALIBRATE:
                    case PLCCommandAndQueryTypeEnum.SET_OBJECTIVE_AZEL_POSITION:
                        {
                            FinalResponseContainer[2] = 0x1;
                            break;
                        }

                    default:
                        {
                            throw new ArgumentException("Invalid PLCCommandAndQueryTypeEnum value seen while NOT expecting a response: " + CommandQueryTypeEnum.ToString());
                        }
                }
            }
            else
            {
                throw new ArgumentException("Invalid PLCCommandResponseExpectationEnum value seen while processing client request in ScaleModelPLCDriver: " + ExpectedResponseStatusEnum.ToString());
            }

            return AttemptToWriteDataToServer(ActiveClientStream, FinalResponseContainer);
        }
    }
}