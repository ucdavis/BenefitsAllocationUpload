-- =============================================
-- Author:		Ken Taylor
-- Create date: June 4, 2013
-- Description:	Given an Organizational ID, return the corresponding chart number
-- Usage:
/*
	SELECT dbo.udf_GetChartNumStringForOrg('DANR', DEFAULT) -- DEFAULT is 2 single quotes.

	--Sample results:
	''L''
*/
-- =============================================
CREATE FUNCTION [dbo].[udf_GetChartNumStringForOrg]
(
	@OrgId varchar(4), --The Organizational ID
	@NumSingleQuotes smallint = 2 -- The number of of single quotes to surround quoted strings with. 
)
RETURNS varchar(10)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @ChartNumString varchar(10) = ''

	DECLARE @Chart3 varchar(2) = '3', @ChartNum varchar(2)

	-- Add the T-SQL statements to compute the return value here
	SELECT @ChartNum = (SELECT DISTINCT [Chart]
	FROM dbo.CentralAccounts
	WHERE OrgId = @OrgId)

	IF @ChartNum IS NULL 
		SELECT @ChartNum = @Chart3

	SELECT @ChartNumString = dbo.udf_CreateQuotedStringList(@NumSingleQuotes, RTRIM(@ChartNum), DEFAULT)

	-- Return the result of the function
	RETURN @ChartNumString

END
