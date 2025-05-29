-- PHẦN 3 (AUDIT)

--1. Kích hoạt việc ghi nhật ký hệ thống
--kiểm tra unified đã bật chưa
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

-- Nếu có policy audit tên AUDIT_NHANVIEN_TCHC_NV00018 thì hủy áp dụng và xóa trước khi tạo mới
conn AdminPdb/123@localhost:1521/PDB;
BEGIN
   FOR rec IN (SELECT POLICY_NAME FROM AUDIT_UNIFIED_ENABLED_POLICIES WHERE POLICY_NAME = 'AUDIT_NHANVIEN_TCHC_NV00018') LOOP
      EXECUTE IMMEDIATE 'NOAUDIT POLICY ' || rec.POLICY_NAME || ' BY NV00018';
      EXECUTE IMMEDIATE 'DROP AUDIT POLICY ' || rec.POLICY_NAME;
   END LOOP;
END;
/

--Tạo Policy Ghi lại các thao tác DML (SELECT, INSERT, UPDATE, DELETE) trên bảng ADMINPDB.NHANVIEN
conn AdminPdb/123@localhost:1521/PDB;
CREATE AUDIT POLICY AUDIT_NHANVIEN_TCHC_NV00018
ACTIONS
  SELECT ON ADMINPDB.NHANVIEN,
  INSERT ON ADMINPDB.NHANVIEN,
  UPDATE ON ADMINPDB.NHANVIEN,
  DELETE ON ADMINPDB.NHANVIEN;

-- Gán policy cho user cụ thể
AUDIT POLICY AUDIT_NHANVIEN_TCHC_NV00018
  BY NV00018
  WHENEVER SUCCESSFUL;
AUDIT POLICY AUDIT_NHANVIEN_TCHC_NV00018
  BY NV00018
  WHENEVER NOT SUCCESSFUL;

-- TEST TRƯỜNG HỢP BỊ GHI NHẬN AUDIT
--NVTCHC update LUONG của NV00001 và thao tác đọc bảng nhân viên
conn NV00018/123@localhost:1521/PDB;
UPDATE ADMINPDB.NHANVIEN
SET LUONG = LUONG + 1000000
WHERE MANLD = 'NV00001';
SELECT * FROM ADMINPDB.NHANVIEN;


-- Đọc xuất dữ liệu nhật ký hệ thống.
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
BEGIN
    EXECUTE IMMEDIATE 'DROP PROCEDURE ADMINPDB.FGA_SEND_LOG';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -4043 THEN -- ORA-04043: object does not exist
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP FUNCTION ADMINPDB.FGA_NOT_TCHC';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -4043 THEN -- ORA-04043: object does not exist
            RAISE;
        END IF;
END;
/
BEGIN
  -- Xoá policy SELECT nếu tồn tại
  BEGIN
    DBMS_FGA.DROP_POLICY(
      object_schema => 'ADMINPDB',
      object_name   => 'NHANVIEN',
      policy_name   => 'AUDIT_SELECT_LUONG_PHUCAP'
    );
  EXCEPTION
    WHEN OTHERS THEN
      IF SQLCODE != -28102 THEN -- ORA-28102: policy does not exist
        RAISE;
      END IF;
  END;

  -- Xoá policy UPDATE nếu tồn tại
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

CREATE TABLE ADMINPDB.FGA_LOG_TABLE (
    event_time     TIMESTAMP DEFAULT SYSTIMESTAMP,
    object_schema  VARCHAR2(50),
    object_name    VARCHAR2(50),
    policy_name    VARCHAR2(50),
    triggered_by   VARCHAR2(50),
    user_roles     VARCHAR2(200),
    action_type    VARCHAR2(20) -- SELECT, INSERT, DELETE, UPDATE
);

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
            object_schema, object_name, policy_name, triggered_by, user_roles, action_type
        ) VALUES (
            object_schema, object_name, policy_name, v_user, v_roles, v_action
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
--3c)

--4. Đọc xuất dữ liệu nhật ký hệ thống.
conn AdminPdb/123@localhost:1521/PDB;
SELECT * FROM FGA_LOG_TABLE;

-- TEST TRƯỜNG HỢP BỊ GHI LOG
conn NV00022/123@localhost:1521/PDB;
SELECT LUONG, PHUCAP FROM ADMINPDB.NHANVIEN;
COMMIT;
