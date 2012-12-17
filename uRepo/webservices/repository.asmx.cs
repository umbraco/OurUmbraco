using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic;
using System.Xml;

namespace uRepo.webservices
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://packages.umbraco.org/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]

    public class Repository : System.Web.Services.WebService
    {
        private string m_guid;
        private string m_name;
        private int m_rootNodeId = 0;

        /// <summary>
        /// Returns the top level categories for the webservice
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public List<Category> Categories(string repositoryGuid)
        {
            return uRepo.Packages.Categories(false, false);
        }

        public List<Package> Packages(string repositoryGuid, int categoryId)
        {
            return uRepo.Packages.GetPackagesByCategory(categoryId);
        }

        [WebMethod]
        public List<Package> Modules()
        {
            return uRepo.Packages.GetPackagesByProperty("isModule", "1");
        }

        [WebMethod]
        public List<Category> ModulesCategorized()
        {
            return uRepo.Packages.GetPackagesByPropertyCategorized("isModule", "1");
        }

        [WebMethod]
        public List<Package> Nitros()
        {
            return uRepo.Packages.GetPackagesByCategory("Nitros");
        }

        [WebMethod]
        public List<Category> NitrosCategorized()
        {
            return uRepo.Packages.GetSubCategories("Nitros", true);
        }

        [WebMethod]
        public string authenticate(string email, string md5Password)
        {
            umbraco.cms.businesslogic.member.Member mem = umbraco.cms.businesslogic.member.Member.GetMemberFromEmail(email);

            if (md5(mem.Password) == md5Password)
            {
                return mem.UniqueId.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        [WebMethod]
        public byte[] fetchPackage(string packageGuid)
        {
            return uRepo.Packages.PackageFileByGuid(new Guid(packageGuid)).ToByteArray();
        }

        [WebMethod]
        public byte[] fetchPackageByVersion(string packageGuid, string repoVersion)
        {
            //first, translate repo version enum to our.umb version strings
            //version3 = v31 
            //version4 = v4
            //version41 = v45
            //if v45, check if default package is v47 as well, as 4.7 install use v45 when calling the repo (thank god we are moving to nuget)
            //everything else = v4

            string version = "v4";
            switch (repoVersion.ToLower())
            {
                case "version3":
                    version = "v31";
                    break;
                case "version4":
                    version = "v4";
                    break;
                case "version41":
                    version = "v45";
                    break;
                case "version41legacy":
                    version = "v45l";
                    break;
                default:
                    version = "v4";
                    break;
            }

            uWiki.Businesslogic.WikiFile wf = uRepo.Packages.PackageFileByGuid(new Guid(packageGuid));


            if (wf != null)
            {


                //if the package doesn't care about the umbraco version needed... 
                if (wf.Version.Version == "nan")
                    return wf.ToByteArray();


                //if v45
                if (version == "v45")
                {
                    int v = 0;
                    if (int.TryParse(wf.Version.Version.Replace("v", ""), out v))
                        if (v >= 45)
                            return wf.ToByteArray();


                    if (wf.Version.Version == "v47" || wf.Version.Version == "v45")
                        return wf.ToByteArray();
                    else if (wf.Version.Version != version && wf.Version.Version != "nan")
                    {
                        wf = uWiki.Businesslogic.WikiFile.FindPackageForUmbracoVersion(wf.NodeId, version);
                        return wf.ToByteArray();
                    }
                }
            }

            return new byte[0];

            /*
            if (wf.Version.Version != version && wf.Version.Version != "nan")
                    wf = uWiki.Businesslogic.WikiFile.FindPackageForUmbracoVersion(wf.NodeId, version);
            
            
            if(wf != null)
                return wf.ToByteArray();
            else
                return new byte[0];*/
        }


        [WebMethod]
        public byte[] fetchProtectedPackage(string packageGuid, string memberKey)
        {

            //Guid package = new Guid(packageGuid);
            byte[] packageByteArray = new byte[0];

            Package pack = PackageByGuid(packageGuid);
            umbraco.cms.businesslogic.member.Member mem = new umbraco.cms.businesslogic.member.Member(new Guid(memberKey));
            umbraco.cms.businesslogic.contentitem.ContentItem packageNode = packageContentItem(packageGuid);


            if (pack.Protected && Access.HasAccess(packageNode.Id, packageNode.Path, System.Web.Security.Membership.GetUser(mem.Id)))
            {

                string FilePath = Server.MapPath(packageNode.getProperty("package").Value.ToString());

                System.IO.FileStream fs1 = null;
                fs1 = System.IO.File.Open(FilePath, FileMode.Open, FileAccess.Read);

                packageByteArray = new byte[fs1.Length];
                fs1.Read(packageByteArray, 0, (int)fs1.Length);

                fs1.Close();

                int downloads = 0;
                string downloadsVal = packageNode.getProperty("downloads").Value.ToString();

                if (downloadsVal != "")
                    downloads = int.Parse(downloadsVal);

                downloads++;

                packageNode.getProperty("downloads").Value = downloads;
                packageNode.Save();
            }

            return packageByteArray;
        }

        [WebMethod]
        public SubmitStatus SubmitPackage(string repositoryGuid, string authorGuid, string packageGuid, byte[] packageFile, byte[] packageDoc, byte[] packageThumbnail, string name, string author, string authorUrl, string description)
        {
            return uRepo.Packages.SubmitPackageAsProject(authorGuid, packageGuid, packageFile, packageDoc, packageThumbnail, name, description);
        }

        [WebMethod]
        public Package PackageByGuid(string packageGuid)
        {
            return uRepo.Packages.GetPackageByGuid(new Guid(packageGuid));
        }

        private static umbraco.cms.businesslogic.contentitem.ContentItem packageContentItem(string guid)
        {

            umbraco.cms.businesslogic.contentitem.ContentItem item = new umbraco.cms.businesslogic.contentitem.ContentItem(1052);

            XPathNodeIterator xpn = umbraco.library.GetXmlNodeByXPath("descendant::node[data [@alias = 'packageGuid'] = '" + guid + "']");

            if (xpn.MoveNext())
            {
                int id = int.Parse(xpn.Current.GetAttribute("id", "")); ;
                item = new umbraco.cms.businesslogic.contentitem.ContentItem(id);
            }

            return item;
        }

        private static umbraco.cms.businesslogic.contentitem.ContentItem repositoryContentItem(string guid)
        {

            umbraco.cms.businesslogic.contentitem.ContentItem item = new umbraco.cms.businesslogic.contentitem.ContentItem(1052);

            XPathNodeIterator xpn = umbraco.library.GetXmlNodeByXPath("descendant::node[data [@alias = 'repositoryGuid'] = '" + guid + "']");

            if (xpn.MoveNext())
            {

                int id = int.Parse(xpn.Current.GetAttribute("id", "")); ;

                item = new umbraco.cms.businesslogic.contentitem.ContentItem(id);
            }

            return item;
        }

        private static string md5(string input)
        {

            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);

            bs = x.ComputeHash(bs);

            System.Text.StringBuilder s = new System.Text.StringBuilder();

            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }

            return s.ToString();
        }
    }


}
