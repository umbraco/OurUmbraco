/****** Object:  Table [dbo].[umbracoNode]    Script Date: 05/15/2009 05:27:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[umbracoNode]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[umbracoNode](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[trashed] [bit] NOT NULL,
	[parentID] [int] NOT NULL,
	[nodeUser] [int] NULL,
	[level] [smallint] NOT NULL,
	[path] [nvarchar](150) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[sortOrder] [int] NOT NULL,
	[uniqueID] [uniqueidentifier] NULL,
	[text] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[nodeObjectType] [uniqueidentifier] NULL,
	[createDate] [datetime] NOT NULL,
 CONSTRAINT [PK_structure] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[umbracoNode]') AND name = N'IX_umbracoNodeObjectType')
CREATE NONCLUSTERED INDEX [IX_umbracoNodeObjectType] ON [dbo].[umbracoNode] 
(
	[nodeObjectType] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[umbracoNode]') AND name = N'IX_umbracoNodeParentId')
CREATE NONCLUSTERED INDEX [IX_umbracoNodeParentId] ON [dbo].[umbracoNode] 
(
	[parentID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  Table [dbo].[forumForums]    Script Date: 05/15/2009 05:27:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[forumForums]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[forumForums](
	[id] [int] NOT NULL,
	[latestTopic] [int] NOT NULL,
	[latestComment] [int] NOT NULL,
	[totalTopics] [int] NOT NULL,
	[totalComments] [int] NOT NULL,
	[latestAuthor] [int] NOT NULL,
	[latestPostDate] [datetime] NOT NULL,
	[sortOrder] [int] NOT NULL,
	[parentId] [int] NOT NULL,
 CONSTRAINT [PK_forumForums] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[forumTopics]    Script Date: 05/15/2009 05:27:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[forumTopics]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[forumTopics](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[parentId] [int] NOT NULL,
	[memberId] [int] NOT NULL,
	[title] [nvarchar](70) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[body] [ntext] COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[created] [datetime] NOT NULL,
	[updated] [datetime] NOT NULL,
	[locked] [bit] NOT NULL,
	[latestReplyAuthor] [int] NOT NULL,
	[isSpam] [bit] NULL
 CONSTRAINT [PK_forumTopics] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[forumComments]    Script Date: 05/15/2009 05:27:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[forumComments]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[forumComments](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[topicId] [int] NOT NULL,
	[memberId] [int] NOT NULL,
	[body] [ntext] COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[created] [datetime] NOT NULL,
	[isSpam] [bit] NULL
 CONSTRAINT [PK_forumComments] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Default [DF_forumTopics_created]    Script Date: 05/15/2009 05:27:55 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_forumTopics_created]') AND parent_object_id = OBJECT_ID(N'[dbo].[forumTopics]'))
Begin
ALTER TABLE [dbo].[forumTopics] ADD  CONSTRAINT [DF_forumTopics_created]  DEFAULT (getdate()) FOR [created]

End
GO
/****** Object:  Default [DF_forumTopics_updated]    Script Date: 05/15/2009 05:27:55 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_forumTopics_updated]') AND parent_object_id = OBJECT_ID(N'[dbo].[forumTopics]'))
Begin
ALTER TABLE [dbo].[forumTopics] ADD  CONSTRAINT [DF_forumTopics_updated]  DEFAULT (getdate()) FOR [updated]

End
GO
/****** Object:  Default [DF_forumTopics_locked]    Script Date: 05/15/2009 05:27:55 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_forumTopics_locked]') AND parent_object_id = OBJECT_ID(N'[dbo].[forumTopics]'))
Begin
ALTER TABLE [dbo].[forumTopics] ADD  CONSTRAINT [DF_forumTopics_locked]  DEFAULT ((0)) FOR [locked]

End
GO
/****** Object:  Default [DF_forumTopics_latestReplyAuthor]    Script Date: 05/15/2009 05:27:55 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_forumTopics_latestReplyAuthor]') AND parent_object_id = OBJECT_ID(N'[dbo].[forumTopics]'))
Begin
ALTER TABLE [dbo].[forumTopics] ADD  CONSTRAINT [DF_forumTopics_latestReplyAuthor]  DEFAULT ((0)) FOR [latestReplyAuthor]

End
GO
/****** Object:  Default [DF_forumForums_latestTopic]    Script Date: 05/15/2009 05:27:55 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_forumForums_latestTopic]') AND parent_object_id = OBJECT_ID(N'[dbo].[forumForums]'))
Begin
ALTER TABLE [dbo].[forumForums] ADD  CONSTRAINT [DF_forumForums_latestTopic]  DEFAULT ((0)) FOR [latestTopic]

End
GO
/****** Object:  Default [DF_forumForums_latestComment]    Script Date: 05/15/2009 05:27:55 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_forumForums_latestComment]') AND parent_object_id = OBJECT_ID(N'[dbo].[forumForums]'))
Begin
ALTER TABLE [dbo].[forumForums] ADD  CONSTRAINT [DF_forumForums_latestComment]  DEFAULT ((0)) FOR [latestComment]

End
GO
/****** Object:  Default [DF_forumForums_totalTopics]    Script Date: 05/15/2009 05:27:55 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_forumForums_totalTopics]') AND parent_object_id = OBJECT_ID(N'[dbo].[forumForums]'))
Begin
ALTER TABLE [dbo].[forumForums] ADD  CONSTRAINT [DF_forumForums_totalTopics]  DEFAULT ((0)) FOR [totalTopics]

End
GO
/****** Object:  Default [DF_forumForums_totalComments]    Script Date: 05/15/2009 05:27:55 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_forumForums_totalComments]') AND parent_object_id = OBJECT_ID(N'[dbo].[forumForums]'))
Begin
ALTER TABLE [dbo].[forumForums] ADD  CONSTRAINT [DF_forumForums_totalComments]  DEFAULT ((0)) FOR [totalComments]

End
GO
/****** Object:  Default [DF_forumForums_latestAuthor]    Script Date: 05/15/2009 05:27:55 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_forumForums_latestAuthor]') AND parent_object_id = OBJECT_ID(N'[dbo].[forumForums]'))
Begin
ALTER TABLE [dbo].[forumForums] ADD  CONSTRAINT [DF_forumForums_latestAuthor]  DEFAULT ((0)) FOR [latestAuthor]

End
GO
/****** Object:  Default [DF_forumForums_latestPostDate]    Script Date: 05/15/2009 05:27:55 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_forumForums_latestPostDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[forumForums]'))
Begin
ALTER TABLE [dbo].[forumForums] ADD  CONSTRAINT [DF_forumForums_latestPostDate]  DEFAULT (getdate()) FOR [latestPostDate]

End
GO
/****** Object:  Default [DF_forumForums_sortOrder]    Script Date: 05/15/2009 05:27:55 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_forumForums_sortOrder]') AND parent_object_id = OBJECT_ID(N'[dbo].[forumForums]'))
Begin
ALTER TABLE [dbo].[forumForums] ADD  CONSTRAINT [DF_forumForums_sortOrder]  DEFAULT ((0)) FOR [sortOrder]

End
GO
/****** Object:  Default [DF_forumComments_created]    Script Date: 05/15/2009 05:27:55 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_forumComments_created]') AND parent_object_id = OBJECT_ID(N'[dbo].[forumComments]'))
Begin
ALTER TABLE [dbo].[forumComments] ADD  CONSTRAINT [DF_forumComments_created]  DEFAULT (getdate()) FOR [created]

End
GO
/****** Object:  Default [DF_umbracoNode_trashed]    Script Date: 05/15/2009 05:27:55 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_umbracoNode_trashed]') AND parent_object_id = OBJECT_ID(N'[dbo].[umbracoNode]'))
Begin
ALTER TABLE [dbo].[umbracoNode] ADD  CONSTRAINT [DF_umbracoNode_trashed]  DEFAULT ((0)) FOR [trashed]

End
GO
/****** Object:  Default [DF_umbracoNode_createDate]    Script Date: 05/15/2009 05:27:55 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_umbracoNode_createDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[umbracoNode]'))
Begin
ALTER TABLE [dbo].[umbracoNode] ADD  CONSTRAINT [DF_umbracoNode_createDate]  DEFAULT (getdate()) FOR [createDate]

End
GO
/****** Object:  ForeignKey [FK_forumTopics_umbracoNode1]    Script Date: 05/15/2009 05:27:55 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_forumTopics_umbracoNode1]') AND parent_object_id = OBJECT_ID(N'[dbo].[forumTopics]'))
ALTER TABLE [dbo].[forumTopics]  WITH CHECK ADD  CONSTRAINT [FK_forumTopics_umbracoNode1] FOREIGN KEY([parentId])
REFERENCES [dbo].[umbracoNode] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[forumTopics] CHECK CONSTRAINT [FK_forumTopics_umbracoNode1]
GO
/****** Object:  ForeignKey [FK_forumComments_forumTopics]    Script Date: 05/15/2009 05:27:55 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_forumComments_forumTopics]') AND parent_object_id = OBJECT_ID(N'[dbo].[forumComments]'))
ALTER TABLE [dbo].[forumComments]  WITH CHECK ADD  CONSTRAINT [FK_forumComments_forumTopics] FOREIGN KEY([topicId])
REFERENCES [dbo].[forumTopics] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[forumComments] CHECK CONSTRAINT [FK_forumComments_forumTopics]
GO
/****** Object:  ForeignKey [FK_umbracoNode_umbracoNode]    Script Date: 05/15/2009 05:27:55 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_umbracoNode_umbracoNode]') AND parent_object_id = OBJECT_ID(N'[dbo].[umbracoNode]'))
ALTER TABLE [dbo].[umbracoNode]  WITH CHECK ADD  CONSTRAINT [FK_umbracoNode_umbracoNode] FOREIGN KEY([parentID])
REFERENCES [dbo].[umbracoNode] ([id])
GO
ALTER TABLE [dbo].[umbracoNode] CHECK CONSTRAINT [FK_umbracoNode_umbracoNode]
GO
CREATE TABLE [dbo].[cmsTagToTopic](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[tagId] [int] NOT NULL,
	[topicId] [int] NOT NULL,
	[weight] [float] NOT NULL,
 CONSTRAINT [PK_cmsTagToTopic] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO