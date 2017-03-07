-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/index.html

-----------------------------------------------------------------------
-- Dangerous Function
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/dangerous_function_exec_ddl.html

PROCEDURE dangerous_function IS
BEGIN
    DBMS_UTILITY.EXEC_DDL_STATEMENT@remote_db('create table t1 (id number)');
END;

-----------------------------------------------------------------------
-- SQL Bad Practices: Underspecified Identifier
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/sql_bad_practices_underspecified_identifier.html

CREATE or REPLACE FUNCTION check_permissions(
  p_name IN VARCHAR2, p_action IN VARCHAR2)
  RETURN BOOLEAN
  AUTHID CURRENT_USER
IS
  r_count NUMBER;
  perm BOOLEAN := FALSE;
BEGIN
  SELECT count(*) INTO r_count FROM PERMISSIONS
    WHERE name = p_name AND action = p_action;
  IF r_count > 0 THEN
    perm := TRUE;
  END IF;
  RETURN perm;
END check_permissions;

-----------------------------------------------------------------------
-- Code Correctness: Erroneous Null Comparison
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/code_correctness_erroneous_null_comparison_plsql.html
-- In PL/SQL, the value of NULL is indeterminate. It is not equal to anything, not even another NULL value. Also, a null value is never not equal to another value.

-- The following statement will always be false.
PROCEDURE erroneous_null_comparison IS
BEGIN
     test_bool(x = NULL);
     test_bool(x != NULL);
END;

-----------------------------------------------------------------------
-- Unreleased Resource
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/unreleased_resource.html
-- The program can potentially fail to release a system resource.

PROCEDURE unreleased_resource IS
BEGIN
  F1 := UTL_FILE.FOPEN('user_dir','u12345.tmp','R',256);
  UTL_FILE.GET_LINE(F1,V1,32767);
  -- UTL_FILE.FCLOSE(F1); is missing
END;

-----------------------------------------------------------------------
-- Unreleased Resource: Cursor Snarfing
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/unreleased_resource_cursor_snarfing.html

CREATE or REPLACE procedure PWD_COMPARE(p_user VARCHAR, p_pwd VARCHAR)
  AUTHID DEFINER
IS
  cursor INTEGER;
BEGIN
  IF p_user != 'SYS' THEN
    cursor := DBMS_SQL.OPEN_CURSOR;
    DBMS_SQL.PARSE(cursor, 'SELECT password FROM SYS.DBA_USERS WHERE username = :u', DBMS_SQL.NATIVE);
    DBMS_SQL.BIND_VARIABLE(cursor, ':u', p_user);
    -- DBMS_SQL.CLOSE_CURSOR(cursor); is missing
  END IF;
END PWD_COMPARE;

-----------------------------------------------------------------------
-- System Information Leak: External
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/system_information_leak.html
-- Revealing system data or debugging information helps an adversary learn about the system and form a plan of attack.

PROCEDURE system_information_leak_external IS
BEGIN
    HTP.htmlOpen;
      HTP.headOpen;
        HTP.title ('Environment Information');
      HTP.headClose;
      HTP.bodyOpen;
        HTP.br;
        HTP.print('Path Information: ' || OWA_UTIL.get_cgi_env('PATH_INFO') || '');
        HTP.print('Script Name: ' || OWA_UTIL.get_cgi_env('SCRIPT_NAME') || '');
        HTP.br;
      HTP.bodyClose;
    HTP.htmlClose;
END;

-----------------------------------------------------------------------
-- Trust Boundary Violation
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/trust_boundary_violation.html
-- Commingling trusted and untrusted data in the same data structure encourages programmers to mistakenly trust unvalidated data.

PROCEDURE trust_boundary_violation IS BEGIN
IF (OWA_COOKIE.get('usrname').num_vals != 0) THEN
usrname := OWA_COOKIE.get('usrname').vals(1);
END IF;
IF (v('ATTR_USR') IS null) THEN
HTMLDB_UTIL.set_session_state('ATTR_USR', usrname);
END IF;
END;

-----------------------------------------------------------------------
-- Poor Error Handling: Empty Default Exception Handler
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/poor_error_handling_empty_default_exception_handler.html
-- Ignoring an exception can cause the program to overlook unexpected states and conditions.

PROCEDURE empty_default_exception_handler IS
BEGIN
    INSERT INTO table1 VALUES(1, 2, 3, 4);
    COMMIT;
  EXCEPTION
    WHEN OTHERS THEN NULL; -- empty state block
END;

-----------------------------------------------------------------------
-- Cross-Site Scripting: Persistent
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/cross_site_scripting_persistent.html
-- Sending unvalidated data to a web browser can result in the browser executing malicious code.

-- The following code segment queries a database for an employee with a given ID and prints the corresponding employee's name.
PROCEDURE cross_site_scripting_persistent IS
BEGIN
SELECT ename INTO name FROM emp WHERE id = eid;
HTP.htmlOpen;
  HTP.headOpen;
    HTP.title ('Employee Information');
  HTP.headClose;
  HTP.bodyOpen;
    HTP.br;
    HTP.print('Employee Name: ' || name || '');
    HTP.br;
  HTP.bodyClose;
HTP.htmlClose;
END;

-- The following code segment reads an employee ID, eid, from an HTTP request and displays it to the user.
PROCEDURE cross_site_scripting_persistent_2 IS
BEGIN
eid := SUBSTR(OWA_UTIL.get_cgi_env('QUERY_STRING'), 5);
HTP.htmlOpen;
  HTP.headOpen;
    HTP.title ('Employee Information');
  HTP.headClose;
  HTP.bodyOpen;
    HTP.br;
    HTP.print('Employee ID: ' || eid || '');
    HTP.br;
  HTP.bodyClose;
HTP.htmlClose;
END;

-----------------------------------------------------------------------
-- Cross-Site Scripting: Poor Validation
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/cross_site_scripting_poor_validation.html
-- Relying on HTML, XML and other types of encoding to validate user input can result in the browser executing malicious code.

-- The following code segment reads an employee ID, eid, from an HTTP request, URL-encodes it, and displays it to the user.

PROCEDURE cross_site_scripting_poor_validation IS
BEGIN
eid := SUBSTR(OWA_UTIL.get_cgi_env('QUERY_STRING'), 5);
HTP.htmlOpen;
  HTP.headOpen;
    HTP.title ('Employee Information');
  HTP.headClose;
  HTP.bodyOpen;
    HTP.br;
    HTP.print('Employee ID: ' || HTMLDB_UTIL.url_encode(eid) || '');
    HTP.br;
  HTP.bodyClose;
HTP.htmlClose;
END;

-- The following code segment queries a database for an employee with a given ID and prints the corresponding URL-encoded employee's name.

PROCEDURE cross_site_scripting_poor_validation_2 IS
BEGIN
SELECT ename INTO name FROM emp WHERE id = eid;
HTP.htmlOpen;
  HTP.headOpen;
    HTP.title ('Employee Information');
  HTP.headClose;
  HTP.bodyOpen;
    HTP.br;
    HTP.print('Employee Name: ' || HTMLDB_UTIL.url_encode(name) || '');
    HTP.br;
  HTP.bodyClose;
HTP.htmlClose;
END;

-----------------------------------------------------------------------
-- Cross-Site Scripting: Reflected
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/cross_site_scripting_reflected.html
-- Sending unvalidated data to a web browser can result in the browser executing malicious code.

--  The following code segment reads an employee ID, eid, from an HTTP request and displays it to the user.
PROCEDURE cross_site_scripting_poor_reflected IS
BEGIN
-- Assume QUERY_STRING looks like EID=EmployeeID
eid := SUBSTR(OWA_UTIL.get_cgi_env('QUERY_STRING'), 5);
HTP.htmlOpen;
  HTP.headOpen;
    HTP.title ('Employee Information');
  HTP.headClose;
  HTP.bodyOpen;
    HTP.br;
    HTP.print('Employee ID: ' || eid || '');
    HTP.br;
  HTP.bodyClose;
HTP.htmlClose;
END;

-- The following code segment queries a database for an employee with a given ID and prints the corresponding employee's name.
PROCEDURE cross_site_scripting_poor_reflected_2 IS
BEGIN
SELECT ename INTO name FROM emp WHERE id = eid;
HTP.htmlOpen;
  HTP.headOpen;
    HTP.title ('Employee Information');
  HTP.headClose;
  HTP.bodyOpen;
    HTP.br;
    HTP.print('Employee Name: ' || name || '');
    HTP.br;
  HTP.bodyClose;
HTP.htmlClose;
END;

-----------------------------------------------------------------------
-- Denial of Service
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/denial_of_service.html
-- An attacker could cause the program to crash or otherwise become unavailable to legitimate users.

-- The following code allows a user to specify the amount of time for which the system should delay further processing. By specifying a large number, an attacker can tie up the system indefinitely.
procedure go_sleep (usrSleepTime in NUMBER) is begin
    dbms_lock.sleep(usrSleepTime);
end;

-----------------------------------------------------------------------
-- Header Manipulation
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/header_manipulation.html
-- Including unvalidated data in an HTTP response header can enable cache-poisoning, cross-site scripting, cross-user defacement, page hijacking, cookie manipulation or open redirect.

PROCEDURE header_manipulation IS
BEGIN
-- Assume QUERY_STRING looks like AUTHOR_PARAM=Name
author := SUBSTR(OWA_UTIL.get_cgi_env('QUERY_STRING'), 14);
OWA_UTIL.mime_header('text/html', false);
OWA_COOKE.send('author', author);
OWA_UTIL.http_header_close;
END;

-----------------------------------------------------------------------
-- Open Redirect
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/open_redirect.html
-- Allowing unvalidated input to control the URL used in a redirect can aid phishing attacks.

PROCEDURE open_redirect IS
BEGIN
    -- Assume QUERY_STRING looks like dest=http://www.wilyhacker.com
    dest := SUBSTR(OWA_UTIL.get_cgi_env('QUERY_STRING'), 6);
    OWA_UTIL.redirect_url('dest');
END;

-----------------------------------------------------------------------
-- Resource Injection
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/resource_injection.html
-- Allowing user input to control resource identifiers could enable an attacker to access or modify otherwise protected system resources.

PROCEDURE resource_injection IS
BEGIN
    filename := SUBSTR(OWA_UTIL.get_cgi_env('PATH_INFO'), 2);
    WPG_DOCLOAD.download_file(filename);
END;

-----------------------------------------------------------------------
-- SQL Injection
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/sql_injection.html
-- Constructing a dynamic SQL statement with input coming from an untrusted source could allow an attacker to modify the statement's meaning or to execute arbitrary SQL commands.

-----------------------------------------------------------------------
-- Access Control: Database
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/access_control_database.html

-----------------------------------------------------------------------
-- Insecure Randomness
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/insecure_randomness.html
-- Standard pseudorandom number generators cannot withstand cryptographic attacks.

CREATE or REPLACE FUNCTION CREATE_RECEIPT_URL
  RETURN VARCHAR2
AS
  rnum VARCHAR2(48);
  time TIMESTAMP;
BEGIN
  time := SYSTIMESTAMP;
  DBMS_RANDOM.SEED(time);
  rnum := DBMS_RANDOM.STRING('x', 48);
  url := 'http://test.com/' || rnum || '.html';
  RETURN url;
END;

-----------------------------------------------------------------------
-- Password Management
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/password_management.html
-- Hardcoding or storing a password in plaintext could result in a system compromise.

CREATE PROCEDURE password_plaintext AS
BEGIN
ip_address := OWA_SEC.get_client_ip;
IF ((OWA_SEC.get_user_id = 'scott') AND
    (OWA_SEC.get_password = 'tiger') AND
    (ip_address(1) = 144) and (ip_address(2) = 25)) THEN
        RETURN TRUE;
ELSE
        RETURN FALSE;
END IF;
END;

-----------------------------------------------------------------------
-- Password Management: Empty, Hardcoded, Null Password
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/password_management_password_in_comment.html
-- Empty passwords may compromise system security in a way that cannot be easily remedied.

CREATE PROCEDURE password_hardcoded AS
DECLARE
    pwd VARCHAR(20);
BEGIN
    pwd := ''; -- empty
    password := 'tiger'; --hardcoded
    password := null; -- null
END;

-----------------------------------------------------------------------
-- Password Management: Password in Comment
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/password_management_password_in_comment.html
-- Storing passwords or password details in plaintext anywhere in the system or system code may compromise system security in a way that cannot be easily remedied.

-- Default username for database connection is "scott"
-- Default password for database connection is "tiger"

-----------------------------------------------------------------------
-- Privacy Violation
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/privacy_violation.html
-- Mishandling private information, such as customer passwords or social security numbers, can compromise user privacy and is often illegal.

CREATE PROCEDURE privacy_violation AS
DECLARE
    pwd VARCHAR(20);
BEGIN
HTP.htmlOpen;
  HTP.headOpen;
    HTP.title ('.Account Information.');
  HTP.headClose;
  HTP.bodyOpen;
    HTP.br;
    HTP.print('User ID: ' || OWA_SEC.get_user_id || '');
    HTP.print('User Password: ' || OWA_SEC.get_password || '');
    HTP.br;
  HTP.bodyClose;
HTP.htmlClose;
END;

-----------------------------------------------------------------------
-- Privilege Management: Default Function or Procedure Rights
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/privilege_management_default_function_or_procedure_rights.html
-- Top-level functions or procedures without an AUTHID clause default to AUTHID DEFINER.

-----------------------------------------------------------------------
-- Privilege Management: Default Package Rights
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/privilege_management_default_package_rights.html
-- Packages without an AUTHID clause default to AUTHID DEFINER.

-----------------------------------------------------------------------
-- Privilege Management: Overly Broad Grant
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/privilege_management_overly_broad_grant.html
-- Granting all privileges on an object may give users more privileges than expected.

CREATE PROCEDURE overly_broad_grant AS
DECLARE
    pwd VARCHAR(20);
BEGIN
    GRANT ALL ON employees TO john_doe;
END;

-----------------------------------------------------------------------
-- Weak Cryptographic Hash (MD2, MD4, MD5, RIPEMD-160, and SHA-1)
-- http://www.hpenterprisesecurity.com/vulncat/en/vulncat/sql/weak_cryptographic_hash.html
-- Weak cryptographic hashes cannot guarantee data integrity and should not be used in security-critical contexts.

PROCEDURE md5 IS BEGIN
    SELECT DBMS_OBFUSCATION_TOOLKIT.md5 (input => UTL_RAW.cast_to_raw('Eddie')) md5_val FROM DUAL;
    lv_hash_value_sh1 := dbms_crypto.hash (src => utl_raw.cast_to_raw (p_string), typ   => dbms_crypto.hash_sh1);
END;

FUNCTION SHA1(STRING_TO_ENCRIPT VARCHAR2) RETURN VARCHAR2 AS 
BEGIN 
    RETURN LOWER(TO_CHAR(RAWTOHEX(SYS.DBMS_CRYPTO.HASH(UTL_RAW.CAST_TO_RAW(STRING_TO_ENCRIPT), SYS.DBMS_CRYPTO.HASH_SH1))));
END SHA1;