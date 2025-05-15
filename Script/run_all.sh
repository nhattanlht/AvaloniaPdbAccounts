#!/bin/bash
# run_all.sh
export NLS_LANG=AMERICAN_AMERICA.AL32UTF8

# 1. Chạy file SQL
sqlplus system/123456@localhost:1521/QLNHANVIEN @database.sql

# 2. Load dữ liệu bằng SQL*Loader
sqlldr system/123456@localhost:1521/QLNHANVIEN control=sinhvien.ctl log=data_sinhvien.log bad=sinhvien.bad
