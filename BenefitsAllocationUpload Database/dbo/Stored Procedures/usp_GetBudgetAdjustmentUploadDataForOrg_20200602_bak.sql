
-- =============================================
-- Author:		Ken Taylor
-- Create date: April 17, 2013
-- Description:	Selects data from either our own local FISDataMart or campus data warehouse,
-- i.e. DaFIS, and inserts into a table parameter that is then passed along to
-- udf_GetBudgetAdjustmentUploadDataFromInputTable as an input parameter, which finishes processing,
-- and formats it for uploading as a flat file, Budget Adjustment (BA) scrubber document.
--
-- Usage:
/*
-- For AAES Using CA&ES Local FIS DataMart:
USE BenefitsAllocationUpload
GO

EXEC [dbo].[usp_GetBudgetAdjustmentUploadDataForOrg] 
	@FiscalYear = '2013',
	@FiscalPeriod = '06', --Period in which adjustments are to be applied
	@TransDescription = 'BA benefits alloc for June 2012-Dec 2013', --Some kind of meaningful description. 40 characters max.
	@OrgDocNumber = '', --Optional: An Organizational Document Number for all the transactions overall.  10 characters max.
	@OrgRefId = '01-06Ben', --An Organizational Reference ID for the transactions overall. 8 characters max.
	@TransDocNumberSequence = '001',  --Change the ### to something meaningful. A unique sequence number for the group of transactions. Must be 3 characters!
	@OrgId = 'AAES', --Kuali level 4 Org ID (College level) or- Kuali level 5 Org Id (Division level), i.e. 'HACS' or 'MPSC' or 'SSCI', etc
	@TransDocOriginCode = 'AG', --Unique code provided by AFS for the corresponding college's dean's office.
	@UseDaFIS = 0, -- Set this to 1 for any org other than AAES or if you want to get the data directly from DaFIS instead of our local FISDataMart.
	@PrintGrandTotalOnly = 0, -- Set this to 1 if you want to run a sanity check and print the grand total only.
	@IsDebug = 0 --Set to 1 to print SQL generated only, and not execute.
GO

-- For AAES Using Campus Data Warehouse:
USE BenefitsAllocationUpload
GO

EXEC [dbo].[usp_GetBudgetAdjustmentUploadDataForOrg] 
	@FiscalYear = '2013',
	@FiscalPeriod = '06', --Period in which adjustments are to be applied
	@TransDescription = 'BA benefits alloc for June 2012-Dec 2013', --Some kind of meaningful description. 40 characters max.
	@OrgDocNumber = '', --Optional: An Organizational Document Number for all the transactions overall.  10 characters max.
	@OrgRefId = '01-06Ben', --An Organizational Reference ID for the transactions overall. 8 characters max.
	@TransDocNumberSequence = '001',  --Change the ### to something meaningful. A unique sequence number for the group of transactions. Must be 3 characters!
	@OrgId = 'AAES', --Kuali level 4 Org ID (College level) or- Kuali level 5 Org Id (Division level), i.e. 'HACS' or 'MPSC' or 'SSCI', etc
	@TransDocOriginCode = 'AG', --Unique code provided by AFS for the corresponding college's dean's office.
	@UseDaFIS = 1, -- Set this to 0 for any org other than AAES or if you want to get the data directly from DaFIS instead of our local FISDataMart.
	@PrintGrandTotalOnly = 0, -- Set this to 1 if you want to run a sanity check and print the grand total only.
	@IsDebug = 0 --Set to 1 to print SQL generated only, and not execute.
GO

---- For SSCI Using Campus Data Warehouse, since our local CA&ES Datamart does not contain any of their data:
USE BenefitsAllocationUpload
GO

EXEC [dbo].[usp_GetBudgetAdjustmentUploadDataForOrg] 
	@FiscalYear = '2013',
	@FiscalPeriod = '06', --Period in which adjustments are to be applied
	@TransDescription = 'BA benefits alloc for Jul 2012-Jun 2013', --Some kind of meaningful description. 40 characters max.
	@OrgDocNumber = '', --Optional: An Organizational Document Number for all the transactions overall.  10 characters max.
	@OrgRefId = '01-12Ben', --An Organizational Reference ID for the transactions overall. 8 characters max.
	@TransDocNumberSequence = '001',  --Change the ### to something meaningful. A unique sequence number for the group of transactions. Must be 3 characters!
	@OrgId = 'SSCI', --Kuali level 4 Org ID (College level) or- Kuali level 5 Org Id (Division level), i.e. 'HACS' or 'MPSC' or 'SSCI', etc
	@TransDocOriginCode = 'SS', --Unique code provided by AFS for the corresponding college's dean's office.
	@UseDaFIS = 1, -- Set this to 0 for any org other than AAES or if you want to get the data directly from DaFIS instead of our local FISDataMart.
	@PrintGrandTotalOnly = 0, -- Set this to 1 if you want to run a sanity check and print the grand total only.
	@IsDebug = 0 --Set to 1 to print SQL generated only, and not execute.
GO

-- For DANR Using Campus Data Warehouse, since our local CA&ES Datamart does not contain any of their data:
USE BenefitsAllocationUpload
GO

EXEC [dbo].[usp_GetBudgetAdjustmentUploadDataForOrg] 
	@FiscalYear = '2013',
	@FiscalPeriod = '06', --Period in which adjustments are to be applied
	@TransDescription = 'BA benefits alloc for June 2012-Dec 2013', --Some kind of meaningful description. 40 characters max.
	@OrgDocNumber = '', --Optional: An Organizational Document Number for all the transactions overall.  10 characters max.
	@OrgRefId = '01-06Ben', --An Organizational Reference ID for the transactions overall. 8 characters max.
	@TransDocNumberSequence = '001',  --Change the ### to something meaningful. A unique sequence number for the group of transactions. Must be 3 characters!
	@OrgId = 'DANR', --Kuali level 4 Org ID (College level) or- Kuali level 5 Org Id (Division level), i.e. 'HACS' or 'MPSC' or 'SSCI', etc
	@TransDocOriginCode = 'DA', --Unique code provided by AFS for the corresponding college's dean's office.
	@UseDaFIS = 1, -- Set this to 0 for any org other than AAES or if you want to get the data directly from DaFIS instead of our local FISDataMart.
	@PrintGrandTotalOnly = 0, -- Set this to 1 if you want to run a sanity check and print the grand total only.
	@IsDebug = 0 --Set to 1 to print SQL generated only, and not execute.
GO
*/
-- Modifications:
--	2013-04-15 by kjt: Added missing @OrgDocNumber to debug statement printing. 
--		Removed commented out columns from OPENQUERY's GROUP BY statement as theses were causing catastrophic failures.
--	2013-04-17 by kjt: Added logic to fetch @TransDocOriginCode if not provided.
--	2013-05-21 by kjt: Revised logic to filter out expired accounts, campus reimbursed accounts and non-resident tuition as per Tom Kaiser.
--	2013-05-24 by kjt: Added logic for included accounts if present in dbo.ReimbursableBenefitsAccounts
--  2013-05-28 by kjt: Revised logic to use period associated with expense/allocation instead of current fiscal period.
--  2013-06-04 by kjt: Revised logic to also be used by DANR for University Chart L accounts.
--	2013-06-11 by kjt: Revised local FISDataMart logic to also include missing "AG" transactions for accounts that a no longer in our college.
--	2013-06-13 by kjt: Revised logic yet again to use similar approach as downloading of transactions because other approach took too long to run.
--  2013-06-17 by kjt: Revised logic to optionally include @ExcludeObjectsString as DANR is going to fund every object.
--	2013-06-20 by kjt: Revised logic to pull latest period's account expiration date for expired date portion of where clause.  
--	2013-06-21 by kjt: Revised local FISDataMart logic to use @ExcludeObjectsString instead of hardcoding. 
--		Note: Using logic very similar to expired accounts query in udf_GetExpiredAccountsForOrg.
--	2014-08-20 by kjt: Revised logic to perform the filtering after returning the results from DaFIS since
--		we're limited to an 8,000 character restriction, and HACS had nearly 12,000 just for their included
--		accounts list, which understandably failed.
--	2018-10-28 by kjt: Revised to use udf_GetBudgetAdjustmentUploadDataFromInputTableForOrg_v2.
CREATE PROCEDURE [dbo].[usp_GetBudgetAdjustmentUploadDataForOrg_20200602_bak] 
	@FiscalYear varchar(4) = '2013',
	@FiscalPeriod varchar(2) = '06', --Period in which adjustments are to be applied
	@TransDescription varchar(100) = 'BA benefits alloc for June 2012-Dec 2013', --Some kind of meaningful description. 40 characters max.
	@OrgDocNumber varchar(20) = '', -- An Organizational Document Number for all the transactions overall.  10 characters max.
	@OrgRefId varchar(10) = '01-06Ben', --An Organizational Reference ID for the transactions overall. 8 characters max.
	@TransDocNumberSequence char(3) = '001',  --Change the ### to something meaningful. A unique sequence number for the group of transactions. Must be 3 characters!
	@OrgId nvarchar(4) = 'AAES', --Kuali level 4 Org ID (College level)  Note: If level 4 org is present, level 5 org(s) are ignored.
	@TransDocOriginCode char(2) = '', --Unique code provided by AFS for the corresponding college's dean's office.
	@UseDaFIS bit = 0, -- Set this to 0 for any org other than AAES or if you want to get the data directly from DaFIS instead of our local FISDataMart.
	@PrintGrandTotalOnly bit = 0, -- Set this to 1 if you want to run a sanity check and print the grand total only.
	@UseV1Udf bit = 0, -- allows for testing to compare v1 results against v2.
	@IsDebug bit = 0 --Set to 1 to print SQL generated only, and not execute.
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    DECLARE @TransactionsForSummation TransactionsForSummationTableType

IF @IsDebug = 1 PRINT '
USE [BenefitsAllocationUpload]
GO

DECLARE @TransactionsForSummation TransactionsForSummationTableType
'
DECLARE @Statement nvarchar(MAX) = ''
DECLARE @OrgLevel smallint;
SELECT @Statement = 'EXEC dbo.usp_GetOrgLevel @OrgId =' + '''' + @OrgId + ''', @OrgLevel = @OrgLevel OUTPUT' 
EXEC sp_executesql @Statement, N'@OrgLevel smallint OUTPUT', @OrgLevel OUTPUT;

IF @IsDebug = 1
	PRINT '--OrgLevel: ' + CONVERT(varchar(5), @OrgLevel)

IF @TransDocOriginCode IS NULL OR @TransDocOriginCode LIKE ''
BEGIN 
	SELECT @Statement = 'SELECT @TransDocOriginCode = (SELECT dbo.udf_GetTransDocOriginCodeForOrg(''' + @OrgId + '''))' 
	EXEC sp_executesql @Statement, N'@TransDocOriginCode nvarchar(2) OUTPUT', @TransDocOriginCode OUTPUT;
END

DECLARE @ExcludeObjectsString varchar(MAX) = dbo.udf_GetExcludeObjectsString(@OrgId, 2)

DECLARE @MySQL varchar(MAX) = ''
IF @UseDaFIS = 0 AND (@OrgLevel IS NOT NULL AND @OrgLevel NOT LIKE '') AND @OrgLevel = 4 AND @OrgId = 'AAES'
	BEGIN

		SELECT @MySQL = '
		SELECT 
			''AAES'' AS ORG_ID,
			NULL AS ORG_NAME,
			NULL AS A11_ACCT_NUM,
			NULL AS HIGHER_ED_FUNC_CODE,
			Chart AS CHART_NUM,
			Account AS ACCT_NUM,
			SubAccount AS SUB_ACCT_NUM,
			ConsolidatnCode AS OBJ_CONSOLIDATN_NUM,
			NULL AS OBJECT_NUM,
			SubObject AS SUB_OBJECT_NUM,
			Project AS TRANS_LINE_PROJECT_NUM,
			Amount AS TRANS_LINE_AMT, 
			FunctionCode AS FUNCTION_CODE,
			NULL AS BALANCE_TYPE_CODE,
			NULL AS IS_PENDING
		FROM ( 
				SELECT TLV.Year, TLV.Chart, TLV.Account, TLV.SubAccount, ConsolidatnCode, TLV.SubObject, TLV.Project, (SUM(LineAmount) * -1) AS Amount, 
				CASE WHEN (A.HigherEdFuncCode = ''ORES'' OR
							LEFT(A11AcctNum, 2) BETWEEN ''44'' AND ''59'' OR
							LEFT(A11AcctNum, 2) IN (''62'')) THEN ''R'' ELSE ''I'' END AS FunctionCode
				FROM  
					FISDataMart.dbo.TransV AS TLV
					--INNER JOIN	FISDataMart.dbo.Accounts AS A ON TLV.AccountsFK = A.AccountPK
					INNER JOIN FISDataMart.dbo.Accounts AS A ON TLV.Year = A.Year AND TLV.Chart = A.Chart AND TLV.Account = A.Account
					INNER JOIN FISDataMart.dbo.OrganizationsV AS O ON A.Year = O.Year AND A.Period = O.Period AND A.Chart = O.Chart AND A.Org = O.Org
					--INNER JOIN FISDataMart.dbo.Objects AS O ON TLV.ObjectsFK = O.ObjectPK
					INNER JOIN FISDataMart.dbo.Objects AS Obj ON TLV.Year = Obj.Year AND TLV.Chart = Obj.Chart AND TLV.Object = Obj.Object

				WHERE	
					(A.Year >= ' + @FiscalYear + ') AND
					(A.Chart = ''3'') AND
					(A.Period IN (
						SELECT MAX(Period)
						FROM FISDataMart.dbo.Accounts
						WHERE Year = ' + @FiscalYear + ' AND Chart = ''3''
					)) AND
					(A.Account NOT IN (SELECT Account FROM dbo.CentralAccounts WHERE OrgId = ''AAES'')) AND
					(A.Chart = ''3'' AND OpFundNum = ''19900'') AND
					(A.Org IN (
						SELECT DISTINCT Org 
						FROM FISDataMart.dbo.OrganizationsV O
						WHERE Year >= ' + @FiscalYear + ' AND
						((Chart1 = ''3'' AND Org1 = ''AAES'') AND
						NOT (Chart2 = ''3'' AND Org2 = ''ACBS''))
						)
					) AND
					(BalType IN (''AC'', ''CB'')) AND
					(ConsolidatnCode IN (''SB28'', ''SUB6'')) AND'

		--DECLARE @ExcludeObjectsString varchar(MAX) = dbo.udf_GetExcludeObjectsString(@OrgId, 2)
		IF LEN(@ExcludeObjectsString) > 0
			SELECT @MySQL += '
					(TLV.Object NOT IN (' + @ExcludeObjectsString + ')) AND'

		SELECT @MySQL += '
					((A.ExpirationDate IS NULL) OR (A.ExpirationDate >= GETDATE()))

				GROUP BY TLV.Year, TLV.Chart, TLV.Account, SubAccount, ConsolidatnCode, SubObject, Project, 
					CASE WHEN (A.HigherEdFuncCode = ''ORES'' OR
						LEFT(A11AcctNum, 2) BETWEEN ''44'' AND ''59'' OR
						LEFT(A11AcctNum, 2) IN (''62'')) THEN ''R'' ELSE ''I'' END
													
				HAVING SUM(LineAmount) <> 0
		) AS TFS
		ORDER BY Year, Chart, Account, SubAccount, ConsolidatnCode, SubObject, Project, Amount, FunctionCode
'
	IF @IsDebug = 1
		BEGIN
			PRINT '
		INSERT INTO @TransactionsForSummation ' + @MySQL;
		END
	ELSE
		BEGIN
			INSERT INTO @TransactionsForSummation EXEC(@MySQL);
		END

	END -- IF @UseDaFIS = 0 AND (@CollegeLevelOrg IS NOT NULL OR @CollegeLevelOrg NOT LIKE '') AND  @CollegeLevelOrg = 'AAES' 
ELSE -- @UseDaFIS = 1 AND/OR @CollegeLevelOrg NOT LIKE 'AAES' 
	BEGIN
		DECLARE @NumSingleQuotes smallint = 2
		DECLARE @UseCollegeLevelOrg bit = 1
		DECLARE @ChartNum varchar(10) = dbo.udf_GetChartNumStringForOrg(@OrgId, @NumSingleQuotes)
		DECLARE @FunctionCodeCaseStatement varchar(MAX) = dbo.udf_GetFunctionCodeCaseStatement(@OrgId, @NumSingleQuotes)
		--2013-06-21 by kjt: Moved this to top of sproc so it can also be used by local FISDataMart section of script.
		--DECLARE @ExcludeObjectsString varchar(MAX) = dbo.udf_GetExcludeObjectsString(@OrgId, @NumSingleQuotes)
		DECLARE @ExcludeAccountsString varchar(MAX) = dbo.udf_GetExcludeAccountsString(@OrgId, @NumSingleQuotes)
		DECLARE @IncludeAccountsString varchar(MAX) = dbo.udf_GetIncludeAccountsString(@OrgId, @NumSingleQuotes)
		DECLARE @IncludeOpFundsString varchar(MAX) = dbo.udf_GetIncludeOpFundsString(@OrgId, @NumSingleQuotes)

		DECLARE @OrgForTableName nvarchar(4) = ''
		DECLARE @OPENQUERY nvarchar(MAX), @TSQL varchar(MAX), @LinkedServer nvarchar(50), @SelectList nvarchar(MAX)
		SET @SelectList = 
'			ORG_ID,
			NULL AS ORG_NAME,
			NULL AS A11_ACCT_NUM,
			NULL AS HIGHER_ED_FUNC_CODE,
			CHART_NUM,
			ACCT_NUM,
			SUB_ACCT_NUM,
			OBJ_CONSOLIDATN_NUM,
			NULL AS OBJECT_NUM, 
			SUB_OBJECT_NUM,
			TRANS_LINE_PROJECT_NUM,
			TRANS_LINE_AMT,
			FUNCTION_CODE,
			NULL AS BALANCE_TYPE_CODE,
			IS_PENDING'
			  
		SET @LinkedServer = 'FIS_DS'
		SET @OPENQUERY = 'OPENQUERY('+ @LinkedServer + ','''

		--DECLARE @ExcludeAccounts TABLE (Account varchar(7))
		DECLARE @QuotedOrgsString varchar(150) = ''
		IF @OrgLevel = 5
			BEGIN
				SELECT @UseCollegeLevelOrg = 0
				--Find parent org for DivisionLevelOrg(s) 
				SELECT @QuotedOrgsString = (SELECT dbo.udf_CreateQuotedStringList(DEFAULT, @OrgId, DEFAULT))
				--IF @IsDebug = 1 PRINT @QuotedOrgsString

				SELECT @statement = 'SELECT @OrgForTableName = (SELECT TOP 1 * FROM OPENQUERY(FIS_DS, ''
			  SELECT DISTINCT
				 O.ORG_ID_LEVEL_4 
			  FROM 
				FINANCE.ORGANIZATION_HIERARCHY O
			  where  
				O.ORG_ID_LEVEL_5 IN (' + @QuotedOrgsString + ') AND
				O.FISCAL_YEAR = 9999 and O.FISCAL_PERIOD = ''''--'''' AND 
				O.CHART_NUM = ' + @ChartNum + '
			  ''))'

				EXEC sp_executesql @statement, N'@OrgForTableName nvarchar(4) OUTPUT', @OrgForTableName OUTPUT;
			END
		ELSE
			BEGIN
				-- Use CollegeLevelOrg
				SELECT @OrgForTableName = @OrgId
				SELECT @QuotedOrgsString = '''' + @OrgId + ''''
			END

		--IF @IsDebug = 1 PRINT @QuotedOrgsString
		--SELECT @statement = 'SELECT DISTINCT Account FROM CentralAccounts WHERE OrgId IN (' + REPLACE(@QuotedOrgsString, '''''', '''')  +')'
		--IF @IsDebug = 1 PRINT @statement

		--INSERT INTO @ExcludeAccounts
		--EXEC (@statement)

		--IF @IsDebug = 1 SELECT * FROM @ExcludeAccounts

		--IF @IsDebug = 1 PRINT @OrgForTableName

		-- Applied Transactions:

		SELECT @TSQL = '
			SELECT '
		IF @UseCollegeLevelOrg = 0
			SELECT @TSQL += '
				' + @QuotedOrgsString + ' AS ORG_ID,'
		ELSE
			SELECT @TSQL += '
				''''' + @OrgId + ''''' AS ORG_ID,'

		SELECT @TSQL += '
				OA.FISCAL_YEAR,
				OA.CHART_NUM,
				OA.ACCT_NUM,
				A.SUB_ACCT_NUM,
				OBJ_CONSOLIDATN_NUM,
				A.SUB_OBJECT_NUM,
				A.TRANS_LINE_PROJECT_NUM,
				(SUM(A.TRANS_LINE_AMT) * -1) TRANS_LINE_AMT,' + @FunctionCodeCaseStatement + ' AS FUNCTION_CODE,
				0 AS IS_PENDING
			FROM	FINANCE.GL_APPLIED_TRANSACTIONS A
				INNER JOIN FINANCE.ORGANIZATION_ACCOUNT OA ON 
					A.FISCAL_YEAR = OA.FISCAL_YEAR AND
					--A.FISCAL_PERIOD = OA.FISCAL_PERIOD AND
					A.CHART_NUM = OA.CHART_NUM AND
					A.ACCT_NUM = OA.ACCT_NUM
				INNER JOIN FINANCE.ORGANIZATION_HIERARCHY O ON 
					OA.FISCAL_YEAR = O.FISCAL_YEAR AND 
					OA.FISCAL_PERIOD = O.FISCAL_PERIOD AND 
					OA.CHART_NUM = O.CHART_NUM AND 
					OA.ORG_ID = O.ORG_ID	
			WHERE
				OA.FISCAL_YEAR >= ' + @FiscalYear + ' AND
				OA.CHART_NUM =  ' + @ChartNum + ' AND
				OA.FISCAL_PERIOD IN (
					SELECT MAX(FISCAL_PERIOD)
					FROM FINANCE.ORGANIZATION_ACCOUNT OA
					WHERE OA.FISCAL_YEAR >= ' + @FiscalYear + ' AND
						OA.CHART_NUM = ' + @ChartNum + ' 
				) AND
				OA.ACCT_NUM NOT IN (' + @ExcludeAccountsString + ') AND'

		-- 2014-08-20 by kjt: see note under modifications.
		--IF LEN(@IncludeAccountsString) > 0  
		--	BEGIN
		--		SELECT @TSQL += '
		--		OA.ACCT_NUM IN (' + @IncludeAccountsString +') AND'
		--	END

		SELECT @TSQL += '
				(' + @IncludeOpFundsString + ') AND
				(OA.ORG_ID) IN (
					SELECT DISTINCT ORG_ID FROM FINANCE.ORGANIZATION_HIERARCHY O
					WHERE FISCAL_YEAR = ' + @FiscalYear + ' AND'

		IF  @UseCollegeLevelOrg = 0
			BEGIN
			--Use DivisionLevelOrg(s) 
				SELECT @TSQL += '
						(O.CHART_NUM_LEVEL_5 = ' + @ChartNum + ' AND O.ORG_ID_LEVEL_5 IN (' + @QuotedOrgsString + '))'
			END
		ELSE
			BEGIN
			-- Use CollegeLevelOrg
				SELECT @TSQL += '
						(O.CHART_NUM_LEVEL_4 = ' + @ChartNum + ' AND O.ORG_ID_LEVEL_4 = ''''' + @OrgId + ''''' )'	
				IF @OrgId = 'AAES'
					BEGIN
						SELECT @TSQL += '
						AND NOT
						(O.CHART_NUM_LEVEL_5 = ' + @ChartNum + ' AND O.ORG_ID_LEVEL_5 = ''''ACBS'''' )'
					END
			END
			
			SELECT @TSQL += '
                ) AND
				A.BALANCE_TYPE_CODE  IN (''''AC'''', ''''CB'''') AND
				OBJ_CONSOLIDATN_NUM IN (''''SB28'''', ''''SUB6'''') AND'
	
		IF LEN(@ExcludeObjectsString) > 0
			BEGIN
				SELECT @TSQL += '
				A.OBJECT_NUM NOT IN (' + @ExcludeObjectsString + ') AND'
			END
			
		SELECT @TSQL += '
				(OA.acct_expiration_date IS NULL OR OA.acct_expiration_date >= SYSDATE)
				GROUP BY
					OA.FISCAL_YEAR,
					OA.CHART_NUM, 
					OA.ACCT_NUM, 
					A.SUB_ACCT_NUM,
					OBJ_CONSOLIDATN_NUM,
					A.SUB_OBJECT_NUM, 
					A.TRANS_LINE_PROJECT_NUM, ' + @FunctionCodeCaseStatement + '
			HAVING        
				SUM(A.TRANS_LINE_AMT) <> 0

		UNION ALL
'
	-- Pending Transactions:

	SELECT @TSQL += '
		SELECT '
			IF @UseCollegeLevelOrg = 0
				SELECT @TSQL += '
			' + @QuotedOrgsString + ' AS ORG_ID,'
			ELSE
				SELECT @TSQL += '
			''''' + @OrgId + ''''' AS ORG_ID,'
			SELECT @TSQL += '
			OA.FISCAL_YEAR, 
			OA.CHART_NUM, 
			OA.ACCT_NUM, 
			P.SUB_ACCT_NUM,
			OBJ_CONSOLIDATN_NUM,
			P.SUB_OBJECT_NUM, 
			P.TRANS_LINE_PROJECT_NUM,
			(SUM(P.TRANS_LINE_AMT) * -1) TRANS_LINE_AMT, ' + @FunctionCodeCaseStatement + ' AS FUNCTION_CODE,
			1 AS IS_PENDING
		FROM 
				FINANCE.GL_PENDING_TRANSACTIONS P
				INNER JOIN FINANCE.ORGANIZATION_ACCOUNT OA ON 
					P.FISCAL_YEAR = OA.FISCAL_YEAR AND
					--P.FISCAL_PERIOD = OA.FISCAL_PERIOD AND
					P.CHART_NUM = OA.CHART_NUM AND
					P.ACCT_NUM = OA.ACCT_NUM
				INNER JOIN FINANCE.ORGANIZATION_HIERARCHY O ON 
					OA.FISCAL_YEAR = O.FISCAL_YEAR AND 
					OA.FISCAL_PERIOD = O.FISCAL_PERIOD AND 
					OA.CHART_NUM = O.CHART_NUM AND 
					OA.ORG_ID = O.ORG_ID
				INNER JOIN FINANCE.OBJECT OBJ ON
                    P.FISCAL_YEAR = OBJ.FISCAL_YEAR AND
                    P.CHART_NUM = OBJ.CHART_NUM AND
                    P.OBJECT_NUM = OBJ.OBJECT_NUM
		WHERE
				OA.FISCAL_YEAR >= ' + @FiscalYear + ' AND
				OA.CHART_NUM =  ' + @ChartNum + ' AND
				OA.FISCAL_PERIOD IN (
					SELECT MAX(FISCAL_PERIOD)
					FROM FINANCE.ORGANIZATION_ACCOUNT OA
					WHERE OA.FISCAL_YEAR >= ' + @FiscalYear + ' AND
						OA.CHART_NUM = ' + @ChartNum + ' 
				) AND
				OA.ACCT_NUM NOT IN (' + @ExcludeAccountsString + ') AND'

		-- 2014-08-20 by kjt: see note under modifications.
		--IF LEN(@IncludeAccountsString) > 0  
		--	BEGIN
		--		SELECT @TSQL += '
		--		OA.ACCT_NUM IN (' + @IncludeAccountsString +') AND'
		--	END

		SELECT @TSQL += '
				(' + @IncludeOpFundsString + ') AND
				(OA.ORG_ID) IN (
					SELECT DISTINCT ORG_ID FROM FINANCE.ORGANIZATION_HIERARCHY O
					WHERE FISCAL_YEAR = ' + @FiscalYear + ' AND'

		IF  @UseCollegeLevelOrg = 0
			BEGIN
			--Use DivisionLevelOrg(s) 
				SELECT @TSQL += '
						(O.CHART_NUM_LEVEL_5 = ' + @ChartNum + ' AND O.ORG_ID_LEVEL_5 IN (' + @QuotedOrgsString + '))'
			END
		ELSE
			BEGIN
			-- Use CollegeLevelOrg
				SELECT @TSQL += '
						(O.CHART_NUM_LEVEL_4 = ' + @ChartNum + ' AND O.ORG_ID_LEVEL_4 = ''''' + @OrgId + ''''' )'	
				IF @OrgId = 'AAES'
					BEGIN
						SELECT @TSQL += '
						AND NOT
						(O.CHART_NUM_LEVEL_5 = ' + @ChartNum + ' AND O.ORG_ID_LEVEL_5 = ''''ACBS'''' )'
					END
			END
			
			SELECT @TSQL += '
                ) AND
				P.BALANCE_TYPE_CODE  IN (''''AC'''', ''''CB'''') AND
				OBJ_CONSOLIDATN_NUM IN (''''SB28'''', ''''SUB6'''') AND'
	
		IF LEN(@ExcludeObjectsString) > 0
			BEGIN
				SELECT @TSQL += '
				P.OBJECT_NUM NOT IN (' + @ExcludeObjectsString + ') AND'
			END
			
		SELECT @TSQL += '
				(OA.acct_expiration_date IS NULL OR OA.acct_expiration_date >= SYSDATE)
		GROUP BY
			OA.FISCAL_YEAR, 
			OA.CHART_NUM, 
			OA.ACCT_NUM, 
			P.SUB_ACCT_NUM,
			OBJ_CONSOLIDATN_NUM,
			P.SUB_OBJECT_NUM, 
			P.TRANS_LINE_PROJECT_NUM, ' + @FunctionCodeCaseStatement + '
		HAVING SUM(P.TRANS_LINE_AMT) <> 0
	'')
'
	IF @IsDebug = 1
		BEGIN
			SELECT @MySQL = '
		INSERT INTO @TransactionsForSummation 
		EXEC(''
		SELECT 	
' +		@SelectList + '			
		FROM ' + REPLACE(@OPENQUERY+@TSQL, '''', '''''') + ''')'

			PRINT @MySQL
		END
	ELSE
		BEGIN
			INSERT INTO @TransactionsForSummation 
			EXEC('
		SELECT 	
' +		@SelectList + '			
		FROM ' + @OPENQUERY+@TSQL + '')
		END

--	-- Pending Transactions:
--			SELECT @TSQL = '
--			SELECT'
--			IF @UseCollegeLevelOrg = 0
--				SELECT @TSQL += '
--				O.ORG_ID_LEVEL_5 AS ORG_ID,'
--			ELSE
--				SELECT @TSQL += '
--				O.ORG_ID_LEVEL_4 AS ORG_ID,'
--			SELECT @TSQL += '		
--				P.CHART_NUM,
--				P.ACCT_NUM,
--				P.SUB_ACCT_NUM,
--				OBJ.OBJ_CONSOLIDATN_NUM,
--				P.SUB_OBJECT_NUM,
--				P.TRANS_LINE_PROJECT_NUM,
--				(SUM(P.TRANS_LINE_AMT) * -1) AS TRANS_LINE_AMT,' + @FunctionCodeCaseStatement + ' AS FUNCTION_CODE,
--				1 AS IS_PENDING
--			FROM 
--				FINANCE.GL_PENDING_TRANSACTIONS P
--			INNER JOIN FINANCE.ORGANIZATION_ACCOUNT OA ON 
--				P.FISCAL_YEAR = OA.FISCAL_YEAR AND
--				P.FISCAL_PERIOD = OA.FISCAL_PERIOD AND
--				P.CHART_NUM = OA.CHART_NUM AND
--				P.ACCT_NUM = OA.ACCT_NUM
--			INNER JOIN FINANCE.ORGANIZATION_HIERARCHY O ON 
--				OA.FISCAL_YEAR = O.FISCAL_YEAR AND 
--				OA.FISCAL_PERIOD = O.FISCAL_PERIOD AND 
--				OA.CHART_NUM = O.CHART_NUM AND 
--				OA.ORG_ID = O.ORG_ID
--			INNER JOIN FINANCE.OBJECT OBJ ON
--				P.FISCAL_YEAR = OBJ.FISCAL_YEAR AND
--				P.CHART_NUM = OBJ.CHART_NUM AND
--				P.OBJECT_NUM = OBJ.OBJECT_NUM
--			WHERE'

--			IF @UseCollegeLevelOrg = 0
--				BEGIN
--					--Use DivisionLevelOrg(s) 
--					SELECT @TSQL += '
--				O.CHART_NUM_LEVEL_5 = ' + @ChartNum + ' AND O.ORG_ID_LEVEL_5 IN (' +  @QuotedOrgsString + ') AND 
--'
--				END
--			ELSE
--				BEGIN
--					-- Use CollegeLevelOrg
--					SELECT @TSQL += '
--				O.CHART_NUM_LEVEL_4 = ' + @ChartNum + ' AND O.ORG_ID_LEVEL_4 = ''''' + @OrgId + ''''' AND
--'	
--					IF @OrgId = 'AAES'
--						BEGIN
--							SELECT @TSQL += '				O.CHART_NUM_LEVEL_5 = ' + @ChartNum + ' AND O.ORG_ID_LEVEL_5 <> ''''ACBS'''' AND
--'
--						END
--				END

--			SELECT @TSQL += 
--'				P.FISCAL_YEAR >= ' + CONVERT(varchar(4), @FiscalYear) + ' AND 
--				P.CHART_NUM = ' + @ChartNum + ' AND
--				(P.BALANCE_TYPE_CODE  IN (''''AC'''', ''''CB'''')) AND	
--				NOT (P.OBJECT_NUM IN (SELECT DISTINCT OBJECT_NUM FROM FINANCE.OBJECT_REPORTING)) AND
--				OBJ.OBJ_CONSOLIDATN_NUM IN (''''SB28'''', ''''SUB6'''') AND
--				P.OBJECT_NUM NOT IN (' + @ExcludeObjectsString + ') AND
--				(OA.acct_expiration_date IS NULL OR OA.acct_expiration_date >= SYSDATE) AND'

--			IF LEN(@IncludeAccountsString) > 0  
--			BEGIN
--				SELECT @TSQL += ' 
--				P.ACCT_NUM IN (' + @IncludeAccountsString +') AND'
--			END

--			SELECT @TSQL += '
--				P.ACCT_NUM NOT IN (' + @ExcludeAccountsString + ') AND 
--				(' + @IncludeOpFundsString + ')
--			GROUP BY'

--			IF @UseCollegeLevelOrg = 0
--				SELECT @TSQL += '
--				O.ORG_ID_LEVEL_5,'
--			ELSE
--				SELECT @TSQL += '
--				O.ORG_ID_LEVEL_4,'

--			SELECT @TSQL += '
--				P.CHART_NUM,
--				P.ACCT_NUM,
--				P.SUB_ACCT_NUM,
--				OBJ.OBJ_CONSOLIDATN_NUM,
--				P.SUB_OBJECT_NUM,
--				P.TRANS_LINE_PROJECT_NUM, ' + @FunctionCodeCaseStatement + '
--			HAVING        
--				SUM(P.TRANS_LINE_AMT) <> 0				
--			'')				
--'
--	IF @IsDebug = 1
--		BEGIN
--			SELECT @MySQL = '
--		INSERT INTO @TransactionsForSummation 
--		EXEC(''
--		SELECT 	
--' +		@SelectList + '			
--		FROM ' + REPLACE(@OPENQUERY+@TSQL, '''', '''''') + ''')'
--			PRINT @MySQL
--		END
--	ELSE
--		BEGIN
--			INSERT INTO @TransactionsForSummation 
--			EXEC('
--		SELECT 
--' +		@SelectList + ' 
--		FROM ' + @OPENQUERY+@TSQL + '')
--		END
--	END
END

-- 2014-08-20 by kjt:  See notes in modifications section.
	IF LEN(@IncludeAccountsString) > 0  
		BEGIN
			IF @IsDebug = 1
				BEGIN
					SELECT @MySQL = '
		DELETE TFS FROM @TransactionsForSummation TFS
		WHERE NOT EXISTS (
			SELECT 1 
			FROM dbo.ReimbursableBenefitsAccounts
			WHERE OrgId = ''' + @OrgId + ''' AND CHART_NUM = Chart AND ACCT_NUM = Account AND IsActive = 1
		)'
					PRINT @MySQL
				END
			ELSE
				BEGIN
					DELETE TFS
					FROM @TransactionsForSummation TFS
					WHERE NOT EXISTS (
						SELECT 1 FROM dbo.ReimbursableBenefitsAccounts 
						WHERE OrgId = @OrgId AND ACCT_NUM = Account AND CHART_NUM = Chart AND IsActive = 1
					)
				END
		END

IF @IsDebug = 1
	BEGIN
		IF @PrintGrandTotalOnly = 1
			SELECT @MySQL = '
		SELECT SUM(TRANS_LINE_AMT) AS Amount FROM  @TransactionsForSummation'
		ELSE
		BEGIN
			IF @UseV1Udf = 1 
			 
				SELECT @MySQL = '
		SELECT * FROM [dbo].[udf_GetBudgetAdjustmentUploadDataFromInputTableForOrg](''' +@FiscalYear + ''',''' +@FiscalPeriod + ''', ''' +@TransDescription + ''', ''' +@OrgDocNumber + ''', ''' +@OrgRefId + ''', ''' +@TransDocNumberSequence + ''', ''' +@TransDocOriginCode + ''', @TransactionsForSummation)'
			ELSE
				SELECT @MySQL = '
		SELECT * FROM [dbo].[udf_GetBudgetAdjustmentUploadDataFromInputTableForOrg_v2](''' +@FiscalYear + ''',''' +@FiscalPeriod + ''', ''' +@TransDescription + ''', ''' +@OrgDocNumber + ''', ''' +@OrgRefId + ''', ''' +@TransDocNumberSequence + ''', ''' +@TransDocOriginCode + ''', @TransactionsForSummation)'
		END	
		PRINT @MySQL;
	END
ELSE
	BEGIN
		IF @PrintGrandTotalOnly = 1
			SELECT SUM(TRANS_LINE_AMT) AS Amount FROM  @TransactionsForSummation
		ELSE
		BEGIN
			IF @UseV1Udf = 1 
				SELECT * FROM [dbo].[udf_GetBudgetAdjustmentUploadDataFromInputTableForOrg](@FiscalYear, @FiscalPeriod, @TransDescription, @OrgDocNumber, @OrgRefId, @TransDocNumberSequence, @TransDocOriginCode, @TransactionsForSummation)
			ELSE
				SELECT * FROM [dbo].[udf_GetBudgetAdjustmentUploadDataFromInputTableForOrg_v2](@FiscalYear, @FiscalPeriod, @TransDescription, @OrgDocNumber, @OrgRefId, @TransDocNumberSequence, @TransDocOriginCode, @TransactionsForSummation)
		END
	END

END