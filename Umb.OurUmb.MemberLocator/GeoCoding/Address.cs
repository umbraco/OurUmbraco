namespace Umb.OurUmb.MemberLocator.GeoCoding
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Address
    {
        public static readonly Address Empty;
        private readonly string _street;
        private readonly string _city;
        private readonly string _state;
        private readonly string _postalCode;
        private readonly string _country;
        private readonly Location _coordinates;
        private readonly AddressAccuracy _accuracy;
        public string Street
        {
            get
            {
                return (this._street ?? "");
            }
        }
        public string City
        {
            get
            {
                return (this._city ?? "");
            }
        }
        public string State
        {
            get
            {
                return (this._state ?? "");
            }
        }
        public string PostalCode
        {
            get
            {
                return (this._postalCode ?? "");
            }
        }
        public string Country
        {
            get
            {
                return (this._country ?? "");
            }
        }
        public Location Coordinates
        {
            get
            {
                return this._coordinates;
            }
        }
        public AddressAccuracy Accuracy
        {
            get
            {
                return this._accuracy;
            }
        }
        public Address(string street, string city, string state, string postalCode, string country) : this(street, city, state, postalCode, country, Location.Empty, AddressAccuracy.Unknown)
        {
        }

        public Address(string street, string city, string state, string postalCode, string country, Location coordinates, AddressAccuracy accuracy)
        {
            this._street = street;
            this._city = city;
            this._state = state;
            this._postalCode = postalCode;
            this._country = country;
            this._coordinates = coordinates;
            this._accuracy = accuracy;
        }

        public Distance DistanceBetween(Address address)
        {
            return this.Coordinates.DistanceBetween(address.Coordinates);
        }

        public Distance DistanceBetween(Address address, DistanceUnits units)
        {
            return this.Coordinates.DistanceBetween(address.Coordinates, units);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}, {2} {3}, {4}", new object[] { this._street, this._city, this._state, this._postalCode, this._country });
        }

        static Address()
        {
            Empty = new Address();
        }
    }
}

