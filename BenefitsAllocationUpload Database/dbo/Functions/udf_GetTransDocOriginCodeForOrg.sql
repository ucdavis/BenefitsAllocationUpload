-- =============================================
-- Author:		Ken Taylor
-- Create date: April 17, 2013
-- Description:	Given an OrgId, return the corresponding TransDocOriginCode, i.e. FS_ORIGIN_CODE, from FISDataMart.dbo.CentralAccounts.
-- Defaults to returning 'AG' if no match is found. 
-- Usage:
-- SELECT dbo.udf_GetTransDocOriginCodeForOrg('SSCI')
-- SELECT dbo.udf_GetTransDocOriginCodeForOrg(DEFAULT) or SELECT dbo.udf_GetTransDocOriginCodeForOrg('AAES') 
-- =============================================
CREATE FUNCTION [dbo].[udf_GetTransDocOriginCodeForOrg] 
(
	-- Add the parameters for the function here
	@OrgId varchar(4) = 'AAES' --DEFAULT
)
RETURNS char(2)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @TransDocOriginCode varchar(2) = 'AG'
	DECLARE @Result char(2)

	-- Add the T-SQL statements to compute the return value here
	SELECT @Result = (SELECT DISTINCT [TransDocOriginCode]
	FROM dbo.CentralAccounts
	WHERE OrgId = @OrgId)

	IF @Result IS NULL 
		SELECT @Result = @TransDocOriginCode

	-- Return the result of the function
	RETURN @Result

END
