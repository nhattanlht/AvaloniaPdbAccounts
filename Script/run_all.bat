@echo off
sqlplus system/123456@localhost:1521/QLNHANVIEN @tao_csdl.sql
sqlldr system/123456@localhost:1521/QLNHANVIEN control=sinhvien.ctl log=data_sinhvien.log bad=sinhvien.bad
pause
