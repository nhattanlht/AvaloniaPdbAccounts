conn sys/123@localhost:1521/PDB as sysdba;
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
    -- Check if user is a student (requires EXEMPT ACCESS POLICY to avoid recursive VPD trigger)
    -- Assuming function runs with privileges to bypass VPD for this check
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
        -- Deny access for other users (including NVPDT for SELECT, as not specified)
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
    -- Check if user is a student (requires EXEMPT ACCESS POLICY to avoid recursive VPD trigger)
    -- Assuming function runs with privileges to bypass VPD for this check
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
        -- NV PDT can update TINHTRANG
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
        update_check    => TRUE,
        sec_relevant_cols => 'DCHI,DT,TINHTRANG'
    );
END;
/

-- Grant necessary privileges to roles
-- Grant SELECT permission to students (SV)
GRANT SELECT ON ADMINPDB.SINHVIEN TO SV;

-- Grant UPDATE on specific columns (DCHI, DT) to students (SV)
GRANT UPDATE (DCHI, DT) ON ADMINPDB.SINHVIEN TO SV;

-- Grant SELECT, INSERT, UPDATE, DELETE permissions to NVPCTSV
GRANT SELECT, INSERT, UPDATE, DELETE ON ADMINPDB.SINHVIEN TO NVPCTSV;

-- Grant SELECT permission to GV (instructors)
GRANT SELECT ON ADMINPDB.SINHVIEN TO GV;

-- Grant UPDATE permission to NVPDT (academic staff) for updating TINHTRANG
GRANT SELECT, UPDATE (TINHTRANG) ON ADMINPDB.SINHVIEN TO NVPDT;

-- Grant EXEMPT ACCESS POLICY to the user executing the policy functions
-- This ensures the policy function can access SINHVIEN without triggering VPD
GRANT EXEMPT ACCESS POLICY TO ADMINPDB;

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

GRANT EXECUTE ON adminpdb.get_semester_start_date TO PUBLIC;

-- Policy function for DANGKY table
CREATE OR REPLACE FUNCTION adminpdb.dangky_policy_function (
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
        v_predicate := 'MAMM IN (SELECT MAMM FROM ADMINPDB.MOMON_PDT WHERE SYSDATE <= adminpdb.get_semester_start_date(HK, NAM) + 40)';
    ELSIF v_role = 'NVPKT' THEN
        -- NV PKT can view all DANGKY data
        v_predicate := '1=1';
    ELSIF v_role = 'GV' THEN
        -- GV can view DANGKY for classes they teach (via MOMON.MAGV)
        v_predicate := 'MAMM IN (SELECT MAMM FROM ADMINPDB.MOMON_GV WHERE MAGV = ''' || v_user || ''')';
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
CREATE OR REPLACE FUNCTION adminpdb.dangky_modify_policy (
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
        v_predicate := 'MASV = ''' || v_user || ''' AND MAMM IN (SELECT MAMM FROM ADMINPDB.MOMON_SV WHERE SYSDATE <= adminpdb.get_semester_start_date(HK, NAM) + 40) AND DIEMTH IS NULL AND DIEMQT IS NULL AND DIEMCK IS NULL AND DIEMTK IS NULL';
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
        v_predicate := 'MAMM IN (SELECT MAMM FROM ADMINPDB.MOMON_PDT WHERE SYSDATE <= adminpdb.get_semester_start_date(HK, NAM) + 40) AND DIEMTH IS NULL AND DIEMQT IS NULL AND DIEMCK IS NULL AND DIEMTK IS NULL';
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

----------------------------
-- Addition procedure to get MetaData
----------------------------
-- Force to delete Dangky (and also delete Momon to ensure consistency) for NVPDT
GRANT EXECUTE ON DBMS_RLS TO ADMINPDB;

QUIT;