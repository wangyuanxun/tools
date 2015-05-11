ALTER PROCEDURE [dbo].[SP_Pagination] 
/* 
*************************************************************** 
** ǧ����������ҳ�洢���� ** 
*************************************************************** 
����˵��: 
1.Tables :������,��ͼ 
2.PrimaryKey :���ؼ��� 
3.Sort :������䣬����Order By ���磺NewsID Desc,OrderRows Asc 
4.CurrentPage :��ǰҳ�� 
5.PageSize :��ҳ�ߴ� 
6.Filter :������䣬����Where 
7.Group :Group���,����Group By 
Ч����ʾ��http://www.cn5135.com/_App/Enterprise/QueryResult.aspx 
***************************************************************/ 
( 
@Tables varchar(1000), 
@PrimaryKey varchar(100), 
@Sort varchar(200) = NULL, 
@CurrentPage int = 1, 
@PageSize int = 10, 
@Fields varchar(1000) = '*', 
@Filter varchar(1000) = NULL, 
@Group varchar(1000) = NULL 
) 
AS 
/*Ĭ������*/ 
IF @Sort IS NULL OR @Sort = '' 
SET @Sort = @PrimaryKey 
DECLARE @SortTable varchar(100) 
DECLARE @SortName varchar(100) 
DECLARE @strSortColumn varchar(200) 
DECLARE @operator char(2) 
DECLARE @type varchar(100) 
DECLARE @prec int 
/*�趨�������.*/ 
IF CHARINDEX('DESC',@Sort)>0 
BEGIN 
SET @strSortColumn = REPLACE(@Sort, 'DESC', '') 
SET @operator = ' <=' 
END 
ELSE 
BEGIN 
IF CHARINDEX('ASC', @Sort) = 0 
SET @strSortColumn = REPLACE(@Sort, 'ASC', '') 
SET @operator = '>=' 
END 
IF CHARINDEX('.', @strSortColumn) > 0 
BEGIN 
SET @SortTable = SUBSTRING(@strSortColumn, 0, CHARINDEX('.',@strSortColumn)) 
SET @SortName = SUBSTRING(@strSortColumn, CHARINDEX('.',@strSortColumn) + 1, LEN(@strSortColumn)) 
END 
ELSE 
BEGIN 
SET @SortTable = @Tables 
SET @SortName = @strSortColumn 
END 
SELECT @type=t.name, @prec=c.prec 
FROM sysobjects o 
JOIN syscolumns c on o.id=c.id 
JOIN systypes t on c.xusertype=t.xusertype 
WHERE o.name = @SortTable AND c.name = @SortName 
IF CHARINDEX('char', @type) > 0 
SET @type = @type + '(' + CAST(@prec AS varchar) + ')' 
DECLARE @strPageSize varchar(50) 
DECLARE @strStartRow varchar(50) 
DECLARE @strFilter varchar(1000) 
DECLARE @strSimpleFilter varchar(1000) 
DECLARE @strGroup varchar(1000) 
/*Ĭ�ϵ�ǰҳ*/ 
IF @CurrentPage < 1 
SET @CurrentPage = 1 
/*���÷�ҳ����.*/ 
SET @strPageSize = CAST(@PageSize AS varchar(50)) 
SET @strStartRow = CAST(((@CurrentPage - 1)*@PageSize + 1) AS varchar(50)) 
/*ɸѡ�Լ��������.*/ 
IF @Filter IS NOT NULL AND @Filter != '' 
BEGIN 
SET @strFilter = ' WHERE ' + @Filter + ' ' 
SET @strSimpleFilter = ' AND ' + @Filter + ' ' 
END 
ELSE 
BEGIN 
SET @strSimpleFilter = '' 
SET @strFilter = '' 
END 
IF @Group IS NOT NULL AND @Group != '' 
SET @strGroup = ' GROUP BY ' + @Group + ' ' 
ELSE 
SET @strGroup = '' 
/*ִ�в�ѯ���*/ 
EXEC( 
' 
DECLARE @SortColumn ' + @type + ' 
SET ROWCOUNT ' + @strStartRow + ' 
SELECT @SortColumn=' + @strSortColumn + ' FROM ' + @Tables + @strFilter + ' ' + @strGroup + ' ORDER BY ' + @Sort + ' 
SET ROWCOUNT ' + @strPageSize + ' 
SELECT ' + @Fields + ' FROM ' + @Tables + ' WHERE ' + @strSortColumn + @operator + ' @SortColumn ' + @strSimpleFilter + ' ' + @strGroup + ' ORDER BY ' + @Sort + ' 
' 
)
