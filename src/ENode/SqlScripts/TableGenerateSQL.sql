CREATE TABLE [dbo].[EventStream](
    [Id] [uniqueidentifier] NOT NULL,
    [AggregateRootId] [nvarchar](36) NOT NULL,
    [Version] [bigint] NOT NULL,
    [AggregateRootName] [nvarchar](128) NOT NULL,
    [CommandId] [uniqueidentifier] NOT NULL,
    [Timestamp] [datetime] NOT NULL,
    [Events] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_EventStream] PRIMARY KEY CLUSTERED
(
    [AggregateRootId] ASC,
    [Version] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[MessageQueue](
    [Id]          INT IDENTITY (1, 1) NOT NULL,
    [MessageId]   UNIQUEIDENTIFIER NOT NULL,
    [MessageData] VARBINARY (MAX)  NOT NULL,
 CONSTRAINT [PK_MessageQueue] PRIMARY KEY CLUSTERED
(
    [Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[EventPublishInfo](
    [AggregateRootId] [nvarchar](36) NOT NULL,
    [PublishedEventStreamVersion] [bigint] NOT NULL,
 CONSTRAINT [PK_EventPublishInfo] PRIMARY KEY CLUSTERED
(
    [AggregateRootId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[EventHandleInfo](
    [EventId] [nvarchar](36) NOT NULL,
    [EventHandlerTypeName] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_EventHandleInfo] PRIMARY KEY CLUSTERED
(
    [EventId] ASC,
    [EventHandlerTypeName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Snapshot](
    [AggregateRootId] [nvarchar](36) NOT NULL,
    [AggregateRootName] [nvarchar](128) NOT NULL,
    [StreamVersion] [bigint] NOT NULL,
    [Payload] [nvarchar](max) NOT NULL,
    [Timestamp] [datetime] NOT NULL,
 CONSTRAINT [PK_Snapshot] PRIMARY KEY CLUSTERED
(
    [AggregateRootId] ASC,
    [StreamVersion] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO