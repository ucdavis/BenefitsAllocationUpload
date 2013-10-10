-- =============================================
-- Author:		Ken Taylor
-- Create date: June 4, 2013
-- Description:	Given an Organizational ID and NumSingleQuotes, return the @IncludeOpFundsString to be inserted in the where clause.
-- Usage: 
--		SELECT dbo.udf_GetIncludeOpFundsString('DANR', DEFAULT) --DEFAULT is 2 single quotes)
-- Sample return value:
/*
	(OA.OP_FUND_NUM = ''''69085'''' AND OA.CHART_NUM = ''''L'''') OR
	(OA.OP_FUND_NUM = ''''21050'''' AND OA.CHART_NUM = ''''L'''') OR 
	(OA.OP_FUND_NUM = ''''21056'''' AND OA.CHART_NUM = ''''L'''') OR
	(OA.OP_FUND_NUM = ''''21057'''' AND OA.CHART_NUM = ''''L'''') OR
	(OA.OP_FUND_NUM = ''''21058'''' AND OA.CHART_NUM = ''''L'''') OR
	(OA.OP_FUND_NUM = ''''21072'''' AND OA.CHART_NUM = ''''L'''')
	-- OR --
	(OA.OP_FUND_NUM = ''''19900'''' AND OA.CHART_NUM = ''''3'''')
*/
-- =============================================
CREATE FUNCTION [dbo].[udf_GetIncludeOpFundsString] 
(
	@OrgId varchar(4), -- The Organizational ID, i.e. 'DANR'
	@NumSingleQuotes smallint = 2 -- The number of of single quotes to surround quoted strings with. 
)
RETURNS varchar(MAX)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @IncludeOpFundsString varchar(MAX) = ''

	DECLARE @Chart varchar(2), @OpFund varchar(5)
	DECLARE opFundCursor CURSOR FOR SELECT * FROM dbo.udf_GetIncludeOpFunds(@OrgId) ORDER BY ChartNum, OpFund FOR READ ONLY
	OPEN opFundCursor
	FETCH NEXT FROM opFundCursor INTO @Chart, @OpFund
	WHILE @@FETCH_STATUS <> -1
	BEGIN
		--Build @IncludeOpFundsString
		SELECT @IncludeOpFundsString += '(OA.CHART_NUM = ' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, @Chart, DEFAULT) + ' AND OA.OP_FUND_NUM = ' + master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, @OpFund, DEFAULT) + ')'
		FETCH NEXT FROM opFundCursor INTO @Chart, @OpFund
		IF @@FETCH_STATUS <> -1
			SELECT @IncludeOpFundsString += ' OR
				 '
	END
	CLOSE opFundCursor
	DEALLOCATE opFundCursor

	-- Return the result of the function
	RETURN @IncludeOpFundsString
END
