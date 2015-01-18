CREATE TABLE [dbo].[Command] (
    [Sequence]                BIGINT IDENTITY (1, 1) NOT NULL,
    [CommandId]               NVARCHAR (128)         NOT NULL,
    [CommandTypeCode]         INT                    NOT NULL,
    [AggregateRootTypeCode]   INT                    NOT NULL,
    [AggregateRootId]         NVARCHAR (36)          NULL,
    [SourceId]                NVARCHAR (36)          NULL,
    [SourceType]              NVARCHAR (36)          NULL,
    [Timestamp]               DATETIME               NOT NULL,
    [CommandData]             NVARCHAR (MAX)         NOT NULL,
    [Events]                  NVARCHAR (MAX)         NULL,
    CONSTRAINT [PK_Command] PRIMARY KEY CLUSTERED ([CommandId] ASC)
)
GO
CREATE TABLE [dbo].[EventStream] (
    [Sequence]                BIGINT IDENTITY (1, 1) NOT NULL,
    [AggregateRootTypeCode]   INT                    NOT NULL,
    [AggregateRootId]         NVARCHAR (36)          NOT NULL,
    [Version]                 INT                    NOT NULL,
    [CommandId]               NVARCHAR (128)         NOT NULL,
    [Timestamp]               DATETIME               NOT NULL,
    [Events]                  NVARCHAR (MAX)         NOT NULL,
    CONSTRAINT [PK_EventStream] PRIMARY KEY CLUSTERED ([AggregateRootId] ASC, [Version] ASC)
)
GO
CREATE TABLE [dbo].[AggregatePublishVersion] (
    [Sequence]                BIGINT IDENTITY (1, 1) NOT NULL,
    [EventProcessorName]      NVARCHAR (128)         NOT NULL,
    [AggregateRootId]         NVARCHAR (36)          NOT NULL,
    [PublishedVersion]        INT                    NOT NULL,
    CONSTRAINT [PK_AggregatePublishVersion] PRIMARY KEY CLUSTERED ([EventProcessorName] ASC, [AggregateRootId] ASC)
)
GO
CREATE TABLE [dbo].[MessageHandleRecord] (
    [Sequence]                  BIGINT IDENTITY (1, 1) NOT NULL,
    [MessageId]                 NVARCHAR (36)          NOT NULL,
    [HandlerTypeCode]           INT                    NOT NULL,
    [MessageTypeCode]           INT                    NOT NULL,
    [Type]                      INT                    NOT NULL,
    [AggregateRootId]           NVARCHAR (36)          NULL,
    [AggregateRootVersion]      INT                    NULL,
    CONSTRAINT [PK_MessageHandleRecord] PRIMARY KEY CLUSTERED ([MessageId] ASC, [HandlerTypeCode] ASC)
)
GO
CREATE TABLE [dbo].[Snapshot] (
    [Sequence]               BIGINT IDENTITY (1, 1)  NOT NULL,
    [AggregateRootId]        NVARCHAR (36)           NOT NULL,
    [Version]                INT                     NOT NULL,
    [AggregateRootTypeCode]  INT                     NOT NULL,
    [Payload]                VARBINARY (MAX)         NOT NULL,
    [Timestamp]              DATETIME                NOT NULL,
    CONSTRAINT [PK_Snapshot] PRIMARY KEY CLUSTERED ([AggregateRootId] ASC, [Version] ASC)
)
GO
CREATE TABLE [dbo].[Lock] (
    [LockKey]                NVARCHAR (128)          NOT NULL,
    CONSTRAINT [PK_Lock] PRIMARY KEY CLUSTERED ([LockKey] ASC)
)
GO