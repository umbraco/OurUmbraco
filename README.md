Our Umbraco
===========

Complete source of the Umbraco community site, [our.umbraco.com](https://our.umbraco.com).

## Build in Visual Studio

Make sure to allow NuGet Package Restore in VS (Tools > Options > Package Manager). The first build of the project will take quite a while, please be patient, it will finish at some point.
Upon build a `web.config` file will be copied into the `OurUmbraco.Site` project which you can use in the following step.

If you're working on the frontend (the js/css/etc parts in `~/OurUmbraco.Client`) then you can either run `~/build/BuildClientFiles.bat` to build them and have them copied into the site. Or if you have npm/gulp installed on your machine you can run the usual commands in the `~/OurUmbraco.Client` folder:

```
npm ci
npm run build
```

When you're working on frontend you can run `npm start` to start watching js/css files for changes. When you save changes they will be copied into the site by the watcher.

## Database Restore

Download the SQL Server Database from: https://umbracoreleases.blob.core.windows.net/ourumbraco/OurDev20200720.bacpac.

Restore the database to SQL Server 2017 (won't work on earlier versions) and update the connection strings (`umbracoDbDSN`) in `OurUmbraco.Site/web.config`.

## Logging in

* To log into the backoffice use  `admin@our.test` / `1234567890`.

* To log into the frontend use `member@our.test` / `1234567890`.

## Projects Area

If the projects area seems empty then that's because you need to rebuild the Examine indexes for it through the Developer section of Umbraco. 

## Documentation Area

If the documentation area seems empty then that's because you need to download the documentation, look for the `documentationIndexer` in the Examine dashboard in the Developer section of Umbraco and Rebuild the index. This will automatically download the latest documentation from GitHub.

## Community Area

Following a recent update we've moved the `/community` pages to another [website](https://community.umbraco.com/). You can read more about the decision [here](https://umbraco.com/blog/the-umbraco-community-website-revisited).

## Contributing

Please read our [Contributing Guidelines](CONTRIBUTING.md) to learn how you can get involved and help with the Umbraco community site.