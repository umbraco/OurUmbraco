Our Umbraco
==========
Download the SQL Server Database from: https://dl.dropbox.com/u/3006713/our-cleaned-db.zip?dl=1

Restore the database to SQL Server 2012 (looking into a 2008 option!) and update the connection string (umbracoDbDSN) in OurUmbraco.Site/web.config 

Make sure to allow NuGet Package Restore in VS (Tools > Options > Package Manager).

To sync your fork with this original one, you'll have to add the upstream url once:

*git remote add upstream git://github.com/umbraco/OurUmbraco.git*

And then each time you want to get the changes:

*git fetch upstream*

.. Yes, this is a scary command line operation, don't you love it?! :-D

Any updates to the main repo (github.com/umbraco/OurUmbraco) get deployed automatically to http://our.sandbox.umbraco.org/ - So when your pull request gets accepted, your changes should show up there within a few minutes.
