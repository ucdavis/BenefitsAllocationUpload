-- =============================================
-- Author:		Ken Taylor
-- Create date: April 18, 2013
-- Description:	Given SchoolCode, return the corresponding OrgId from FISDataMart.dbo.CentralAccounts.
-- Returns NULL if no match is found. 
-- Usage:
-- SELECT dbo.udf_GetOrgIdForSchoolCode('01')
-- =============================================
CREATE FUNCTION [dbo].[udf_GetOrgIdForSchoolCode] 
(
	-- Add the parameters for the function here
	@SchoolCode varchar(2)
)
RETURNS varchar(4)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Result varchar(4)

	-- Add the T-SQL statements to compute the return value here
	SELECT @Result = (SELECT DISTINCT [OrgId]
	FROM dbo.CentralAccounts
	WHERE SchoolCode = @SchoolCode)

	-- Return the result of the function
	RETURN @Result

END
