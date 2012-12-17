namespace Umb.OurUmb.MemberLocator.GeoCoding
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Distance
    {
        public const double EarthRadiusInMiles = 3956.545;
        public const double EarthRadiusInKilometers = 3438.147;
        private readonly double _value;
        private readonly DistanceUnits _units;
        public double Value
        {
            get
            {
                return this._value;
            }
        }
        public DistanceUnits Units
        {
            get
            {
                return this._units;
            }
        }
        public Distance(double value, DistanceUnits units)
        {
            this._value = Math.Round(value, 8);
            this._units = units;
        }

        public static Distance FromMiles(double miles)
        {
            return new Distance(miles, DistanceUnits.Miles);
        }

        public static Distance FromKilometers(double kilometers)
        {
            return new Distance(kilometers, DistanceUnits.Kilometers);
        }

        private Distance ConvertUnits(DistanceUnits units)
        {
            double num;
            if (this._units == units)
            {
                return this;
            }
            switch (units)
            {
                case DistanceUnits.Miles:
                    num = this._value * 0.621371192;
                    break;

                case DistanceUnits.Kilometers:
                    num = this._value / 0.621371192;
                    break;

                default:
                    num = 0.0;
                    break;
            }
            return new Distance(num, units);
        }

        public Distance ToMiles()
        {
            return this.ConvertUnits(DistanceUnits.Miles);
        }

        public Distance ToKilometers()
        {
            return this.ConvertUnits(DistanceUnits.Kilometers);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(Distance obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(Distance obj, bool normalizeUnits)
        {
            if (normalizeUnits)
            {
                obj = obj.ConvertUnits(this.Units);
            }
            return this.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", this._value, this._units);
        }

        public static Distance operator *(Distance d1, double d)
        {
            return new Distance(d1.Value * d, d1.Units);
        }

        public static Distance operator +(Distance left, Distance right)
        {
            return new Distance(left.Value + right.ConvertUnits(left.Units).Value, left.Units);
        }

        public static Distance operator -(Distance left, Distance right)
        {
            return new Distance(left.Value - right.ConvertUnits(left.Units).Value, left.Units);
        }

        public static bool operator ==(Distance left, Distance right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Distance left, Distance right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Distance left, Distance right)
        {
            return (left.Value < right.ConvertUnits(left.Units).Value);
        }

        public static bool operator <=(Distance left, Distance right)
        {
            return (left.Value <= right.ConvertUnits(left.Units).Value);
        }

        public static bool operator >(Distance left, Distance right)
        {
            return (left.Value > right.ConvertUnits(left.Units).Value);
        }

        public static bool operator >=(Distance left, Distance right)
        {
            return (left.Value >= right.ConvertUnits(left.Units).Value);
        }

        public static implicit operator double(Distance distance)
        {
            return distance.Value;
        }
    }
}

