/*License Statement
 Copyright (c) Microsoft Corporation.  All rights reserved.

The use and distribution terms for this software are covered by the 
Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
which can be found in the file CPL.TXT at the root of this distribution.
By using this software in any fashion, you are agreeing to be bound by 
the terms of this license.

You must not remove this notice, or any other, from this software.
*/

use [${database}]
GO
if not exists (select * from master.dbo.syslogins where loginname = N'${user}')
exec sp_grantlogin N'${user}'
exec sp_defaultdb N'${user}', N'master'
exec sp_defaultlanguage N'${user}', N'us_english'
GO
if not exists (select * from dbo.sysusers where name = N'${ntuser}' and uid < 16382)
EXEC sp_grantdbaccess N'${user}'
GO
exec sp_addrolemember N'db_owner', N'${user}'
GO