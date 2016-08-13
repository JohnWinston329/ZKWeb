﻿### 1.0.0 rc 1 Release Note

### Changes

- Translate all comments to english
- Fix Asp.Net Core 502 error when returnning 304
- IoC container support constructor dependency injection
- Serveral small fixes

### Breaking Changes

- Provide full support for .Net Core
	- Support multiple ORM
		- ~~Support Dapper~~
		- Support EntityFramework Core
		- Support InMemory
		- ~~Support MongoDB~~
		- Support NHibernate
	- Replace System.Drawing with CoreCompat.System.Drawing
		- Require mono's libgdiplus.dll on linux and osx

### Upgrade from previous version

- Replace "DatabaseContext" with "IDatabaseContext"
- Replace "DatabaseManager.GetContext" with "DatabaseManager.CreateContext"
- Use "IEntityOperationHandler" Instead of "IDataSaveCallback" and "IDataDeleteCallback"
- Install "ZKWeb.ORM.NHibernate" and "ZKWeb.ORM.InMemory" from nuget
	- You can also try other ORM but you may need redesign you database scheme
- Database transaction is not enabled by default anymore, enable it manually if needed
