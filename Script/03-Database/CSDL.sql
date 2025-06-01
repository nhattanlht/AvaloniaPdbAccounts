-- ---------------------------------------------------------
-- Init PDB 
--Lưu ý cần đổi FILE_NAME_CONVERT cho phù hợp trên máy để chạy local
-- ---------------------------------------------------------

conn sys/123@localhost:1521/ORCL21 as sysdba;
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
  FILE_NAME_CONVERT = ('D:\Installed\app\oracle\oradata\ORCL21\pdbseed', 'D:\Installed\app\oracle\oradata\ORCL21\pdbseed\PDB');
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

conn AdminPdb/123@localhost:1521/PDB;
SET NLS_LANG=AMERICAN_AMERICA.AL32UTF8;

-- Bảng DONVI
CREATE TABLE DONVI (
    MADV VARCHAR2(10) PRIMARY KEY,
    TENDV VARCHAR2(100),
    LOAIDV VARCHAR2(20),
    TRGDV VARCHAR2(10) 
);
-- MADV: HOA => TENDV: Khoa hoá học => LOAIDV: KHOA
-- MADV PDT => TENDV: Phòng đào tạo => LOAIDV: PHONG

-- Bảng NHANVIEN
CREATE TABLE NHANVIEN (
    MANLD VARCHAR2(20) PRIMARY KEY,
    HOTEN VARCHAR2(100),
    PHAI CHAR(1), -- 1: male, 0: female
    NGSINH DATE,
    LUONG NUMBER, 
    PHUCAP NUMBER,
    DT VARCHAR2(15),
    VAITRO VARCHAR2(50),
    MADV VARCHAR2(10), 
    FOREIGN KEY (MADV) REFERENCES DONVI(MADV)
);
ALTER TABLE DONVI ADD FOREIGN KEY (TRGDV) REFERENCES NHANVIEN(MANLD);
ALTER TABLE NHANVIEN ADD CONSTRAINT CS_NHANVIEN_PHAI CHECK (PHAI IN ('1', '0'));
-- NHANVIEN: VAITRO
-- NVCB: 500
-- GV: 200
-- NVPDT: 20
-- NVPKT: 10
-- NVTCHC: 15
-- NVPCTSV: 10
-- TRGDV: 15

-- Bảng SINHVIEN
CREATE TABLE SINHVIEN (
    MASV VARCHAR2(20) PRIMARY KEY,
    HOTEN VARCHAR2(100),
    PHAI CHAR(1),
    NGSINH DATE,
    DCHI VARCHAR2(256), 
    DT VARCHAR2(15),
    KHOA VARCHAR2(10),
    TINHTRANG VARCHAR2(100)
);
ALTER TABLE SINHVIEN ADD CONSTRAINT CS_SINHVIEN_PHAI CHECK (PHAI IN ('1', '0'));
-- NHANVIEN: 4000
ALTER TABLE SINHVIEN
MODIFY TINHTRANG VARCHAR2(100);

-- Bảng HOCPHAN
CREATE TABLE HOCPHAN (
    MAHP VARCHAR2(10) PRIMARY KEY,
    TENHP VARCHAR2(100),
    SOTC NUMBER,
    STLT NUMBER,
    STTH NUMBER,
    MADV VARCHAR2(10),
    FOREIGN KEY (MADV) REFERENCES DONVI(MADV)
);

-- Bảng MOMON
CREATE TABLE MOMON (
    MAMM VARCHAR2(20) PRIMARY KEY,
    MAHP VARCHAR2(10),
    MAGV VARCHAR2(10),
    HK NUMBER,
    NAM NUMBER,
    FOREIGN KEY (MAHP) REFERENCES HOCPHAN(MAHP),
    FOREIGN KEY (MAGV) REFERENCES NHANVIEN(MANLD)
);

-- Bảng DANGKY
CREATE TABLE DANGKY (
    MASV VARCHAR2(20),
    MAMM VARCHAR2(20),
    DIEMTH NUMBER,
    DIEMQT NUMBER,
    DIEMCK NUMBER,
    DIEMTK NUMBER,
    PRIMARY KEY (MASV, MAMM),
    FOREIGN KEY (MASV) REFERENCES SINHVIEN(MASV),
    FOREIGN KEY (MAMM) REFERENCES MOMON(MAMM)
);
-- DIEMTK: => DIEMQT*20% + DIEMTH*30% + DIEMCK*50%

CREATE ROLE NVCB;
CREATE ROLE GV;
CREATE ROLE NVPDT;
CREATE ROLE NVPKT;
CREATE ROLE NVTCHC;
CREATE ROLE NVPCTSV;
CREATE ROLE TRGDV;
CREATE ROLE SV;

GRANT CREATE SESSION TO NVCB;
GRANT CREATE SESSION TO GV;
GRANT CREATE SESSION TO NVPDT;
GRANT CREATE SESSION TO NVPKT;
GRANT CREATE SESSION TO NVTCHC;
GRANT CREATE SESSION TO NVPCTSV;
GRANT CREATE SESSION TO TRGDV;
GRANT CREATE SESSION TO SV;

-- ---------------------------------------------------------
-- Tạo tài khoản cho nhân viên và sinh viên
-- ---------------------------------------------------------

DECLARE
    v_count NUMBER; -- Biến đếm để kiểm tra sự tồn tại của user
BEGIN
    -- Tạo tài khoản cho nhân viên
    FOR nv IN (SELECT MANLD FROM NHANVIEN) LOOP
        -- Kiểm tra xem username (mã nhân viên) đã tồn tại chưa
        SELECT COUNT(*) INTO v_count 
        FROM all_users 
        WHERE username = nv.MANLD;

        IF v_count = 0 THEN
            -- Tạo user với password là 123
            EXECUTE IMMEDIATE 'CREATE USER "' || nv.MANLD || '" IDENTIFIED BY 123';
            DBMS_OUTPUT.PUT_LINE('Đã tạo tài khoản cho nhân viên ' || nv.MANLD);
        ELSE
            DBMS_OUTPUT.PUT_LINE('Tài khoản ' || nv.MANLD || ' đã tồn tại');
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
            -- Gán quyền đọc trên các bảng cần thiết
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
    FOR nv IN (SELECT MANLD, VAITRO FROM NHANVIEN) LOOP
        -- Kiểm tra xem user (MANLD) có tồn tại không
        SELECT COUNT(*) INTO v_count FROM all_users WHERE username = UPPER(nv.MANLD);

        IF v_count > 0 THEN
            -- Mặc định gán role NVCB cho tất cả vai trò
            EXECUTE IMMEDIATE 'GRANT NVCB TO "' || nv.MANLD || '"';
            -- Gán role tương ứng với VAITRO
            CASE nv.VAITRO
                WHEN 'GV' THEN
                    EXECUTE IMMEDIATE 'GRANT GV TO "' || nv.MANLD || '"';
                    DBMS_OUTPUT.PUT_LINE('Đã gán role GV cho ' || nv.MANLD);
                WHEN 'NVPDT' THEN
                    EXECUTE IMMEDIATE 'GRANT NVPDT TO "' || nv.MANLD || '"';
                    DBMS_OUTPUT.PUT_LINE('Đã gán role NVPDT cho ' || nv.MANLD);
                WHEN 'NVPKT' THEN
                    EXECUTE IMMEDIATE 'GRANT NVPKT TO "' || nv.MANLD || '"';
                    DBMS_OUTPUT.PUT_LINE('Đã gán role NVPKT cho ' || nv.MANLD);
                WHEN 'NVTCHC' THEN
                    EXECUTE IMMEDIATE 'GRANT NVTCHC TO "' || nv.MANLD || '"';
                    DBMS_OUTPUT.PUT_LINE('Đã gán role NVTCHC cho ' || nv.MANLD);
                WHEN 'NVPCTSV' THEN
                    EXECUTE IMMEDIATE 'GRANT NVPCTSV TO "' || nv.MANLD || '"';
                    DBMS_OUTPUT.PUT_LINE('Đã gán role NVPCTSV cho ' || nv.MANLD);
                WHEN 'TRGDV' THEN
                    EXECUTE IMMEDIATE 'GRANT TRGDV TO "' || nv.MANLD || '"';
                    DBMS_OUTPUT.PUT_LINE('Đã gán role TRGDV cho ' || nv.MANLD);
                ELSE
                    -- Mặc định gán role NVCB nếu không khớp với vai trò nào
                    DBMS_OUTPUT.PUT_LINE('Đã gán role NVCB (mặc định) cho ' || nv.MANLD);
            END CASE;
        ELSE
            DBMS_OUTPUT.PUT_LINE('User "' || nv.MANLD || '" không tồn tại, bỏ qua.');
        END IF;
    END LOOP;

    DBMS_OUTPUT.PUT_LINE('Hoàn thành gán role cho tất cả nhân viên.');
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
    FOR sv IN (SELECT MASV FROM SINHVIEN) LOOP
        -- Kiểm tra xem user (MASV) có tồn tại không
        SELECT COUNT(*) INTO v_count FROM all_users WHERE username = UPPER(sv.MASV);

        IF v_count > 0 THEN
            -- Gán role SV
            EXECUTE IMMEDIATE 'GRANT SV TO "' || sv.MASV || '"';
            DBMS_OUTPUT.PUT_LINE('Đã gán role SV cho sinh viên ' || sv.MASV);
        ELSE
            DBMS_OUTPUT.PUT_LINE('User "' || sv.MASV || '" không tồn tại, bỏ qua.');
        END IF;
    END LOOP;

    DBMS_OUTPUT.PUT_LINE('Hoàn thành gán role SV cho tất cả sinh viên.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Lỗi khi gán role SV: ' || SQLERRM);
END;
/

--------------------------------
-- Câu 1
--------------------------------
-- VAITRO NVCB có quyền SELECT, UPDATE (DT) trên quan hệ NHANVIEN_INFO
CREATE OR REPLACE VIEW NHANVIEN_NVCB AS
SELECT * FROM ADMINPDB.NHANVIEN
WHERE MANLD = SYS_CONTEXT('USERENV','SESSION_USER');

GRANT SELECT, UPDATE (DT) ON NHANVIEN_NVCB TO NVCB;

-- VAITRO TRGDV có quyền SELECT trên quan hệ NHANVIEN_DONVI_INFO trừ LUONG va PHUCAP
CREATE OR REPLACE VIEW NHANVIEN_TRGDV AS
SELECT 
    NV.MANLD, 
    NV.HOTEN, 
    NV.PHAI, 
    NV.NGSINH, 
    NV.DT, 
    NV.VAITRO, 
    NV.MADV,
    CASE 
        WHEN NV.MANLD = SYS_CONTEXT('USERENV','SESSION_USER') THEN NV.LUONG
        ELSE NULL 
    END AS LUONG,
    CASE 
        WHEN NV.MANLD = SYS_CONTEXT('USERENV','SESSION_USER') THEN NV.PHUCAP
        ELSE NULL 
    END AS PHUCAP
FROM ADMINPDB.NHANVIEN NV
WHERE NV.MADV = (SELECT TRG.MADV
                 FROM ADMINPDB.NHANVIEN TRG
                 WHERE TRG.MANLD = SYS_CONTEXT('USERENV','SESSION_USER'));

GRANT SELECT ON NHANVIEN_TRGDV TO TRGDV;


-- VAITRO TCHC có quyền SELECT, INSERT, UPDATE, DELETE trên quan hệ NHANVIEN

GRANT SELECT, INSERT, UPDATE, DELETE ON ADMINPDB.NHANVIEN TO NVTCHC;

--------------------------------
-- Câu 2
--------------------------------
-- VAITRO GV có quyền SELECT trên quan hệ MOMON_INFO
CREATE OR REPLACE VIEW MOMON_GV AS
SELECT * FROM ADMINPDB.MOMON
WHERE MAGV = SYS_CONTEXT('USERENV','SESSION_USER');

GRANT SELECT ON MOMON_GV TO GV;

-- VAITRO NVPDT SELECT, INSERT, UPDATE, DELETE trên quan hệ MOMON
CREATE OR REPLACE VIEW MOMON_PDT AS
SELECT * FROM ADMINPDB.MOMON
WHERE
    (TO_NUMBER(TO_CHAR(SYSDATE, 'MM')) BETWEEN 9 AND 12 AND HK = 1 AND NAM = TO_NUMBER(TO_CHAR(SYSDATE, 'YYYY')))
 OR (TO_NUMBER(TO_CHAR(SYSDATE, 'MM')) BETWEEN 1 AND 4 AND HK = 2 AND NAM = TO_NUMBER(TO_CHAR(SYSDATE, 'YYYY')) - 1)
 OR (TO_NUMBER(TO_CHAR(SYSDATE, 'MM')) BETWEEN 5 AND 8 AND HK = 3 AND NAM = TO_NUMBER(TO_CHAR(SYSDATE, 'YYYY')) - 1);

GRANT SELECT, INSERT, UPDATE, DELETE ON MOMON_PDT TO NVPDT;

-- VAITRO TRGDV có quyền SELECT trên quan hệ MOMON_TRGDV
CREATE OR REPLACE VIEW MOMON_TRGDV AS
SELECT * FROM ADMINPDB.MOMON
WHERE MAGV IN (SELECT NV.MANLD
               FROM ADMINPDB.NHANVIEN NV
               WHERE NV.MADV = (SELECT TRG.MADV
				                FROM ADMINPDB.NHANVIEN TRG
                                WHERE TRG.MANLD = SYS_CONTEXT('USERENV','SESSION_USER')));

GRANT SELECT ON MOMON_TRGDV TO TRGDV;

-- VAITRO SV có quyền SELECT trên quan hệ MOMON_SV
CREATE OR REPLACE VIEW MOMON_SV AS
SELECT * FROM ADMINPDB.MOMON MM
WHERE MM.MAHP IN (SELECT HP.MAHP 
                 FROM ADMINPDB.HOCPHAN HP
                 WHERE HP.MADV = (SELECT SV.KHOA
                                  FROM ADMINPDB.SINHVIEN SV
                                  WHERE SV.MASV = SYS_CONTEXT('USERENV','SESSION_USER')));

GRANT SELECT ON MOMON_SV TO SV;

EXIT;