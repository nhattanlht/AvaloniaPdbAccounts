@ECHO OFF
REM ==============================================================================
REM Script Batch Windows để tự động sao lưu Oracle PDB bằng RMAN
REM Chạy bởi Oracle Scheduler trên một máy chủ Windows cài đặt Oracle DB.
REM ==============================================================================

REM --- CẤU HÌNH NGƯỜI DÙNG (CHỈNH SỬA CHO PHÙ HỢP) ---
SET TARGET_PDB_NAME=YOUR_PDB_NAME_HERE   REM << THAY BẰNG TÊN PDB THỰC TẾ, ví dụ FREEPDB1
SET ORACLE_HOME=D:\app\your_oracle_user\product\23ai\dbhomeFree  REM << THAY BẰNG ĐƯỜNG DẪN ORACLE_HOME ĐÚNG TRÊN WINDOWS
SET ORACLE_SID=YOUR_CDB_SID_HERE          REM << THAY BẰNG SID CỦA CDB (ví dụ FREE)
SET BACKUP_BASE_DIR=D:\Oracle\Backups\RMAN       REM << THAY BẰNG THƯ MỤC BACKUP CỦA BẠN
SET LOG_BASE_DIR=D:\Oracle\Backups\RMAN_Logs  REM << THAY BẰNG THƯ MỤC LOG CỦA BẠN
REM --- KẾT THÚC CẤU HÌNH NGƯỜI DÙNG ---


REM --- THIẾT LẬP MÔI TRƯỜNG VÀ BIẾN ---
ECHO Setting environment variables...
SET PATH=%ORACLE_HOME%\bin;%PATH%

REM Tạo thư mục backup và log nếu chưa tồn tại
ECHO Ensuring backup directory exists: %BACKUP_BASE_DIR%
IF NOT EXIST "%BACKUP_BASE_DIR%" MKDIR "%BACKUP_BASE_DIR%"
IF ERRORLEVEL 1 (
    ECHO ERROR: Could not create or access backup directory: %BACKUP_BASE_DIR%
    EXIT /B 1
)

ECHO Ensuring log directory exists: %LOG_BASE_DIR%
IF NOT EXIST "%LOG_BASE_DIR%" MKDIR "%LOG_BASE_DIR%"
IF ERRORLEVEL 1 (
    ECHO ERROR: Could not create or access log directory: %LOG_BASE_DIR%
    EXIT /B 1
)

REM Tạo timestamp YYYYMMDD_HHMMSS (hơi phức tạp trong batch)
FOR /F "tokens=1-4 delims=/ " %%i IN ("%date%") DO (
    FOR /F "tokens=1-3 delims=:. " %%a IN ("%time%") DO (
        SET T_YEAR=%%l
        SET T_MONTH=%%j
        SET T_DAY=%%k
        SET T_HOUR=%%a
        SET T_MINUTE=%%b
        SET T_SECOND=%%c
    )
)
REM Đảm bảo các thành phần ngày giờ có 2 chữ số
IF "%T_MONTH:~1%"=="" SET T_MONTH=0%T_MONTH%
IF "%T_DAY:~1%"=="" SET T_DAY=0%T_DAY%
IF "%T_HOUR:~1%"=="" SET T_HOUR=0%T_HOUR%
IF "%T_MINUTE:~1%"=="" SET T_MINUTE=0%T_MINUTE%
IF "%T_SECOND:~1%"=="" SET T_SECOND=0%T_SECOND%
SET TIMESTAMP=%T_YEAR%%T_MONTH%%T_DAY%_%T_HOUR%%T_MINUTE%%T_SECOND%

SET RMAN_LOG_FILE="%LOG_BASE_DIR%\rman_backup_%TARGET_PDB_NAME%_%TIMESTAMP%.log"
SET RMAN_CMD_FILE="%TEMP%\rman_cmd_%TARGET_PDB_NAME%_%TIMESTAMP%.rman"
SET BACKUP_TAG=%TARGET_PDB_NAME%_SAB_%TIMESTAMP%

REM RMAN format strings cần dấu % kép (%%) trong batch file
SET BACKUP_FILE_FORMAT="%BACKUP_BASE_DIR%\%TARGET_PDB_NAME%_%%d_%%T_%%s_%%p.bak"
SET CONTROLFILE_AUTOBACKUP_FORMAT="%BACKUP_BASE_DIR%\cf_%%F.bak"

REM --- Ghi vào file log chính (bắt đầu) ---
ECHO --- SCRIPT DEBUG INFO (run_rman_backup_windows.bat) --- > %RMAN_LOG_FILE%
ECHO TARGET_PDB_NAME: [%TARGET_PDB_NAME%] >> %RMAN_LOG_FILE%
ECHO ORACLE_HOME: [%ORACLE_HOME%] >> %RMAN_LOG_FILE%
ECHO ORACLE_SID: [%ORACLE_SID%] >> %RMAN_LOG_FILE%
ECHO BACKUP_BASE_DIR: [%BACKUP_BASE_DIR%] >> %RMAN_LOG_FILE%
ECHO LOG_BASE_DIR: [%LOG_BASE_DIR%] >> %RMAN_LOG_FILE%
ECHO PATH: [%PATH%] >> %RMAN_LOG_FILE%
ECHO TIMESTAMP: [%TIMESTAMP%] >> %RMAN_LOG_FILE%
ECHO RMAN_LOG_FILE: [%RMAN_LOG_FILE%] >> %RMAN_LOG_FILE%
ECHO RMAN_CMD_FILE: [%RMAN_CMD_FILE%] >> %RMAN_LOG_FILE%
ECHO BACKUP_TAG: [%BACKUP_TAG%] >> %RMAN_LOG_FILE%
ECHO BACKUP_FILE_FORMAT: [%BACKUP_FILE_FORMAT%] >> %RMAN_LOG_FILE%
ECHO CONTROLFILE_AUTOBACKUP_FORMAT: [%CONTROLFILE_AUTOBACKUP_FORMAT%] >> %RMAN_LOG_FILE%
ECHO --- END SCRIPT DEBUG INFO --- >> %RMAN_LOG_FILE%


REM --- KIỂM TRA RMAN EXECUTABLE ---
IF NOT EXIST "%ORACLE_HOME%\bin\rman.exe" (
    ECHO ERROR: RMAN executable not found at "%ORACLE_HOME%\bin\rman.exe" >> %RMAN_LOG_FILE%
    EXIT /B 1
)

REM --- THỰC THI RMAN ---
ECHO ---------------------------------------------------------------------- >> %RMAN_LOG_FILE%
ECHO Starting PDB backup for: %TARGET_PDB_NAME% >> %RMAN_LOG_FILE%
ECHO Start time: %DATE% %TIME% >> %RMAN_LOG_FILE%
ECHO RMAN Temp Command File: %RMAN_CMD_FILE% >> %RMAN_LOG_FILE%
ECHO RMAN Log File: %RMAN_LOG_FILE% >> %RMAN_LOG_FILE%
ECHO Backup Directory: %BACKUP_BASE_DIR% >> %RMAN_LOG_FILE%
ECHO Backup Tag: %BACKUP_TAG% >> %RMAN_LOG_FILE%
ECHO ---------------------------------------------------------------------- >> %RMAN_LOG_FILE%

ECHO Creating RMAN command file: %RMAN_CMD_FILE% >> %RMAN_LOG_FILE%
REM Tạo nội dung cho file command RMAN
(
ECHO CONNECT TARGET /;
ECHO CONFIGURE DEFAULT DEVICE TYPE TO DISK;
ECHO CONFIGURE CONTROLFILE AUTOBACKUP ON;
ECHO CONFIGURE CONTROLFILE AUTOBACKUP FORMAT FOR DEVICE TYPE DISK TO '%CONTROLFILE_AUTOBACKUP_FORMAT%';
ECHO RUN {
ECHO   ALLOCATE CHANNEL ch1 DEVICE TYPE DISK FORMAT '%BACKUP_FILE_FORMAT%';
ECHO   BACKUP PLUGGABLE DATABASE %TARGET_PDB_NAME%
ECHO     PLUS ARCHIVELOG DELETE INPUT
ECHO     TAG '%BACKUP_TAG%';
ECHO   RELEASE CHANNEL ch1;
ECHO }
ECHO LIST BACKUP OF PLUGGABLE DATABASE %TARGET_PDB_NAME% SUMMARY TAG '%BACKUP_TAG%';
ECHO LIST BACKUP OF ARCHIVELOG ALL COMPLETED AFTER "SYSDATE - 1/24" TAG '%BACKUP_TAG%';
ECHO EXIT;
) > %RMAN_CMD_FILE%

IF NOT EXIST %RMAN_CMD_FILE% (
    ECHO CRITICAL ERROR: Could not create RMAN command file at %RMAN_CMD_FILE%. >> %RMAN_LOG_FILE%
    EXIT /B 1
)
ECHO RMAN command file created successfully. >> %RMAN_LOG_FILE%

ECHO Executing RMAN... >> %RMAN_LOG_FILE%
REM Chạy RMAN. Output của RMAN sẽ được ghi vào file log chính RMAN_LOG_FILE do tham số LOG=
rman CMDFILE=%RMAN_CMD_FILE% LOG=%RMAN_LOG_FILE%

SET RMAN_EXIT_CODE=%ERRORLEVEL%
REM Các thông báo chi tiết của RMAN đã nằm trong RMAN_LOG_FILE

IF %RMAN_EXIT_CODE% NEQ 0 (
    ECHO ---------------------------------------------------------------------- >> %RMAN_LOG_FILE%
    ECHO ERROR: RMAN finished with exit code %RMAN_EXIT_CODE%. >> %RMAN_LOG_FILE%
    ECHO Please check this log file (%RMAN_LOG_FILE%) for RMAN specific errors. >> %RMAN_LOG_FILE%
    ECHO ---------------------------------------------------------------------- >> %RMAN_LOG_FILE%
) ELSE (
    ECHO ---------------------------------------------------------------------- >> %RMAN_LOG_FILE%
    ECHO SUCCESS: PDB %TARGET_PDB_NAME% backup process completed. >> %RMAN_LOG_FILE%
    ECHO RMAN Log: %RMAN_LOG_FILE% >> %RMAN_LOG_FILE%
    ECHO Backup files located at: %BACKUP_BASE_DIR% >> %RMAN_LOG_FILE%
    ECHO End time: %DATE% %TIME% >> %RMAN_LOG_FILE%
    ECHO ---------------------------------------------------------------------- >> %RMAN_LOG_FILE%
)

REM Xóa file command RMAN tạm thời
IF EXIST %RMAN_CMD_FILE% (
    DEL %RMAN_CMD_FILE%
    ECHO Deleted temporary RMAN command file: %RMAN_CMD_FILE% >> %RMAN_LOG_FILE%
)

EXIT /B %RMAN_EXIT_CODE%