Our Umbraco
==========
Download the SQL Server Database from: https://dl.dropbox.com/u/3006713/our-cleaned-db.zip?dl=1

Restore the database to SQL Server 2012 (looking into a 2008 option!) and update the connection string (umbracoDbDSN) in OurUmbraco.Site/web.config 

Make sure to allow NuGet Package Restore in VS (Tools > Options > Package Manager).
