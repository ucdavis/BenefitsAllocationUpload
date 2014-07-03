
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
-- =============================================
CREATE FUNCTION [dbo].[udf_GetBudgetAdjustmentUploadDataFromInputTableForOrg]
(
	@FiscalYear char(4) = '2013', --Fiscal Year to be processed.
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
	UNIV_FISCAL_YEAR char(4), 
	FIN_COA_CD char(2), 
	ACCOUNT_NBR char(7), 
	SUB_ACCT_NBR char(5), 
	FIN_OBJECT_CD char(4), 
	FIN_SUB_OBJ_CD char(3),
	FIN_BALANCE_TYP_CD char(2),
	FIN_OBJ_TYP_CD char(2),
	UNIV_FISCAL_PRD_CD char(2),
	FDOC_TYP_CD char(4),
	FS_ORIGIN_CD char(2),
	FDOC_NBR char(9),
	TRN_ENTR_SEQ_NBR char(5),
	TRN_LDGR_ENTR_DESC char(40),
	TRN_LDGR_ENTR_AMT char(14),
	TRN_DEBIT_CRDT_CD char(1),
	TRANSACTION_DT char(8),
	ORG_DOC_NBR char(10) ,
	PROJECT_CD char(10),
	ORG_REFERENCE_ID char(8),
	FDOC_REF_TYP_CD char(4),
	FS_REF_ORIGIN_CD char(2),
	FDOC_REF_NBR char(9),
	FDOC_REVERSAL_DT char(8),
	TRN_ENCUM_UPDT_CD char(1)
)
AS
BEGIN
	SELECT @FiscalPeriod = RTRIM(@FiscalPeriod)

DECLARE @FiscalPeriodLen int = (SELECT LEN(@FiscalPeriod))
	IF @FiscalPeriodLen < 2
		BEGIN
			SELECT @FiscalPeriod = '0' + @FiscalPeriod
		END 

SELECT @TransDescription = RTRIM(@TransDescription) 
DECLARE @DescLen int = (SELECT LEN(@TransDescription))
	IF @DescLen > 40 
		BEGIN
			-- Truncate to 40
			SELECT @TransDescription =(SUBSTRING(@TransDescription, 1, 40)) 
		END


SELECT @OrgDocNumber = RTRIM(@OrgDocNumber)
DECLARE @OrgDocNumLen int = (SELECT LEN(@OrgDocNumber))

	IF @OrgDocNumLen > 10
		BEGIN
			-- Truncate to 10
			SELECT @OrgDocNumber =(SUBSTRING(@OrgDocNumber, 1, 10)) 
		END

SELECT @OrgRefId = RTRIM(@OrgRefId)
DECLARE @OrgRefLen int = (SELECT LEN(@OrgRefId))

	IF @OrgRefLen > 8
		BEGIN
			-- Truncate to 8
			SELECT @OrgRefId =(SUBSTRING(@OrgRefId, 1, 8)) 
		END

-- The rest of these fields remain constant or predicable throughout all the BA uploads.
DECLARE 
	@TransDocNumber char(9) = @FiscalYear + @FiscalPeriod + @TransDocNumberSequence, -- Unique per file or group of transactions.
	@TransCreationDate char(8) = CONVERT(char(8),GETDATE(), 112), -- Dynamically generated for the current date
	@ObjectType char(2) = '  ', -- blank
	@TransBalanceType char(2) = 'CB', -- Code for Current Budget
	@TransDocType char(4) = 'GLCB', -- Code for General Ledger, Current Budget
	--@TransDocOrigin char(2) = 'AG', -- Unique code provided by AFS for the dean's office.
	@TransDebitCreditCode char(1) = ' ', --blank
	--@OrgDocNumber char(10) = '          ', --blank commented out to allow for Org Doc Number to be passed in.
	@RefDocType char(4) = '    ', --blank
	@RefOrigin char(2) = '  ', --blank
	@RefDocNum char(9) = '         ', --blank
	@ReverseDate char(8) = '        ', --blank
	@EncumUpdateCode char(1) = ' ' --blank

DECLARE @Table1 TABLE (Chart char(2), AccountNum char(7), SubAccount char(5), ConsolidationCode char(4), SubObject char(3), Amount numeric(18,2), ProjectCode char(10) --, FunctionCode char(1)
, CentralAccount char(7), CentralSubAccount varchar(5), CentralObjectConsolidation varchar(4), CentralSubObject varchar(3))


-- Supply this data instead as an import parameter, so the data can come from DaFIS or out local FISDataMart tables.
--INSERT INTO @Table1
--SELECT        TOP (100) PERCENT 
--	Chart AS Chart, --Automatically right pad chart as per instructions.
--	AccountNum, 
--	ISNULL(SubAccount, '-----') SubAccount, 
--	ConsolidationCode, 
--	CASE WHEN SubObject IS NULL OR SubObject LIKE '---%' THEN '---'
--	ELSE SubObject END AS SubObject, 
--	--CONVERT(char(5), rank() OVER (ORDER BY FiscalYear, Chart, AccountNum, SubAccount, ConsolidationCode,  SubObject, ProjectCode)) AS LineSequenceNum,
--	--CONVERT(char(14),(SUM(Approp) + SUM(Expend)) * - 1) AS Amount,
--	-- 2013-02-08 by kjt: Reversed the signs:
--	--(SUM(Approp) + SUM(Expend)) AS Amount,
--	((SUM(Approp) + SUM(Expend)) * -1) AS Amount,
--	ISNULL(ProjectCode, '----------') AS ProjectCode,
--	--FunctionCode,
--CASE WHEN FunctionCode = 'R' THEN 'APRRGAU' ELSE 'APRIGAU' END AS CentralAccount
--FROM            (SELECT        FiscalYear, Chart, AccountNum, SubAccount, ConsolidationCode, SubObject, ProjectCode, TransLineAmount AS Amount, AppropAmount AS Approp, 
--                                                    ExpendAmount AS Expend, CASE WHEN (TLV.HigherEdFunctionCode = 'ORES' OR
--                                                    LEFT(OPAccount, 2) BETWEEN '44' AND '59' OR
--                                                    LEFT(OPAccount, 2) IN ('62')) THEN 'R' ELSE 'I' END AS FunctionCode
--                          FROM            dbo.TransactionLogV AS TLV
--                          WHERE			(FiscalYear = @FiscalYear) AND --Include charges/expenses for the fiscal year provided.
--										(Chart = '3') AND --Include only chart 3
--										(TransBalanceType IN ('AC', 'CB')) AND --Include only current budget (CB) and actuals (balance sheet) (AC).
--										(OPFund = '19900') AND --Include only charges/expenses for state funded accounts.
--										(ConsolidationCode IN ('SB28', 'SUB6')) AND --Include only benefits related charges/expenses.
--                                        (IsCAESTrans = 1) AND --Include only AAES accounts, not those weird AAES-BIOS accounts.
--										(AccountNum NOT IN ('APRRGAU', 'APRIGAU')) --Exclude AAES provisional accounts as these are the where the benefits are funded from.
--				) AS TLV_1
--GROUP BY FiscalYear, Chart, AccountNum, SubAccount, ConsolidationCode, SubObject, ProjectCode , FunctionCode
--HAVING        (SUM(Approp) + SUM(Expend) <> 0)
--ORDER BY FiscalYear, Chart, AccountNum, SubAccount, ConsolidationCode, SubObject, ProjectCode , FunctionCode

INSERT INTO @Table1
SELECT 
	CHART_NUM AS Chart, --Automatically right pad chart as per instructions.
	ACCT_NUM AccountNum, 
	ISNULL(SUB_ACCT_NUM, '-----') SubAccount, 
	OBJ_CONSOLIDATN_NUM ConsolidationCode, 
	CASE WHEN SUB_OBJECT_NUM IS NULL OR SUB_OBJECT_NUM LIKE '---%' THEN '---'
	ELSE SUB_OBJECT_NUM END AS SubObject, 
	SUM(TRANS_LINE_AMT) AS Amount, -- Has already been summed in inner query, i.e. sproc.
	ISNULL(TRANS_LINE_PROJECT_NUM, '----------') AS ProjectCode,
	Account AS CentralAccount,
	ISNULL(SubAccount, '-----') AS CentralSubAccount,
	ISNULL(FundingObjectConsolidation, OBJ_CONSOLIDATN_NUM) AS CentralObjectConsolidation,
	ISNULL(SubObject, '---') AS CentralSubObject
FROM  @TransactionsForSummation TLV1
INNER JOIN dbo.CentralAccounts CA ON TLV1.ORG_ID = CA.OrgId AND TLV1.FUNCTION_CODE = CA.FunctionCode AND ISNULL(CA.ObjectConsolidation, TLV1.OBJ_CONSOLIDATN_NUM) = TLV1.OBJ_CONSOLIDATN_NUM 
GROUP BY CHART_NUM, ACCT_NUM, SUB_ACCT_NUM, OBJ_CONSOLIDATN_NUM, SUB_OBJECT_NUM, TRANS_LINE_PROJECT_NUM, Account, SubAccount, ObjectConsolidation, FundingObjectConsolidation, SubObject
HAVING   SUM(TRANS_LINE_AMT) <> 0
ORDER BY CHART_NUM, ACCT_NUM, SUB_ACCT_NUM, OBJ_CONSOLIDATN_NUM, SUB_OBJECT_NUM, TRANS_LINE_PROJECT_NUM, Account, SubAccount, ObjectConsolidation, SubObject

-- Insert the combined sum, 4 lines only, for the provisional account adjustments:
INSERT INTO @Table1
SELECT Chart, CentralAccount AS AccountNum, CentralSubAccount AS SubAccount, ISNULL(CentralObjectConsolidation, ConsolidationCode) AS ConsolidationCode, CentralSubObject AS SubObject, (SUM(AMOUNT) *-1) AS Amount, '----------' AS ProjectCode, CentralAccount, CentralSubAccount, ConsolidationCode CentralObjectConsolidation, CentralSubObject 
FROM @Table1
GROUP BY Chart, CentralAccount, CentralSubAccount, ConsolidationCode, CentralObjectConsolidation, CentralSubObject

INSERT INTO @TransferTable
SELECT  
	@FiscalYear FiscalYear, 
	Chart, 
	AccountNum, 
	SubAccount, 
	ConsolidationCode, 
	SubObject, 
	@TransBalanceType AS TransBalanceType,
	@ObjectType AS ObjectType,
	@FiscalPeriod AS FiscalPeriod,
	@TransDocType AS TransDocType,
	@TransDocOriginCode AS TransDocOrigin,
	@TransDocNumber AS TransDocNumber,
	(
		REPLICATE ('0', 5 - LEN(CONVERT(varchar(5), rank() OVER (ORDER BY Chart, AccountNum, SubAccount, ConsolidationCode,  SubObject, ProjectCode)))) + CONVERT(varchar(5), rank() OVER (ORDER BY Chart, AccountNum, SubAccount, ConsolidationCode,  SubObject, ProjectCode))
	) AS LineSequenceNum, -- generate a unique sequence number for each row and left pad, i.e. right justify. 
	CONVERT(char(40), @TransDescription) AS TransDescription,
    (
		REPLICATE(' ', 14 - LEN(CONVERT(varchar(14),Amount))) + CONVERT(varchar(14), Amount)
	) AS Amount, -- Left pad, i.e. right justify.
	@TransDebitCreditCode AS TransDebitCreditCode,
	@TransCreationDate AS TransCreationDate,
	CONVERT(char(10), ISNULL(@OrgDocNumber, '')) AS OrgDocNumber,
	ProjectCode,
	CONVERT(char(8), @OrgRefId) AS OrgRefId,
	@RefDocType AS RefDocType,
	@RefOrigin AS RefOrigin,
	@RefDocNum AS RefDocNum,
	@ReverseDate AS ReverseDate,
	@EncumUpdateCode AS EncumUpdateCode
FROM @Table1
	
	RETURN 
END

