using System;
using OurUmbraco.MarketPlace.Interfaces;

namespace OurUmbraco.MarketPlace.Providers
{
    public class Category : ICategory
    {

        protected int _id;
        protected Guid _categoryGuid;
        protected string _name;
        protected string _description;
        protected string _image;
        protected string _url;
        protected int _projectCount;
        protected bool _HQOnly;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }


        public Guid CategoryGuid
        {
            get { return _categoryGuid; }
            set { _categoryGuid = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string Image
        {
            get { return _image; }
            set { _image = value; }
        }

        public bool HQOnly
        {
            get { return _HQOnly; }
            set { _HQOnly = value; }
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }
        public int ProjectCount
        {
            get { return _projectCount; }
            set { _projectCount = value; }
        }

    }
}
