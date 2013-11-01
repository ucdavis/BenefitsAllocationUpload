-- =============================================
-- Author:		Ken Taylor
-- Create date: May 21, 2013
-- Description:	Selects expired account data from either our own local FISDataMart or campus data warehouse,
-- i.e. DaFIS.
-- Usage:
/*
-- For AAES Using CA&ES Local FIS DataMart:
USE [BenefitsAllocationUpload]
GO

EXEC [dbo].[usp_GetExpiredAccountsForOrg] 
	@FiscalYear = '2013',
	@OrgId = 'AAES', --Kuali level 4 Org ID (College level) or- Kuali level 5 Org Id (Division level), i.e. 'HACS' or 'MPSC' or 'SSCI', etc
	@UseDaFIS = 0, -- Set this to 1 for any org other than AAES or if you want to get the data directly from DaFIS instead of our local FISDataMart.
	@IsDebug = 0 --Set to 1 to print SQL generated only, and not execute.
GO

-- For AAES Using Campus Data Warehouse:
USE [BenefitsAllocationUpload]
GO

EXEC [dbo].[usp_GetExpiredAccountsForOrg] 
	@FiscalYear = '2013',
	@OrgId = 'AAES', --Kuali level 4 Org ID (College level) or- Kuali level 5 Org Id (Division level), i.e. 'HACS' or 'MPSC' or 'SSCI', etc
	@UseDaFIS = 1, -- Set this to 0 for any org other than AAES or if you want to get the data directly from DaFIS instead of our local FISDataMart.
	@IsDebug = 0 --Set to 1 to print SQL generated only, and not execute.
GO

-- For SSCI Using Campus Data Warehouse, since our local CA&ES Datamart does not contain any of their data:
USE [BenefitsAllocationUpload]
GO

EXEC [dbo].[usp_GetExpiredAccountsForOrg] 
	@FiscalYear = '2013',
	@OrgId = 'SSCI', --Kuali level 4 Org ID (College level) or- Kuali level 5 Org Id (Division level), i.e. 'HACS' or 'MPSC' or 'SSCI', etc
	@UseDaFIS = 1, -- Set this to 0 for any org other than AAES or if you want to get the data directly from DaFIS instead of our local FISDataMart.
	@IsDebug = 0 --Set to 1 to print SQL generated only, and not execute.
GO

-- For DANR Using Campus Data Warehouse, since our local CA&ES Datamart does not contain any of their data:
USE [BenefitsAllocationUpload]
GO

EXEC [dbo].[usp_GetExpiredAccountsForOrg] 
	@FiscalYear = '2013',
	@OrgId = 'DANR', --Kuali level 4 Org ID (College level) or- Kuali level 5 Org Id (Division level), i.e. 'HACS' or 'MPSC' or 'SSCI', etc
	@UseDaFIS = 1, -- Set this to 0 for any org other than AAES or if you want to get the data directly from DaFIS instead of our local FISDataMart.
	@IsDebug = 0 --Set to 1 to print SQL generated only, and not execute.
GO
*/
-- Modifications:
--	2013-05-21 by kjt: Initial creation. 
--	2012-05-23 by kjt: Added logic for included accounts if present in dbo.ReimbursableBenefitsAccounts
--	2013-06-18 by kjt: Added logic to not include exclude object portion of where clause as DANR is going to fund all objects
--		as per Regina Ranoa 2013-06-17.
--	2013-06-20 by kjt: Revised logic handle formerly expired accounts that are now active.
--	2013-06-21 by kjt: Revised local FISDataMart logic to use @ExcludeObjectsString instead of hardcoding. 
--		Note: Using logic very similar to expired accounts query in udf_GetExpiredAccountsForOrg.
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetExpiredAccountsForOrg] 
	@FiscalYear varchar(4) = '2013',
	@OrgId nvarchar(4) = 'AAES', --Kuali level 4 Org ID (College level)  Note: If level 4 org is present, level 5 org(s) are ignored.
	@UseDaFIS bit = 0, -- Set this to 0 for any org other than AAES or if you want to get the data directly from DaFIS instead of our local FISDataMart.
	@IsDebug bit = 0 --Set to 1 to print SQL generated only, and not execute.
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    DECLARE @ExpiredAccounts TABLE (ORG_ID varchar(4), CHART_NUM varchar(2), ACCT_NUM varchar(7), ACCT_EXPIRATION_DATE datetime,  TRANS_LINE_AMT decimal(15,2), ACCT_MGR_ID varchar(8), ACCT_MGR_NAME varchar(30))

	DECLARE @NumSingleQuotes smallint = 2
	DECLARE @UseCollegeLevelOrg bit = 1
	DECLARE @ChartNum varchar(10) = dbo.udf_GetChartNumStringForOrg(@OrgId, @NumSingleQuotes)
	DECLARE @FunctionCodeCaseStatement varchar(MAX) = dbo.udf_GetFunctionCodeCaseStatement(@OrgId, @NumSingleQuotes)
	DECLARE @ExcludeObjectsString varchar(MAX) = dbo.udf_GetExcludeObjectsString(@OrgId, @NumSingleQuotes)
	DECLARE @ExcludeAccountsString varchar(MAX) = dbo.udf_GetExcludeAccountsString(@OrgId, @NumSingleQuotes)
	DECLARE @IncludeAccountsString varchar(MAX) = dbo.udf_GetIncludeAccountsString(@OrgId, @NumSingleQuotes)
	DECLARE @IncludeOpFundsString varchar(MAX) = dbo.udf_GetIncludeOpFundsString(@OrgId, @NumSingleQuotes)

IF @IsDebug = 1 PRINT 'USE [FISDataMart]
GO

DECLARE @ExpiredAccounts TABLE (ORG_ID varchar(4), CHART_NUM varchar(2), ACCT_NUM varchar(7), ACCT_EXPIRATION_DATE datetime,  TRANS_LINE_AMT decimal(15,2), ACCT_MGR_ID varchar(8), ACCT_MGR_NAME varchar(30))
'
DECLARE @Statement nvarchar(MAX) = ''
DECLARE @OrgLevel smallint;
SELECT @Statement = 'EXEC dbo.usp_GetOrgLevel @OrgId =' + '''' + @OrgId + ''', @OrgLevel = @OrgLevel OUTPUT' 
EXEC sp_executesql @Statement, N'@OrgLevel smallint OUTPUT', @OrgLevel OUTPUT;

IF @IsDebug = 1
	PRINT '--OrgLevel: = ' + CONVERT(varchar(5), @OrgLevel)

DECLARE @MySQL varchar(MAX) = ''
IF @UseDaFIS = 0 AND (@OrgLevel IS NOT NULL AND @OrgLevel NOT LIKE '') AND @OrgLevel = 4 AND @OrgId = 'AAES'
	BEGIN
		SELECT @MySQL = '
		SELECT 
			Org3 AS ORG_ID,
			Chart AS CHART_NUM,
			Account AS ACCT_NUM,
			ExpirationDate AS ACCT_EXPIRATION_DATE,
			--SUM(Amount) AS TRANS_LINE_AMT, 
			Amount AS TRANS_LINE_AMT, 
			MgrID AS ACCT_MGR_ID,
			MgrName AS ACCT_MGR_NAME
			FROM ( 
				SELECT Org3, TLV.Chart, TLV.Account, A.ExpirationDate, --(SUM(LineAmount) * -1) AS Amount, 
				LineAmount AS Amount,A.MgrId, A.MgrName
				FROM  
					FISDataMart.dbo.TransV AS TLV
					--INNER JOIN	FISDataMart.dbo.Accounts AS A ON TLV.AccountsFK = A.AccountPK
					INNER JOIN FISDataMart.dbo.Accounts AS A ON TLV.Year = A.Year AND TLV.Chart = A.Chart AND TLV.Account = A.Account
					--INNER JOIN FISDataMart.dbo.OrganizationsV AS O ON A.OrgFK = O.OrganizationPK
					INNER JOIN FISDataMart.dbo.OrganizationsV AS O ON A.Year = O.Year AND A.Period = O.Period AND A.Chart = O.Chart AND A.Org = O.Org
					--INNER JOIN FISDataMart.dbo.Objects AS Obj ON TLV.ObjectsFK = Obj.ObjectPK
					INNER JOIN FISDataMart.dbo.Objects AS Obj ON TLV.Year = Obj.Year AND TLV.Chart = Obj.Chart AND TLV.Object = Obj.Object

				WHERE	
					(A.Year >= ' + @FiscalYear + ') AND
					(A.Chart = ''3'') AND
					(A.Period IN (
						SELECT MAX(Period)
						FROM FISDataMart.dbo.Accounts
						WHERE Year = '+ @FiscalYear + ' AND Chart = ''3''
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

		IF LEN(@ExcludeObjectsString) > 0
			SELECT @MySQL += '
					(TLV.Object NOT IN (' + @ExcludeObjectsString + ')) AND'

		SELECT @MySQL += '
					((A.ExpirationDate IS NOT NULL) AND (A.ExpirationDate < GETDATE()))
				--GROUP BY Org3, TLV.Chart, TLV.Account, A.MgrId, A.MgrName, A.ExpirationDate							
				--HAVING SUM(LineAmount) <> 0
			) AS TLV
			--GROUP BY Org3, Chart, Account, MgrID, MgrName, ExpirationDate
			--ORDER BY Org3, Chart, Account, MgrId, MgrName, ExpirationDate
'
		IF @IsDebug = 1
			BEGIN
				PRINT '
		INSERT INTO @ExpiredAccounts ' + @MySQL;
			END
		ELSE	
			INSERT INTO @ExpiredAccounts EXEC(@MySQL);

	END -- IF @UseDaFIS = 0 AND (@CollegeLevelOrg IS NOT NULL OR @CollegeLevelOrg NOT LIKE '') AND  @CollegeLevelOrg = 'AAES' 
ELSE -- @UseDaFIS = 1 AND/OR @CollegeLevelOrg NOT LIKE 'AAES' 
	BEGIN
		--2013-06-21 by kjt: Moved this to top of sproc so it can also be used by local FISDataMart section of script.
		--DECLARE @NumSingleQuotes smallint = 2
		--DECLARE @UseCollegeLevelOrg bit = 1
		--DECLARE @ChartNum varchar(10) = dbo.udf_GetChartNumStringForOrg(@OrgId, @NumSingleQuotes)
		--DECLARE @FunctionCodeCaseStatement varchar(MAX) = dbo.udf_GetFunctionCodeCaseStatement(@OrgId, @NumSingleQuotes)
		--DECLARE @ExcludeObjectsString varchar(MAX) = dbo.udf_GetExcludeObjectsString(@OrgId, @NumSingleQuotes)
		--DECLARE @ExcludeAccountsString varchar(MAX) = dbo.udf_GetExcludeAccountsString(@OrgId, @NumSingleQuotes)
		--DECLARE @IncludeAccountsString varchar(MAX) = dbo.udf_GetIncludeAccountsString(@OrgId, @NumSingleQuotes)
		--DECLARE @IncludeOpFundsString varchar(MAX) = dbo.udf_GetIncludeOpFundsString(@OrgId, @NumSingleQuotes)

		DECLARE @OrgForTableName nvarchar(4) = ''
		DECLARE @OPENQUERY nvarchar(MAX), @TSQL varchar(MAX), @LinkedServer nvarchar(50), @SelectList nvarchar(MAX)
		SET @SelectList = 
'			ORG_ID,
			CHART_NUM,
			ACCT_NUM,
			ACCT_EXPIRATION_DATE,
			TRANS_LINE_AMT,
			ACCT_MGR_ID,
			ACCT_MGR_NAME
			'
		SET @LinkedServer = 'FIS_DS'
		SET @OPENQUERY = 'OPENQUERY('+ @LinkedServer + ','''

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
				O.CHART_NUM =  ' + @ChartNum + '
			  ''))'

				EXEC sp_executesql @statement, N'@OrgForTableName nvarchar(4) OUTPUT', @OrgForTableName OUTPUT;
			END
		ELSE
			BEGIN
				-- Use CollegeLevelOrg
				SELECT @OrgForTableName = @OrgId
				SELECT @QuotedOrgsString = '''' + @OrgId + ''''
			END

		-- Applied Transactions:

		SELECT @TSQL = '
			SELECT '
		IF @UseCollegeLevelOrg = 0 OR @ChartNum = 'L'
			SELECT @TSQL += '
				O.ORG_ID_LEVEL_7 AS ORG_ID,'
		ELSE
			SELECT @TSQL += '
				O.ORG_ID_LEVEL_6 AS ORG_ID,'
		SELECT @TSQL += '
				OA.CHART_NUM,
				OA.ACCT_NUM,
				OA.ACCT_EXPIRATION_DATE,
				SUM(A.TRANS_LINE_AMT) TRANS_LINE_AMT,
				OA.ACCT_MGR_ID,
				OA.ACCT_MGR_NAME
			FROM 
				FINANCE.GL_APPLIED_TRANSACTIONS A
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

		IF LEN(@IncludeAccountsString) > 0  
			BEGIN
				SELECT @TSQL += '
				OA.ACCT_NUM IN (' + @IncludeAccountsString +') AND'
			END

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
				(OA.acct_expiration_date IS NOT NULL AND OA.acct_expiration_date < SYSDATE)
			GROUP BY'
		IF @UseCollegeLevelOrg = 0 OR @ChartNum = 'L'
			SELECT @TSQL += '
				O.ORG_ID_LEVEL_7,'
		ELSE
			SELECT @TSQL += '
				O.ORG_ID_LEVEL_6,'

		SELECT @TSQL += '
				OA.CHART_NUM,
				OA.ACCT_NUM,
				OA.ACCT_EXPIRATION_DATE,
				OA.acct_mgr_id,
				OA.acct_mgr_name
			HAVING        
				SUM(A.TRANS_LINE_AMT) <> 0

		UNION ALL
'
	-- Pending Transactions:

		SELECT @TSQL += '
			SELECT '
		IF @UseCollegeLevelOrg = 0 OR @ChartNum = 'L'
			SELECT @TSQL += '
				O.ORG_ID_LEVEL_7 AS ORG_ID,'
		ELSE
			SELECT @TSQL += '
				O.ORG_ID_LEVEL_6 AS ORG_ID,'
		SELECT @TSQL += '
				OA.CHART_NUM,
				OA.ACCT_NUM,
				OA.ACCT_EXPIRATION_DATE,
				SUM(P.TRANS_LINE_AMT) TRANS_LINE_AMT,
				OA.ACCT_MGR_ID,
				OA.ACCT_MGR_NAME
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

		IF LEN(@IncludeAccountsString) > 0  
			BEGIN
				SELECT @TSQL += '
				OA.ACCT_NUM IN (' + @IncludeAccountsString +') AND'
			END

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
				(OA.acct_expiration_date IS NOT NULL AND OA.acct_expiration_date < SYSDATE)
			GROUP BY'
		IF @UseCollegeLevelOrg = 0 OR @ChartNum = 'L'
			SELECT @TSQL += '
				O.ORG_ID_LEVEL_7,'
		ELSE
			SELECT @TSQL += '
				O.ORG_ID_LEVEL_6,'

		SELECT @TSQL += '
				OA.CHART_NUM,
				OA.ACCT_NUM,
				OA.ACCT_EXPIRATION_DATE,
				OA.acct_mgr_id,
				OA.acct_mgr_name
			HAVING        
				SUM(P.TRANS_LINE_AMT) <> 0
			'')				
'
	IF @IsDebug = 1
	BEGIN
		SELECT @MySQL = '
		INSERT INTO @ExpiredAccounts 
		EXEC(''
		SELECT 	
' +		@SelectList + '			
		FROM ' + REPLACE(@OPENQUERY+@TSQL, '''', '''''') + ''')'

		PRINT @MySQL
	END
	ELSE
	BEGIN
		INSERT INTO @ExpiredAccounts 
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
--				O.ORG_ID_LEVEL_7 AS ORG_ID,'
--			ELSE
--				SELECT @TSQL += '
--				O.ORG_ID_LEVEL_6 AS ORG_ID,'
--			SELECT @TSQL += '		
--				P.CHART_NUM,
--				P.ACCT_NUM,
--				OA.ACCT_EXPIRATION_DATE,
--				(SUM(P.TRANS_LINE_AMT) * -1) TRANS_LINE_AMT,
--				OA.ACCT_MGR_ID,
--				OA.ACCT_MGR_NAME
--			FROM 
--				FINANCE.GL_PENDING_TRANSACTIONS P
--			INNER JOIN FINANCE.ORGANIZATION_ACCOUNT OA ON 
--				P.FISCAL_YEAR = OA.FISCAL_YEAR AND
--				OA.FISCAL_PERIOD = ''''--'''' AND
--				P.CHART_NUM = OA.CHART_NUM AND
--				P.ACCT_NUM = OA.ACCT_NUM
--			INNER JOIN FINANCE.ORGANIZATION_HIERARCHY O ON 
--				OA.FISCAL_YEAR = O.FISCAL_YEAR AND 
--				O.FISCAL_PERIOD = ''''--'''' AND 
--				OA.CHART_NUM = O.CHART_NUM AND 
--				OA.ORG_ID = O.ORG_ID
--			INNER JOIN FINANCE.OBJECT OBJ ON
--				P.FISCAL_YEAR = OBJ.FISCAL_YEAR AND
--				P.CHART_NUM = OBJ.CHART_NUM AND
--				P.OBJECT_NUM = OBJ.OBJECT_NUM
--			WHERE
--				OA.OP_FUND_NUM = ''''19900'''' AND'

--			IF @UseCollegeLevelOrg = 0
--				BEGIN
--					--Use DivisionLevelOrg(s) 
--					SELECT @TSQL += '
--				O.CHART_NUM_LEVEL_5 = ''''3'''' AND O.ORG_ID_LEVEL_5 IN (' +  @QuotedOrgsString + ') AND 
--'
--				END
--			ELSE
--				BEGIN
--					-- Use CollegeLevelOrg
--					SELECT @TSQL += '
--				O.CHART_NUM_LEVEL_4 = ''''3'''' AND O.ORG_ID_LEVEL_4 = ''''' + @OrgId + ''''' AND
--'	
--					IF @OrgId = 'AAES'
--						BEGIN
--							SELECT @TSQL += '				O.CHART_NUM_LEVEL_5 = ''''3'''' AND O.ORG_ID_LEVEL_5 <> ''''ACBS'''' AND
--'
--						END
--				END

--			SELECT @TSQL += 
--'				P.FISCAL_YEAR >= ' + CONVERT(varchar(4), @FiscalYear) + ' AND 
--				P.CHART_NUM = ''''3'''' AND
--				(P.BALANCE_TYPE_CODE  IN (''''AC'''', ''''CB'''')) AND	
--				NOT (P.OBJECT_NUM IN (SELECT DISTINCT OBJECT_NUM FROM FINANCE.OBJECT_REPORTING)) AND
--				OBJ.OBJ_CONSOLIDATN_NUM IN (''''SB28'''', ''''SUB6'''') AND
--				P.OBJECT_NUM NOT IN (''''8570'''',''''8590'''',''''8970'''') AND
--				(OA.acct_expiration_date IS NOT NULL AND OA.acct_expiration_date < SYSDATE) AND'
				
--			IF LEN(@Accounts) > 0  
--			BEGIN
--				SELECT @TSQL += ' 
--				P.ACCT_NUM IN (' + @Accounts +') AND'
--			END

--			SELECT @TSQL += '
--				P.ACCT_NUM NOT IN (' + @QuotedProvisionalAccounts + ')
--			GROUP BY'

--			IF @UseCollegeLevelOrg = 0
--				SELECT @TSQL += '
--				O.ORG_ID_LEVEL_7,'
--			ELSE
--				SELECT @TSQL += '
--				O.ORG_ID_LEVEL_6,'

--			SELECT @TSQL += '
--				P.CHART_NUM,
--				P.ACCT_NUM,
--				OA.ACCT_EXPIRATION_DATE,
--				OA.ACCT_MGR_ID,
--				OA.ACCT_MGR_NAME
--			--HAVING        
--			--	SUM(P.TRANS_LINE_AMT) <> 0				
--			'')				
--'
--	IF @IsDebug = 1
--		BEGIN
--			SELECT @MySQL = '
--		INSERT INTO @ExpiredAccounts 
--		EXEC(''
--		SELECT 	
--' +		@SelectList + '			
--		FROM ' + REPLACE(@OPENQUERY+@TSQL, '''', '''''') + ''')'
--			PRINT @MySQL
--		END
--	ELSE
--		BEGIN
--			INSERT INTO @ExpiredAccounts 
--			EXEC('
--		SELECT 
--' +		@SelectList + ' 
--		FROM ' + @OPENQUERY+@TSQL + '')
--		END
	END

	IF @IsDebug = 1
		BEGIN
			SELECT @MySQL = '
		SELECT ORG_ID, CHART_NUM, ACCT_NUM, ACCT_EXPIRATION_DATE, (SUM(TRANS_LINE_AMT) * -1) TRANS_LINE_AMT, ACCT_MGR_ID, ACCT_MGR_NAME 
		FROM @ExpiredAccounts
		GROUP BY ORG_ID, CHART_NUM, ACCT_NUM, ACCT_MGR_ID, ACCT_MGR_NAME, ACCT_EXPIRATION_DATE
		HAVING SUM(TRANS_LINE_AMT) <> 0
		ORDER BY ORG_ID, CHART_NUM, ACCT_NUM, ACCT_MGR_ID, ACCT_MGR_NAME, ACCT_EXPIRATION_DATE
	'
			PRINT @MySQL
		END
	ELSE
		SELECT ORG_ID, CHART_NUM, ACCT_NUM, ACCT_EXPIRATION_DATE, (SUM(TRANS_LINE_AMT) * -1) TRANS_LINE_AMT, ACCT_MGR_ID, ACCT_MGR_NAME 
		FROM @ExpiredAccounts
		GROUP BY ORG_ID, CHART_NUM, ACCT_NUM, ACCT_MGR_ID, ACCT_MGR_NAME, ACCT_EXPIRATION_DATE
		HAVING SUM(TRANS_LINE_AMT) <> 0
		ORDER BY ORG_ID, CHART_NUM, ACCT_NUM, ACCT_MGR_ID, ACCT_MGR_NAME, ACCT_EXPIRATION_DATE
END
