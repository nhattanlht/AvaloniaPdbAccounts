ALTER SESSION SET CONTAINER = PDB;
conn sys/123@localhost:1521/FREE as sysdba;
-- Đặt thời gian giữ undo là 3600 giây (1 giờ)
ALTER SYSTEM SET UNDO_RETENTION = 3600 SCOPE=BOTH;
-- ===============TRƯỜNG HỢP BỊ GHI NHẬN AUDIT VÀ KHÔI PHỤC LẠI DỮ LIỆU BẰNG FLASHBACK===============
-- ***************Ví dụ trường hợp cập nhật điểm cuối kỳ sai qui định của PKT NV00016***********************

-- Kiểm tra dữ liệu hiện tại
conn AdminPdb/123@localhost:1521/PDB;
SELECT MASV, MAMM, DIEMTH, DIEMQT, DIEMCK, DIEMTK 
FROM ADMINPDB.DANGKY 
WHERE MASV = 'A536166';

--Cập nhật điểm CK trên UI =====> dotnet run

-- Kiểm tra dữ liệu tại thời điểm trong quá khứ (điền thời gian vào)
SELECT MASV, MAMM, DIEMTH, DIEMQT, DIEMCK, DIEMTK 
FROM ADMINPDB.DANGKY AS OF TIMESTAMP 
    TO_TIMESTAMP('2025-05-31 20:18:41', 'YYYY-MM-DD HH24:MI:SS')
WHERE MASV = 'A536166';

-- viết procedure undo điểm cuối kỳ ngay thời điểm chỉnh sửa
conn AdminPdb/123@localhost:1521/PDB;
CREATE OR REPLACE PROCEDURE UNDO_DIEM_FROMTIME (
    p_masv IN VARCHAR2,
    p_time IN TIMESTAMP
) AS
    v_count NUMBER := 0;
BEGIN
    -- Cập nhật tất cả điểm của sinh viên từ dữ liệu tại thời điểm p_time
    FOR rec IN (
        SELECT MASV, MAMM, DIEMTH, DIEMQT, DIEMCK, DIEMTK
        FROM ADMINPDB.DANGKY
        AS OF TIMESTAMP p_time
        WHERE MASV = p_masv
    ) LOOP
        UPDATE ADMINPDB.DANGKY
        SET DIEMTH = rec.DIEMTH,
            DIEMQT = rec.DIEMQT,
            DIEMCK = rec.DIEMCK,
            DIEMTK = rec.DIEMTK
        WHERE MASV = rec.MASV 
        AND MAMM = rec.MAMM;
        
        v_count := v_count + 1;
        
        DBMS_OUTPUT.PUT_LINE('Đã khôi phục điểm của SV ' || rec.MASV || ' môn ' || rec.MAMM ||
            ' về: TH=' || rec.DIEMTH || 
            ', QT=' || rec.DIEMQT || 
            ', CK=' || rec.DIEMCK || 
            ', TK=' || rec.DIEMTK);
    END LOOP;
    
    COMMIT;
    
    DBMS_OUTPUT.PUT_LINE('Tổng số môn học đã được khôi phục: ' || v_count);
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('Không tìm thấy dữ liệu tại thời điểm yêu cầu cho SV ' || p_masv);
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('Lỗi: ' || SQLERRM);
END;
/

-- Thực hiện undo tất cả điểm (điền thời gian vào)
BEGIN
  UNDO_DIEM_FROMTIME(
    'A536166',
    TO_TIMESTAMP('2025-05-31 20:18:41', 'YYYY-MM-DD HH24:MI:SS')
  );
END;
/

--kiểm tra lại điểm
SELECT * FROM ADMINPDB.DANGKY WHERE MASV = 'A536166';

--***********Ví dụ trường hợp cập nhật lương sai quy định của NVTCHC NV00018********
--NVTCHC update LUONG của NV00001 và thao tác đọc bảng nhân viên

--Kiểm tra lương hiện tại
SELECT LUONG
FROM ADMINPDB.NHANVIEN WHERE MANLD = 'NV00001';


conn NV00018/123@localhost:1521/PDB;
UPDATE ADMINPDB.NHANVIEN
SET LUONG = LUONG + 1500000
WHERE MANLD = 'NV00001';
SELECT * FROM ADMINPDB.NHANVIEN;

--Proc khôi phục lại lương tại thời điểm vi phạm
conn AdminPdb/123@localhost:1521/PDB;
CREATE OR REPLACE PROCEDURE UNDO_LUONG (
    p_MANLD IN VARCHAR2,
    p_time  IN TIMESTAMP
) AS
    v_luong_old NHANVIEN.LUONG%TYPE;
BEGIN
    -- Lấy giá trị lương cũ tại thời điểm trước cập nhật
    SELECT LUONG INTO v_luong_old
    FROM ADMINPDB.NHANVIEN
    AS OF TIMESTAMP p_time
    WHERE MANLD = p_MANLD;

    -- Cập nhật lại lương
    UPDATE ADMINPDB.NHANVIEN
    SET LUONG = v_luong_old
    WHERE MANLD = p_MANLD;

    DBMS_OUTPUT.PUT_LINE('Đã khôi phục LUONG của ' || p_MANLD || ' về: ' || v_luong_old);
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('Không tìm thấy dữ liệu tại thời điểm yêu cầu.');
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Lỗi: ' || SQLERRM);
END;
/
--thay thời gian xảy ra vi phạm vào
BEGIN
  UNDO_LUONG('NV00001', TO_TIMESTAMP('2025-05-31 15:51:56', 'YYYY-MM-DD HH24:MI:SS'));
END;
/

--kiểm tra lại
SELECT LUONG
FROM ADMINPDB.NHANVIEN WHERE MANLD = 'NV00001';






