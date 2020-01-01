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

Download the SQL Server Database from: https://umbracoreleases.blob.core.windows.net/ourumbraco/OurDev20190430.zip.

Restore the database to SQL Server 2016 SP1 (won't work on earlier version) and update the connection strings (`umbracoDbDSN`) in `OurUmbraco.Site/web.config`.

## Logging in

All users and members use the same password: Not_A_Real_Password

To log in, try `root` / `Not_A_Real_Password` for the backoffice and `member423@non-existing-mail-provider.none` / `Not_A_Real_Password` for the frontend.

You will need to set requireSSL in the `Web.Config` to **false** to login to the frontend.

```
<authentication mode="Forms">
    <forms requireSSL="false" name="yourAuthCookie" loginUrl="login.aspx" protection="All" path="/" slidingExpiration="true" timeout="525600" />
</authentication>
```

## Projects Area

If the projects area seems empty then that's because you need to rebuild the Examine indexes for it through the Developer section of Umbraco.

## Documentation Area

If the documentation area seems empty then that's because you need to download the documentation, look for the `documentationIndexer` in the Examine dashboard in the Developer section of Umbraco and Rebuild the index. This will automatically download the latest documentation from GitHub.

## Contributing

Please read our [Contributing Guidelines](CONTRIBUTING.md) to learn how you can get involved and help with the Umbraco community site.
