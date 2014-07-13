CREATE TABLE [dbo].[SectionIndex] (
    [IndexId]        NVARCHAR (32)           NOT NULL,
    [SectionId]      NVARCHAR (32)           NOT NULL,
    [SectionName]    NVARCHAR (64)           NOT NULL,
    CONSTRAINT [PK_SectionIndex] PRIMARY KEY CLUSTERED ([IndexId] ASC)
)
GO
CREATE TABLE [dbo].[Lock] (
    [LockKey]                NVARCHAR (64)           NOT NULL,
    CONSTRAINT [PK_Lock] PRIMARY KEY CLUSTERED ([LockKey] ASC)
)
GO