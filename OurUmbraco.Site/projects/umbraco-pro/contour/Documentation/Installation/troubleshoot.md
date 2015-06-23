#Common installation issues

##My umbraco directory is not called /umbraco
You can either use the manual installation process, or install from the repository, after installing the
package you will need to move all the files the installation placed in the /umbraco folder and move them to
the folder you've placed the umbraco core files in.

##Database tables failed to create
If this happened you'll need to execute the create script manually, check out the [manual installation guide](manual.md) for further details

##Web.config could not be updated
You web.config file is either write protected or the asp.net process do not have the rights to modify it.
If this step failed you'll need to execute it manually, check out the [manual installation guide](manual.md)

##Formpicker could not be added
There might have been an issue with installing the database. Check your database to see if any tables
starting with "UF" have been added, for example "UFforms". If the tables are missing, run the sql file
mentioned in the section "installing the database" and then go through the section on "adding the form picker" in the [manual installation guide](manual.md)

##Dashboard could not be installed
The /config/dashboard.config file is either write protected or the asp.net user does not have the rights to
modify it. Go through the chapter ["adding the contour dashboard section"](manual.md)

##ui.xml could not be updated
The ui.xml file is either write protected or the asp.net user does not have the rights to modify it. Go
through the chapter ["adding configuration to ui.xml"](manual.md)