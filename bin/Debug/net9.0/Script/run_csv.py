import pandas as pd
import oracledb

# 1. Đọc file CSV
df = pd.read_csv('data_sinhvien_4000.csv')

# 2. Kết nối Oracle - THAY THẾ THÔNG TIN SAU BẰNG CỦA BẠN
try:
    conn = oracledb.connect(
        user='AdminPdb',
        password='123',
        dsn='localhost:1521/PDB'  # ← ĐIỀU CHỈNH DSN TẠI ĐÂY
    )

    # 3. Insert dữ liệu
    cursor = conn.cursor()
    for _, row in df.iterrows():
        cursor.execute("""
            INSERT INTO SINHVIEN 
            VALUES (:1, :2, :3, TO_DATE(:4, 'YYYY-MM-DD'), :5, :6, :7, :8)
            """,
                       (row['MASV'], row['HOTEN'], row['PHAI'], row['NGSINH'],
                        row['DCHI'], row['DT'], row['KHOA'], row['TINHTRANG'])
                       )
    conn.commit()
    print("Import dữ liệu thành công!")

except oracledb.DatabaseError as e:
    print("Lỗi Oracle:", e)
except Exception as e:
    print("Lỗi:", e)
finally:
    if 'conn' in locals():
        conn.close()