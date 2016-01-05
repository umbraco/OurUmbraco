Our Umbraco
==========

Complete source of the umbraco community site, our.umbraco.org. 

##Build in visual studio
Make sure to allow NuGet Package Restore in VS (Tools > Options > Package Manager). The first buid of the project will take quite a while, be patient, it will finish at some point.
Upon build a web.config file will be copied into the `OurUmbraco.Site` project which you can use in the following step.

##Database restore
Download the SQL Server Database from: http://umbracoreleases.blob.core.windows.net/ourumbraco/OurDev.zip

Restore the database to SQL Server 2012 SP2 (won't work on earlier version) and update the connection strings (`umbracoDbDSN`) in `OurUmbraco.Site/web.config`.

##Logging in
All users and members use the same password: Not_A_Real_Password

To log in, try `root` / `Not_A_Real_Password` for the backoffice and `member423@non-existing-mail-provider.none` / `Not_A_Real_Password` for the frontend.

##Projects Area
If the projects area seems empty then that's because you need to rebuild the Examine indexes for it through the Developer section of Umbraco

##Documentation area
If the documentation area seems empty then that's because you need to download the documentation, look for the the `Github Documentation` tab in the dashboards in the Developer section of Umbraco and use the `Get Docs!!` button.

##Syncing your fork with the original repository
To sync your fork with this original one, you'll have to add the upstream url once:

	git remote add upstream git://github.com/umbraco/OurUmbraco.git

And then each time you want to get the changes:

	git fetch upstream
	git rebase upstream/master

Yes, this is a scary command line operation, don't you love it?! :-D

(More info on how this works: http://robots.thoughtbot.com/post/5133345960/keeping-a-git-fork-updated)

##Issues
If you're creating a pull request, make sure that it's backed by an issue on the tracker: http://issues.umbraco.org/issues?q=project%3A+our.umbraco.org  

Mention the issue number in your pull request so we can merge it in more easily. 

Even if you're not planning on sending a pull request, you can always create an issue on the tracker if it doesn't exist yet, it helps other find ways to contribute.
