-- ---------------------------------------------------------
-- Quản lý PDB
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
FILE_NAME_CONVERT = ('D:\installed\app\oracle\oradata\orcl21\pdbseed\', 'D:\installed\app\oracle\oradata\orcl21\pdbseed\pdb\');

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

-- ---------------------------------------------------------
-- Tạo role
-- ---------------------------------------------------------
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

-- ---------------------------------------------------------
-- Tạo tài khoản cho nhân viên và sinh viên
-- ---------------------------------------------------------

DECLARE
    v_count NUMBER; -- Biến đếm để kiểm tra sự tồn tại của user
BEGIN
    -- Tạo tài khoản cho nhân viên
    FOR nv IN (SELECT MANV FROM NHANVIEN) LOOP
        -- Kiểm tra xem username (mã nhân viên) đã tồn tại chưa
        SELECT COUNT(*) INTO v_count 
        FROM all_users 
        WHERE username = nv.MANV;

        IF v_count = 0 THEN
            -- Tạo user với password là 123
            EXECUTE IMMEDIATE 'CREATE USER "' || nv.MANV || '" IDENTIFIED BY 123';
            -- Gán quyền kết nối
            EXECUTE IMMEDIATE 'GRANT CREATE SESSION TO "' || nv.MANV || '"';
            -- Gán quyền đọc trên các bảng cần thiết
            EXECUTE IMMEDIATE 'GRANT SELECT ON AdminPdb.NHANVIEN TO "' || nv.MANV || '"';
            EXECUTE IMMEDIATE 'GRANT SELECT ON AdminPdb.DONVI TO "' || nv.MANV || '"';
            DBMS_OUTPUT.PUT_LINE('Đã tạo tài khoản cho nhân viên ' || nv.MANV);
        ELSE
            DBMS_OUTPUT.PUT_LINE('Tài khoản ' || nv.MANV || ' đã tồn tại');
        END IF;
    END LOOP;

    -- Tạo tài khoản cho sinh viên
    FOR sv IN (SELECT MASV FROM SINHVIEN) LOOP
        -- Kiểm tra xem username (mã sinh viên) đã tồn tại chưa
        SELECT COUNT(*) INTO v_count 
        FROM all_users 
        WHERE username = sv.MASV;

        IF v_count = 0 THEN
            -- Tạo user với password là 123
            EXECUTE IMMEDIATE 'CREATE USER "' || sv.MASV || '" IDENTIFIED BY 123';
            -- Gán quyền kết nối
            EXECUTE IMMEDIATE 'GRANT CREATE SESSION TO "' || sv.MASV || '"';
            -- Gán quyền đọc trên các bảng cần thiết
            EXECUTE IMMEDIATE 'GRANT SELECT ON AdminPdb.SINHVIEN TO "' || sv.MASV || '"';
            EXECUTE IMMEDIATE 'GRANT SELECT ON AdminPdb.DANGKY TO "' || sv.MASV || '"';
            EXECUTE IMMEDIATE 'GRANT SELECT ON AdminPdb.MOMON TO "' || sv.MASV || '"';
            EXECUTE IMMEDIATE 'GRANT SELECT ON AdminPdb.HOCPHAN TO "' || sv.MASV || '"';
            DBMS_OUTPUT.PUT_LINE('Đã tạo tài khoản cho sinh viên ' || sv.MASV);
        ELSE
            DBMS_OUTPUT.PUT_LINE('Tài khoản ' || sv.MASV || ' đã tồn tại');
        END IF;
    END LOOP;
END;
/

-- ---------------------------------------------------------
-- Gán role theo vai trò trong bảng NHANVIEN
-- ---------------------------------------------------------
DECLARE
    v_count NUMBER; -- Biến đếm để kiểm tra sự tồn tại của user
BEGIN
    -- Duyệt qua tất cả nhân viên trong bảng NHANVIEN
    FOR nv IN (SELECT MANV, DT, VAITRO FROM NHANVIEN) LOOP
        -- Kiểm tra xem user (số điện thoại) có tồn tại không
        SELECT COUNT(*) INTO v_count FROM all_users WHERE username = nv.DT;

        IF v_count > 0 THEN
            -- Gán role tương ứng với VAITRO
            CASE nv.VAITRO
                WHEN 'NVCB' THEN
                    EXECUTE IMMEDIATE 'GRANT NVCB TO "' || nv.DT || '"';
                    DBMS_OUTPUT.PUT_LINE('Đã gán role NVCB cho ' || nv.MANV || ' (' || nv.DT || ')');
                WHEN 'GV' THEN
                    EXECUTE IMMEDIATE 'GRANT GV TO "' || nv.DT || '"';
                    DBMS_OUTPUT.PUT_LINE('Đã gán role GV cho ' || nv.MANV || ' (' || nv.DT || ')');
                WHEN 'NVPDT' THEN
                    EXECUTE IMMEDIATE 'GRANT NVPDT TO "' || nv.DT || '"';
                    DBMS_OUTPUT.PUT_LINE('Đã gán role NVPDT cho ' || nv.MANV || ' (' || nv.DT || ')');
                WHEN 'NVPKT' THEN
                    EXECUTE IMMEDIATE 'GRANT NVPKT TO "' || nv.DT || '"';
                    DBMS_OUTPUT.PUT_LINE('Đã gán role NVPKT cho ' || nv.MANV || ' (' || nv.DT || ')');
                WHEN 'NVTCHC' THEN
                    EXECUTE IMMEDIATE 'GRANT NVTCHC TO "' || nv.DT || '"';
                    DBMS_OUTPUT.PUT_LINE('Đã gán role NVTCHC cho ' || nv.MANV || ' (' || nv.DT || ')');
                WHEN 'NVCTSV' THEN
                    EXECUTE IMMEDIATE 'GRANT NVCTSV TO "' || nv.DT || '"';
                    DBMS_OUTPUT.PUT_LINE('Đã gán role NVCTSV cho ' || nv.MANV || ' (' || nv.DT || ')');
                WHEN 'TRGDV' THEN
                    EXECUTE IMMEDIATE 'GRANT TRGDV TO "' || nv.DT || '"';
                    DBMS_OUTPUT.PUT_LINE('Đã gán role TRGDV cho ' || nv.MANV || ' (' || nv.DT || ')');
                ELSE
                    -- Mặc định gán role NHANVIEN nếu không khớp với vai trò nào
                    EXECUTE IMMEDIATE 'GRANT NHANVIEN TO "' || nv.DT || '"';
                    DBMS_OUTPUT.PUT_LINE('Đã gán role NHANVIEN (mặc định) cho ' || nv.MANV || ' (' || nv.DT || ')');
            END CASE;
        ELSE
            DBMS_OUTPUT.PUT_LINE('User ' || nv.DT || ' không tồn tại, bỏ qua nhân viên ' || nv.MANV);
        END IF;
    END LOOP;

    DBMS_OUTPUT.PUT_LINE('Hoàn thành gán role cho tất cả nhân viên');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Lỗi: ' || SQLERRM);
END;
/

-- ---------------------------------------------------------
-- Gán role SV cho tất cả sinh viên
-- ---------------------------------------------------------
DECLARE
    v_count NUMBER; -- Biến đếm để kiểm tra sự tồn tại của user
BEGIN
    -- Duyệt qua tất cả sinh viên trong bảng SINHVIEN
    FOR sv IN (SELECT MASV, DT FROM SINHVIEN) LOOP
        -- Kiểm tra xem user (số điện thoại) có tồn tại không
        SELECT COUNT(*) INTO v_count FROM all_users WHERE username = sv.DT;

        IF v_count > 0 THEN
            -- Gán role SV
            EXECUTE IMMEDIATE 'GRANT SV TO "' || sv.DT || '"';
            DBMS_OUTPUT.PUT_LINE('Đã gán role SV cho sinh viên ' || sv.MASV || ' (' || sv.DT || ')');
        ELSE
            DBMS_OUTPUT.PUT_LINE('User ' || sv.DT || ' không tồn tại, bỏ qua sinh viên ' || sv.MASV);
        END IF;
    END LOOP;

    DBMS_OUTPUT.PUT_LINE('Hoàn thành gán role SV cho tất cả sinh viên');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Lỗi khi gán role SV: ' || SQLERRM);
END;
/

QUIT;