SELECT username FROM dba_users WHERE common = 'NO' ORDER BY username;


conn NV00016/123@localhost:1521/PDB;
SELECT GRANTED_ROLE FROM USER_ROLE_PRIVS;


SELECT *
FROM
    dba_tab_privs
WHERE
    grantee != 'Public';




SELECT
*
FROM
    dba_tab_privs;


