/*License Statement
 Copyright (c) Microsoft Corporation.  All rights reserved.

The use and distribution terms for this software are covered by the 
Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
which can be found in the file CPL.TXT at the root of this distribution.
By using this software in any fashion, you are agreeing to be bound by 
the terms of this license.

You must not remove this notice, or any other, from this software.
*/

use ${database}
GO
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[FK_Topic_Namespace]') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
ALTER TABLE [dbo].[Topic] DROP CONSTRAINT FK_Topic_Namespace
GO

/****** Object:  Stored Procedure dbo.CheckNamespaceExists    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[CheckNamespaceExists]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[CheckNamespaceExists]
GO

/****** Object:  Stored Procedure dbo.GetSqlTopicInfoForNonArchiveTopicsSortedDescending    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetSqlTopicInfoForNonArchiveTopicsSortedDescending]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetSqlTopicInfoForNonArchiveTopicsSortedDescending]
GO

/****** Object:  Stored Procedure dbo.GetSqlTopicArchiveInfosSince    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetSqlTopicArchiveInfosSince]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetSqlTopicArchiveInfosSince]
GO

/****** Object:  Stored Procedure dbo.CheckTopicExists    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[CheckTopicExists]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[CheckTopicExists]
GO

/****** Object:  Stored Procedure dbo.CreateNamespace    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[CreateNamespace]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[CreateNamespace]
GO

/****** Object:  Stored Procedure dbo.DeleteNamespace    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DeleteNamespace]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[DeleteNamespace]
GO

/****** Object:  Stored Procedure dbo.DeleteTopic    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DeleteTopic]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[DeleteTopic]
GO

/****** Object:  Stored Procedure dbo.GetSqlTopicArchiveInfos    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetSqlTopicArchiveInfos]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetSqlTopicArchiveInfos]
GO

/****** Object:  Stored Procedure dbo.GetSqlTopicInfoForLatestVersion    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetSqlTopicInfoForLatestVersion]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetSqlTopicInfoForLatestVersion]
GO

/****** Object:  Stored Procedure dbo.GetSqlTopicInfoForNonArchiveTopics    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetSqlTopicInfoForNonArchiveTopics]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetSqlTopicInfoForNonArchiveTopics]
GO

/****** Object:  Stored Procedure dbo.GetTopicBody    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetTopicBody]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetTopicBody]
GO

/****** Object:  Stored Procedure dbo.GetTopicCreationTime    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetTopicCreationTime]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetTopicCreationTime]
GO

/****** Object:  Stored Procedure dbo.GetTopicLastWriteTime    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetTopicLastWriteTime]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetTopicLastWriteTime]
GO

/****** Object:  Stored Procedure dbo.IsExistingTopicWritable    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[IsExistingTopicWritable]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[IsExistingTopicWritable]
GO

/****** Object:  Stored Procedure dbo.RenameTopic    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RenameTopic]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[RenameTopic]
GO

/****** Object:  Stored Procedure dbo.WriteTopic    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[WriteTopic]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[WriteTopic]
GO

/****** Object:  Table [dbo].[Namespace]    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[Namespace]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[Namespace]
GO

/****** Object:  Table [dbo].[Topic]    Script Date: 1/30/2005 11:35:25 AM ******/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[Topic]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[Topic]
GO



/****** Object:  Table [dbo].[Namespace]    Script Date: 1/30/2005 11:35:25 AM ******/
CREATE TABLE [dbo].[Namespace] (
	[NamespaceId] [int] IDENTITY (1, 1) NOT NULL ,
	[Name] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Topic]    Script Date: 1/30/2005 11:35:25 AM ******/
CREATE TABLE [dbo].[Topic] (
	[TopicId] [int] IDENTITY (1, 1) NOT NULL ,
	[NamespaceId] [int] NOT NULL ,
	[Name] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	[LastWriteTime] [datetime] NOT NULL ,
	[CreationTime] [datetime] NOT NULL ,
	[Archive] [bit] NOT NULL ,
	[Deleted] [bit] NOT NULL ,
	[Writable] [bit] NOT NULL ,
	[Hidden] [bit] NOT NULL ,
	[Body] [ntext] COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Namespace] WITH NOCHECK ADD 
	CONSTRAINT [PK_Namespace] PRIMARY KEY  CLUSTERED 
	(
		[NamespaceId]
	)  ON [PRIMARY] 
GO

ALTER TABLE [dbo].[Topic] WITH NOCHECK ADD 
	CONSTRAINT [PK_Topic] PRIMARY KEY  CLUSTERED 
	(
		[TopicId]
	)  ON [PRIMARY] 
GO

ALTER TABLE [dbo].[Namespace] ADD 
	CONSTRAINT [DF_Namespace_Name] DEFAULT ('') FOR [Name]
GO

ALTER TABLE [dbo].[Topic] ADD 
	CONSTRAINT [DF_Topic_LastModifiedDate] DEFAULT (getdate()) FOR [LastWriteTime],
	CONSTRAINT [DF_Topic_Archive] DEFAULT (0) FOR [Archive],
	CONSTRAINT [DF_Topic_TopicDeleted] DEFAULT (0) FOR [Deleted],
	CONSTRAINT [DF__Topic__ReadOnly__10566F31] DEFAULT (1) FOR [Writable],
	CONSTRAINT [DF_Topic_Hidden] DEFAULT (0) FOR [Hidden]
GO

 CREATE  INDEX [IX_Topic_NamespaceId] ON [dbo].[Topic]([TopicId]) ON [PRIMARY]
GO

 CREATE  INDEX [IX_Topic_NamespaceId_LastWriteTime] ON [dbo].[Topic]([NamespaceId], [LastWriteTime] DESC ) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Topic] ADD 
	CONSTRAINT [FK_Topic_Namespace] FOREIGN KEY 
	(
		[NamespaceId]
	) REFERENCES [dbo].[Namespace] (
		[NamespaceId]
	) NOT FOR REPLICATION 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

/****** Object:  Stored Procedure dbo.CheckNamespaceExists    Script Date: 1/30/2005 11:35:25 AM ******/

CREATE  PROCEDURE Dbo.CheckNamespaceExists
(
	@namespace NVARCHAR(255),
	@namespaceExists BIT OUTPUT
)
AS
	SET NOCOUNT ON

	DECLARE @namespaceId INTEGER

	SET @namespaceId = null

	SELECT @namespaceId = NamespaceId
	FROM dbo.Namespace
	WHERE [Name] = @namespace
	
	IF @namespaceId IS NULL
	BEGIN
		SET @namespaceExists = 0
	END	
	ELSE
	BEGIN
		SET @namespaceExists = 1
	END



GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

/****** Object:  Stored Procedure dbo.CheckTopicExists    Script Date: 1/30/2005 11:35:25 AM ******/

CREATE  PROCEDURE Dbo.CheckTopicExists
(
	@namespace NVARCHAR(255),
	@topicName NVARCHAR(255),
	@topicExists BIT OUTPUT
)
AS
	SET NOCOUNT ON

	DECLARE @topicId INTEGER

	SET @topicId = null

	SELECT @topicId = TopicId
	FROM dbo.Namespace namespaceTable JOIN 
			dbo.Topic topicTable ON namespaceTable.NamespaceId = topicTable.NamespaceId
	WHERE namespaceTable.[Name] = @namespace
			AND topicTable.[Name] = @topicName
	
	IF @topicId IS NULL
	BEGIN
		SET @topicExists = 0
	END	
	ELSE
	BEGIN
		SET @topicExists = 1
	END



GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

/****** Object:  Stored Procedure dbo.CreateNamespace    Script Date: 1/30/2005 11:35:25 AM ******/

CREATE PROCEDURE Dbo.CreateNamespace
(
	@namespace NVARCHAR(255)
)
AS
	SET NOCOUNT ON

	DECLARE @namespaceId INTEGER

	SET @namespaceId = null

	SELECT @namespaceId = NamespaceId
	FROM dbo.Namespace
	WHERE [Name] = @namespace
	
	IF @namespaceId IS NOT NULL
	BEGIN
		RAISERROR('Namespace: %s already exists.', 16, 1, @namespace)
	END	

	INSERT INTO dbo.Namespace ([Name])
	VALUES (@namespace)


GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

/****** Object:  Stored Procedure dbo.DeleteNamespace    Script Date: 1/30/2005 11:35:25 AM ******/

CREATE    PROCEDURE Dbo.DeleteNamespace
(
	@namespace NVARCHAR(255)
)
AS
	SET NOCOUNT ON

	DECLARE @namespaceId INTEGER

	SET @namespaceId = null

	SELECT @namespaceId = NamespaceId
	FROM dbo.Namespace namespaceTable
	WHERE namespaceTable.[Name] = @namespace

	IF @namespaceId IS NULL 
	BEGIN
		RAISERROR('Specified namespace: %s does not exist.',16,1, @namespace)
	END

	BEGIN TRAN

		DELETE FROM dbo.Topic
		WHERE NamespaceId = @namespaceId

		DELETE FROM dbo.Namespace
		WHERE NamespaceId = @namespaceId

	IF @@ERROR = 0
	BEGIN
		COMMIT TRAN
	END
	ELSE
	BEGIN
		ROLLBACK TRAN
	END


GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

/****** Object:  Stored Procedure dbo.DeleteTopic    Script Date: 1/30/2005 11:35:25 AM ******/

CREATE   PROCEDURE Dbo.DeleteTopic
(
	@namespace NVARCHAR(255),
	@topicName NVARCHAR(255)
)
AS
	SET NOCOUNT ON

	DECLARE @topicId INTEGER

	SET @topicId = null

	SELECT @topicId = topicTable.TopicId
	FROM dbo.Namespace namespaceTable JOIN 
			dbo.Topic topicTable ON namespaceTable.NamespaceId = topicTable.NamespaceId
	WHERE namespaceTable.[Name] = @namespace
			AND topicTable.[Name] = @topicName


	IF @topicId IS NULL 
	BEGIN
		RAISERROR('Specified namespace: %s does not contain the topic: %s.',16,1, @namespace, @topicName)
	END

	DELETE FROM dbo.Topic
	WHERE TopicId = @topicId


GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

/****** Object:  Stored Procedure dbo.GetSqlTopicArchiveInfos    Script Date: 1/30/2005 11:35:25 AM ******/

CREATE PROCEDURE Dbo.GetSqlTopicArchiveInfos
(
	@namespace NVARCHAR(255),
	@topicName NVARCHAR(255)
)
AS
	SET NOCOUNT ON

	SELECT topicTable.[Name], topicTable.LastWriteTime
	FROM dbo.Namespace namespaceTable JOIN 
			dbo.Topic topicTable ON namespaceTable.NamespaceId = topicTable.NamespaceId
	WHERE namespaceTable.[Name] = @namespace
			AND topicTable.[Name] like @topicName
			AND topicTable.Archive = 1


GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

/****** Object:  Stored Procedure dbo.GetSqlTopicInfoForLatestVersion    Script Date: 1/30/2005 11:35:25 AM ******/

CREATE  PROCEDURE Dbo.GetSqlTopicInfoForLatestVersion
(
	@namespace NVARCHAR(255),
	@topicName NVARCHAR(255)
)
AS
	SET NOCOUNT ON

	SELECT Top 1 topicTable.[Name], topicTable.LastWriteTime
	FROM dbo.Namespace namespaceTable JOIN 
			dbo.Topic topicTable ON namespaceTable.NamespaceId = topicTable.NamespaceId
	WHERE namespaceTable.[Name] = @namespace
			AND topicTable.[Name] like @topicName
			AND topicTable.Archive = 1
	ORDER BY topicTable.LastWriteTime DESC



GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

/****** Object:  Stored Procedure dbo.GetSqlTopicInfoForNonArchiveTopics    Script Date: 1/30/2005 11:35:25 AM ******/

CREATE  PROCEDURE Dbo.GetSqlTopicInfoForNonArchiveTopics
(
	@namespace NVARCHAR(255)
)
AS
	SET NOCOUNT ON

	SELECT topicTable.[Name], topicTable.LastWriteTime
	FROM dbo.Namespace namespaceTable JOIN 
			dbo.Topic topicTable ON namespaceTable.NamespaceId = topicTable.NamespaceId
	WHERE namespaceTable.[Name] = @namespace
			AND topicTable.Archive = 0



GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

/****** Object:  Stored Procedure dbo.GetTopicBody    Script Date: 1/30/2005 11:35:25 AM ******/

/****** Object:  Stored Procedure dbo.GetTopicBody    Script Date: 1/14/2005 6:16:48 AM ******/

CREATE    PROCEDURE Dbo.GetTopicBody
(
	@namespace NVARCHAR(255),
	@topicName NVARCHAR(255)
)
AS
	SET NOCOUNT ON

	SELECT Body
	FROM dbo.Namespace namespaceTable JOIN 
			dbo.Topic topicTable ON namespaceTable.NamespaceId = topicTable.NamespaceId
	WHERE namespaceTable.[Name] = @namespace
			AND topicTable.[Name] = @topicName




GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

/****** Object:  Stored Procedure dbo.GetTopicCreationTime    Script Date: 1/30/2005 11:35:25 AM ******/

CREATE  PROCEDURE Dbo.GetTopicCreationTime
(
	@namespace NVARCHAR(255),
	@topicName NVARCHAR(255),
	@topicCreationTime DATETIME OUTPUT
)
AS
	SET NOCOUNT ON

	DECLARE @topicId INTEGER

	SET @topicId = null

	SELECT @topicId = topicTable.TopicId, @topicCreationTime = CreationTime
	FROM dbo.Namespace namespaceTable JOIN 
			dbo.Topic topicTable ON namespaceTable.NamespaceId = topicTable.NamespaceId
	WHERE namespaceTable.[Name] = @namespace
			AND topicTable.[Name] = @topicName


	IF @topicId IS NULL 
	BEGIN
		RAISERROR('The specified namespace: %s does not contain the topic: %s.', 16, 1, @namespace, @topicName)
	END
	



GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

/****** Object:  Stored Procedure dbo.GetTopicLastWriteTime    Script Date: 1/30/2005 11:35:25 AM ******/

CREATE  PROCEDURE Dbo.GetTopicLastWriteTime
(
	@namespace NVARCHAR(255),
	@topicName NVARCHAR(255),
	@topicLastWriteTime DATETIME OUTPUT
)
AS
	SET NOCOUNT ON

	DECLARE @topicId INTEGER

	SET @topicId = null

	SELECT @topicId = topicTable.TopicId, @topicLastWriteTime = LastWriteTime
	FROM dbo.Namespace namespaceTable JOIN 
			dbo.Topic topicTable ON namespaceTable.NamespaceId = topicTable.NamespaceId
	WHERE namespaceTable.[Name] = @namespace
			AND topicTable.[Name] = @topicName


	IF @topicId IS NULL 
	BEGIN
		RAISERROR('The specified namespace: %s does not contain the topic: %s.', 16, 1, @namespace, @topicName)
	END
	


GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

/****** Object:  Stored Procedure dbo.IsExistingTopicWritable    Script Date: 1/30/2005 11:35:25 AM ******/

/****** Object:  Stored Procedure dbo.IsExistingTopicWritable    Script Date: 1/14/2005 6:17:10 AM ******/

CREATE   PROCEDURE Dbo.IsExistingTopicWritable
(
	@namespace NVARCHAR(255),
	@topicName NVARCHAR(255),
	@topicWritable BIT OUTPUT
)
AS
	SET NOCOUNT ON

	DECLARE @topicId INTEGER

	SET @topicId = null

	SELECT @topicId = TopicId, @topicWritable = Writable
	FROM dbo.Namespace namespaceTable JOIN 
			dbo.Topic topicTable ON namespaceTable.NamespaceId = topicTable.NamespaceId
	WHERE namespaceTable.[Name] = @namespace
			AND topicTable.[Name] = @topicName
	
	IF @topicId IS NULL
	BEGIN
		RAISERROR('Specified namespace: %s does not contain topic: %s', 16,1, @namespace, @topicName)
	END	




GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

/****** Object:  Stored Procedure dbo.RenameTopic    Script Date: 1/30/2005 11:35:25 AM ******/

/****** Object:  Stored Procedure dbo.RenameTopic    Script Date: 1/14/2005 7:42:06 AM ******/

CREATE  PROCEDURE Dbo.RenameTopic
(
	@namespace NVARCHAR(255),
	@topicCurrentName NVARCHAR(255),
	@topicNewName NVARCHAR(255)
)
AS
	SET NOCOUNT ON

	DECLARE @topicId INTEGER

	SET @topicId = null

	SELECT @topicId = topicTable.TopicId
	FROM dbo.Namespace namespaceTable JOIN 
			dbo.Topic topicTable ON namespaceTable.NamespaceId = topicTable.NamespaceId
	WHERE namespaceTable.[Name] = @namespace
			AND topicTable.[Name] = @topicCurrentName


	IF @topicId IS NULL 
	BEGIN
		RAISERROR('Specified namespace: %s does not contain the topic: %s.', 16, 1, @namespace, @topicCurrentName)
	END

	UPDATE dbo.Topic
	SET [Name] = @topicNewName
	WHERE TopicId = @topicId




GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

/****** Object:  Stored Procedure dbo.WriteTopic    Script Date: 1/30/2005 11:35:25 AM ******/

CREATE     PROCEDURE Dbo.WriteTopic
(
	@namespace NVARCHAR(255),
	@topicName NVARCHAR(255),
	@body NTEXT,
	@archive BIT,
	@lastWriteTime DATETIME
)
AS
	SET NOCOUNT ON

	DECLARE @topicId INTEGER
	DECLARE @namespaceId INTEGER

	SET @topicId = null
	SET @namespaceId = null

	SELECT @namespaceId = namespaceTable.NamespaceId
	FROM dbo.Namespace namespaceTable 
	WHERE namespaceTable.[Name] = @namespace

	SELECT @topicId = topicTable.TopicId
	FROM dbo.Namespace namespaceTable JOIN 
			dbo.Topic topicTable ON namespaceTable.NamespaceId = topicTable.NamespaceId
	WHERE namespaceTable.[Name] = @namespace
			AND topicTable.[Name] = @topicName


	IF @topicId IS NULL 
	BEGIN
		INSERT INTO dbo.Topic (NamespaceId, [Name], Archive, CreationTime, LastWriteTime, Body) 
		VALUES (@namespaceId, @topicName, @archive, @lastWriteTime, @lastWriteTime, @body)
	END
	ELSE
	BEGIN
		UPDATE dbo.Topic
		SET Body = @body, LastWriteTime = @lastWriteTime
		WHERE TopicId = @topicId
	END




GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

CREATE  PROCEDURE Dbo.GetSqlTopicInfoForNonArchiveTopicsSortedDescending
(
	@namespace NVARCHAR(255)
)
AS
	SET NOCOUNT ON

	SELECT topicTable.[Name], topicTable.LastWriteTime
	FROM dbo.Namespace namespaceTable JOIN 
			dbo.Topic topicTable ON namespaceTable.NamespaceId = topicTable.NamespaceId
	WHERE namespaceTable.[Name] = @namespace
			AND topicTable.Archive = 0
	ORDER BY LastWriteTime DESC

GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

CREATE  PROCEDURE Dbo.GetSqlTopicArchiveInfosSince
(
	@namespace NVARCHAR(255),
	@topicName NVARCHAR(255),
	@stamp DATETIME
)
AS
	SET NOCOUNT ON

	SELECT topicTable.[Name], topicTable.LastWriteTime
	FROM dbo.Namespace namespaceTable JOIN 
			dbo.Topic topicTable ON namespaceTable.NamespaceId = topicTable.NamespaceId
	WHERE namespaceTable.[Name] = @namespace
			AND topicTable.[Name] like @topicName
			AND topicTable.Archive = 1
			AND topicTable.LastWriteTime >= @stamp
	ORDER BY topicTable.LastWriteTime DESC

GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO