CREATE TABLE [dbo].[ReimbursableBenefitsAccounts] (
    [OrgID]    VARCHAR (4) NOT NULL,
    [Chart]    VARCHAR (2) NOT NULL,
    [Account]  VARCHAR (7) NOT NULL,
    [IsActive] BIT         NULL,
    CONSTRAINT [PK_ReimbursedBenefitsAccounts] PRIMARY KEY CLUSTERED ([Chart] ASC, [Account] ASC, [OrgID] ASC)
);

