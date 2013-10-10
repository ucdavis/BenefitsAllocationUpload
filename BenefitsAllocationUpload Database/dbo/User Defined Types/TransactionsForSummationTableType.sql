CREATE TYPE [dbo].[TransactionsForSummationTableType] AS TABLE (
    [ORG_ID]                 VARCHAR (4)     NULL,
    [ORG_NAME]               VARCHAR (40)    NULL,
    [A11_ACCT_NUM]           VARCHAR (7)     NULL,
    [HIGHER_ED_FUNC_CODE]    VARCHAR (4)     NULL,
    [CHART_NUM]              VARCHAR (2)     NULL,
    [ACCT_NUM]               VARCHAR (7)     NULL,
    [SUB_ACCT_NUM]           VARCHAR (5)     NULL,
    [OBJ_CONSOLIDATN_NUM]    VARCHAR (4)     NULL,
    [OBJECT_NUM]             VARCHAR (4)     NULL,
    [SUB_OBJECT_NUM]         VARCHAR (4)     NULL,
    [TRANS_LINE_PROJECT_NUM] VARCHAR (10)    NULL,
    [TRANS_LINE_AMT]         NUMERIC (15, 2) NULL,
    [FUNCTION_CODE]          VARCHAR (5)     NULL,
    [BALANCE_TYPE_CODE]      VARCHAR (2)     NULL,
    [IS_PENDING]             BIT             NULL);

