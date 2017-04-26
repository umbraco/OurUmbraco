using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Web.Services;
using System.Xml.XPath;
using OurUmbraco.Repository.Services;
using OurUmbraco.Wiki.BusinessLogic;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Web;

namespace OurUmbraco.Repository.webservices
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
            return OurUmbraco.Repository.Packages.Categories(false, false);
        }

        public List<Package> Packages(string repositoryGuid, int categoryId)
        {
            return OurUmbraco.Repository.Packages.GetPackagesByCategory(categoryId);
        }

        [WebMethod]
        public List<Package> Modules()
        {
            return OurUmbraco.Repository.Packages.GetPackagesByProperty("isModule", "1");
        }

        [WebMethod]
        public List<Category> ModulesCategorized()
        {
            return OurUmbraco.Repository.Packages.GetPackagesByPropertyCategorized("isModule", "1");
        }

        [WebMethod]
        public List<Package> Nitros()
        {
            return OurUmbraco.Repository.Packages.GetPackagesByCategory("Nitros");
        }

        [WebMethod]
        public List<Category> NitrosCategorized()
        {
            return OurUmbraco.Repository.Packages.GetSubCategories("Nitros", true);
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

        /// <summary>
        /// This will return the byte array for the package file for the package id specified
        /// </summary>
        /// <param name="packageGuid"></param>
        /// <param name="umbracoVersion">The Version of the umbraco install requesting the package file, this is in the normaly version format such as 7.6.0</param>
        /// <returns></returns>
        /// <remarks>
        /// This endpoint is new and is only referenced from 7.5.14 and above
        /// </remarks>
        [WebMethod]
        public byte[] GetPackageFile(string packageGuid, string umbracoVersion)
        {
            var umbHelper = new UmbracoHelper(UmbracoContext.Current);
            var pckRepoService = new PackageRepositoryService(umbHelper, umbHelper.MembershipHelper, ApplicationContext.Current.DatabaseContext);

            System.Version currUmbracoVersion;
            if (!System.Version.TryParse(umbracoVersion, out currUmbracoVersion))
                throw new InvalidOperationException("Could not parse the version specified " + umbracoVersion);

            var guid = new Guid(packageGuid);
            var details = pckRepoService.GetDetails(guid, currUmbracoVersion, true);
            if (details == null)
                throw new InvalidOperationException("No package found with id " + packageGuid);

            if (details.ZipUrl.IsNullOrWhiteSpace())
                throw new InvalidOperationException("This package is not compatible with the Umbraco version " + umbracoVersion);

            var wf = new WikiFile(details.ZipFileId);
            if (wf == null)
                throw new InvalidOperationException("Could not find wiki file by id " + details.ZipFileId);
            wf.UpdateDownloadCounter(true, true);

            return wf.ToByteArray();
        }

        /// <summary>
        /// This will return the byte array for the package file for the package id specified - since this endpoint is ONLY used 
        /// for Umbraco installs that are less than 7.6.0 and only when legacy XML schema is enabled, we know that this is only used for old umbraco versions
        /// </summary>
        /// <param name="packageGuid"></param>
        /// <returns></returns>
        [WebMethod]
        public byte[] fetchPackage(string packageGuid)
        {
            var umbHelper = new UmbracoHelper(UmbracoContext.Current);
            var pckRepoService = new PackageRepositoryService(umbHelper, umbHelper.MembershipHelper, ApplicationContext.Current.DatabaseContext);

            //This doesn't matter what we set it to so long as it's below 7.5 since that is the version we introduce strict dependencies
            var currUmbracoVersion = new System.Version(4, 0, 0);
            var guid = new Guid(packageGuid);
            var details = pckRepoService.GetDetails(guid, currUmbracoVersion ,true);
            if (details == null)
                throw new InvalidOperationException("No package found with id " + packageGuid);

            if (details.ZipUrl.IsNullOrWhiteSpace())
                throw new InvalidOperationException("This package is not compatible with your Umbraco version");

            var wf = new WikiFile(details.ZipFileId);
            if (wf == null)
                throw new InvalidOperationException("Could not find wiki file by id " + details.ZipFileId);
            wf.UpdateDownloadCounter(true, true);

            return wf.ToByteArray();            
        }

        /// <summary>
        /// This will return the byte array for the package file for the package id specified - since this endpoint is ONLY used 
        /// for Umbraco installs that are less than 7.6.0 and only when legacy XML schema is enabled, we know that this is only used for old umbraco versions
        /// </summary>
        /// <param name="packageGuid"></param>
        /// <param name="repoVersion">
        /// This is a strange Umbraco version parameter - but it's not really an umbraco version, it's a special/odd version format like Version41
        /// 
        /// The repoVersion will never be null for for 7.5.x and this method will never be used by 7.6+
        /// </param>
        /// <returns></returns>
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

            var umbHelper = new UmbracoHelper(UmbracoContext.Current);
            var pckRepoService = new PackageRepositoryService(umbHelper, umbHelper.MembershipHelper, ApplicationContext.Current.DatabaseContext);

            //This doesn't matter what we set it to so long as it's below 7.5 since that is the version we introduce strict dependencies
            //Side note: Umbraco 7.5.0 uses this endpoint but since 7.5.0 is already out there's nothing we can do about this, if we pass in 7.5.x then
            // the logic will use strict file versions which we cannot do because this endpoint is also used for < 7.5! 
            // The worst that can happen in this case is that a strict package dependency has been made on 7.5 and then an umbraco 7.5 package 
            // requests the file, well it won't get it because this will only return non strict packages.
            var currUmbracoVersion = new System.Version(4, 0, 0);
            var guid = new Guid(packageGuid);
            var details = pckRepoService.GetDetails(guid, currUmbracoVersion, true);
            if (details == null)
                return new byte[0];

            if (details.ZipUrl.IsNullOrWhiteSpace())
                throw new InvalidOperationException("This package is not compatible with the Umbraco version " + currUmbracoVersion);

            var wf = new WikiFile(details.ZipFileId);
            
            //WikiFile wf = OurUmbraco.Repository.Packages.PackageFileByGuid(new Guid(packageGuid));

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

                if (wf.Version.Version != version && wf.Version.Version != "nan")
                {
                    wf = WikiFile.FindPackageForUmbracoVersion(wf.NodeId, version);
                    return wf.ToByteArray();
                }
            }

            return new byte[0];            
        }

        /// <summary>
        /// SD: Pretty sure this is no longer used/needed it should probably be removed - maybe really old versions might try to use this?
        /// </summary>
        /// <param name="packageGuid"></param>
        /// <param name="memberKey"></param>
        /// <returns></returns>
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
            return OurUmbraco.Repository.Packages.SubmitPackageAsProject(authorGuid, packageGuid, packageFile, packageDoc, packageThumbnail, name, description);
        }

        [WebMethod]
        public Package PackageByGuid(string packageGuid)
        {
            return OurUmbraco.Repository.Packages.GetPackageByGuid(new Guid(packageGuid));
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
