-- create_oracle_scheduler_rman_job.sql
-- Kết nối vào CDB$ROOT với quyền SYS hoặc user có đủ quyền.
-- Ví dụ: sqlplus sys/YourPassword@//localhost:1521/FREEPDB1 AS SYSDBA (nếu CDB là FREE)
-- HOẶC sqlplus sys/YourPassword AS SYSDBA (nếu ORACLE_SID trỏ đến CDB)

SET SERVEROUTPUT ON;
DECLARE
  l_job_name VARCHAR2(100) := 'NIGHTLY_PDB_RMAN_BACKUP_JOB'; -- Đổi tên job một chút
  l_job_exists NUMBER;
BEGIN
  -- Kiểm tra xem job đã tồn tại chưa
  SELECT COUNT(*)
  INTO l_job_exists
  FROM dba_scheduler_jobs
  WHERE job_name = l_job_name;

  IF l_job_exists > 0 THEN
    DBMS_OUTPUT.PUT_LINE('Job ' || l_job_name || ' already exists. Dropping it first...');
    DBMS_SCHEDULER.DROP_JOB(job_name => l_job_name, force => TRUE);
    DBMS_OUTPUT.PUT_LINE('Job ' || l_job_name || ' dropped.');
  END IF;

  DBMS_OUTPUT.PUT_LINE('Creating Scheduler Job: ' || l_job_name);
  DBMS_SCHEDULER.CREATE_JOB (
    job_name        => l_job_name,
    job_type        => 'EXECUTABLE', -- Loại job sẽ chạy một file thực thi hoặc script từ OS
    job_action      => '/tmp/run_pdb_backup.sh', -- ĐÂY LÀ SCRIPT SHELL SẼ ĐƯỢC CHẠY (đường dẫn bên trong container)
    -- job_action      => 'D:\OracleScripts\run_rman_backup_windows.bat', -- << THAY THẾ BẰNG ĐƯỜNG DẪN THỰC TẾ ĐẾN SCRIPT WINDOWS CỦA BẠN
    -- thay đổi file sh thành file bat chạy thực thi được bên windows 
    start_date      => TRUNC(SYSTIMESTAMP AT TIME ZONE 'UTC') + INTERVAL '2' HOUR, -- Bắt đầu từ 2h sáng UTC (điều chỉnh múi giờ nếu cần)
                                                                              -- Hoặc dùng SYSTIMESTAMP nếu CSDL và OS cùng múi giờ với mong muốn của bạn
    repeat_interval => 'FREQ=DAILY; BYHOUR=2; BYMINUTE=0; BYSECOND=0', -- Lặp lại mỗi ngày lúc 2h sáng
    enabled         => TRUE, -- Kích hoạt job ngay sau khi tạo
    auto_drop       => FALSE, -- Không tự động xóa job sau khi hoàn thành
    comments        => 'Nightly RMAN backup for a specific PDB, triggered by Oracle Scheduler. Ensures run_pdb_backup.sh is at /tmp/ and executable.'
  );
  DBMS_OUTPUT.PUT_LINE('Job ' || l_job_name || ' created and enabled.');
  DBMS_OUTPUT.PUT_LINE('It will run the script: /tmp/run_pdb_backup.sh');
  DBMS_OUTPUT.PUT_LINE('Ensure the script /tmp/run_pdb_backup.sh is present, configured, and executable inside the Oracle Docker container.');

EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Error creating job ' || l_job_name || ': ' || SQLERRM);
    RAISE;
END;
/

PROMPT Verification: Check DBA_SCHEDULER_JOBS for the job status.
SELECT JOB_NAME, STATE, ENABLED, LAST_START_DATE, NEXT_RUN_DATE
FROM DBA_SCHEDULER_JOBS
WHERE JOB_NAME = 'NIGHTLY_PDB_RMAN_BACKUP_JOB';

PROMPT To see run details later:
PROMPT SELECT LOG_DATE, JOB_NAME, STATUS, ERROR#, CPU_USED, ADDITIONAL_INFO FROM DBA_SCHEDULER_JOB_RUN_DETAILS WHERE JOB_NAME = 'NIGHTLY_PDB_RMAN_BACKUP_JOB' ORDER BY LOG_DATE DESC;