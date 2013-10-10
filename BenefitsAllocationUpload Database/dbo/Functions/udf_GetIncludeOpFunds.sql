-- =============================================
-- Author:		Ken Taylor
-- Create date: June 4, 2013
-- Description:	Given an Organizational ID, return a list of OpFunds to be included.
-- Usage:
-- SELECT * FROM dbo.udf_GetIncludeOpFunds('DANR')
-- Sample Return:
/*
ChartNum	OpFund
L	21050
L	21056
L	21057
L	21058
L	21072
L	69085
*/
-- =============================================
CREATE FUNCTION [dbo].[udf_GetIncludeOpFunds] 
(
	-- Add the parameters for the function here
	@OrgId varchar(4) --The Organizational ID, i.e. 'DANR'
)
RETURNS 
@IncludeOpFunds TABLE 
(
	-- Add the column definitions for the TABLE variable here
	ChartNum varchar(2), 
	OpFund varchar(5)
)
AS
BEGIN
	-- Fill the table variable with the rows for your result set
	IF EXISTS(SELECT 1 FROM dbo.CentralAccounts WHERE OrgId = @OrgId AND OpFund IS NOT NULL)
		INSERT INTO @IncludeOpFunds
		SELECT DISTINCT Chart, OPFund FROM dbo.CentralAccounts WHERE OrgId = @OrgId ORDER BY OpFund
	ELSE
		INSERT INTO @IncludeOpFunds
		SELECT '3', '19900'
	RETURN 
END
