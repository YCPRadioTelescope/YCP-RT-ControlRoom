﻿using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace ControlRoomApplication.Entities
{
    /// <summary>
    /// Data class for serializing and deserializing JSON strings into
    /// RadioTelescopeConfig objects, used to tell the ControlRoom software
    /// which telescope to run from the database given an ID.
    /// 
    /// If no ID is given, the newTelescope flag is set which tells the 
    /// ControlRoom to create a new one.
    /// </summary>
    public class RadioTelescopeConfig
    {
        private static string filename = "RadioTelescopeConfig.json";
        public int telescopeID;
        public bool newTelescope;

        /// <contains>An integer representing the ID of a RadioTelescope instance to be retrived from the database</contains>
        /// <contains>A boolean value to represent whether or not a new telescope should be created </contains>
        public RadioTelescopeConfig(int telescopeID, bool newTelescope)
        {
            this.telescopeID = telescopeID;
            this.newTelescope = newTelescope;
        }


        /// <summary>
        /// Method to deserialize the output of the JSON file into a RadioTelescopConfig object. 
        /// File.ReadAllText is guranteed to close the file.
        /// </summary>
        /// <returns>null, if any errors occur in parsing or creating a new RT instance. Otherwise, it will return an instance of an RT</returns>
        public static RadioTelescopeConfig DeserializeRTConfig()
        {
           
            string fileContents = File.ReadAllText(filename);
            RadioTelescopeConfig RTConfig;

            // check to make sure JSON file contains valid JSON
            try
            {
                JObject.Parse(fileContents);
                RTConfig = JsonConvert.DeserializeObject<RadioTelescopeConfig>(fileContents);
            }
            catch(JsonReaderException e)
            {
                Console.WriteLine(e);
                return null;
            }
            // also check to ensure the value was not null after parsing (if the user entered no value)
            if(RTConfig.telescopeID.Equals(null) || RTConfig.newTelescope.Equals(null))
            {
                return null;
            }
            Console.WriteLine("The Selected ID is " + RTConfig.telescopeID + "\n");
            Console.WriteLine("The newTelescope flag was set to "+ RTConfig.newTelescope + "\n");

            return RTConfig;
        }

        /// <summary>
        /// Method to serialize an instance of RadioTelescopeConfig into JSON format and write it to the config file
        /// </summary>
        /// <param name="RTConfig"></param>
        public static void SerializeRTConfig(RadioTelescopeConfig RTConfig)
        {
            string toJson = JsonConvert.SerializeObject(RTConfig);
            File.WriteAllText(filename, toJson);
         
        }


        /// <summary>
        /// Helper method to determine if a new RadioTelescope is being created, instead of
        /// pulling an existing one from the database
        /// </summary>
        /// <param name="RTConfig">An instance of the RadioTelescopeConfig object that corresponds to the JSON File.</param> 
        ///        
        /// <returns></returns>
        public static bool IsTelescopeNew(RadioTelescopeConfig RTConfig)
        {
            return RTConfig.newTelescope == true;
        }
    }
}