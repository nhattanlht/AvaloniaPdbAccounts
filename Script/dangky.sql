-- Kết nối:
conn AdminPdb/123@localhost:1521/PDB


CREATE OR REPLACE FUNCTION fn_vpd_dangky (
  schema_name IN VARCHAR2,
  table_name IN VARCHAR2
) RETURN VARCHAR2 AS
  v_role VARCHAR2(50);
BEGIN
  -- Lấy vai trò người dùng hiện tại
  SELECT MAX(VAITRO) INTO v_role 
  FROM NHANVIEN
  WHERE MANV = SYS_CONTEXT('USERENV','SESSION_USER'); -- giả sử MANV trùng với USER Oracle
  
  -- Kiểm tra nếu vai trò là NVPKT (để đơn giản, không phân biệt chữ hoa/thường)
  IF UPPER(v_role) = 'NVPKT' THEN
    RETURN '1=1'; -- Cho phép xem hết dữ liệu
  ELSE
    RETURN '1=0'; -- Không cho phép xem dữ liệu
  END IF;
EXCEPTION
  WHEN NO_DATA_FOUND THEN
    RETURN '1=0'; -- Không cho phép nếu không tìm thấy user
END;
/
BEGIN
  DBMS_RLS.ADD_POLICY(
    object_schema => 'ADMINPDB', -- thay bằng schema chứa bảng DANGKY
    object_name   => 'DANGKY',
    policy_name   => 'POLICY_DANGKY_VPD',
    function_schema => 'ADMINPDB',
    policy_function => 'fn_vpd_dangky',
    statement_types => 'SELECT,UPDATE',
    update_check => TRUE
  );
END;
/
CREATE OR REPLACE TRIGGER trg_check_update_dangky
BEFORE UPDATE ON DANGKY
FOR EACH ROW
DECLARE
  v_role VARCHAR2(50);
BEGIN
  SELECT MAX(VAITRO) INTO v_role FROM NHANVIEN
  WHERE MANV = SYS_CONTEXT('USERENV','SESSION_USER');

  IF UPPER(v_role) != 'NVPKT' THEN
    RAISE_APPLICATION_ERROR(-20001, 'Bạn không có quyền cập nhật bảng DANGKY');
  END IF;

  -- Kiểm tra chỉ cho phép cập nhật các trường điểm
  IF UPDATING THEN
    IF
      ( :OLD.DIEMTH <> :NEW.DIEMTH OR
        :OLD.DIEMQT <> :NEW.DIEMQT OR
        :OLD.DIEMCK <> :NEW.DIEMCK OR
        :OLD.DIEMTK <> :NEW.DIEMTK ) THEN
        NULL; -- cho phép thay đổi điểm
    ELSE
      -- Kiểm tra có cập nhật cột khác không
      IF (:OLD.MASV <> :NEW.MASV OR :OLD.MAMM <> :NEW.MAMM) THEN
        RAISE_APPLICATION_ERROR(-20002, 'Không được phép cập nhật trường ngoài điểm số');
      END IF;
    END IF;
  END IF;
END;
/

-- 7. Grant quyền SELECT cho user có role NVPKT
DECLARE
    v_count NUMBER;
BEGIN
    FOR usr IN (
        SELECT grantee 
        FROM dba_role_privs 
        WHERE granted_role = 'NVPKT'
          AND grantee IN (SELECT username FROM all_users)
    ) LOOP
        SELECT COUNT(*) INTO v_count FROM all_users WHERE username = usr.grantee;
        IF v_count > 0 THEN
            BEGIN
                EXECUTE IMMEDIATE 'GRANT SELECT ON MOMON TO "' || usr.grantee || '"';

                EXECUTE IMMEDIATE 'GRANT SELECT ON HOCPHAN TO "' || usr.grantee || '"';
                EXECUTE IMMEDIATE 'GRANT SELECT ON SINHVIEN TO "' || usr.grantee || '"';
                EXECUTE IMMEDIATE 'GRANT SELECT, UPDATE ON DANGKY TO "' || usr.grantee || '"';

                DBMS_OUTPUT.PUT_LINE('✅ Grant SELECT to user: ' || usr.grantee);
            EXCEPTION WHEN OTHERS THEN
                DBMS_OUTPUT.PUT_LINE('❌ Error granting SELECT to user ' || usr.grantee || ': ' || SQLERRM);
            END;
        END IF;
    END LOOP;
    DBMS_OUTPUT.PUT_LINE('🎯 Finished granting SELECT to NVPKT users.');
END;
/

-- Kết thúc session
QUIT;
