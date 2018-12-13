 CREATE PROCEDURE sp1 (in x VARCHAR(5))
BEGIN
    DECLARE passwordValue VARCHAR(50);
	SET passwordValue = 'hardcoded';
    SET passwordValue = '';
    SET passwordValue = null;
END;
 

-- Password Management: Password in Comment
-- Storing passwords or password details in plaintext anywhere in the system or system code may compromise system security in a way that cannot be easily remedied.

-- Default username for database connection is "scott"
-- Default password for database connection is "tiger"

 CREATE FUNCTION CREATE_URL_with_rand()
 RETURNS VARCHAR(200)
 BEGIN
  DECLARE rnum VARCHAR(40);
  DECLARE urlS VARCHAR(100);
  SET rnum = SUBSTRING(MD5(RAND()) FROM 1 FOR 40);
  SET urlS = 'http://test.com/' || rnum || '.html';
  RETURN urlS;
  END;

  CREATE FUNCTION compare_with_null(x INT)
 RETURNS INT
 BEGIN
    DECLARE result INT;
    if argument = null then set result = null;
    else set result = 1;
    end if;
  RETURN result;
  END;