CREATE TABLE [dbo].[Events] (
    [Sequence]                BIGINT          IDENTITY (1, 1) NOT NULL,
    [AggregateRootTypeCode]   INT  NOT NULL,
    [AggregateRootId]         NVARCHAR (36)   NOT NULL,
    [Version]                 INT             NOT NULL,
    [CommitId]                NVARCHAR (36)   NOT NULL,
    [Timestamp]               DATETIME        NOT NULL,
    [Events]                  VARBINARY (MAX) NOT NULL,
    CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED ([Sequence] ASC)
)
GO
CREATE UNIQUE INDEX [IX_Events_VersonIndex] ON [dbo].[Events] ([AggregateRootId], [Version])
GO
CREATE UNIQUE INDEX [IX_Events_CommitIndex] ON [dbo].[Events] ([AggregateRootId], [CommitId])
GO

CREATE TABLE [dbo].[EventPublishInfo] (
    [AggregateRootId]  NVARCHAR (36) NOT NULL,
    [PublishedVersion] INT           NOT NULL,
    CONSTRAINT [PK_EventPublishInfo] PRIMARY KEY CLUSTERED ([AggregateRootId] ASC)
)
GO
CREATE TABLE [dbo].[EventHandleInfo] (
    [EventId]              NVARCHAR (36)  NOT NULL,
    [EventHandlerTypeCode] INT NOT NULL,
    CONSTRAINT [PK_EventHandleInfo] PRIMARY KEY CLUSTERED ([EventId] ASC, [EventHandlerTypeCode] ASC)
)
GO
CREATE TABLE [dbo].[Snapshot] (
    [AggregateRootId]        NVARCHAR (36)   NOT NULL,
    [Version]                INT             NOT NULL,
    [AggregateRootTypeCode]  INT  NOT NULL,
    [Payload]                VARBINARY (MAX) NOT NULL,
    [Timestamp]              DATETIME        NOT NULL,
    CONSTRAINT [PK_Snapshot] PRIMARY KEY CLUSTERED ([AggregateRootId] ASC, [Version] ASC)
)
GO