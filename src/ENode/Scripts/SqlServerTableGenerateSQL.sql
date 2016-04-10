CREATE DATABASE [ENode]
GO
USE [ENode]
GO
CREATE TABLE [dbo].[Command] (
    [Sequence]                BIGINT IDENTITY (1, 1) NOT NULL,
    [CommandId]               NVARCHAR (36)          NOT NULL,
    [CreatedOn]               DATETIME               NOT NULL,
    [AggregateRootId]         NVARCHAR (36)          NULL,
    [MessagePayload]          NVARCHAR (MAX)         NULL,
    [MessageTypeName]         NVARCHAR (256)         NULL,
    CONSTRAINT [PK_Command] PRIMARY KEY CLUSTERED ([Sequence] ASC)
)
GO
CREATE UNIQUE INDEX [IX_Command_CommandId] ON [dbo].[Command] ([CommandId] ASC)
GO

CREATE TABLE [dbo].[EventStream] (
    [Sequence]              BIGINT IDENTITY (1, 1) NOT NULL,
    [AggregateRootTypeName] NVARCHAR (256)         NOT NULL,
    [AggregateRootId]       NVARCHAR (36)          NOT NULL,
    [Version]               INT                    NOT NULL,
    [CommandId]             NVARCHAR (36)          NOT NULL,
    [CreatedOn]             DATETIME               NOT NULL,
    [Events]                NVARCHAR (MAX)         NOT NULL,
    CONSTRAINT [PK_EventStream] PRIMARY KEY CLUSTERED ([Sequence] ASC)
)
GO
CREATE UNIQUE INDEX [IX_EventStream_AggId_Version]   ON [dbo].[EventStream] ([AggregateRootId] ASC, [Version] ASC)
GO
CREATE UNIQUE INDEX [IX_EventStream_AggId_CommandId] ON [dbo].[EventStream] ([AggregateRootId] ASC, [CommandId] ASC)
GO

CREATE TABLE [dbo].[PublishedVersion] (
    [Sequence]                BIGINT IDENTITY (1, 1) NOT NULL,
    [ProcessorName]           NVARCHAR (128)         NOT NULL,
    [AggregateRootTypeName]   NVARCHAR (256)         NOT NULL,
    [AggregateRootId]         NVARCHAR (36)          NOT NULL,
    [Version]                 INT                    NOT NULL,
    [CreatedOn]               DATETIME               NOT NULL,
    CONSTRAINT [PK_PublishedVersion] PRIMARY KEY CLUSTERED ([Sequence] ASC)
)
GO
CREATE UNIQUE INDEX [IX_PublishedVersion_AggId_Version]   ON [dbo].[PublishedVersion] ([ProcessorName] ASC, [AggregateRootId] ASC, [Version] ASC)
GO

CREATE TABLE [dbo].[LockKey] (
    [Name]                   NVARCHAR (128)          NOT NULL,
    CONSTRAINT [PK_LockKey] PRIMARY KEY CLUSTERED ([Name] ASC)
)
GO