-- =============================================
-- Author:		Ken Taylor
-- Create date: June 4, 2013
-- Description:	Given an OrgId, and NumSingleQuotes, Return the @ExcludeAccountsString, i.e. @Result, to be substituted in the query
-- Usage:
-- SELECT dbo.udf_GetExcludeAccountsString('DANR', DEFAULT) -- DEFAULT is 2 single quotes.
-- SAMPLE Result: ''240FED1'', '' 8449999'', '' 8629999'', '' 8649999'', '' PR21050'', '' PR21056'', '' PR21057'', '' PR21058''
-- =============================================
CREATE FUNCTION [dbo].[udf_GetExcludeAccountsString]
(
	@OrgId varchar(4), --The Organizational ID
	@NumSingleQuotes smallint = 2 -- The number of of single quotes to surround quoted strings with. 
)
RETURNS varchar(MAX)
AS
BEGIN
	DECLARE @ExcludeAccountsString varchar(MAX) = ''

	DECLARE @ExcludeAccounts TABLE (ChartNum varchar(2), AccountNum varchar(7))
	INSERT INTO @ExcludeAccounts
	SELECT DISTINCT Chart, Account FROM dbo.CentralAccounts
	WHERE OrgId = @OrgId
	ORDER BY Chart, Account

	DECLARE @Account varchar(7)
	DECLARE mycursor CURSOR FOR SELECT AccountNum FROM @ExcludeAccounts ORDER BY AccountNum FOR READ ONLY

	OPEN mycursor
	FETCH NEXT FROM mycursor INTO @Account
	WHILE @@FETCH_STATUS <> -1
	BEGIN
		--Build @ExcludeAccountString
		SELECT @ExcludeAccountsString += @Account
		FETCH NEXT FROM mycursor INTO @Account
		IF @@FETCH_STATUS <> -1
			SELECT @ExcludeAccountsString += ','
	END
	CLOSE mycursor
	DEALLOCATE mycursor

	SELECT @ExcludeAccountsString = master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, @ExcludeAccountsString, DEFAULT)
	
	-- Return the result of the function
	RETURN @ExcludeAccountsString
END
