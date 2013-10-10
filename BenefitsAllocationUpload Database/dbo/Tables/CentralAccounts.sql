CREATE TABLE [dbo].[CentralAccounts] (
    [OrgId]                      VARCHAR (4) NOT NULL,
    [SchoolCode]                 CHAR (2)    NOT NULL,
    [Chart]                      VARCHAR (2) NOT NULL,
    [Account]                    VARCHAR (7) NOT NULL,
    [SubAccount]                 VARCHAR (5) NULL,
    [ObjectConsolidation]        VARCHAR (4) NULL,
    [FundingObjectConsolidation] VARCHAR (4) NULL,
    [SubObject]                  VARCHAR (5) NULL,
    [FunctionCode]               VARCHAR (5) NOT NULL,
    [TransDocOriginCode]         VARCHAR (2) NULL,
    [OpFund]                     VARCHAR (5) NULL
);

