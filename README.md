Our Umbraco
==========

Complete source of the umbraco community site, our.umbraco.org. 


##Database restore
Download the SQL Server Database from: https://dl.dropbox.com/u/3006713/our-cleaned-db-2008R2.zip?dl=1

Restore the database to SQL Server 2008 R2 (at least R2 is required!) and update the connection string (umbracoDbDSN) in OurUmbraco.Site/web.config 

##Build in visual studio
Make sure to allow NuGet Package Restore in VS (Tools > Options > Package Manager).

##Logging in
All users and members use the same password: Not_A_Real_Password

To log in, try root / Not_A_Real_Password for the backoffice and member423@non-existing-mail-provider.none / Not_A_Real_Password for the frontend.

##Syncing your fork with the original repository
To sync your fork with this original one, you'll have to add the upstream url once:

	git remote add upstream git://github.com/umbraco/OurUmbraco.git

And then each time you want to get the changes:

	git fetch upstream

Yes, this is a scary command line operation, don't you love it?! :-D

##Ignore your local web.config
To ensure you don't accidently commit your local web.config file, you can tell git ignore any changes done to this file.
Simply run the below command in git bash:

	git update-index --assume-unchanged OurUmbraco.Site/web.config

.. Yes, this is a scary command line operation, don't you love it?! :-D

Any updates to the main repo (github.com/umbraco/OurUmbraco) get deployed automatically to http://our.sandbox.umbraco.org/ - So when your pull request gets accepted, your changes should show up there within a few minutes.
