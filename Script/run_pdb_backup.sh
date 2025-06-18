#!/bin/bash

# ==============================================================================
# Script tự động sao lưu một Pluggable Database (PDB) bằng RMAN
# Chạy script này BÊN TRONG Docker container Oracle.
# ==============================================================================

# --- CẤU HÌNH NGƯỜI DÙNG (CHỈNH SỬA CÁC GIÁ TRỊ NÀY NẾU CẦN) ---

# Tên Pluggable Database (PDB) cần sao lưu.
# Với image container-registry.oracle.com/database/free, tên PDB mặc định thường là "FREEPDB1".
# Hãy xác minh lại bằng cách kết nối SQL*Plus vào CDB và chạy: SELECT NAME FROM V$PDBS;
TARGET_PDB_NAME="PDB" # << THAY THẾ BẰNG TÊN PDB THỰC TẾ CỦA BẠN

# Thư mục lưu trữ các file backup RMAN (bên trong container).
# Đường dẫn này phải khớp với đích của volume mount trong lệnh 'docker run'.
BACKUP_BASE_DIR="/opt/oracle/oradata/backups" # << Ví dụ: đã mount từ host vào đây

# Thư mục lưu trữ file log của RMAN (bên trong container).
# Đường dẫn này phải khớp với đích của volume mount trong lệnh 'docker run'.
LOG_BASE_DIR="/opt/oracle/oradata/logs"       # << Ví dụ: đã mount từ host vào đây

# Đường dẫn đầy đủ đến Oracle Home BÊN TRONG CONTAINER.
# Kiểm tra lại đường dẫn này nếu bạn dùng image Oracle khác hoặc phiên bản khác.
ORACLE_HOME="/opt/oracle/product/23ai/dbhomeFree" # << THAY THẾ NẾU PHIÊN BẢN KHÁC (vd: /opt/oracle/product/21c/dbhome_1)
# --- KẾT THÚC CẤU HÌNH NGƯỜI DÙNG ---


# --- THIẾT LẬP MÔI TRƯỜNG VÀ BIẾN ---

export ORACLE_HOME
PATH="${ORACLE_HOME}/bin:${PATH}"
export PATH

# Đường dẫn đến RMAN executable
RMAN_EXECUTABLE="${ORACLE_HOME}/bin/rman"

# Tạo thư mục backup và log nếu chưa tồn tại
# Lệnh 'mkdir -p' sẽ không báo lỗi nếu thư mục đã tồn tại.
# Quan trọng: User chạy script (thường là 'oracle' trong container) phải có quyền ghi
# vào thư mục cha của BACKUP_BASE_DIR và LOG_BASE_DIR để tạo chúng.
# Ví dụ: nếu BACKUP_BASE_DIR="/opt/oracle/oradata/backups", user oracle phải có quyền
# ghi vào "/opt/oracle/oradata". Điều này thường đúng với các image Oracle chuẩn.
echo "Đảm bảo thư mục backup tồn tại: ${BACKUP_BASE_DIR}"
mkdir -p "${BACKUP_BASE_DIR}"
RC_MKDIR_BACKUP=$?
if [ ${RC_MKDIR_BACKUP} -ne 0 ]; then
    echo "LỖI: Không thể tạo hoặc truy cập thư mục backup: ${BACKUP_BASE_DIR}. Mã lỗi: ${RC_MKDIR_BACKUP}"
    echo "Kiểm tra quyền ghi và volume mount."
    exit 1
fi

echo "Đảm bảo thư mục log tồn tại: ${LOG_BASE_DIR}"
mkdir -p "${LOG_BASE_DIR}"
RC_MKDIR_LOG=$?
if [ ${RC_MKDIR_LOG} -ne 0 ]; then
    echo "LỖI: Không thể tạo hoặc truy cập thư mục log: ${LOG_BASE_DIR}. Mã lỗi: ${RC_MKDIR_LOG}"
    echo "Kiểm tra quyền ghi và volume mount."
    exit 1
fi

# Tạo các biến tên file động
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
RMAN_LOG_FILE="${LOG_BASE_DIR}/rman_backup_${TARGET_PDB_NAME}_${TIMESTAMP}.log"
RMAN_CMD_FILE="/tmp/rman_cmd_${TARGET_PDB_NAME}_${TIMESTAMP}.rman" # File tạm trong /tmp

# Tag cho bản backup để dễ nhận diện
BACKUP_TAG="${TARGET_PDB_NAME}_SAB_${TIMESTAMP}"

# Định dạng tên cho file backup (RMAN format string).
BACKUP_FILE_FORMAT="${BACKUP_BASE_DIR}/${TARGET_PDB_NAME}_%d_%T_%s_%p.bak"

# Định dạng tên cho file controlfile autobackup.
CONTROLFILE_AUTOBACKUP_FORMAT="${BACKUP_BASE_DIR}/cf_%F.bak"


# --- DEBUGGING: In ra các giá trị biến quan trọng ---
echo "--- DEBUG INFO ---"
echo "TARGET_PDB_NAME: [${TARGET_PDB_NAME}]"
echo "BACKUP_BASE_DIR: [${BACKUP_BASE_DIR}]"
echo "LOG_BASE_DIR:    [${LOG_BASE_DIR}]"
echo "ORACLE_HOME:     [${ORACLE_HOME}]"
echo "PATH:            [${PATH}]"
echo "RMAN_EXECUTABLE: [${RMAN_EXECUTABLE}]"
echo "TIMESTAMP:       [${TIMESTAMP}]"
echo "RMAN_LOG_FILE:   [${RMAN_LOG_FILE}]"
echo "RMAN_CMD_FILE:   [${RMAN_CMD_FILE}]"
echo "BACKUP_TAG:      [${BACKUP_TAG}]"
echo "BACKUP_FILE_FORMAT:            [${BACKUP_FILE_FORMAT}]"
echo "CONTROLFILE_AUTOBACKUP_FORMAT: [${CONTROLFILE_AUTOBACKUP_FORMAT}]"
echo "--- END DEBUG INFO ---"


# --- KIỂM TRA RMAN EXECUTABLE ---
if [ -z "${RMAN_EXECUTABLE}" ]; then
    echo "LỖI: Biến RMAN_EXECUTABLE rỗng. Kiểm tra ORACLE_HOME."
    exit 1
fi
if [ ! -x "${RMAN_EXECUTABLE}" ]; then
    echo "LỖI: Không tìm thấy RMAN executable tại '${RMAN_EXECUTABLE}' hoặc không có quyền thực thi."
    echo "Vui lòng kiểm tra biến ORACLE_HOME và PATH trong script."
    exit 1
fi


# --- THỰC THI RMAN ---
echo "----------------------------------------------------------------------"
echo "Bắt đầu quá trình sao lưu PDB: ${TARGET_PDB_NAME}"
echo "Thời gian bắt đầu: $(date)"
echo "File RMAN command tạm thời: ${RMAN_CMD_FILE}"
echo "File Log RMAN: ${RMAN_LOG_FILE}"
echo "Thư mục Backup: ${BACKUP_BASE_DIR}"
echo "Tag Backup: ${BACKUP_TAG}"
echo "----------------------------------------------------------------------"

# Tạo nội dung cho file command RMAN
echo "Đang tạo file command RMAN: [${RMAN_CMD_FILE}]"
cat > "${RMAN_CMD_FILE}" <<EOF
CONNECT TARGET /;

CONFIGURE DEFAULT DEVICE TYPE TO DISK;
CONFIGURE CONTROLFILE AUTOBACKUP ON;
CONFIGURE CONTROLFILE AUTOBACKUP FORMAT FOR DEVICE TYPE DISK TO '${CONTROLFILE_AUTOBACKUP_FORMAT}';

RUN {
    ALLOCATE CHANNEL ch1 DEVICE TYPE DISK FORMAT '${BACKUP_FILE_FORMAT}';
    BACKUP PLUGGABLE DATABASE ${TARGET_PDB_NAME}
        PLUS ARCHIVELOG DELETE INPUT
        TAG '${BACKUP_TAG}';
    RELEASE CHANNEL ch1;
}

LIST BACKUP OF PLUGGABLE DATABASE ${TARGET_PDB_NAME} SUMMARY TAG '${BACKUP_TAG}';
LIST BACKUP OF ARCHIVELOG ALL COMPLETED AFTER "SYSDATE - 1/24" TAG '${BACKUP_TAG}';

EXIT;
EOF

# Kiểm tra xem file command RMAN đã thực sự được tạo chưa
if [ ! -f "${RMAN_CMD_FILE}" ]; then
    echo "LỖI CRITICAL: Không thể tạo file command RMAN tại '${RMAN_CMD_FILE}'."
    echo "Kiểm tra quyền ghi vào /tmp hoặc các biến liên quan đến tên file."
    exit 1
fi
echo "File command RMAN đã được tạo thành công."

# Chạy RMAN với file command đã tạo và ghi log
echo "Đang thực thi RMAN..."
"${RMAN_EXECUTABLE}" CMDFILE="${RMAN_CMD_FILE}" LOG="${RMAN_LOG_FILE}"

# Kiểm tra mã thoát của RMAN
RMAN_EXIT_CODE=$?
if [ ${RMAN_EXIT_CODE} -ne 0 ]; then
    echo "----------------------------------------------------------------------"
    echo "LỖI: RMAN kết thúc với mã lỗi ${RMAN_EXIT_CODE}."
    echo "Vui lòng kiểm tra file log: ${RMAN_LOG_FILE} để biết chi tiết."
    echo "----------------------------------------------------------------------"
else
    echo "----------------------------------------------------------------------"
    echo "THÀNH CÔNG: Quá trình sao lưu PDB ${TARGET_PDB_NAME} đã hoàn tất."
    echo "File Log RMAN: ${RMAN_LOG_FILE}"
    echo "Các file backup đã được lưu tại: ${BACKUP_BASE_DIR}"
    echo "Thời gian kết thúc: $(date)"
    echo "----------------------------------------------------------------------"
fi

# Xóa file command RMAN tạm thời
if [ -f "${RMAN_CMD_FILE}" ]; then
    rm -f "${RMAN_CMD_FILE}"
    echo "Đã xóa file RMAN command tạm thời: ${RMAN_CMD_FILE}"
fi

exit ${RMAN_EXIT_CODE}