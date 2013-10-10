-- =============================================
-- Author:		Ken Taylor
-- Create date: June 4, 2013
-- Description:	Given an Organizational ID and NumSingleQuotes, 
-- return a string of quoted accounts to be included in the where clause.
-- Usage:
/*
	SELECT dbo.udf_GetIncludeAccountsString('SSCI', DEFAULT)
	-- Sample results:
	''ANTGENA'', ''ANTGENT'', ''COMGENA'', ''COMGENT'', ''ECNGENA'', ''ECNGENT'', ''HISGENA'', ''HISGENT'', ''IREGENT'', ''JESGENT'', ''LNGGENA'', ''LNGGENT'', ''MESAGNT'', ''PHIGENA'', ''PHIGENT'', ''PLSGENA'', ''POLGENT'', ''PSCGENA'', ''PSCGENT'', ''SHRGEND'', ''SITGEND'', ''SOCGENA'', ''SOCGENT'', ''SRSGEND'', ''SSBGEND'', ''SSGGEND'', ''SSIGEXD'', ''SSOGEND'', ''SSRGEND'', ''SSYGEND'', ''STSGENA'', ''STSGENT'', ''UEAGEND''
*/
-- Notes:  Assumes that the included accounts are chart indifferent.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetIncludeAccountsString] 
(
	-- Add the parameters for the function here
	@OrgId varchar(4), --The Organizational ID
	@NumSingleQuotes smallint = 2 -- The number of of single quotes to surround quoted strings with. 
)
RETURNS varchar(MAX)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @IncludeAccountsString varchar(MAX) = ''
	-- Begin build the Accounts if accounts are present in the ReimbursableBenefitsAccounts table.
	DECLARE @Account varchar(7) = ''

	DECLARE mycursor CURSOR FOR
		SELECT [Account]
		FROM [dbo].ReimbursableBenefitsAccounts
		WHERE isactive = 1 AND OrgID = @OrgId
		ORDER BY Account
		FOR READ ONLY

	OPEN mycursor
	FETCH NEXT FROM mycursor INTO @Account
	WHILE @@FETCH_STATUS <> -1
		BEGIN
			SELECT @IncludeAccountsString += @Account;
			FETCH NEXT FROM mycursor INTO @Account
			IF @@FETCH_STATUS <> -1
			SELECT @IncludeAccountsString += ',';
		END
	CLOSE mycursor
	DEALLOCATE mycursor
	-- end Build the Accounts if accounts are present in the ReimbursableBenefitsAccounts table.
	
	-- Add the appropriate number of single quotes between account numbers:
	IF LEN(@IncludeAccountsString) > 0  
		SELECT @IncludeAccountsString = (SELECT master.dbo.udf_CreateQuotedStringList(@NumSingleQuotes, @IncludeAccountsString, DEFAULT))

	-- Return the result of the function
	RETURN @IncludeAccountsString

END
