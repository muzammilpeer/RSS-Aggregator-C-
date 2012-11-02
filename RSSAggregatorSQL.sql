
CREATE TABLE [dbo].[Channel](
	[ChannelID] [int] IDENTITY(1,1) NOT NULL,
	[ChannelName] [varchar](200) NOT NULL,
	[Link] [varchar](300) NOT NULL,
	[LastUpdated] [datetime] NULL,
 CONSTRAINT [PK_Channel] PRIMARY KEY CLUSTERED 
(
	[ChannelID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[RSSItem]    Script Date: 03/29/2011 10:36:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[RSSItem](
	[RSSItemID] [varchar](300) NOT NULL,
	[ChannelID] [int] NULL,
	[Title] [varchar](300) NOT NULL,
	[Description] [varchar](max) NOT NULL,
	[Link] [varchar](300) NOT NULL,
	[pubDate] [datetime] NOT NULL,
 CONSTRAINT [PK_RSSItem] PRIMARY KEY CLUSTERED 
(
	[RSSItemID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  ForeignKey [FK_RSSItem_Channel]    Script Date: 03/29/2011 10:36:30 ******/
ALTER TABLE [dbo].[RSSItem]  WITH CHECK ADD  CONSTRAINT [FK_RSSItem_Channel] FOREIGN KEY([ChannelID])
REFERENCES [dbo].[Channel] ([ChannelID])
GO
ALTER TABLE [dbo].[RSSItem] CHECK CONSTRAINT [FK_RSSItem_Channel]
GO
