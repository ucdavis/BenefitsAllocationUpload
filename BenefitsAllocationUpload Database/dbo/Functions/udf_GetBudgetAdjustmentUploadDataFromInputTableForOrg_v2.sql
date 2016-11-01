
-- =============================================
-- Author:		Ken Taylor
-- Create date: February 25, 2013
-- Description:	Format data provided as input table parameter for BA upload.
-- Modifications:
--	2013-02-26 by kjt:	Changed name to udf_GetBudgetAdjustmentUploadDataFromInputTable, 
--						meaning table variable supplied as input table param, fetched elsewhere.
--	2013-04-16 by kjt:	Added param for FS_ORIGIN_CD, i.e. @TransDocOriginCode, and modified logic accordingly.
--	2013-04-17 by kjt:	Removed params for @CollegeLevelOrg and @DivisionLevelOrgs as they were no longer being used.
--	2013-05-22 by kjt:	Added logic to handle null @OrgDocNumber
--	2013-06-02 by kjt	Revised logic to use central object consolidation for provision accounts if present; otherwise object consolidation code.
--	2013-07-02 by kjt:	Revised logic to use funding object consolidation for provision accounts if present; otherwise object consolidation code.
--		That way we can fund SB28 and SUB6 expenses from different objects as requested by DANR.
--	2014-06-25 by kjt:	Revised to pad LineSequenceNum with leading zeros instead of spaces.
--  2016-10-28 by kjt:	Removed all of the formatting so that it could be done in C# code, meaning with FileHelpers annotations.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetBudgetAdjustmentUploadDataFromInputTableForOrg_v2]
(
	@FiscalYear int = '2013', --Fiscal Year to be processed.
	@FiscalPeriod varchar(2) = '06', --Period in which adjustments are to be applied
	@TransDescription varchar(100) = 'BA benefits alloc for June 2012-Dec 2013', --Some kind of meaningful description. 40 characters max.
	@OrgDocNumber varchar(20) = '', -- An Organizational Document Number for all the transactions overall.  10 characters max.
	@OrgRefId varchar(10) = '01-06Ben', --An Organizational Reference ID for the transactions overall. 8 characters max.
	@TransDocNumberSequence char(3) = '001',  --Change the ### to something meaningful. A unique sequence number for the group of transactions. Must be 3 characters!
	@TransDocOriginCode char(2) = 'AG', --Unique code provided by AFS for the corresponding college's dean's office.
	@TransactionsForSummation TransactionsForSummationTableType READONLY
)
RETURNS 
@TransferTable TABLE 
(
	UNIV_FISCAL_YEAR int, 
	FIN_COA_CD varchar(2), 
	ACCOUNT_NBR varchar(7), 
	SUB_ACCT_NBR varchar(5), 
	FIN_OBJECT_CD varchar(4), 
	FIN_SUB_OBJ_CD varchar(4),
	FIN_BALANCE_TYP_CD varchar(2),
	FIN_OBJ_TYP_CD varchar(2),
	UNIV_FISCAL_PRD_CD varchar(2),
	FDOC_TYP_CD varchar(4),
	FS_ORIGIN_CD varchar(2),
	FDOC_NBR varchar(10),--varchar(9),
	TRN_ENTR_SEQ_NBR int,
	TRN_LDGR_ENTR_DESC varchar(100),--varchar(40),
	TRN_LDGR_ENTR_AMT decimal(18,2),
	TRN_DEBIT_CRDT_CD varchar(1),
	TRANSACTION_DT datetime2,
	ORG_DOC_NBR varchar(20),--varchar(10) ,
	PROJECT_CD varchar(10),
	ORG_REFERENCE_ID varchar(10),--varchar(8),
	FDOC_REF_TYP_CD varchar(4),
	FS_REF_ORIGIN_CD varchar(2),
	FDOC_REF_NBR varchar(9),
	FDOC_REVERSAL_DT varchar(8),
	TRN_ENCUM_UPDT_CD varchar(1)
)
AS
BEGIN

-- The rest of these fields remain constant or predicable throughout all the BA uploads.
DECLARE 
	@TransDocNumber varchar(9) = CONVERT(char(4),@FiscalYear) + @FiscalPeriod + @TransDocNumberSequence, -- Unique per file or group of transactions.
	@TransCreationDate datetime2 = GETDATE(), -- Not sure we'll do this here.
	@TransBalanceType varchar(2) = 'CB', -- Code for Current Budget
	@TransDocType varchar(4) = 'GLCB' -- Code for General Ledger, Current Budget

DECLARE @Table1 TABLE (Chart varchar(2), AccountNum varchar(7), SubAccount varchar(5), ConsolidationCode varchar(4), SubObject varchar(4), Amount numeric(18,2), ProjectCode varchar(10) --, FunctionCode char(1)
, CentralAccount varchar(7), CentralSubAccount varchar(5), CentralObjectConsolidation varchar(4), CentralSubObject varchar(4))

-- Supply this data instead as an import parameter, so the data can come from DaFIS or out local FISDataMart tables.

INSERT INTO @Table1
SELECT 
	CHART_NUM AS Chart, 
	ACCT_NUM AccountNum, 
	SUB_ACCT_NUM SubAccount, 
	OBJ_CONSOLIDATN_NUM ConsolidationCode, 
	SUB_OBJECT_NUM SubObject,
	SUM(TRANS_LINE_AMT) AS Amount, -- Has already been summed in inner query, i.e. sproc.
	TRANS_LINE_PROJECT_NUM  ProjectCode,
	Account AS CentralAccount,
	SubAccount CentralSubAccount,
	ISNULL(FundingObjectConsolidation, OBJ_CONSOLIDATN_NUM) AS CentralObjectConsolidation,
	SubObject AS CentralSubObject
FROM  @TransactionsForSummation TLV1
INNER JOIN dbo.CentralAccounts CA ON TLV1.ORG_ID = CA.OrgId AND TLV1.FUNCTION_CODE = CA.FunctionCode AND ISNULL(CA.ObjectConsolidation, TLV1.OBJ_CONSOLIDATN_NUM) = TLV1.OBJ_CONSOLIDATN_NUM 
GROUP BY CHART_NUM, ACCT_NUM, SUB_ACCT_NUM, OBJ_CONSOLIDATN_NUM, SUB_OBJECT_NUM, TRANS_LINE_PROJECT_NUM, Account, SubAccount, ObjectConsolidation, FundingObjectConsolidation, SubObject
HAVING   SUM(TRANS_LINE_AMT) <> 0
ORDER BY CHART_NUM, ACCT_NUM, SUB_ACCT_NUM, OBJ_CONSOLIDATN_NUM, SUB_OBJECT_NUM, TRANS_LINE_PROJECT_NUM, Account --, SubAccount --, ObjectConsolidation --, SubObject

-- Insert the combined sum, 4 lines only, for the provisional account adjustments:
INSERT INTO @Table1
SELECT Chart, CentralAccount AS AccountNum, CentralSubAccount AS SubAccount, ISNULL(CentralObjectConsolidation, ConsolidationCode) AS ConsolidationCode, CentralSubObject AS SubObject, (SUM(AMOUNT) *-1) AS Amount, 
null AS ProjectCode, CentralAccount, CentralSubAccount, ConsolidationCode CentralObjectConsolidation, CentralSubObject 
FROM @Table1
GROUP BY Chart, CentralAccount, CentralSubAccount, ConsolidationCode, CentralObjectConsolidation, CentralSubObject HAVING SUM(AMOUNT) <> 0

INSERT INTO @TransferTable
SELECT  
	@FiscalYear FiscalYear, 
	Chart, 
	AccountNum, 
	SubAccount, 
	ConsolidationCode, 
	SubObject, 
	@TransBalanceType AS TransBalanceType,
	null AS ObjectType, -- this is always blank
	@FiscalPeriod AS FiscalPeriod,
	@TransDocType AS TransDocType,
	@TransDocOriginCode AS TransDocOrigin,
	@TransDocNumber AS TransDocNumber,
	(
		rank() OVER (ORDER BY Chart, AccountNum, SubAccount, ConsolidationCode,  SubObject, ProjectCode)
	) AS LineSequenceNum, --generate a unique sequence number for each row.
	@TransDescription AS TransDescription,
	Amount,
	null AS TransDebitCreditCode, -- this is always blank
	--@TransCreationDate AS TransCreationDate, 
	null  AS TransCreationDate, -- leaving this null here will results in file helpers setting it at the program level instead of at the DB level.
	@OrgDocNumber AS OrgDocNumber,
	ProjectCode,
	@OrgRefId AS OrgRefId,
	null AS RefDocType, -- this is always blank
	null AS RefOrigin, -- this is always blank
	null AS RefDocNum, -- this is always blank
	null AS ReverseDate, -- this is always blank
	null AS EncumUpdateCode -- this is always blank
FROM @Table1
	
	RETURN 
END

