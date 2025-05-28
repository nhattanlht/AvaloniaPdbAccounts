conn AdminPdb/123@localhost:1521/PDB;
-- PHẦN 3 ( TẠO AUDIT TRUY VẾT )
-- XÓA BẢNG VÀ CHÍNH SAACHS NẾU TỒN TẠI
--3a)

-- 3b)
GRANT SELECT ON DBA_ROLE_PRIVS TO ADMINPDB;
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