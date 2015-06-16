Our Umbraco
==========

Complete source of the umbraco community site, our.umbraco.org. 


##Database restore
Download the SQL Server Database from: http://umbracoreleases.blob.core.windows.net/ourumbraco/our-database-backup.zip

Restore the database to SQL Server 2008 R2 (at least R2 is required!) and update the connection string (umbracoDbDSN) in OurUmbraco.Site/web.config.   
**Note:** there's 2 connection strings, make sure to update them both. 

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
	git rebase upstream/master

Yes, this is a scary command line operation, don't you love it?! :-D

(More info on how this works: http://robots.thoughtbot.com/post/5133345960/keeping-a-git-fork-updated)

##Ignore your local web.config
To ensure you don't accidently commit your local web.config file, you can tell git ignore any changes done to this file.
Simply run the below command in git bash:

	git update-index --assume-unchanged OurUmbraco.Site/web.config

.. Yes, this is a scary command line operation, don't you love it?! :-D

##Projects section
Note: the projects section doesn't work well at the moment, due to missing Marketplace.*.dll files. We still need to seperate our some of the Deli processes into a seperate DLL.

##Issues
If you're creating a pull request, make sure that it's backed by an issue on the tracker: http://issues.umbraco.org/issues?q=project%3A+our.umbraco.org  

Mention the issue number in your pull request so we can merge it in more easily. 

Even if you're not planning on sending a pull request, you can always create an issue on the tracker if it doesn't exist yet, it helps other find ways to contribute.
