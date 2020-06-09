-- =============================================
-- Author:		Ken Taylor
-- Create date: June 4, 2013
-- Description:	Given an Organizational ID and NumSingleQuotes, return the @ExcludeObjectsString to be substituted in the where clause.
-- Usage:
/*
	SELECT dbo.udf_GetExcludeObjectsString('AAES', DEFAULT) --DEFAULT is 2 single quotes

	Sample return value:
	''8570'', ''8590'', ''8970''
*/
-- Modifications:
--	2013-06-17 by kjt: Removed Object Code 8970 to be included by default for all orgs
--		as DANR is now going to fund ALL objects as per Regina Ranoa 2013-06-17.  8970 is now included for orgs that use OP Fund 19900 only.
--	2013-06-21 by kjt: Removed excluded objects for all uses.  Basically this is just a place holder until we
	-- design more complicated logic that removes the expenses associated with these objects.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetExcludeObjectsString_20200602_bak] 
(
	-- Add the parameters for the function here
	@OrgId varchar(4), -- The Organizational ID
	@NumSingleQuotes smallint = 2 -- The number of of single quotes to surround quoted strings with. 
)
RETURNS varchar(MAX)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @ExcludeObjectsString varchar(MAX) = ''

	DECLARE @ExcludeObjects TABLE (Object varchar(4))
	-- 2013-06-21 by kjt: Removed excluded objects for all uses.  Basically this is just a place holder until we
	-- design more complicated logic that removes the expenses associated with these objects.
	--IF EXISTS(SELECT 1 FROM dbo.udf_GetIncludeOpFunds(@OrgId) WHERE OpFund = '19900')
	--	INSERT INTO @ExcludeObjects VALUES ('8570'), ('8590'), ('8970') --NON-RESIDENT FEE REMISSION Always applicable for exclusion

	-- 2013-06-17 by kjt: DANR is now going to fund every object as per Regina Ranoa.
	--INSERT INTO @ExcludeObjects VALUES ('8970') --NON-RESIDENT FEE REMISSION Always applicable for exclusion

	DECLARE @Object varchar(4)
	DECLARE objectCursor CURSOR FOR SELECT * FROM @ExcludeObjects ORDER BY [Object] FOR READ ONLY
	OPEN objectCursor
	FETCH NEXT FROM objectCursor INTO @Object
	WHILE @@FETCH_STATUS <> -1
	BEGIN
		SELECT @ExcludeObjectsString += @Object
		FETCH NEXT FROM objectCursor INTO @Object
		IF @@FETCH_STATUS <> -1
			SELECT @ExcludeObjectsString += ','
	END
	CLOSE objectCursor
	DEALLOCATE objectCursor

	IF LEN(@ExcludeObjectsString) > 0
		SELECT @ExcludeObjectsString = dbo.udf_CreateQuotedStringList(@NumSingleQuotes, @ExcludeObjectsString, DEFAULT)

	-- Return the result of the function
	RETURN @ExcludeObjectsString
END