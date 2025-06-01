conn AdminPdb/123@localhost:1521/PDB;

CREATE OR REPLACE PROCEDURE force_delete_dangky_and_momon(p_mamm VARCHAR2)
AUTHID DEFINER
IS
BEGIN
    -- Tạm tắt VPD policy
    DBMS_RLS.ENABLE_POLICY(
        object_schema  => 'ADMINPDB',
        object_name    => 'DANGKY',
        policy_name    => 'DANGKY_MODIFY_POLICY',
        enable         => FALSE
    );

    DELETE FROM ADMINPDB.DANGKY
    WHERE MAMM = p_mamm;
    
    DELETE FROM ADMINPDB.MOMON
    WHERE MAMM = p_mamm;

    -- Bật lại VPD policy
    DBMS_RLS.ENABLE_POLICY(
        object_schema  => 'ADMINPDB',
        object_name    => 'DANGKY',
        policy_name    => 'DANGKY_MODIFY_POLICY',
        enable         => TRUE
    );
END;
/

GRANT EXECUTE ON ADMINPDB.force_delete_dangky_and_momon TO NVPDT;

CREATE OR REPLACE PROCEDURE get_modules(p_result OUT SYS_REFCURSOR) AUTHID DEFINER IS
BEGIN
  OPEN p_result FOR
    SELECT MAHP, TENHP FROM adminpdb.HOCPHAN;
END;
/

-- Procedure to get instructors
CREATE OR REPLACE PROCEDURE get_instructors(p_result OUT SYS_REFCURSOR) AUTHID DEFINER IS
BEGIN
  OPEN p_result FOR
    SELECT MANLD, HOTEN FROM adminpdb.NHANVIEN WHERE VAITRO = 'GV';
END;
/

GRANT EXECUTE ON ADMINPDB.get_modules TO NVPDT;
GRANT EXECUTE ON ADMINPDB.get_instructors TO NVPDT;


QUIT;
