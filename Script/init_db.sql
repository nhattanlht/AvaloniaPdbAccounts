-- ---------------------------------------------------------
-- Init PDB 
--Lưu ý cần đổi FILE_NAME_CONVERT cho phù hợp trên máy để chạy local
-- ---------------------------------------------------------


-- Chuyển về CDB$ROOT trước khi xóa PDB
ALTER SESSION SET CONTAINER = CDB$ROOT;

-- Xóa PDB nếu có
BEGIN
    EXECUTE IMMEDIATE 'ALTER PLUGGABLE DATABASE PDB CLOSE IMMEDIATE';
EXCEPTION
    WHEN OTHERS THEN NULL; -- Bỏ qua nếu PDB không tồn tại
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP PLUGGABLE DATABASE PDB INCLUDING DATAFILES';
EXCEPTION
    WHEN OTHERS THEN NULL; -- Bỏ qua nếu PDB không tồn tại
END;
/

-- Tạo PDB
CREATE PLUGGABLE DATABASE PDB
  ADMIN USER AdminPdb IDENTIFIED BY 123
  ROLES = (DBA)
  FILE_NAME_CONVERT = (
    'D:\Installed\app\oracle\oradata\ORCL21\PDBSEED\', 
    'D:\Installed\app\oracle\oradata\ORCL21\PDB\'
);

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
        DBMS_OUTPUT.PUT_LINE('PDB ' || p_pdb_name || ' đã được mở.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('PDB ' || p_pdb_name || ' đã mở sẵn.');
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('Không tìm thấy PDB tên: ' || p_pdb_name);
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Lỗi: ' || SQLERRM);
END;
/
EXECUTE Open_PDB_If_Closed('PDB');
/

-- Kết nối vào PDB
ALTER SESSION SET CONTAINER = PDB;

-- Cấp quota cho AdminPdb trên tablespace SYSTEM
ALTER USER AdminPdb QUOTA UNLIMITED ON SYSTEM;

-- Cấp quyền cho AdminPdb
GRANT EXECUTE ON DBMS_FGA TO AdminPdb;
GRANT SELECT ON DBA_FGA_AUDIT_TRAIL TO AdminPdb;

-- Đăng nhập PDB với tài khoản AdminPdb
conn AdminPdb/123@localhost:1521/PDB;

-- Xóa các đối tượng nếu tồn tại
-- Xóa View
BEGIN
    EXECUTE IMMEDIATE 'DROP VIEW vw_employee';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN -- -942: view không tồn tại
            RAISE;
        END IF;
END;
/

-- Xóa Procedure
BEGIN
    EXECUTE IMMEDIATE 'DROP PROCEDURE sp_raise_salary';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -4043 THEN -- -4043: object does not exist
            RAISE;
        END IF;
END;
/

-- Xóa Function
BEGIN
    EXECUTE IMMEDIATE 'DROP FUNCTION fn_get_salary';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -4043 THEN
            RAISE;
        END IF;
END;
/

-- Xóa Table
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE employee CASCADE CONSTRAINTS';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN
            RAISE;
        END IF;
END;
/

QUIT;