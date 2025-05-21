
--chuyển về CDB$ROOT trước khi xóa PDB
ALTER SESSION SET CONTAINER = CDB$ROOT;

-- Xóa và tạo lại PDB nếu có
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

--Tạo PDB
CREATE PLUGGABLE DATABASE PDB
ADMIN USER AdminPdb IDENTIFIED BY 123
ROLES = (DBA)
FILE_NAME_CONVERT = ('/pdbseed/', '/PDB/');

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
-- CREATE OR REPLACE DIRECTORY DATA_DIR AS '/Users/jakepham/Documents/SourceCode/new/AvaloniaPdbAccounts/Script';
-- GRANT READ, WRITE ON DIRECTORY DATA_DIR TO AdminPdb;


--Đăng nhập PDB với tài khoản AdminPdb
conn AdminPdb/123@localhost:1521/PDB;
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

-- ---------------------------------------------------------
-- TẠO LẠI TABLE, VIEW, PROCEDURE, FUNCTION
-- ---------------------------------------------------------

-- 1. Tạo Table
CREATE TABLE employee (
    emp_id NUMBER PRIMARY KEY,
    emp_name VARCHAR2(100),
    salary NUMBER,
    department_id NUMBER
);

-- 2. Tạo View
CREATE VIEW vw_employee AS
SELECT emp_id, emp_name FROM employee;

-- 3. Tạo Stored Procedure
CREATE OR REPLACE PROCEDURE sp_raise_salary(p_emp_id NUMBER, p_amount NUMBER) IS
BEGIN
    UPDATE employee SET salary = salary + p_amount WHERE emp_id = p_emp_id;
END;
/

-- 4. Tạo Function
CREATE OR REPLACE FUNCTION fn_get_salary(p_emp_id NUMBER) RETURN NUMBER IS
    v_salary NUMBER;
BEGIN
    SELECT salary INTO v_salary FROM employee WHERE emp_id = p_emp_id;
    RETURN v_salary;
END;
/

-- 5. Insert dữ liệu mẫu
INSERT INTO employee VALUES (1, 'Alice', 5000, 10);
INSERT INTO employee VALUES (2, 'Bob', 6000, 20);
INSERT INTO employee VALUES (3, 'Charlie', 7000, 30);
INSERT INTO employee VALUES (4, 'David', 7500, 40);
INSERT INTO employee VALUES (5, 'Eva', 8000, 50);
INSERT INTO employee VALUES (6, 'Frank', 8500, 60);
INSERT INTO employee VALUES (7, 'Grace', 9000, 70);
INSERT INTO employee VALUES (8, 'Hannah', 9500, 80);
COMMIT;

-- 6. Tạo tài khoản cho từng nhân viên
DECLARE
    CURSOR c_employees IS
        SELECT emp_name FROM employee; -- lấy tên nhân viên từ bảng employee
    v_username VARCHAR2(100);
BEGIN
    FOR emp_rec IN c_employees LOOP
        -- Username: bỏ khoảng trắng trong tên, để an toàn
        v_username := REPLACE(emp_rec.emp_name, ' ', '');

        BEGIN
            EXECUTE IMMEDIATE 'CREATE USER ' || v_username || 
                              ' IDENTIFIED BY ' || v_username || 
                              ' DEFAULT TABLESPACE SYSTEM QUOTA UNLIMITED ON SYSTEM';
            EXECUTE IMMEDIATE 'GRANT CREATE SESSION TO ' || v_username;

            DBMS_OUTPUT.PUT_LINE('Đã tạo tài khoản: ' || v_username);
        EXCEPTION
            WHEN OTHERS THEN
                DBMS_OUTPUT.PUT_LINE('Không thể tạo tài khoản: ' || v_username || ' - Lỗi: ' || SQLERRM);
        END;
    END LOOP;
END;
/

-- Tạo role KETOAN
CREATE ROLE KETOAN;
CREATE ROLE QUANLY;
CREATE ROLE HR;
CREATE ROLE NHANVIEN;
CREATE ROLE NVCB;
CREATE ROLE GV;
CREATE ROLE NVPDT;
CREATE ROLE NVPKT;
CREATE ROLE NVTCHC;
CREATE ROLE NVCTSV;
CREATE ROLE TRGDV;
CREATE ROLE SV;



QUIT;