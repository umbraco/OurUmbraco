Our Umbraco
==========

Complete source of the umbraco community site, our.umbraco.org. 

## Build in visual studio
Make sure to allow NuGet Package Restore in VS (Tools > Options > Package Manager). The first buid of the project will take quite a while, be patient, it will finish at some point.
Upon build a web.config file will be copied into the `OurUmbraco.Site` project which you can use in the following step.

If you're working on the frontend (the js/css/etc parts in `~/OurUmbraco.Client`) then you can either run `~/build/BuildClientFiles.bat` to build them and have them copied into the site. Or if you have npm/gulp installed on your machine you can run the usual commands in the `~/OurUmbraco.Client` folder:

```
npm install
npm install -g install gulp -g
gulp
```

## Database restore
Download the SQL Server Database from: http://umbracoreleases.blob.core.windows.net/ourumbraco/OurDev.zip

Restore the database to SQL Server 2012 SP2 (won't work on earlier version) and update the connection strings (`umbracoDbDSN`) in `OurUmbraco.Site/web.config`.

## Logging in
All users and members use the same password: Not_A_Real_Password

To log in, try `root` / `Not_A_Real_Password` for the backoffice and `member423@non-existing-mail-provider.none` / `Not_A_Real_Password` for the frontend.

You will need to set requireSSL in the Web.Config to **false** to login to the frontend.

```
<authentication mode="Forms">
    <forms requireSSL="false" name="yourAuthCookie" loginUrl="login.aspx" protection="All" path="/" slidingExpiration="true" timeout="525600" />
</authentication>
```

## Projects Area
If the projects area seems empty then that's because you need to rebuild the Examine indexes for it through the Developer section of Umbraco

## Documentation area
If the documentation area seems empty then that's because you need to download the documentation, look for the `documentationIndexer` in the Examine dashboard in the Developer section of Umbraco and Rebuild the index.  This will automatically download the latest documentation from github.

## Contributing
Please read our [Contributing Guidelines](CONTRIBUTING.md) to learn how you can get involved and help with the Umbraco community site.