-- =============================================
-- Author:		Ken Taylor
-- Create date: April 17, 2013
-- Description:	Given an FIS OrgID, return the corresponding OrgLevel
-- Usage:
/*
-- AAES returns @OrgLevel = 4, i.e. CollegeLevelOrg
USE [FISDataMart]
GO

DECLARE @OrgLevel smallint

EXEC	[dbo].[usp_GetOrgLevel]
		@OrgId = N'AAES', @OrgLevel = @OrgLevel OUTPUT
GO

-- SSCI returns @OrgLevel = 5, i.e. DivisionLevelOrg
USE [FISDataMart]
GO

DECLARE @OrgLevel smallint

EXEC	[dbo].[usp_GetOrgLevel]
		@OrgId = N'SSCI', @OrgLevel = @OrgLevel OUTPUT
GO
*/
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetOrgLevel] 
(
	@OrgId varchar(4),
	@IsDebug bit = 0,
	@OrgLevel smallint OUTPUT
)
AS
BEGIN
	DECLARE @OPENQUERY nvarchar(1024), @TSQL nvarchar(MAX), @LinkedServer nvarchar(128)
	SET @LinkedServer = 'FIS_DS'
	SET @OPENQUERY = 'SELECT @OrgLevel = (SELECT TOP 1
	ORG_HIERARCHY_LEVEL AS OrgLevel 
FROM OPENQUERY('+ @LinkedServer + ','''

	SELECT @TSQL = 'SELECT DISTINCT
	ORG_HIERARCHY_LEVEL
  FROM
	FINANCE.ORGANIZATION_HIERARCHY Orgs
  WHERE 
	Orgs.FISCAL_YEAR = 9999 AND
	Orgs.FISCAL_PERIOD =  ' + master.dbo.udf_CreateQuotedStringList(default, '--', default) + ' AND
	Orgs.CHART_NUM = ' + master.dbo.udf_CreateQuotedStringList(default, '3', default)  + ' AND
	Orgs.ORG_ID = ' + master.dbo.udf_CreateQuotedStringList(default, @OrgId, default) + 
	'
''))'
 
 DECLARE @Statement nvarchar(MAX) = @OPENQUERY+@TSQL
 EXEC sp_executesql @Statement, N'@OrgLevel smallint OUTPUT', @OrgLevel OUTPUT;

 IF @IsDebug = 1
	PRINT 'EXEC sp_executesql ' + @Statement + ', N''@OrgLevel smallint OUTPUT'', @OrgLevel OUTPUT;'
 ELSE
	EXEC sp_executesql @Statement, N'@OrgLevel smallint OUTPUT', @OrgLevel OUTPUT;

END
