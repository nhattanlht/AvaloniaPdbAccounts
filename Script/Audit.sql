-- PHẦN 3 (AUDIT)

--1. Kích hoạt việc ghi nhật ký hệ thống
--kiểm tra unified đã bật chưa
ALTER SESSION SET CONTAINER = PDB;
conn sys/123@localhost:1521/FREE as sysdba;
SELECT VALUE FROM V$OPTION WHERE PARAMETER = 'Unified Auditing'; --True là đã bật

--nếu chưa được bật
-- -- Tắt database (ở chế độ SHUTDOWN IMMEDIATE)
-- SHUTDOWN IMMEDIATE;
-- -- Kích hoạt unified audit
-- STARTUP UPGRADE;
-- -- Thiết lập tham số init.ora hoặc spfile:
-- ALTER SYSTEM SET UNIFIED_AUDIT_ENABLED = TRUE SCOPE=SPFILE;
-- -- Khởi động lại database bình thường
-- SHUTDOWN IMMEDIATE;
-- STARTUP;
-- ALTER SESSION SET CONTAINER = PDB;

---- Kiểm tra còn policy nào đang bật
-- conn AdminPdb/123@localhost:1521/PDB;
-- SELECT * FROM AUDIT_UNIFIED_ENABLED_POLICIES;

-- -- Tắt các chính sách mặc định (nếu có)
-- NOAUDIT POLICY ORA_SECURECONFIG;
-- NOAUDIT POLICY ORA_LOGON_FAILURES;
-- NOAUDIT POLICY ORA_LOGIN_LOGOUT;
-- NOAUDIT POLICY ORA_DV_SCHEMA_CHANGES;
-- NOAUDIT POLICY ORA_DV_DEFAULT_PROTECTION;
-- NOAUDIT POLICY ORA$DICTIONARY_SENS_COL_ACCESS;

-- Xóa policy cũ nếu tồn tại (nếu bạn chạy lại)
conn AdminPdb/123@localhost:1521/PDB;
BEGIN
   FOR rec IN (
     SELECT POLICY_NAME 
     FROM AUDIT_UNIFIED_ENABLED_POLICIES 
     WHERE POLICY_NAME = 'AUDIT_DANGKY'
   ) LOOP
      EXECUTE IMMEDIATE 'NOAUDIT POLICY ' || rec.POLICY_NAME || ' BY NV00016';
      EXECUTE IMMEDIATE 'DROP AUDIT POLICY ' || rec.POLICY_NAME;
   END LOOP;
END;
/
BEGIN
   FOR rec IN (SELECT POLICY_NAME FROM AUDIT_UNIFIED_ENABLED_POLICIES WHERE POLICY_NAME = 'AUDIT_NHANVIEN') LOOP
      EXECUTE IMMEDIATE 'NOAUDIT POLICY ' || rec.POLICY_NAME || ' BY NV00018';
      EXECUTE IMMEDIATE 'DROP AUDIT POLICY ' || rec.POLICY_NAME;
   END LOOP;
END;
/
BEGIN
   FOR rec IN (SELECT POLICY_NAME FROM AUDIT_UNIFIED_ENABLED_POLICIES WHERE POLICY_NAME = 'AUDIT_SINHVIEN') LOOP
      EXECUTE IMMEDIATE 'NOAUDIT POLICY ' || rec.POLICY_NAME || ' BY NV00020';
      EXECUTE IMMEDIATE 'DROP AUDIT POLICY ' || rec.POLICY_NAME;
   END LOOP;
END;
/
BEGIN
   FOR rec IN (SELECT POLICY_NAME FROM AUDIT_UNIFIED_ENABLED_POLICIES WHERE POLICY_NAME = 'AUDIT_MOMON') LOOP
      EXECUTE IMMEDIATE 'NOAUDIT POLICY ' || rec.POLICY_NAME || ' BY NV00024';
      EXECUTE IMMEDIATE 'DROP AUDIT POLICY ' || rec.POLICY_NAME;
   END LOOP;
END;
/

--Tạo policy Audit các thao tác DML trên bảng SINHVIEN
CREATE AUDIT POLICY AUDIT_DANGKY
ACTIONS
  SELECT ON ADMINPDB.DANGKY,
  INSERT ON ADMINPDB.DANGKY,
  UPDATE ON ADMINPDB.DANGKY,
  DELETE ON ADMINPDB.DANGKY;

--Tạo policy Audit các thao tác DML trên bảng NHANVIEN
CREATE AUDIT POLICY AUDIT_NHANVIEN
ACTIONS
  SELECT ON ADMINPDB.NHANVIEN,
  INSERT ON ADMINPDB.NHANVIEN,
  UPDATE ON ADMINPDB.NHANVIEN,
  DELETE ON ADMINPDB.NHANVIEN;

-- Tạo policy audit các thao tác DML trên bảng DANGKY
CREATE AUDIT POLICY AUDIT_SINHVIEN
ACTIONS
  SELECT ON ADMINPDB.SINHVIEN,
  INSERT ON ADMINPDB.SINHVIEN,
  UPDATE ON ADMINPDB.SINHVIEN,
  DELETE ON ADMINPDB.SINHVIEN;

-- Tạo policy audit các thao tác DML trên bảng MOMON
CREATE AUDIT POLICY AUDIT_MOMON
ACTIONS
  SELECT ON ADMINPDB.MOMON,
  INSERT ON ADMINPDB.MOMON,
  UPDATE ON ADMINPDB.MOMON,
  DELETE ON ADMINPDB.MOMON;

-- Gán policy AUDIT_DANGKY cho user NV00016 (PKT)
AUDIT POLICY AUDIT_DANGKY
  BY NV00016
  WHENEVER SUCCESSFUL;
AUDIT POLICY AUDIT_DANGKY
  BY NV00016
  WHENEVER NOT SUCCESSFUL;

-- Gán policy AUDIT_NHANVIEN cho user NV00018 (TCHC)
AUDIT POLICY AUDIT_NHANVIEN
  BY NV00018
  WHENEVER SUCCESSFUL;
AUDIT POLICY AUDIT_NHANVIEN
  BY NV00018
  WHENEVER NOT SUCCESSFUL;

-- Gán policy AUDIT_MOMON cho user NV00024 (PDT)
AUDIT POLICY AUDIT_MOMON
  BY NV00024
  WHENEVER SUCCESSFUL;
AUDIT POLICY AUDIT_MOMON
  BY NV00024
  WHENEVER NOT SUCCESSFUL;

-- Gán policy AUDIT_SINHVIEN cho user NV00020 (PCTSV)
AUDIT POLICY AUDIT_SINHVIEN
  BY NV00020
  WHENEVER SUCCESSFUL;
AUDIT POLICY AUDIT_SINHVIEN
  BY NV00020
  WHENEVER NOT SUCCESSFUL;
-- Đọc dữ liệu nhật ký hệ thống.
--BANG NHANVIEN
conn AdminPdb/123@localhost:1521/PDB;
SELECT 
  AUDIT_TYPE,
  EVENT_TIMESTAMP,
  DBUSERNAME,
  OBJECT_SCHEMA,
  OBJECT_NAME,
  ACTION_NAME,
  SQL_TEXT,
  RETURN_CODE
FROM 
  UNIFIED_AUDIT_TRAIL
WHERE 
  OBJECT_NAME = 'NHANVIEN'
ORDER BY 
  EVENT_TIMESTAMP DESC;
--BANG MOMON
conn AdminPdb/123@localhost:1521/PDB;
SELECT 
  AUDIT_TYPE,
  EVENT_TIMESTAMP,
  DBUSERNAME,
  OBJECT_SCHEMA,
  OBJECT_NAME,
  ACTION_NAME,
  SQL_TEXT,
  RETURN_CODE
FROM 
  UNIFIED_AUDIT_TRAIL
WHERE 
  OBJECT_NAME = 'MOMON'
ORDER BY 
  EVENT_TIMESTAMP DESC;
--BANG SINHVIEN
conn AdminPdb/123@localhost:1521/PDB;
SELECT 
  AUDIT_TYPE,
  EVENT_TIMESTAMP,
  DBUSERNAME,
  OBJECT_SCHEMA,
  OBJECT_NAME,
  ACTION_NAME,
  SQL_TEXT,
  RETURN_CODE
FROM 
  UNIFIED_AUDIT_TRAIL
WHERE 
  OBJECT_NAME = 'SINHVIEN'
ORDER BY 
  EVENT_TIMESTAMP DESC;
--BANG DANGKY
conn AdminPdb/123@localhost:1521/PDB;
SELECT 
  AUDIT_TYPE,
  EVENT_TIMESTAMP,
  DBUSERNAME,
  OBJECT_SCHEMA,
  OBJECT_NAME,
  ACTION_NAME,
  SQL_TEXT,
  RETURN_CODE
FROM 
  UNIFIED_AUDIT_TRAIL
WHERE 
  OBJECT_NAME = 'DANGKY'
ORDER BY 
  EVENT_TIMESTAMP DESC;

--3 Dùng Fine-grained Audit cho các tình huống
conn sys/123@localhost:1521/FREE as sysdba;
ALTER SESSION SET CONTAINER = PDB;
GRANT SELECT ON DBA_ROLE_PRIVS TO AdminPdb;
--3a) Hành vi cập nhật quan hệ ĐANGKY tại các trường liên quan đến điểm số nhưng 
--người đó không thuộc vai trò “NV PKT”.


-- 3b) Hành vi của người dùng (không thuộc vai trò “NV TCHC”) có thể đọc trên  
--trường LUONG, PHUCAP của người khác hoặc cập nhật ở quan hệ NHANVIEN.
conn AdminPdb/123@localhost:1521/PDB;
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE ADMINPDB.FGA_LOG_TABLE CASCADE CONSTRAINTS';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN -- -942: table does not exist
            RAISE;
        END IF;
END;
/


-- Drop policy AUDIT_UPDATE_DIEM_NOT_NVPKT nếu đã tồn tại
BEGIN
  BEGIN
    DBMS_FGA.DROP_POLICY(
      object_schema => 'ADMINPDB',
      object_name   => 'DANGKY',
      policy_name   => 'AUDIT_UPDATE_DIEM_NOT_NVPKT'
    );
  EXCEPTION
    WHEN OTHERS THEN
      IF SQLCODE != -28102 THEN
        RAISE;
      END IF;
  END;
END;
/

-- Drop policy AUDIT_SELECT_LUONG_PHUCAP nếu đã tồn tại
BEGIN
  BEGIN
    DBMS_FGA.DROP_POLICY(
      object_schema => 'ADMINPDB',
      object_name   => 'NHANVIEN',
      policy_name   => 'AUDIT_SELECT_LUONG_PHUCAP'
    );
  EXCEPTION
    WHEN OTHERS THEN
      IF SQLCODE != -28102 THEN
        RAISE;
      END IF;
  END;
END;
/

-- Drop policy AUDIT_UPDATE_LUONG_PHUCAP nếu đã tồn tại
BEGIN
  BEGIN
    DBMS_FGA.DROP_POLICY(
      object_schema => 'ADMINPDB',
      object_name   => 'NHANVIEN',
      policy_name   => 'AUDIT_UPDATE_LUONG_PHUCAP'
    );
  EXCEPTION
    WHEN OTHERS THEN
      IF SQLCODE != -28102 THEN
        RAISE;
      END IF;
  END;
END;
/

-- Drop policy AUDIT_DANGKY_UPDATE_OTHER nếu đã tồn tại
BEGIN
  BEGIN
    DBMS_FGA.DROP_POLICY(
      object_schema => 'ADMINPDB',
      object_name   => 'DANGKY',
      policy_name   => 'AUDIT_DANGKY_UPDATE_OTHER'
    );
  EXCEPTION
    WHEN OTHERS THEN
      IF SQLCODE != -28102 THEN
        RAISE;
      END IF;
  END;
END;
/

-- Tạo bảng để lưu thông tin AUDIT
CREATE TABLE ADMINPDB.FGA_LOG_TABLE (
    event_time     TIMESTAMP DEFAULT SYSTIMESTAMP,
    object_schema  VARCHAR2(50),
    object_name    VARCHAR2(50),
    policy_name    VARCHAR2(50),
    triggered_by   VARCHAR2(50),
    user_roles     VARCHAR2(200),
    action_type    VARCHAR2(20),-- SELECT, INSERT, DELETE, UPDATE
    violation_reason VARCHAR2(200)
    
);
--3A

CREATE OR REPLACE PROCEDURE ADMINPDB.FGA_NOT_NVPKT (
    object_schema  VARCHAR2,
    object_name    VARCHAR2,
    policy_name    VARCHAR2
)
AS
    v_user     VARCHAR2(50) := SYS_CONTEXT('USERENV', 'SESSION_USER');
    v_roles    VARCHAR2(200);
    v_has_nvpkt NUMBER := 0;
BEGIN
    -- Kiểm tra người dùng có vai trò NVPKT không
    SELECT COUNT(*) INTO v_has_nvpkt
    FROM dba_role_privs
    WHERE grantee = v_user AND granted_role = 'NVPKT';

    IF v_has_nvpkt = 0 THEN
        -- Lấy danh sách các vai trò đang bật
       SELECT LISTAGG(granted_role, ',') WITHIN GROUP (ORDER BY granted_role)
        INTO v_roles
        FROM dba_role_privs
        WHERE grantee = UPPER(v_user);

        -- Ghi log
       INSERT INTO ADMINPDB.FGA_LOG_TABLE (event_time, object_schema, object_name, policy_name, triggered_by, user_roles, action_type, violation_reason) 
       VALUES (SYSTIMESTAMP, object_schema, object_name, policy_name, v_user, v_roles, 'UPDATE', 'Cập nhật điểm nhưng không thuộc vai trò NVPKT');

        COMMIT;
    END IF;
END;
/   


BEGIN
  DBMS_FGA.ADD_POLICY(
    object_schema      => 'ADMINPDB',
    object_name        => 'DANGKY',
    policy_name        => 'AUDIT_UPDATE_DIEM_NOT_NVPKT',
    audit_column       => 'DIEMTH,DIEMQT,DIEMCK,DIEMTK',
    statement_types    => 'UPDATE',
    audit_column_opts  => DBMS_FGA.ANY_COLUMNS,
    handler_schema     => 'ADMINPDB',
    handler_module     => 'FGA_NOT_NVPKT'
  );
END;

-- 3B
CREATE OR REPLACE PROCEDURE ADMINPDB.FGA_NOT_TCHC (
    object_schema  VARCHAR2,
    object_name    VARCHAR2,
    policy_name    VARCHAR2
)
AS
    v_user     VARCHAR2(50) := SYS_CONTEXT('USERENV', 'SESSION_USER');
    v_roles    VARCHAR2(200);
    v_action   VARCHAR2(20);
    v_count    NUMBER := 0;
BEGIN
    -- Suy đoán hành động từ policy name
    IF policy_name LIKE '%SELECT%' THEN
        v_action := 'SELECT';
    ELSIF policy_name LIKE '%UPDATE%' THEN
        v_action := 'UPDATE';
    ELSIF policy_name LIKE '%INSERT%' THEN
        v_action := 'INSERT';
    ELSIF policy_name LIKE '%DELETE%' THEN
        v_action := 'DELETE';
    ELSE
        v_action := 'UNKNOWN';
    END IF;

    -- Đếm số vai trò NVTCHC
    SELECT COUNT(*)
    INTO v_count
    FROM dba_role_privs
    WHERE grantee = v_user AND granted_role = 'NVTCHC';

    -- Nếu KHÔNG có vai trò NVTCHC thì ghi log
    IF v_count = 0 THEN
        -- Lấy danh sách role
        SELECT LISTAGG(granted_role, ',') WITHIN GROUP (ORDER BY granted_role)
        INTO v_roles
        FROM dba_role_privs
        WHERE grantee = v_user;

        INSERT INTO ADMINPDB.FGA_LOG_TABLE (
            object_schema, object_name, policy_name, triggered_by, user_roles, action_type, violation_reason
        ) VALUES (
            object_schema, object_name, policy_name, v_user, v_roles, v_action, NULL
        );
        COMMIT;
    END IF;
END;
/

BEGIN
  -- Audit SELECT trên LUONG, PHUCAP
  DBMS_FGA.ADD_POLICY (
    object_schema       => 'ADMINPDB',
    object_name         => 'NHANVIEN',
    policy_name         => 'AUDIT_SELECT_LUONG_PHUCAP',
    audit_column        => 'LUONG,PHUCAP',
    statement_types     => 'SELECT',
    audit_column_opts   => DBMS_FGA.ANY_COLUMNS,
    handler_schema      => 'ADMINPDB',
    handler_module      => 'FGA_NOT_TCHC'
  );
END;
/
BEGIN
  -- Audit UPDATE trên LUONG, PHUCAP
  DBMS_FGA.ADD_POLICY (
    object_schema       => 'ADMINPDB',
    object_name         => 'NHANVIEN',
    policy_name         => 'AUDIT_UPDATE_LUONG_PHUCAP',
    audit_column        => 'LUONG,PHUCAP,DT',
    statement_types     => 'UPDATE',
    audit_column_opts   => DBMS_FGA.ANY_COLUMNS,
    handler_schema      => 'ADMINPDB',
    handler_module      => 'FGA_NOT_TCHC'
  );
END;
/

--3C 
CREATE OR REPLACE PROCEDURE ADMINPDB.FGA_NOT_STUDENT_HIMSELF (
    object_schema  VARCHAR2,
    object_name    VARCHAR2,
    policy_name    VARCHAR2
)
AS
    v_user     VARCHAR2(50) := SYS_CONTEXT('USERENV', 'SESSION_USER');
    v_violation_reason VARCHAR2(200);
    v_roles    VARCHAR2(200);
    v_action VARCHAR2(20);
    v_is_student NUMBER := 0;
BEGIN
 IF policy_name LIKE '%SELECT%' THEN
        v_action := 'SELECT';
    ELSIF policy_name LIKE '%UPDATE%' THEN
        v_action := 'UPDATE';
    ELSIF policy_name LIKE '%INSERT%' THEN
        v_action := 'INSERT';
    ELSIF policy_name LIKE '%DELETE%' THEN
        v_action := 'DELETE';
    ELSE
        v_action := 'UNKNOWN';
    END IF;
    -- Kiểm tra nếu là sinh viên
    SELECT COUNT(*)
    INTO v_is_student
    FROM ADMINPDB.SINHVIEN
    WHERE MASV = v_user;

    IF v_is_student > 0 THEN
        -- Gán lý do vi phạm
        v_violation_reason := 'Truy cập dữ liệu không thuộc về sinh viên';

        -- Lấy vai trò nếu có
        BEGIN
            SELECT LISTAGG(granted_role, ',') WITHIN GROUP (ORDER BY granted_role)
            INTO v_roles
            FROM dba_role_privs
            WHERE grantee = UPPER(v_user);
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                v_roles := NULL;
        END;

        -- Ghi log
        INSERT INTO ADMINPDB.FGA_LOG_TABLE (
            event_time, object_schema, object_name, policy_name,
            triggered_by, user_roles, action_type,
            violation_reason
        ) VALUES (
            SYSTIMESTAMP, object_schema, object_name, policy_name,
            v_user, v_roles,v_action,
            v_violation_reason
        );
        COMMIT;
    END IF;
END;
/
BEGIN
  DBMS_FGA.ADD_POLICY (
    object_schema      => 'ADMINPDB',
    object_name        => 'DANGKY',
    policy_name        => 'AUDIT_DANGKY_UPDATE_OTHER',
    audit_condition    => 'MASV != SYS_CONTEXT(''USERENV'', ''SESSION_USER'')',
    audit_column       => NULL, -- tất cả cột
    statement_types    => 'INSERT,UPDATE,DELETE',
    audit_column_opts  => DBMS_FGA.ALL_COLUMNS,
    handler_schema     => 'ADMINPDB',
    handler_module     => 'FGA_NOT_STUDENT_HIMSELF',
    enable             => TRUE
  );
END;
/


--TẮT POLICY ĐỂ TEST
BEGIN
  DBMS_RLS.ENABLE_POLICY(
    object_schema   => 'ADMINPDB',
    object_name     => 'DANGKY',
    policy_name     => 'DANGKY_MODIFY_POLICY',
    enable          => FALSE
  );
END;
/

--NV00020: NVPCTSV
--NV00018: NVTCHC
--NV00640: NVCB
--NV00076: TRDV

SELECT * FROM ADMINPDB.FGA_LOG_TABLE;
--LOGIN VÀO VÀ TEST

--Login vào NHÂN VIÊN PĐT TEST
conn NV00029/123@localhost:1521/PDB;
UPDATE ADMINPDB.DANGKY
SET
DIEMQT = 6.0,
DIEMCK = 9.0,
DIEMTK = 7.5
WHERE MASV = '20A536166' AND MAMM = 'MTH00003_2_2024';

-- Login vào sinh viên và test
conn 20A536166/123@localhost:1521/PDB;
UPDATE ADMINPDB.DANGKY
SET DIEMTK = 4
WHERE MASV = '20A921624' AND MAMM = 'MTH00003_2_2024';


-- XUẤT FILE LƯU THÔNG TIN AUDIT
-- 1) Thiết lập môi trường xuất
SET TERMOUT OFF
SET FEEDBACK OFF
SET VERIFY OFF
SET COLSEP '|'  -- Tạm dùng dấu '|' làm separator
SET LINESIZE 1000
SET PAGESIZE 50000
SET TRIMSPOOL ON

-- Bước 1: Xuất header
SPOOL D:/fga_log_table_headers.csv
SELECT 
  'event_time'       AS event_time,
  'object_schema'    AS object_schema,
  'object_name'      AS object_name,
  'policy_name'      AS policy_name,
  'triggered_by'     AS triggered_by,
  'user_roles'       AS user_roles,
  'action_type'      AS action_type,
  'violation_reason' AS violation_reason
FROM DUAL;
SPOOL OFF

-- Bước 2: Xuất dữ liệu thực tế
SPOOL D:/fga_log_table_data.csv
SELECT
  TO_CHAR(event_time, 'YYYY-MM-DD HH24:MI:SS'),
  object_schema,
  object_name,
  policy_name,
  triggered_by,
  user_roles,
  action_type,
  violation_reason
FROM ADMINPDB.FGA_LOG_TABLE;
SPOOL OFF

-- Bước 3: Gộp header và dữ liệu
HOST copy /Y D:/fga_log_table_headers.csv + D:/fga_log_table_data.csv D:/fga_log_table_final.csv

-- Bước 4: Xóa file tạm
HOST del D:/fga_log_table_headers.csv
HOST del D:/fga_log_table_data.csv

-- Bước 5: Định dạng file cuối cùng (thay '|' bằng ',')
HOST powershell -Command "(Get-Content D:/fga_log_table_final.csv) | ForEach-Object { $_ -replace '\|', ',' } | Set-Content D:/fga_log_table.csv -Encoding UTF8"
HOST del D:/fga_log_table_final.csv

EXIT
--3c)

-- --Đọc xuất dữ liệu nhật ký hệ thống.
-- conn AdminPdb/123@localhost:1521/PDB;
-- SELECT * FROM FGA_LOG_TABLE;

-- -- TEST TRƯỜNG HỢP BỊ GHI LOG
-- conn NV00022/123@localhost:1521/PDB;
-- SELECT LUONG, PHUCAP FROM ADMINPDB.NHANVIEN;
COMMIT;
