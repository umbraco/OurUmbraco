namespace Umb.OurUmb.MemberLocator.GeoCoding.Services.Google
{

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Web;
    using System.Xml;
    using System.Xml.XPath;

    public class GoogleGeoCoder : IGeoCoder
    {
        private string _accessKey;
        private XmlNamespaceManager _namespaceManager;
        public const string ServiceUrl = "http://maps.google.com/maps/geo?output=xml&q={0}&key={1}&oe=utf8";

        public GoogleGeoCoder(string accessKey)
        {
            if (string.IsNullOrEmpty(accessKey))
            {
                throw new ArgumentNullException("accessKey");
            }
            this._accessKey = accessKey;
        }

        private HttpWebRequest BuildWebRequest(string address)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(string.Format("http://maps.google.com/maps/geo?output=xml&q={0}&key={1}&oe=utf8", HttpUtility.UrlEncode(address), this._accessKey));
            request.Method = "GET";
            return request;
        }

        private XPathNavigator CreateSubNavigator(XPathNavigator nav)
        {
            using (StringReader reader = new StringReader(nav.OuterXml))
            {
                XPathDocument document = new XPathDocument(reader);
                return document.CreateNavigator();
            }
        }

        private XmlNamespaceManager CreateXmlNamespaceManager(XPathNavigator nav)
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(nav.NameTable);
            manager.AddNamespace("kml", "http://earth.google.com/kml/2.0");
            manager.AddNamespace("adr", "urn:oasis:names:tc:ciq:xsdschema:xAL:2.0");
            return manager;
        }

        private string EvaluateXPath(string xpath, XPathNavigator nav)
        {
            XPathExpression expr = nav.Compile(xpath);
            expr.SetContext(this._namespaceManager);
            return (string) nav.Evaluate(expr);
        }

        private Location FromCoordinates(string[] coordinates)
        {
            double longitude = double.Parse(coordinates[0],Utility.GetNumberFormatInfo());
            return new Location(double.Parse(coordinates[1],Utility.GetNumberFormatInfo()), longitude);
        }

        public Address[] GeoCode(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentNullException("address");
            }
            using (WebResponse response = this.BuildWebRequest(address).GetResponse())
            {
                return this.ProcessWebResponse(response);
            }
        }

        public Address[] GeoCode(string street, string city, string state, string postalCode, string country)
        {
            string address = string.Format("{0} {1}, {2} {3}, {4}", new object[] { street, city, state, postalCode, country });
            return this.GeoCode(address);
        }

        private XPathDocument LoadXmlResponse(WebResponse response)
        {
            using (Stream stream = response.GetResponseStream())
            {
                return new XPathDocument(stream);
            }
        }

        private AddressAccuracy MapAccuracy(GoogleAddressAccuracy accuracy)
        {
            switch (accuracy)
            {
                case GoogleAddressAccuracy.UnknownLocation:
                    return AddressAccuracy.Unknown;

                case GoogleAddressAccuracy.CountryLevel:
                    return AddressAccuracy.CountryLevel;

                case GoogleAddressAccuracy.RegionLevel:
                    return AddressAccuracy.StateLevel;

                case GoogleAddressAccuracy.SubRegionLevel:
                    return AddressAccuracy.StateLevel;

                case GoogleAddressAccuracy.TownLevel:
                    return AddressAccuracy.CityLevel;

                case GoogleAddressAccuracy.ZipCodeLevel:
                    return AddressAccuracy.PostalCodeLevel;

                case GoogleAddressAccuracy.StreetLevel:
                    return AddressAccuracy.StreetLevel;

                case GoogleAddressAccuracy.IntersectionLevel:
                    return AddressAccuracy.StreetLevel;

                case GoogleAddressAccuracy.AddressLevel:
                    return AddressAccuracy.AddressLevel;
            }
            return AddressAccuracy.Unknown;
        }

        private Address[] ProcessWebResponse(WebResponse response)
        {
            XPathNavigator nav = this.LoadXmlResponse(response).CreateNavigator();
            this._namespaceManager = this.CreateXmlNamespaceManager(nav);
            GoogleStatusCode code = (GoogleStatusCode) int.Parse(this.EvaluateXPath("string(kml:kml/kml:Response/kml:Status/kml:code)", nav));
            List<Address> list = new List<Address>();
            if (code == GoogleStatusCode.Success)
            {
                XPathExpression expr = nav.Compile("kml:kml/kml:Response/kml:Placemark");
                expr.SetContext(this._namespaceManager);
                XPathNodeIterator iterator = nav.Select(expr);
                while (iterator.MoveNext())
                {
                    list.Add(this.RetrieveAddress(iterator.Current));
                }
            }
            return list.ToArray();
        }

        private Address RetrieveAddress(XPathNavigator nav)
        {
            nav = this.CreateSubNavigator(nav);
            GoogleAddressAccuracy accuracy = (GoogleAddressAccuracy) int.Parse(this.EvaluateXPath("string(//adr:AddressDetails/@Accuracy)", nav));
            this.EvaluateXPath("string(//kml:address)", nav);
            string country = this.EvaluateXPath("string(//adr:CountryNameCode)", nav);
            string state = this.EvaluateXPath("string(//adr:AdministrativeAreaName)", nav);
            this.EvaluateXPath("string(//adr:SubAdministrativeAreaName)", nav);
            string city = this.EvaluateXPath("string(//adr:LocalityName)", nav);
            string street = this.EvaluateXPath("string(//adr:ThoroughfareName)", nav);
            string postalCode = this.EvaluateXPath("string(//adr:PostalCodeNumber)", nav);
            string[] coordinates = this.EvaluateXPath("string(//kml:Point/kml:coordinates)", nav).Split(new char[] { ',' });
            return new Address(street, city, state, postalCode, country, this.FromCoordinates(coordinates), this.MapAccuracy(accuracy));
        }

        public override string ToString()
        {
            return string.Format("Google GeoCoder: {0}", this._accessKey);
        }

        public Address[] Validate(Address address)
        {
            return this.GeoCode(address.Street, address.City, address.State, address.PostalCode, address.Country);
        }

        public string AccessKey
        {
            get
            {
                return this._accessKey;
            }
        }
    }
}

