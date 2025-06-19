mkdir -p ~/Documents/oracle-datapump

docker exec -it oracle-db /bin/bash

sqlplus ADMINPDB/123@//localhost:1521/PDB;

-- Tạo directory object
CREATE OR REPLACE DIRECTORY Orcl_full AS '/opt/oracle/oradata/backups';
-- Thoát khỏi sqlplus
exit;

sqlplus sys/123456@//localhost:1521/PDB as sysdba


-- Cấp quyền đọc/ghi trên directory cho user
GRANT READ, WRITE ON DIRECTORY Orcl_full TO adminpdb;

-- Cấp quyền thực hiện Data Pump
GRANT DATAPUMP_EXP_FULL_DATABASE TO adminpdb;
GRANT DATAPUMP_IMP_FULL_DATABASE TO adminpdb;

-- Thoát
exit;


expdp adminpdb/123@//localhost:1521/PDB \
directory=Orcl_full \
dumpfile=pdb_export.dmp \
logfile=pdb_export.log \
full=y


-- Tạo PDB
CREATE PLUGGABLE DATABASE PDB_CLONE
  ADMIN USER AdminPdb IDENTIFIED BY 123
  ROLES = (DBA)
  FILE_NAME_CONVERT = ('/pdbseed/', '/PDB_CLONE/');

-- Mở PDB nếu chưa mở 
CREATE OR REPLACE PROCEDURE Open_PDB_If_Closed(p_pdb_name IN VARCHAR2) 
IS
    v_open_mode VARCHAR2(20);
BEGIN
    SELECT open_mode INTO v_open_mode
    FROM v$pdbs
    WHERE name = UPPER(p_pdb_name);

    IF v_open_mode != 'READ WRITE' THEN
        EXECUTE IMMEDIATE 'ALTER PLUGGABLE DATABASE ' || p_pdb_name || ' OPEN';
        DBMS_OUTPUT.PUT_LINE('PDB_CLONE ' || p_pdb_name || ' đã được mở.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('PDB_CLONE ' || p_pdb_name || ' đã mở sẵn.');
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('Không tìm thấy PDB_CLONE tên: ' || p_pdb_name);
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Lỗi: ' || SQLERRM);
END;
/
EXECUTE Open_PDB_If_Closed('PDB_CLONE');
/

-- Kết nối vào PDB
ALTER SESSION SET CONTAINER = PDB_CLONE;

-- Cấp quota cho AdminPdb trên tablespace SYSTEM
ALTER USER AdminPdb QUOTA UNLIMITED ON SYSTEM;


impdp adminpdb/123@//localhost:1521/PDB_CLONE \
directory=Orcl_full \
dumpfile=pdb_export.dmp \
logfile=pdb_import.log \
full=y
