conn sys/123456@localhost:1521/FREEPDB1 as sysdba;
ALTER SESSION SET CONTAINER = PDB;
GRANT SELECT ON DBA_ROLE_PRIVS TO AdminPdb;

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
--3a) Hành vi cập nhật quan hệ ĐANGKY tại các trường liên quan đến điểm số nhưng 
--người đó không thuộc vai trò “NV PKT”.
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
/

-- 3b) Hành vi của người dùng (không thuộc vai trò “NV TCHC”) có thể đọc trên  
--trường LUONG, PHUCAP của người khác hoặc cập nhật ở quan hệ NHANVIEN.
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
            object_schema, object_name, policy_name, v_user, v_roles, v_action, 'Hành vi ' || v_action || ' trên bảng NHANVIEN'
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

--3C Hành vi thêm, xóa, sửa trên quan hệ DANGKY của sinh viên trên dòng dữ liệu của sinh viên khác
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
    audit_column       => NULL, 
    statement_types    => 'INSERT,UPDATE,DELETE',
    audit_column_opts  => DBMS_FGA.ALL_COLUMNS,
    handler_schema     => 'ADMINPDB',
    handler_module     => 'FGA_NOT_STUDENT_HIMSELF',
    enable             => TRUE
  );
END;
/
--Dang ky qua thoi gian cho phep
BEGIN
  DBMS_FGA.DROP_POLICY('ADMINPDB','DANGKY','AUDIT_LATE_MODIFY');
EXCEPTION WHEN OTHERS THEN NULL; END;
/

CREATE OR REPLACE PROCEDURE ADMINPDB.FGA_LATE_MODIFY (
  p_object_schema VARCHAR2,
  p_object_name   VARCHAR2,
  p_policy_name   VARCHAR2
)
IS
  PRAGMA AUTONOMOUS_TRANSACTION;  
  v_user  VARCHAR2(50)  := SYS_CONTEXT('USERENV','SESSION_USER');
  v_roles VARCHAR2(1000);
BEGIN
  v_roles := 'N/A';
  BEGIN
    SELECT LISTAGG(GRANTED_ROLE, ',') 
           WITHIN GROUP (ORDER BY GRANTED_ROLE)
    INTO v_roles
    FROM USER_ROLE_PRIVS
    WHERE USERNAME = UPPER(v_user);
   
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      v_roles := '';        
    WHEN OTHERS THEN
      v_roles := 'ERROR';   
  END;

  BEGIN
      INSERT INTO ADMINPDB.FGA_LOG_TABLE (
            event_time, object_schema, object_name, policy_name,
            triggered_by, user_roles, action_type, violation_reason
        ) VALUES (
            SYSTIMESTAMP, p_object_schema, p_object_name, p_policy_name,
            v_user, v_roles, ORA_SYSEVENT, 'Hiệu chỉnh ngoài thời gian cho phép'
        );
    COMMIT;
  EXCEPTION
    WHEN OTHERS THEN

      NULL;
  END;
END FGA_LATE_MODIFY;
/



BEGIN
  DBMS_FGA.ADD_POLICY(
    object_schema     => 'ADMINPDB',
    object_name       => 'DANGKY',
    policy_name       => 'AUDIT_LATE_MODIFY',
     audit_condition   => q'[
      EXISTS (
        SELECT 1
          FROM ADMINPDB.MOMON m
         WHERE m.MAMM = MAMM
           AND SYSDATE > ADMINPDB.get_semester_start_date(m.HK, m.NAM) + 14
      )
    ]',
    statement_types   => 'INSERT,DELETE',
    audit_column      => 'MAMM',
    audit_column_opts => DBMS_FGA.ALL_COLUMNS,
    handler_schema    => 'ADMINPDB',
    handler_module    => 'FGA_LATE_MODIFY'
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
