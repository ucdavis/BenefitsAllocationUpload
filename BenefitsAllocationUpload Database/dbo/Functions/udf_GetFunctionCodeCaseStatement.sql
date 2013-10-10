-- =============================================
-- Author:		Ken Taylor
-- Create date: June 4, 2013
-- Description:	Given an Organizational ID and the NumSingleQuotes, 
-- return the FunctionCode CASE statement that is to be used in the select statement, where and grouping statemnemt
-- Usage:
/*
	SELECT dbo.udf_GetFunctionCodeCaseStatement('DANR', DEFAULT) -- DEFAULT is 2 single quotes.

	--Sample result:
				CASE WHEN OA.OP_FUND_NUM = ''69085'' THEN
					CASE WHEN HIGHER_ED_FUNC_CODE = ''ORES'' OR SUBSTR(OA.A11_ACCT_NUM, 1, 2) BETWEEN ''44'' AND ''59'' THEN ''ORES''
						 WHEN HIGHER_ED_FUNC_CODE = ''PBSV'' OR SUBSTR(OA.A11_ACCT_NUM, 1, 2) IN (''62'') THEN ''PBSV''
						 WHEN HIGHER_ED_FUNC_CODE = ''MOPP'' OR SUBSTR(OA.A11_ACCT_NUM, 1, 2) IN (''64'') THEN ''MOPP''
					END 
				ELSE OA.OP_FUND_NUM END
*/
-- =============================================
CREATE FUNCTION [dbo].[udf_GetFunctionCodeCaseStatement] 
(
	-- Add the parameters for the function here
	@OrgId varchar(4), -- The Organizational ID
	@NumSingleQuotes smallint = 2 -- The number of of single quotes to surround quoted strings with. 
)
RETURNS varchar(MAX)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @CASE_FUNCTION_CODE varchar(MAX) = ''

	IF EXISTS(SELECT 1 FROM dbo.udf_GetIncludeOpFunds(@OrgId) WHERE ChartNum = 'L' AND OpFund = '69085')
	BEGIN
		SELECT @CASE_FUNCTION_CODE += '
				CASE WHEN OA.OP_FUND_NUM = ' +  master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, '69085', DEFAULT) + ' THEN
					CASE WHEN HIGHER_ED_FUNC_CODE = ' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, 'ORES', DEFAULT) + ' OR SUBSTR(OA.A11_ACCT_NUM, 1, 2) BETWEEN ' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, '44', DEFAULT) + ' AND ' +  master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, '59', DEFAULT) + ' THEN ' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, 'ORES', DEFAULT) + '
						 WHEN HIGHER_ED_FUNC_CODE = ' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, 'PBSV', DEFAULT) + ' OR SUBSTR(OA.A11_ACCT_NUM, 1, 2) IN (' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, '62', DEFAULT) + ') THEN ' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, 'PBSV', DEFAULT) + '
						 WHEN HIGHER_ED_FUNC_CODE = ' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, 'MOPP', DEFAULT) + ' OR SUBSTR(OA.A11_ACCT_NUM, 1, 2) IN (' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, '64', DEFAULT) + ') THEN ' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, 'MOPP', DEFAULT) + '
					END 
				ELSE OA.OP_FUND_NUM END'
	END
	ELSE
		SELECT @CASE_FUNCTION_CODE += '
				CASE WHEN HIGHER_ED_FUNC_CODE = ' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, 'ORES', DEFAULT) + ' OR 
					SUBSTR(OA.A11_ACCT_NUM, 1, 2) BETWEEN ' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, '44', DEFAULT) + ' AND ' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, '59', DEFAULT) + ' OR 
					SUBSTR(OA.A11_ACCT_NUM, 1, 2) IN (' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, '62', DEFAULT) + ') THEN ' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, 'R', DEFAULT) + ' 
				ELSE ' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, 'I', DEFAULT) + ' END'

	-- Return the result of the function
	RETURN @CASE_FUNCTION_CODE
END
