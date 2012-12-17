namespace Umb.OurUmb.MemberLocator.GeoCoding
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Location
    {
        public static readonly Location Empty;
        private readonly double _latitude;
        private readonly double _longitude;
        public double Latitude
        {
            get
            {
                return this._latitude;
            }
        }
        public double Longitude
        {
            get
            {
                return this._longitude;
            }
        }
        public Location(double latitude, double longitude)
        {
            this._latitude = latitude;
            this._longitude = longitude;
        }

        private double ToRadian(double val)
        {
            return ((Math.PI / 180) * val);
        }

        public Distance DistanceBetween(Location location)
        {
            return this.DistanceBetween(location, DistanceUnits.Miles);
        }

        public Distance DistanceBetween(Location location, DistanceUnits units)
        {
            double num = (units == DistanceUnits.Miles) ? 3956.545 : 6378.2;//3438.147;
            double num2 = this.ToRadian(location.Latitude - this.Latitude);
            double num3 = this.ToRadian(location.Longitude - this.Longitude);
            double d = Math.Pow(Math.Sin(num2 / 2.0), 2.0) + ((Math.Cos(this.ToRadian(this.Latitude)) * Math.Cos(this.ToRadian(location.Latitude))) * Math.Pow(Math.Sin(num3 / 2.0), 2.0));
            double num5 = 2.0 * Math.Asin(Math.Min(1.0, Math.Sqrt(d)));
            return new Distance(num * num5, units);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this._latitude.ToString(Utility.GetNumberFormatInfo()), this._longitude.ToString(Utility.GetNumberFormatInfo()));
        }

        static Location()
        {
            Empty = new Location();
        }
    }
}

