# Contributing Guidelines
To contribute to the community site, you can fork & clone our repository, make your edits, and simply push back to GitHub and send us a pull request. All items that get pulled into the main repository will automatically get pushed to [our.umbraco.org](http://our.umbraco.org).

#### Getting started with Git and GitHub
 * [Setting up Git for Windows and connecting to GitHub](http://windows.github.com)
 * [Forking a GitHub repository](https://help.github.com/articles/fork-a-repo/)
 * [The simple guide to GIT](http://rogerdudler.github.io/git-guide/)

## Repository organisation
All active work done on the documentation is currently being done on the `master` branch.

### Contributing
First fork and clone the repository so that you have your own working copy. Then create a new branch on your local copy to make your changes. Once you are happy with your edits, use GitHub to issue a "pull request", which means your edits will be reviewed, and once accepted, merged into the main repository.

## Syncing your fork with the original repository
It's a good idea to pull in upstream changes, merge and commit to your own fork before submitting a pull request. To do this and sync your fork with this original one, you'll have to add the upstream url once:

	git remote add upstream git://github.com/umbraco/OurUmbraco.git

And then each time you want to get the changes:

	git fetch upstream
	git rebase upstream/master

There's more information on how this works [here](http://robots.thoughtbot.com/post/5133345960/keeping-a-git-fork-updated)

## Planning & discussions
Use [Github issues](https://github.com/umbraco/OurUmbraco/issues) for reporting and discussing issues.