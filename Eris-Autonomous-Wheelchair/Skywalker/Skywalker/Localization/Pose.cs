using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Sensors.IPS;
using Skywalker.Mapping;
using Skywalker.Sensors.SensorArray;
using Skywalker.Utils;
using System.Windows;

namespace Skywalker.Localization
{
    public class Pose
    {
        // The angle the raw compass reading must be rotated by to match the map orientation (in degrees)
        private readonly double CompassToMapCorrectionAngle = 0;

        private PositioningSystem Positioning;
        private Compass Compass;

        public MapRoom CurrentRoom
        {
            get;
            set;
        }

        public mPoint CurrentPositionInXY
        {
            get
            {
               // return new mPoint(this.Positioning.X, this.Positioning.Y, this.CurrentRoom);
                return new mPoint(this.Positioning.Y, this.Positioning.X, this.CurrentRoom);
           
            }
        }

        public mPoint CurrentPositionInUV
        {
            get
            {
                return this.CurrentPositionInXY.GetUVFromXY(this.CurrentRoom);
            }
        }

        // Returns the current corrected direction in degrees
        // It is corrected, meaning that the actual compass reading will be rotated by CompassToMapCorrectionAngle
        // to match the direction expected when the map was created
        public double CurrentDirection
        {
            get
            {
                double rawAngle = this.Compass.X + CompassToMapCorrectionAngle;

                // Ensure that rawAngle is in [0, 360)
                // Add 360 because CompassToMapCorrectionAngle could be negative
                return (rawAngle + 360) % 360;
            }
        }

        // Returns CurrentDirection converted to a vector
        public Vector CurrentDirectionVector
        {
            get
            {
                double angle = AngleUtils.DegreesToRadians(CurrentDirection);
                return new Vector(Math.Cos(angle), Math.Sin(angle));
            }
        }

        public Pose(PositioningSystem positioning, MapRoom startingRoom, Compass compass, double compassToMapCorrectionAngle)
        {
            this.CurrentRoom = startingRoom;
            this.Positioning = positioning;
            this.Compass = compass;
            this.CompassToMapCorrectionAngle = compassToMapCorrectionAngle;
        }
    }
}
