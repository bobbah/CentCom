# CentCom
CentCom is a software suite used to provide a global bans database for Space Station 13. It is capable of ingesting data from several public ban lists, and provides an API through which to search these collected records.

## Setup
To run your own local instance of CentCom, you must have the following...
- A supported database to store the ban data within (Postgres, MySql, MariaDB)
- A machine to run the CentCom Parsing Server
- A machine to run the CentCom API Server (this could be the same machine as the parsing server)

Once you have collected these things, you must then:
- Get the latest CentCom release
- Edit the ``hostsettings.json`` of the API server to configure the URLs that the server will bind to. This should be an HTTPS url. **By default, it will bind to any IP on port 6658.**
- Edit the appconfig.json of both the parsing server and API server to be configured for your database. You must provide a connection string, as well as the type of the database. This can be one of ``postgres``, ``mysql``, or ``mariadb``.  **I would strongly recommend having two different accounts/roles on your database for CentCom, one for the parsing server which has write access to the CentCom database, and one for the API server which can only read. This helps to isolate any possible security concerns./**

Finally:
- Run the parsing server first (``CentCom.Server``) to create the database schema and apply any necessary migrations to the database. This will also begin to populate the database based on the parsing schedule, which by default will parse all ban sources for new bans every 5 minutes except for at 00 and 30 minutes of every hour, at which time a full ban pass will occur.
- AFTER the migration/database setup has occurred successfully, you can now start the API server (``CentCom.API``) without any concerns. This server will now take API requests, the documentation of which you can view at the ``/swagger`` pages.

## Contributing
PRs can be opened on this repository to propose changes to the codebase. There are a few things to note...

**Adding a New Ban Source**:

If you are going to add an additional ban source, you typically have to add two new objects: a subclass of ``CentCom.Server.BanSources.BanParser``, which is **required**, and optionally a ``BanService`` in ``CentCom.Server.Services`` which helps to isolate the code used for parsing web pages and other forms of online resources.

When you sub-class ``BanParser``, you **must** override the ``Sources`` field to provide a definition of the ban sources that are for that server. As well as this, you optionally may override the ``SourceSupportsBanIDs`` field if the server exposes unique Ban IDs from their own database. These are ideal, as it removes any ambiguity as to if two bans that are found from a ban source are the same ban, so use them when provided. **If you use a ban source with IDs and flag it as such, you MUST set the ``BanID`` property on the ``Ban`` itself.**

As well as this, you must provide implementations for two different methods, ``FetchNewBansAsync()``, and ``FetchAllBansAsync()``. ``FetchNewBansAsync()`` should only return bans that are new as-of the last time that bans were fetched from this source, typically by getting the last bans for that source from the database and getting new bans from the source until you find one of the pre-existing bans. ``FetchAllBansAsync()`` should not have this behaviour, and instead should provide all bans from the ban source. This method is used on a much less common schedule to ensure that the ban database stores any bans that have updated reasons, or have been changed in general. It will also help to identify bans that have been deleted from the source so that we may do the same in our database.

Once you provide these methods, the base implementation of ``BanParser`` will handle the rest of the work of sorting through and storing bans. You essentially just have to provide it a means of getting those bans.

**Making Changes to Database Objects**:

If you make any changes to database objects (typically types in CentCom.Common.Models), you will need to generate a migration. You can do this from Visual Studio using the EntityFrameWorkCore tools, and the following powershell lines:
```
Add-Migration -Context NpgsqlDbContext -OutputDir Migrations/Postgres <Name of migration>
Add-Migration -Context MySqlDbContext -OutputDir Migrations/MySql <Name of migration>
```
*Note: You must add a migration for each database type supported, these types should be in ``CentCom.Common.Data``, subclassed from ``DatabaseContext``. The two lines above are intended to be an example and may not represent all the ``DatabaseContext`` subclasses.*

**Adding a New Database Backend**:

If you wish to support an additional database backend, you will need to create a new subclass of ``CentCom.Common.Data.DatabaseContext``. If necessary, you can overload how model types are stored in the new database backend by providing an override for the ``OnModelCreating`` method of ``DatabaseContext``, which you can see an example of in ``CentCom.Common.Data.MySqlDbContext``. As well as this, you will need to add a migration for your new database backend. You can do so using the same format as found in '*Making Changes to Database Objects*' above.

## FAQ

**How often does the parser run?**

I will likely make this configurable in the future, but at the moment it will run at every 5 minute interval in the hour (``XX:05``, ``XX:10``...) for a latest refresh (getting all new bans), except for ``XX:00`` and ``XX:30``, which will be full refreshes. At this point the entire source set is taken from each ban source and compared with what is stored in the database, at which point any differences will be resolved.