CREATE TABLE [dbo].[Command] (
    [Sequence]                BIGINT IDENTITY (1, 1) NOT NULL,
    [CommandId]               NVARCHAR (64)          NOT NULL,
    [AggregateRootTypeCode]   INT                    NOT NULL,
    [AggregateRootId]         NVARCHAR (32)          NOT NULL,
    [ProcessId]               NVARCHAR (32)          NULL,
    [Timestamp]               DATETIME               NOT NULL,
    [Payload]                 VARBINARY (MAX)        NOT NULL,
    [Items]                   VARBINARY (MAX)        NOT NULL,
    CONSTRAINT [PK_Command] PRIMARY KEY CLUSTERED ([CommandId] ASC)
)
GO
CREATE TABLE [dbo].[Event] (
    [Sequence]                BIGINT IDENTITY (1, 1) NOT NULL,
    [AggregateRootTypeCode]   INT                    NOT NULL,
    [AggregateRootId]         NVARCHAR (32)          NOT NULL,
    [Version]                 INT                    NOT NULL,
    [CommitId]                NVARCHAR (64)          NOT NULL,
    [ProcessId]               NVARCHAR (32)          NULL,
    [Timestamp]               DATETIME               NOT NULL,
    [Events]                  VARBINARY (MAX)        NOT NULL,
    [Items]                   VARBINARY (MAX)        NOT NULL,
    CONSTRAINT [PK_Event] PRIMARY KEY CLUSTERED ([AggregateRootId] ASC, [Version] ASC)
)
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