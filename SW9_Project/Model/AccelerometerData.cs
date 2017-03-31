using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW9_Project {
    class AccelerometerData : MobileData {

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public long Time { get; set; }
        public long LastUpdate { get; set; }

        private AccelerometerData() {
            Type = "AccelerometerData";
        }

        public AccelerometerData(float x, float y, float z, long time) : this() {
            X = x; Y = y; Z = z;
            Time = time;
        }

        public AccelerometerData(dynamic jsonObject) : this() {
            X = Convert.ToSingle(jsonObject.X);
            Y = Convert.ToSingle(jsonObject.Y);
            Z = Convert.ToSingle(jsonObject.Z);
            Time = Convert.ToInt64(jsonObject.Time);

        }

        /// <summary>
        /// This method should be paired with some Kinect depth data
        /// to see if a throw is towards the screen or away from the screen.
        /// </summary>
        /// <returns>True if throw recognized</returns>
        public bool IsThrown()
        {
            float accelationSquareRoot = (X * X + Y * Y + Z * Z) / (9.80665f * 9.80665f);
            if (accelationSquareRoot >= 2)
            {
                if (Time - LastUpdate < 4E9) 
                    return false; // Won't recognize a throw within a 4 second interval
                LastUpdate = Time;
                // Throw recognized
                return true;
            }
            return false;
        }

    }
}
