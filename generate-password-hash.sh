#!/bin/bash

# BCrypt 비밀번호 해시 생성 스크립트
# 사용법: ./generate-password-hash.sh [password]

PASSWORD="${1:-password123}"

# Python으로 BCrypt 해시 생성
python3 -c "
import bcrypt
password = '$PASSWORD'.encode('utf-8')
salt = bcrypt.gensalt(rounds=11)
hashed = bcrypt.hashpw(password, salt)
print('Password:', '$PASSWORD')
print('BCrypt Hash:', hashed.decode('utf-8'))
"
