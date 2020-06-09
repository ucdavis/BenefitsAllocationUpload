CREATE TABLE [dbo].[CentralAccounts] (
    [PK_CentralAccounts]         AS          (((((((((((((((((((([OrgId]+'|')+[SchoolCode])+'|')+[Chart])+'|')+[Account])+'|')+isnull([SubAccount],''))+'|')+isnull([ObjectConsolidation],''))+'|')+isnull([FundingObjectConsolidation],''))+'|')+isnull([SubObject],''))+'|')+[FunctionCode])+'|')+isnull([TransDocOriginCode],''))+'|')+isnull([OpFund],'')) PERSISTED NOT NULL,
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
    [OpFund]                     VARCHAR (5) NULL,
    [IsActive]                   BIT         CONSTRAINT [DF_CentralAccounts_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_CentralAccounts] PRIMARY KEY CLUSTERED ([PK_CentralAccounts] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [CentralAccounts_OrgIdSchCdChartAcctSubAcctObjConsFundingObjConsSubObjFunctCdTransDocOrignCd]
    ON [dbo].[CentralAccounts]([OrgId] ASC, [SchoolCode] ASC, [Chart] ASC, [Account] ASC, [SubAccount] ASC, [ObjectConsolidation] ASC, [FundingObjectConsolidation] ASC, [SubObject] ASC, [FunctionCode] ASC, [TransDocOriginCode] ASC);

