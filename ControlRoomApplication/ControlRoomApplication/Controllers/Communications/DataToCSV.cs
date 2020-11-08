﻿using ControlRoomApplication.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ControlRoomApplication.Controllers.Communications
{
    public class DataToCSV
    {
        public static bool ExportToCSV(List<RFData> Data, string fname)
        {
            StringBuilder sb = new StringBuilder();
            string path = AppDomain.CurrentDomain.BaseDirectory;

            string final_loc = Path.Combine(path, $"{fname}.csv");

            PropertyInfo[] info = typeof(RFData).GetProperties();

            string header = "";
            bool success = false;

            try
            {
                if (!File.Exists(final_loc))
                {
                    FileStream file = File.Create(final_loc);
                    file.Close();

                    foreach(PropertyInfo property in info)
                    {
                        // We only want the TimeCaptured and Intensity fields to be listed in the CSV
                        if(property.Name != "Appointment" && property.Name != "Id" && property.Name != "appointment_id")
                        {
                            header += property.Name + ", ";
                        }
                    }
                    header = header.Substring(0, header.Length - 2);
                    sb.AppendLine(header);
                    TextWriter sw = new StreamWriter(final_loc, true);
                    sw.Write(sb.ToString());
                    sw.Close();
                }

                foreach (RFData rf in Data)
                {
                    sb = new StringBuilder();
                    var line = "";
                    foreach (PropertyInfo property in info)
                    {
                        if(property.Name != "Appointment" && property.Name != "Id" && property.Name != "appointment_id")
                        {
                            line += property.GetValue(rf, null) + ", ";
                        }
                    }
                    line = line.Substring(0, line.Length - 2);
                    sb.AppendLine(line);
                    TextWriter sw = new StreamWriter(final_loc, true);
                    sw.Write(sb.ToString());
                    sw.Close();
                }
                success = true;
            }
            catch (Exception e)
            {
                Console.Out.WriteLine($"Could not write data to CSV! Error: {e}");
                success = false;
            }
            return success;
        }
    }
}
