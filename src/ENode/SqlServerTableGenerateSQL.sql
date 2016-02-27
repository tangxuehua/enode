CREATE TABLE [dbo].[Command] (
    [Sequence]                BIGINT IDENTITY (1, 1) NOT NULL,
    [CommandId]               NVARCHAR (36)          NOT NULL,
    [AggregateRootId]         NVARCHAR (36)          NULL,
    [MessagePayload]          NVARCHAR (MAX)         NULL,
    [MessageTypeName]         NVARCHAR (256)         NOT NULL,
    [CreatedOn]               DATETIME               NOT NULL,
    CONSTRAINT [PK_Command] PRIMARY KEY CLUSTERED ([CommandId] ASC)
)
GO
CREATE TABLE [dbo].[EventStream] (
    [Sequence]              BIGINT         IDENTITY (1, 1) NOT NULL,
    [AggregateRootTypeName] NVARCHAR (256) NOT NULL,
    [AggregateRootId]       NVARCHAR (36)  NOT NULL,
    [Version]               INT            NOT NULL,
    [CommandId]             NVARCHAR (36)  NOT NULL,
    [CreatedOn]             DATETIME       NOT NULL,
    [Events]                NVARCHAR (MAX) NOT NULL, 
    CONSTRAINT [PK_EventStream] PRIMARY KEY ([Sequence])
)
GO
CREATE UNIQUE INDEX [IX_EventStream_AggId_Version]   ON [dbo].[EventStream] ([AggregateRootId], [Version])
GO
CREATE UNIQUE INDEX [IX_EventStream_AggId_CommandId] ON [dbo].[EventStream] ([AggregateRootId], [CommandId])
GO
CREATE TABLE [dbo].[SequenceMessagePublishedVersion] (
    [Sequence]                BIGINT IDENTITY (1, 1) NOT NULL,
    [ProcessorName]           NVARCHAR (128)         NOT NULL,
    [AggregateRootTypeName]   NVARCHAR (256)         NOT NULL,
    [AggregateRootId]         NVARCHAR (36)          NOT NULL,
    [PublishedVersion]        INT                    NOT NULL,
    [CreatedOn]               DATETIME               NOT NULL,
    CONSTRAINT [PK_SequenceMessagePublishedVersion] PRIMARY KEY CLUSTERED ([ProcessorName] ASC, [AggregateRootId] ASC, [PublishedVersion] ASC)
)
GO
CREATE TABLE [dbo].[MessageHandleRecord] (
    [Sequence]                  BIGINT IDENTITY (1, 1) NOT NULL,
    [MessageId]                 NVARCHAR (36)          NOT NULL,
    [HandlerTypeName]           NVARCHAR (256)         NOT NULL,
    [MessageTypeName]           NVARCHAR (256)         NOT NULL,
    [AggregateRootTypeName]     NVARCHAR (256)         NULL,
    [AggregateRootId]           NVARCHAR (36)          NULL,
    [Version]                   INT                    NULL,
    [CreatedOn]                 DATETIME               NOT NULL,
    CONSTRAINT [PK_MessageHandleRecord] PRIMARY KEY CLUSTERED ([MessageId] ASC, [HandlerTypeName] ASC)
)
GO
CREATE TABLE [dbo].[TwoMessageHandleRecord] (
    [Sequence]                  BIGINT IDENTITY (1, 1) NOT NULL,
    [MessageId1]                NVARCHAR (36)          NOT NULL,
    [MessageId2]                NVARCHAR (36)          NOT NULL,
    [HandlerTypeName]           NVARCHAR (256)         NOT NULL,
    [Message1TypeName]          NVARCHAR (256)         NOT NULL,
    [Message2TypeName]          NVARCHAR (256)         NOT NULL,
    [AggregateRootTypeName]     NVARCHAR (256)         NULL,
    [AggregateRootId]           NVARCHAR (36)          NULL,
    [Version]                   INT                    NULL,
    [CreatedOn]                 DATETIME               NOT NULL,
    CONSTRAINT [PK_TwoMessageHandleRecord] PRIMARY KEY CLUSTERED ([MessageId1] ASC, [MessageId2] ASC, [HandlerTypeName] ASC)
)
GO
CREATE TABLE [dbo].[ThreeMessageHandleRecord] (
    [Sequence]                  BIGINT IDENTITY (1, 1) NOT NULL,
    [MessageId1]                NVARCHAR (36)          NOT NULL,
    [MessageId2]                NVARCHAR (36)          NOT NULL,
    [MessageId3]                NVARCHAR (36)          NOT NULL,
    [HandlerTypeName]           NVARCHAR (256)         NOT NULL,
    [Message1TypeName]          NVARCHAR (256)         NOT NULL,
    [Message2TypeName]          NVARCHAR (256)         NOT NULL,
    [Message3TypeName]          NVARCHAR (256)         NOT NULL,
    [AggregateRootTypeName]     NVARCHAR (256)         NULL,
    [AggregateRootId]           NVARCHAR (36)          NULL,
    [Version]                   INT                    NULL,
    [CreatedOn]                 DATETIME               NOT NULL,
    CONSTRAINT [PK_ThreeMessageHandleRecord] PRIMARY KEY CLUSTERED ([MessageId1] ASC, [MessageId2] ASC, [MessageId3] ASC, [HandlerTypeName] ASC)
)
GO
CREATE TABLE [dbo].[LockKey] (
    [Name]                   NVARCHAR (128)          NOT NULL,
    CONSTRAINT [PK_LockKey] PRIMARY KEY CLUSTERED ([Name] ASC)
)
GO