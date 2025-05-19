#!/bin/bash
# run_all.sh
export NLS_LANG=AMERICAN_AMERICA.AL32UTF8

# 1. Chạy file SQL
sqlplus AdminPdb/123@localhost:1521/PDB @database.sql

# 2. Load dữ liệu bằng SQL*Loader
# sqlldr AdminPdb/123@localhost:1521/PDB control=sinhvien.ctl log=data_sinhvien.log bad=sinhvien.bad
