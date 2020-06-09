CREATE TABLE [dbo].[ExcludedObjects] (
    [OrgID]     VARCHAR (4) NOT NULL,
    [ObjectNum] VARCHAR (4) NOT NULL,
    [IsActive]  BIT         NULL,
    CONSTRAINT [PK_ExcludedObjects] PRIMARY KEY CLUSTERED ([OrgID] ASC, [ObjectNum] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'ExcludedObjects: Contains object numbers that are to be ignored in terms of reimbursement.  Note that these exclusions may not apply to accounts listed in reimbursable benefits accounts table.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ExcludedObjects';

