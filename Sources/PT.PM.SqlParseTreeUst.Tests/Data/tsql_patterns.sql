-----------------------------------------------------------------------
-- Dangerous Function
-- Returning a list of executable files

EXEC master..xp_cmdshell 'dir *.exe'

--+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
-- Returning no output

USE master;
EXEC xp_cmdshell 'copy c:\SQLbcks\AdvWorks.bck
    \\server2\backups\SQLbcks, NO_OUTPUT';
GO

--+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
-- Using return status

DECLARE @result int;
EXEC @result = xp_cmdshell 'dir *.exe';
IF (@result = 0)
   PRINT 'Success'
ELSE
   PRINT 'Failure';

--+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
-- Writing variable contents to a file

DECLARE @cmd sysname, @var sysname;
SET @var = 'Hello world';
SET @cmd = 'echo ' + @var + ' > var_out.txt';
EXEC master..xp_cmdshell @cmd;

--+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
-- Capturing the result of a command to a file

DECLARE @cmd sysname, @var sysname;
SET @var = 'dir/p';
SET @cmd = @var + ' > dir_out.txt';
EXEC master..xp_cmdshell @cmd;

-----------------------------------------------------------------------
-- Code Correctness: Erroneous Null Comparison

SELECT * FROM MyTable WHERE MyColumn != NULL
SELECT * FROM MyTable WHERE MyColumn <> NULL
SELECT * FROM MyTable WHERE MyColumn IS NOT NULL

-----------------------------------------------------------------------
-- Unreleased Resource: Cursor Snarfing

DECLARE Employee_Cursor CURSOR FOR
SELECT EmployeeID, Title FROM AdventureWorks2012.HumanResources.Employee;
OPEN Employee_Cursor;
FETCH NEXT FROM Employee_Cursor;
WHILE @@FETCH_STATUS = 0
   BEGIN
      FETCH NEXT FROM Employee_Cursor;
   END;
--CLOSE Employee_Cursor; is missing
--DEALLOCATE Employee_Cursor; is missing
GO

-----------------------------------------------------------------------
-- Empty try catch

BEGIN TRY
    SELECT 1/0 AS DivideByZero
END TRY
BEGIN CATCH
END CATCH
GO

-----------------------------------------------------------------------
-- Denial of Service
-- An attacker could cause the program to crash or otherwise become unavailable to legitimate users.

CREATE PROCEDURE dbo.TimeDelay_hh_mm_ss @DelayLength char(8)
AS
BEGIN
WAITFOR DELAY @DelayLength;
END;
GO

-----------------------------------------------------------------------
-- Insecure Randomness
SELECT RAND(100), RAND(), RAND()
GO

-----------------------------------------------------------------------
-- Password Management: Empty Password
-- Empty passwords may compromise system security in a way that cannot be easily remedied.

CREATE PROCEDURE test_proc AS
DECLARE
    @pwd VARCHAR(20);
BEGIN
    SET @pwd = ''; -- empty
    SET @pwd = 'tiger'; -- hardcoded
    SET @pwd = null; -- null
END;
GO

-----------------------------------------------------------------------
-- Password Management: Password in Comment
-- Storing passwords or password details in plaintext anywhere in the system or system code may compromise system security in a way that cannot be easily remedied.

-- Default username for database connection is "scott"
-- Default password for database connection is "tiger"

-----------------------------------------------------------------------
-- Privilege Management: Overly Broad Grant

GRANT ALL ON employees TO john_doe;

-----------------------------------------------------------------------
-- Weak Cryptographic Hash (MD2, MD4, MD5, RIPEMD-160, and SHA-1)

SELECT HashBytes('MD5', 'email@dot.com')
SELECT CONVERT(NVARCHAR(32), HashBytes('MD5', 'email@dot.com'),2)
