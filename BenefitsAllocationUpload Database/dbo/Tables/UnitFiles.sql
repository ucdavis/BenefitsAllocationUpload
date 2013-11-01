CREATE TABLE [dbo].[UnitFiles] (
    [Id]         INT          NOT NULL,
    [UnitId]     INT          NOT NULL,
    [Filename]   VARCHAR (50) NOT NULL,
    [SchoolCode] VARCHAR (2)  NULL,
    [Created]    DATETIME     NULL,
    [CreatedBy]  VARCHAR (50) NULL,
    [Uploaded]   DATETIME     NULL,
    [UploadedBy] VARCHAR (50) NULL,
    CONSTRAINT [PK_UnitFiles] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unit File Creation and Upload Details: Contains details on whom and when a file was created and subsequently uploaded.  Used by the Benefits Allocation Upload web application.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UnitFiles';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary Key: Unique Identifier for each table row.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UnitFiles', @level2type = N'COLUMN', @level2name = N'Id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Catbert Unit ID: The Unit ID of the User who created the file.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UnitFiles', @level2type = N'COLUMN', @level2name = N'UnitId';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Balance Allocation Upload File Name: The system generated filename of the file.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UnitFiles', @level2type = N'COLUMN', @level2name = N'Filename';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'PPS School Code: The normalized PPS CAES School Code or non-normalized PPS School Code, if not CAES.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UnitFiles', @level2type = N'COLUMN', @level2name = N'SchoolCode';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'File Creation Date: The timestamp the file was initially created.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UnitFiles', @level2type = N'COLUMN', @level2name = N'Created';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Created By: The UCD login ID of the user creating the file.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UnitFiles', @level2type = N'COLUMN', @level2name = N'CreatedBy';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date Uploaded: The timestamp when the file was uploaded to the AFS host.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UnitFiles', @level2type = N'COLUMN', @level2name = N'Uploaded';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Uploaded By: The UCD login ID of the user who uploaded the file.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UnitFiles', @level2type = N'COLUMN', @level2name = N'UploadedBy';

