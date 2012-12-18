Our Umbraco
==========

Complete source of the umbraco community site, our.umbraco.org. 


##Database restore
Download the SQL Server Database from: https://dl.dropbox.com/u/3006713/our-cleaned-db.zip?dl=1

Restore the database to SQL Server 2012 (looking into a 2008 option!) and update the connection string (umbracoDbDSN) in OurUmbraco.Site/web.config 


##Build in visual studio
Make sure to allow NuGet Package Restore in VS (Tools > Options > Package Manager).


##Syncing your form with tje original repository
To sync your fork with this original one, you'll have to add the upstream url once:

	git remote add upstream git://github.com/umbraco/OurUmbraco.git

And then each time you want to get the changes:

	git fetch upstream

Yes, this is a scary command line operation, don't you love it?! :-D

##Ignore your local web.config
To ensure you don't accidently commit your local web.config file, you can tell git ignore any changes done to this file.
Simply run the below command in git bash:

	git update-index --assume-unchanged OurUmbraco.Site/web.config

