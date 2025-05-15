@echo off

:: Đặt mã hóa đầu ra là UTF-8
chcp 65001

:: Đặt NLS_LANG cho Oracle thành UTF-8
set NLS_LANG=AMERICAN_AMERICA.AL32UTF8

:: Gọi sqlplus để chạy file SQL
sqlplus system/123456@localhost:1521/QLNHANVIEN @database.sql

:: Gọi SQL*Loader để import dữ liệu từ CSV
sqlldr system/123456@localhost:1521/QLNHANVIEN control=sinhvien.ctl log=data_sinhvien.log bad=sinhvien.bad

pause
