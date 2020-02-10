Our Umbraco
===========

Complete source of the Umbraco community site, [our.umbraco.com](https://our.umbraco.com).

## Build in Visual Studio

Make sure to allow NuGet Package Restore in VS (Tools > Options > Package Manager). The first build of the project will take quite a while, please be patient, it will finish at some point.
Upon build a `web.config` file will be copied into the `OurUmbraco.Site` project which you can use in the following step.

If you're working on the frontend (the js/css/etc parts in `~/OurUmbraco.Client`) then you can either run `~/build/BuildClientFiles.bat` to build them and have them copied into the site. Or if you have npm/gulp installed on your machine you can run the usual commands in the `~/OurUmbraco.Client` folder:

```
npm install
npm install gulp -g
gulp
```

## Database Restore

Download the SQL Server Database from: https://umbracoreleases.blob.core.windows.net/ourumbraco/OurDev20200107.7z.  If you don't have the 7zip utility installed you can download it from [7-zip.org](https://www.7-zip.org/) 

Restore the database to SQL Server 2017 (won't work on earlier versions) and update the connection strings (`umbracoDbDSN`) in `OurUmbraco.Site/web.config`.

If your SQL Server instance has the 'containment' feature enabled you can use these credentials in your connection string:
`user id=OurDevAnon;password=gQW435Jg32;`

## Logging in

All users and members use the same password: Not_A_Real_Password

* To log into the backoffice use  `root` / `Not_A_Real_Password`.

* To log into the frontend use `member_login@umbraco.org` / `Not_A_Real_Password`. You will be logging as Sebastiaan from Umbraco HQ.  All other profile information is anonymous.

⚠⚠⚠

In the web.config you get when you first build the project, you will need to change the following appSetting, from true to false:

```
<add key="umbracoUseSSL" value="false" />
```

Additionally, you will need to set requireSSL in the `Web.Config` to **false** to be able to login to the frontend.

```
<authentication mode="Forms">
    <forms requireSSL="false" name="yourAuthCookie" loginUrl="login.aspx" protection="All" path="/" slidingExpiration="true" timeout="525600" />
</authentication>
```

⚠⚠⚠

## Projects Area

If the projects area seems empty then that's because you need to rebuild the Examine indexes for it through the Developer section of Umbraco.

## Documentation Area

If the documentation area seems empty then that's because you need to download the documentation, look for the `documentationIndexer` in the Examine dashboard in the Developer section of Umbraco and Rebuild the index. This will automatically download the latest documentation from GitHub.

## Contributing

Please read our [Contributing Guidelines](CONTRIBUTING.md) to learn how you can get involved and help with the Umbraco community site.
