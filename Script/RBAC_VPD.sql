-- Kết nối:
conn AdminPdb/123@localhost:1521/PDB

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
SELECT NV.MANLD, NV.HOTEN, NV.PHAI, NV.NGSINH, NV.DT, NV.VAITRO, NV.MADV FROM ADMINPDB.NHANVIEN NV
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

--------------------------------
-- CÂU 3
--------------------------------
-- Policy function for SELECT operations on SINHVIEN
CREATE OR REPLACE FUNCTION sinhvien_select_policy (
    p_schema IN VARCHAR2,
    p_object IN VARCHAR2
) RETURN VARCHAR2 
AUTHID CURRENT_USER AS
    v_user VARCHAR2(20) := SYS_CONTEXT('USERENV', 'SESSION_USER');
    v_role VARCHAR2(50);
    v_predicate VARCHAR2(4000);
    v_student_count NUMBER;
BEGIN
    -- Check if user is a student
    SELECT COUNT(*) 
    INTO v_student_count 
    FROM ADMINPDB.SINHVIEN 
    WHERE MASV = v_user;
    
    IF v_student_count > 0 THEN
        -- Students can view their own data
        v_predicate := 'MASV = ''' || v_user || '''';
        RETURN v_predicate;
    END IF;

    -- Check user's role from NHANVIEN
    BEGIN
        SELECT VAITRO 
        INTO v_role 
        FROM ADMINPDB.NHANVIEN 
        WHERE MANLD = v_user;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            v_role := NULL;
    END;

    IF v_role = 'NVPCTSV' THEN
        -- NV PCTSV can view all SINHVIEN data
        v_predicate := '1=1';
    ELSIF v_role = 'GV' THEN
        -- GV can view SINHVIEN data for their department
        v_predicate := 'KHOA = (SELECT MADV FROM ADMINPDB.NHANVIEN WHERE MANLD = ''' || v_user || ''')';
    ELSE
        -- Deny access for other users (including NV PĐT for SELECT, as not specified)
        v_predicate := '1=0';
    END IF;

    RETURN v_predicate;
END;
/

-- Policy function for INSERT, UPDATE, DELETE operations on SINHVIEN
CREATE OR REPLACE FUNCTION sinhvien_modify_policy (
    p_schema IN VARCHAR2,
    p_object IN VARCHAR2
) RETURN VARCHAR2 
AUTHID CURRENT_USER AS
    v_user VARCHAR2(20) := SYS_CONTEXT('USERENV', 'SESSION_USER');
    v_role VARCHAR2(50);
    v_predicate VARCHAR2(4000);
    v_student_count NUMBER;
BEGIN
    -- Check if user is a student
    SELECT COUNT(*) 
    INTO v_student_count 
    FROM ADMINPDB.SINHVIEN 
    WHERE MASV = v_user;
    
    IF v_student_count > 0 THEN
        -- Students can update their own DCHI and DT
        v_predicate := 'MASV = ''' || v_user || '''';
        RETURN v_predicate;
    END IF;

    -- Check user's role from NHANVIEN
    BEGIN
        SELECT VAITRO 
        INTO v_role 
        FROM ADMINPDB.NHANVIEN 
        WHERE MANLD = v_user;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            v_role := NULL;
    END;

    IF v_role = 'NVPCTSV' THEN
        -- NV PCTSV can modify all SINHVIEN data, but TINHTRANG must be NULL
        v_predicate := 'TINHTRANG IS NULL';
    ELSIF v_role = 'NVPDT' THEN
        -- NV PĐT can update TINHTRANG
        v_predicate := '1=1';
    ELSE
        -- Deny modification for other users (including GV)
        v_predicate := '1=0';
    END IF;

    RETURN v_predicate;
END;
/

-- Apply VPD policies to SINHVIEN table
BEGIN
    -- Policy for SELECT
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'ADMINPDB',
        object_name     => 'SINHVIEN',
        policy_name     => 'SINHVIEN_SELECT_POLICY',
        function_schema => 'ADMINPDB',
        policy_function => 'sinhvien_select_policy',
        statement_types  => 'SELECT',
        update_check    => FALSE
    );

    -- Policy for INSERT, UPDATE, DELETE
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'ADMINPDB',
        object_name     => 'SINHVIEN',
        policy_name     => 'SINHVIEN_MODIFY_POLICY',
        function_schema => 'ADMINPDB',
        policy_function => 'sinhvien_modify_policy',
        statement_types  => 'INSERT,UPDATE,DELETE',
        update_check    => TRUE
    );
END;
/

-- Grant SELECT permission to students (SV)
GRANT SELECT ON ADMINPDB.SINHVIEN TO SV;

-- Grant SELECT, INSERT, UPDATE, DELETE permissions to NVPCTSV
GRANT SELECT, INSERT, UPDATE, DELETE ON ADMINPDB.SINHVIEN TO NVPCTSV;

-- Grant SELECT permission to GV (instructors)
GRANT SELECT ON ADMINPDB.SINHVIEN TO GV;

-- Grant UPDATE permission to NVPDT (academic staff) for updating TINHTRANG
GRANT UPDATE ON ADMINPDB.SINHVIEN TO NVPDT;

--------------------------------
-- CÂU 4
--------------------------------
-- Function to determine semester start date based on HK and NAM
CREATE OR REPLACE FUNCTION get_semester_start_date (
    p_hk NUMBER,
    p_nam NUMBER
) RETURN DATE AS
BEGIN
    CASE p_hk
        WHEN 1 THEN RETURN TO_DATE(TO_CHAR(p_nam) || '-09-01', 'YYYY-MM-DD');
        WHEN 2 THEN RETURN TO_DATE(TO_CHAR(p_nam + 1) || '-01-01', 'YYYY-MM-DD');
        WHEN 3 THEN RETURN TO_DATE(TO_CHAR(p_nam + 1) || '-05-01', 'YYYY-MM-DD');
        ELSE RAISE_APPLICATION_ERROR(-20001, 'Invalid semester (HK)');
    END CASE;
END;
/

-- Policy function for DANGKY table
CREATE OR REPLACE FUNCTION dangky_policy_function (
    p_schema IN VARCHAR2,
    p_object IN VARCHAR2
) RETURN VARCHAR2 
AUTHID CURRENT_USER AS
    v_user VARCHAR2(20) := SYS_CONTEXT('USERENV', 'SESSION_USER');
    v_role VARCHAR2(50);
    v_predicate VARCHAR2(4000);
    v_student_count NUMBER;
BEGIN
    -- Check if user is a student
    SELECT COUNT(*) 
    INTO v_student_count 
    FROM ADMINPDB.SINHVIEN 
    WHERE MASV = v_user;
    
    IF v_student_count > 0 THEN
        -- Students can view their own DANGKY data
        v_predicate := 'MASV = ''' || v_user || '''';
        RETURN v_predicate;
    END IF;

    -- Check user's role from NHANVIEN
    BEGIN
        SELECT VAITRO 
        INTO v_role 
        FROM ADMINPDB.NHANVIEN 
        WHERE MANLD = v_user;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            v_role := NULL;
    END;

    IF v_role = 'NVPDT' THEN
        -- NVPDT can view DANGKY for MOMON within 14 days of semester start
        v_predicate := 'MAMM IN (SELECT MAMM FROM ADMINPDB.MOMON WHERE SYSDATE <= get_semester_start_date(HK, NAM) + 14)';
    ELSIF v_role = 'NVPKT' THEN
        -- NV PKT can view all DANGKY data
        v_predicate := '1=1';
    ELSIF v_role = 'GV' THEN
        -- GV can view DANGKY for classes they teach (via MOMON.MAGV)
        v_predicate := 'MAMM IN (SELECT MAMM FROM ADMINPDB.MOMON WHERE MAGV = ''' || v_user || ''')';
    ELSE
        -- Deny access for other users
        v_predicate := '1=0';
    END IF;

    RETURN v_predicate;
END;
/

BEGIN
    DBMS_RLS.ADD_POLICY (
        object_schema   => 'ADMINPDB',
        object_name     => 'DANGKY',
        policy_name     => 'DANGKY_SELECT_POLICY',
        function_schema => 'ADMINPDB',
        policy_function => 'dangky_policy_function',
        statement_types => 'SELECT',
        update_check    => FALSE
    );
END;
/

-- Policy function for INSERT, UPDATE, DELETE operations on DANGKY
CREATE OR REPLACE FUNCTION dangky_modify_policy (
    p_schema IN VARCHAR2,
    p_object IN VARCHAR2
) RETURN VARCHAR2 
AUTHID CURRENT_USER AS
    v_user VARCHAR2(20) := SYS_CONTEXT('USERENV', 'SESSION_USER');
    v_role VARCHAR2(50);
    v_predicate VARCHAR2(4000);
    v_student_count NUMBER;
BEGIN
    -- Check if user is a student
    SELECT COUNT(*) 
    INTO v_student_count 
    FROM ADMINPDB.SINHVIEN 
    WHERE MASV = v_user;
    
    IF v_student_count > 0 THEN
        -- Students can modify their own DANGKY records within 14 days of semester start, where grades are NULL
        v_predicate := 'MASV = ''' || v_user || ''' AND MAMM IN (SELECT MAMM FROM ADMINPDB.MOMON WHERE SYSDATE <= get_semester_start_date(HK, NAM) + 14) AND DIEMTH IS NULL AND DIEMQT IS NULL AND DIEMCK IS NULL AND DIEMTK IS NULL';
        RETURN v_predicate;
    END IF;

    -- Check user's role from NHANVIEN
    BEGIN
        SELECT VAITRO 
        INTO v_role 
        FROM ADMINPDB.NHANVIEN 
        WHERE MANLD = v_user;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            v_role := NULL;
    END;

    IF v_role = 'NVPDT' THEN
        -- NV PĐT can modify DANGKY for MOMON within 14 days of semester start, where grades are NULL
        v_predicate := 'MAMM IN (SELECT MAMM FROM ADMINPDB.MOMON WHERE SYSDATE <= get_semester_start_date(HK, NAM) + 14) AND DIEMTH IS NULL AND DIEMQT IS NULL AND DIEMCK IS NULL AND DIEMTK IS NULL';
    ELSIF v_role = 'NVPKT' THEN
        -- NV PKT can update grade fields (no restriction on time or grades)
        v_predicate := '1=1';
    ELSE
        -- Deny modification for other users
        v_predicate := '1=0';
    END IF;

    RETURN v_predicate;
END;
/

-- Apply VPD policies to DANGKY table
BEGIN
    -- Policy for INSERT, UPDATE, DELETE
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'ADMINPDB',
        object_name     => 'DANGKY',
        policy_name     => 'DANGKY_MODIFY_POLICY',
        function_schema => 'ADMINPDB',
        policy_function => 'dangky_modify_policy',
        statement_types  => 'INSERT,UPDATE,DELETE',
        update_check    => TRUE
    );
END;
/

-- Cấp quyền cho sinh viên (SV)
GRANT SELECT, INSERT, UPDATE ON ADMINPDB.DANGKY TO SV;

-- Cấp quyền cho nhân viên phòng đào tạo (NVPDT)
GRANT SELECT, INSERT, UPDATE, DELETE ON ADMINPDB.DANGKY TO NVPDT;

-- Cấp quyền cho nhân viên phòng khảo thí (NVPKT)
GRANT SELECT, UPDATE ON ADMINPDB.DANGKY TO NVPKT;

-- Cấp quyền cho giảng viên (GV)
GRANT SELECT ON ADMINPDB.DANGKY TO GV;

QUIT;
