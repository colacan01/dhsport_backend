# DhSport Backend 서버 배포 가이드

이 문서는 DhSport ASP.NET Core 9.0 백엔드 애플리케이션을 Linux 서버에 배포하는 방법을 설명합니다.

## 목차

1. [서버 요구사항](#1-서버-요구사항)
2. [사전 준비](#2-사전-준비)
3. [서버 초기 설정](#3-서버-초기-설정)
4. [애플리케이션 배포](#4-애플리케이션-배포)
5. [Nginx 리버스 프록시 설정](#5-nginx-리버스-프록시-설정)
6. [SSL/TLS 인증서 설정](#6-ssltls-인증서-설정)
7. [배포 스크립트 사용법](#7-배포-스크립트-사용법)
8. [운영 및 모니터링](#8-운영-및-모니터링)
9. [트러블슈팅](#9-트러블슈팅)
10. [보안 체크리스트](#10-보안-체크리스트)

---

## 1. 서버 요구사항

### 1.1 하드웨어 요구사항
- **CPU**: 2코어 이상 권장
- **RAM**: 최소 2GB, 4GB 이상 권장
- **디스크**: 10GB 이상 (로그 및 업로드 파일 고려)

### 1.2 소프트웨어 요구사항
- **운영체제**: Ubuntu 20.04+ 또는 CentOS 7+
- **.NET Runtime**: .NET 9.0 ASP.NET Core Runtime
- **웹 서버**: Nginx (최신 안정 버전)
- **버전 관리**: Git
- **데이터베이스**: PostgreSQL 15+ (172.30.1.51:5432에 이미 구성됨)
- **캐시**: Redis (172.30.1.51:6379에 이미 구성됨)

### 1.3 네트워크 요구사항
- PostgreSQL 서버(172.30.1.51:5432) 접근 가능
- Redis 서버(172.30.1.51:6379) 접근 가능
- 인터넷 연결 (패키지 다운로드 및 Git 저장소 접근)
- 포트 80, 443 외부 접근 가능 (웹 서비스용)
- 포트 5000 로컬 접근 (애플리케이션 내부 포트)

---

## 2. 사전 준비

### 2.1 Git 저장소 접근 권한
- SSH 키 또는 HTTPS 인증 설정
- 저장소 URL 확인

### 2.2 데이터베이스 준비
```bash
# PostgreSQL 접속 확인
psql -h 172.30.1.51 -U dhsport -d postgres

# 프로덕션 데이터베이스 생성
CREATE DATABASE dhsports_prd;
GRANT ALL PRIVILEGES ON DATABASE dhsports_prd TO dhsport;
```

### 2.3 Redis 접속 확인
```bash
# Redis 연결 테스트
redis-cli -h 172.30.1.51 ping
# 응답: PONG
```

### 2.4 SSL 인증서 준비
- Let's Encrypt 사용 또는
- 기존 인증서 파일 (.crt, .key) 준비

### 2.5 보안 정보 준비
- 데이터베이스 비밀번호
- JWT Secret (최소 32자 이상 랜덤 문자열)
- 도메인 이름 (예: api.dhsport.com)

---

## 3. 서버 초기 설정

### 3.1 배포 사용자 생성

```bash
# dhsport 사용자 생성
sudo useradd -m -s /bin/bash dhsport
sudo usermod -aG sudo dhsport

# 사용자 전환
sudo su - dhsport
```

### 3.2 .NET 9.0 Runtime 설치

#### Ubuntu 20.04+
```bash
# Microsoft 패키지 저장소 등록
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update

# ASP.NET Core Runtime 9.0 설치
sudo apt install -y aspnetcore-runtime-9.0

# 설치 확인
dotnet --list-runtimes
```

#### CentOS 7+
```bash
# Microsoft 패키지 저장소 등록
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm

# ASP.NET Core Runtime 9.0 설치
sudo dnf install -y aspnetcore-runtime-9.0

# 설치 확인
dotnet --list-runtimes
```

### 3.3 Nginx 설치

#### Ubuntu
```bash
sudo apt update
sudo apt install -y nginx
sudo systemctl enable nginx
sudo systemctl start nginx
```

#### CentOS
```bash
sudo dnf install -y nginx
sudo systemctl enable nginx
sudo systemctl start nginx
```

### 3.4 Git 설치

```bash
# Ubuntu
sudo apt install -y git

# CentOS
sudo dnf install -y git
```

### 3.5 방화벽 설정

#### Ubuntu (UFW)
```bash
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw allow ssh
sudo ufw enable
```

#### CentOS (firewalld)
```bash
sudo firewall-cmd --permanent --add-service=http
sudo firewall-cmd --permanent --add-service=https
sudo firewall-cmd --permanent --add-service=ssh
sudo firewall-cmd --reload
```

---

## 4. 애플리케이션 배포

### 4.1 디렉토리 구조 생성

```bash
# 기본 디렉토리 생성
sudo mkdir -p /opt/dhsport/{app,logs,uploads,backups,scripts}
sudo chown -R dhsport:dhsport /opt/dhsport
```

최종 디렉토리 구조:
```
/opt/dhsport/
├── app/                    # 애플리케이션 파일
│   ├── DhSport.API.dll
│   ├── appsettings.json
│   ├── appsettings.Production.json
│   └── ...
├── logs/                   # Serilog 로그 파일
├── uploads/                # 업로드 파일
├── backups/                # 백업 파일
└── scripts/                # 배포 스크립트
    ├── deploy.sh
    ├── backup.sh
    ├── dhsport.service
    └── nginx-dhsport.conf
```

### 4.2 Git 저장소 클론

```bash
cd /opt/dhsport
git clone https://github.com/your-org/dhsport_backend.git app
cd app
```

### 4.3 appsettings.Production.json 생성

```bash
cd /opt/dhsport/app/src/DhSport.API

# 프로덕션 설정 파일 생성
cat > appsettings.Production.json << 'EOF'
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=172.30.1.51;Port=5432;Database=dhsports_prd;Username=dhsport;Password=YOUR_DB_PASSWORD",
    "Redis": "172.30.1.51:6379"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "/opt/dhsport/logs/dhsport-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  },
  "Jwt": {
    "Secret": "YOUR_JWT_SECRET_MUST_BE_AT_LEAST_32_CHARACTERS_LONG",
    "Issuer": "DhSport.API",
    "Audience": "DhSport.Client",
    "ExpirationHours": 24
  },
  "AllowedHosts": "api.dhsport.com"
}
EOF

# 보안 설정 (읽기 전용)
chmod 600 appsettings.Production.json
```

**중요**: `YOUR_DB_PASSWORD`와 `YOUR_JWT_SECRET`을 실제 값으로 변경하세요.

**JWT Secret 생성 예시**:
```bash
# 안전한 랜덤 문자열 생성
openssl rand -base64 32
```

### 4.4 애플리케이션 빌드 및 퍼블리시

```bash
cd /opt/dhsport/app

# 의존성 복원
dotnet restore

# 릴리스 빌드 및 퍼블리시
dotnet publish src/DhSport.API/DhSport.API.csproj \
    -c Release \
    -o /opt/dhsport/app/publish \
    --self-contained false

# 퍼블리시된 파일을 app 디렉토리로 복사
cp -r publish/* /opt/dhsport/app/
```

### 4.5 데이터베이스 마이그레이션

```bash
cd /opt/dhsport/app

# 마이그레이션 적용
dotnet ef database update --project src/DhSport.Infrastructure/DhSport.Infrastructure.csproj

# 또는 자동 마이그레이션 (애플리케이션 시작 시 자동 실행됨)
```

### 4.6 테스트 데이터 로드 (선택사항)

```bash
# test-data.sql 실행
psql -h 172.30.1.51 -U dhsport -d dhsports_prd -f test-data.sql
```

### 4.7 systemd 서비스 등록

```bash
# 서비스 파일 생성
sudo nano /etc/systemd/system/dhsport.service
```

다음 내용을 입력:
```ini
[Unit]
Description=DhSport ASP.NET Core Web API
After=network.target

[Service]
Type=notify
User=dhsport
Group=dhsport
WorkingDirectory=/opt/dhsport/app
ExecStart=/usr/bin/dotnet /opt/dhsport/app/DhSport.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dhsport
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=ASPNETCORE_URLS=http://localhost:5000

# 로그 설정
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
```

서비스 활성화:
```bash
# 데몬 리로드
sudo systemctl daemon-reload

# 서비스 활성화 (부팅 시 자동 시작)
sudo systemctl enable dhsport

# 서비스 시작
sudo systemctl start dhsport

# 상태 확인
sudo systemctl status dhsport
```

### 4.8 애플리케이션 접속 테스트

```bash
# 로컬에서 API 테스트
curl http://localhost:5000/api

# 로그 확인
journalctl -u dhsport -f
```

---

## 5. Nginx 리버스 프록시 설정

### 5.1 Nginx 설정 파일 생성

```bash
sudo nano /etc/nginx/sites-available/dhsport
```

다음 내용을 입력:
```nginx
upstream dhsport_backend {
    server 127.0.0.1:5000;
    keepalive 32;
}

# HTTP to HTTPS 리다이렉트
server {
    listen 80;
    server_name api.dhsport.com;  # 실제 도메인으로 변경

    # Let's Encrypt 인증서 검증용
    location /.well-known/acme-challenge/ {
        root /var/www/html;
    }

    # 나머지는 HTTPS로 리다이렉트
    location / {
        return 301 https://$server_name$request_uri;
    }
}

# HTTPS 서버
server {
    listen 443 ssl http2;
    server_name api.dhsport.com;  # 실제 도메인으로 변경

    # SSL 인증서 설정 (Let's Encrypt 사용 시 자동 생성됨)
    ssl_certificate /etc/letsencrypt/live/api.dhsport.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.dhsport.com/privkey.pem;

    # SSL 설정
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384';
    ssl_prefer_server_ciphers off;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;

    # 보안 헤더
    add_header Strict-Transport-Security "max-age=63072000" always;
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;

    # 로그 파일
    access_log /var/log/nginx/dhsport_access.log;
    error_log /var/log/nginx/dhsport_error.log;

    # 업로드 파일 크기 제한
    client_max_body_size 10M;

    # API 프록시
    location / {
        proxy_pass http://dhsport_backend;
        proxy_http_version 1.1;

        # 헤더 설정
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-Host $server_name;

        # 타임아웃 설정
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;

        # 버퍼링 설정
        proxy_buffering off;
        proxy_cache_bypass $http_upgrade;
        proxy_redirect off;
    }

    # 정적 파일 직접 서빙 (업로드 파일)
    location /uploads/ {
        alias /opt/dhsport/uploads/;
        expires 30d;
        add_header Cache-Control "public, immutable";
    }

    # gzip 압축
    gzip on;
    gzip_vary on;
    gzip_min_length 1000;
    gzip_proxied any;
    gzip_types
        text/plain
        text/css
        text/xml
        text/javascript
        application/json
        application/javascript
        application/xml+rss
        application/rss+xml
        application/atom+xml
        image/svg+xml;
}
```

### 5.2 Nginx 설정 활성화

```bash
# 심볼릭 링크 생성
sudo ln -s /etc/nginx/sites-available/dhsport /etc/nginx/sites-enabled/

# 기본 사이트 비활성화 (선택사항)
sudo rm /etc/nginx/sites-enabled/default

# 설정 테스트
sudo nginx -t

# Nginx 재시작
sudo systemctl reload nginx
```

---

## 6. SSL/TLS 인증서 설정

### 6.1 Let's Encrypt 사용 (권장)

```bash
# Certbot 설치 (Ubuntu)
sudo apt install -y certbot python3-certbot-nginx

# Certbot 설치 (CentOS)
sudo dnf install -y certbot python3-certbot-nginx

# 인증서 발급
sudo certbot --nginx -d api.dhsport.com

# 프롬프트에서 이메일 입력 및 약관 동의

# 자동 갱신 테스트
sudo certbot renew --dry-run

# 자동 갱신은 systemd timer로 설정됨
sudo systemctl list-timers | grep certbot
```

### 6.2 자체 인증서 사용

```bash
# 인증서 디렉토리 생성
sudo mkdir -p /etc/ssl/dhsport

# 인증서 파일 복사
sudo cp your-certificate.crt /etc/ssl/dhsport/dhsport.crt
sudo cp your-private-key.key /etc/ssl/dhsport/dhsport.key

# 권한 설정
sudo chmod 644 /etc/ssl/dhsport/dhsport.crt
sudo chmod 600 /etc/ssl/dhsport/dhsport.key

# Nginx 설정 수정 (인증서 경로 변경)
sudo nano /etc/nginx/sites-available/dhsport
```

Nginx 설정에서 SSL 인증서 경로 수정:
```nginx
ssl_certificate /etc/ssl/dhsport/dhsport.crt;
ssl_certificate_key /etc/ssl/dhsport/dhsport.key;
```

---

## 7. 배포 스크립트 사용법

### 7.1 deploy.sh - 자동 배포 스크립트

`scripts/deploy.sh` 파일을 사용하여 간편하게 배포할 수 있습니다.

```bash
# 스크립트 복사
cp /opt/dhsport/app/scripts/deploy.sh /opt/dhsport/scripts/
chmod +x /opt/dhsport/scripts/deploy.sh

# 배포 실행
cd /opt/dhsport/scripts
./deploy.sh
```

스크립트는 다음 작업을 수행합니다:
1. Git Pull (최신 코드 가져오기)
2. 빌드 및 퍼블리시
3. 현재 버전 백업
4. 새 버전 배포
5. 데이터베이스 마이그레이션
6. 서비스 재시작

### 7.2 backup.sh - 백업 스크립트

```bash
# 스크립트 복사
cp /opt/dhsport/app/scripts/backup.sh /opt/dhsport/scripts/
chmod +x /opt/dhsport/scripts/backup.sh

# 백업 실행
cd /opt/dhsport/scripts
./backup.sh
```

### 7.3 Cron을 이용한 자동 백업 설정

```bash
# crontab 편집
crontab -e

# 매일 새벽 2시에 백업 실행
0 2 * * * /opt/dhsport/scripts/backup.sh >> /opt/dhsport/logs/backup.log 2>&1
```

---

## 8. 운영 및 모니터링

### 8.1 서비스 관리

```bash
# 상태 확인
sudo systemctl status dhsport

# 시작
sudo systemctl start dhsport

# 중지
sudo systemctl stop dhsport

# 재시작
sudo systemctl restart dhsport

# 로그 확인 (실시간)
journalctl -u dhsport -f

# 최근 100줄 로그 확인
journalctl -u dhsport -n 100

# 지난 1시간 로그 확인
journalctl -u dhsport --since "1 hour ago"
```

### 8.2 로그 확인

#### 애플리케이션 로그 (Serilog)
```bash
# 실시간 로그
tail -f /opt/dhsport/logs/dhsport-*.log

# 최근 로그 파일 확인
ls -lht /opt/dhsport/logs/ | head
```

#### Nginx 로그
```bash
# 액세스 로그
tail -f /var/log/nginx/dhsport_access.log

# 에러 로그
tail -f /var/log/nginx/dhsport_error.log

# 특정 HTTP 상태 코드 필터링 (예: 5xx 에러)
grep " 5[0-9][0-9] " /var/log/nginx/dhsport_access.log
```

#### Systemd 저널 로그
```bash
# dhsport 서비스 로그
journalctl -u dhsport --since today

# 우선순위 에러 이상만 표시
journalctl -u dhsport -p err
```

### 8.3 성능 모니터링

#### CPU 및 메모리 사용량
```bash
# 실시간 모니터링
top -p $(pgrep -f DhSport.API)

# 또는 htop (설치 필요)
htop -p $(pgrep -f DhSport.API)
```

#### 네트워크 연결
```bash
# 포트 5000 리스닝 확인
sudo netstat -tlnp | grep 5000

# 또는 ss 명령어
sudo ss -tlnp | grep 5000

# 활성 연결 수 확인
sudo netstat -an | grep :5000 | grep ESTABLISHED | wc -l
```

#### 디스크 사용량
```bash
# 전체 디스크 사용량
df -h

# 애플리케이션 디렉토리 사용량
du -sh /opt/dhsport/*

# 로그 파일 크기 확인
du -sh /opt/dhsport/logs/
```

### 8.4 데이터베이스 관리

#### 마이그레이션 확인
```bash
cd /opt/dhsport/app

# 적용된 마이그레이션 확인
dotnet ef migrations list --project src/DhSport.Infrastructure/DhSport.Infrastructure.csproj

# 마이그레이션 적용
dotnet ef database update --project src/DhSport.Infrastructure/DhSport.Infrastructure.csproj
```

#### 데이터베이스 연결 테스트
```bash
psql -h 172.30.1.51 -U dhsport -d dhsports_prd -c "\dt"
```

### 8.5 Redis 모니터링

```bash
# Redis 연결 테스트
redis-cli -h 172.30.1.51 ping

# Redis 정보 확인
redis-cli -h 172.30.1.51 info

# 메모리 사용량 확인
redis-cli -h 172.30.1.51 info memory
```

### 8.6 로그 로테이션

Serilog는 자동으로 일일 로그 파일을 생성하지만, Nginx 로그는 logrotate로 관리합니다.

```bash
# logrotate 설정 확인
cat /etc/logrotate.d/nginx

# 수동 로그 로테이션 테스트
sudo logrotate -f /etc/logrotate.d/nginx
```

---

## 9. 트러블슈팅

### 9.1 애플리케이션이 시작되지 않음

**증상**: `systemctl status dhsport`에서 실패 상태

**진단**:
```bash
# 상세 로그 확인
journalctl -u dhsport -n 100 --no-pager

# 포트 충돌 확인
sudo lsof -i :5000

# 파일 권한 확인
ls -la /opt/dhsport/app/
```

**해결책**:
1. 포트가 이미 사용 중인 경우: 다른 프로세스 종료 또는 포트 변경
2. 권한 문제: `sudo chown -R dhsport:dhsport /opt/dhsport`
3. 설정 파일 오류: `appsettings.Production.json` 문법 확인

### 9.2 데이터베이스 연결 실패

**증상**: 로그에 `Npgsql` 또는 연결 오류 메시지

**진단**:
```bash
# PostgreSQL 연결 테스트
psql -h 172.30.1.51 -U dhsport -d dhsports_prd

# 방화벽 확인
sudo ufw status
telnet 172.30.1.51 5432
```

**해결책**:
1. 연결 문자열 확인: `appsettings.Production.json`의 `ConnectionStrings:DefaultConnection`
2. 비밀번호 확인
3. 네트워크 접근 확인: 방화벽, 보안 그룹 설정
4. PostgreSQL pg_hba.conf 설정 확인 (서버 측)

### 9.3 Redis 연결 실패

**증상**: 로그에 Redis 연결 오류 (애플리케이션은 계속 동작)

**진단**:
```bash
# Redis 연결 테스트
redis-cli -h 172.30.1.51 ping

# Redis 서비스 상태 확인 (서버 측)
sudo systemctl status redis
```

**해결책**:
- Redis는 선택사항이므로 연결 실패 시에도 애플리케이션은 작동
- Redis 서버 시작 확인
- 방화벽 설정 확인

### 9.4 Nginx 502 Bad Gateway

**증상**: 웹 브라우저에서 502 에러

**진단**:
```bash
# 백엔드 서비스 확인
sudo systemctl status dhsport

# 백엔드 포트 확인
sudo netstat -tlnp | grep 5000

# Nginx 에러 로그 확인
sudo tail -f /var/log/nginx/dhsport_error.log

# Nginx 설정 테스트
sudo nginx -t
```

**해결책**:
1. 백엔드 서비스가 중지된 경우: `sudo systemctl start dhsport`
2. 포트 불일치: Nginx 설정과 애플리케이션 포트 확인
3. Nginx 설정 오류: `sudo nginx -t`로 문법 확인
4. SELinux 문제 (CentOS): `sudo setsebool -P httpd_can_network_connect 1`

### 9.5 JWT 인증 실패

**증상**: API 호출 시 401 Unauthorized 또는 토큰 관련 오류

**진단**:
```bash
# JWT Secret 확인
grep "Jwt:Secret" /opt/dhsport/app/src/DhSport.API/appsettings.Production.json

# 로그에서 JWT 관련 에러 확인
journalctl -u dhsport | grep -i jwt
```

**해결책**:
1. JWT Secret이 32자 이상인지 확인
2. Issuer, Audience 설정 확인
3. 클라이언트와 서버 시간 동기화 확인: `timedatectl`
4. 토큰 만료 시간 확인

### 9.6 파일 업로드 실패

**증상**: 파일 업로드 시 에러 또는 413 Entity Too Large

**진단**:
```bash
# 업로드 디렉토리 권한 확인
ls -la /opt/dhsport/uploads/

# Nginx 설정 확인
grep client_max_body_size /etc/nginx/sites-available/dhsport
```

**해결책**:
1. 디렉토리 권한: `sudo chown -R dhsport:dhsport /opt/dhsport/uploads`
2. Nginx 업로드 크기 제한: `client_max_body_size 10M;` 확인
3. 애플리케이션 크기 제한 확인: MediaLibsController의 `MaxFileSize`

### 9.7 마이그레이션 실패

**증상**: 데이터베이스 마이그레이션 중 오류

**진단**:
```bash
# 마이그레이션 상태 확인
cd /opt/dhsport/app
dotnet ef migrations list --project src/DhSport.Infrastructure/DhSport.Infrastructure.csproj

# 데이터베이스 연결 확인
psql -h 172.30.1.51 -U dhsport -d dhsports_prd -c "SELECT version();"
```

**해결책**:
1. 데이터베이스 권한 확인
2. 충돌하는 마이그레이션 제거
3. 수동 마이그레이션: SQL 스크립트 직접 실행

### 9.8 높은 메모리 사용량

**증상**: 메모리 사용량이 계속 증가

**진단**:
```bash
# 프로세스 메모리 확인
ps aux | grep DhSport.API

# 전체 메모리 상태
free -h
```

**해결책**:
1. 메모리 누수 확인: 애플리케이션 재시작 후 모니터링
2. Kestrel 설정 조정: `appsettings.json`에서 연결 제한 설정
3. 서버 리소스 증설 고려

---

## 10. 보안 체크리스트

배포 전 다음 항목들을 확인하세요:

### 10.1 설정 파일 보안
- [ ] `appsettings.Production.json`을 Git에서 제외 (.gitignore)
- [ ] JWT Secret을 안전하게 생성 (최소 32자 이상 랜덤 문자열)
- [ ] 데이터베이스 비밀번호 변경 및 강력한 비밀번호 사용
- [ ] 환경 변수 또는 Secret Manager 사용 고려

### 10.2 네트워크 보안
- [ ] 방화벽 설정 (포트 80, 443만 외부 공개)
- [ ] SSH 포트 변경 (선택사항)
- [ ] Fail2ban 설치 (무차별 대입 공격 방지)

### 10.3 애플리케이션 보안
- [ ] CORS 정책을 특정 도메인으로 제한
- [ ] SSL/TLS 인증서 설치 및 HTTPS 강제
- [ ] Swagger UI 프로덕션에서 비활성화 (자동 비활성화됨)
- [ ] Rate Limiting 설정 고려

### 10.4 운영 보안
- [ ] 정기적인 보안 업데이트 (`sudo apt update && sudo apt upgrade`)
- [ ] 로그 파일 로테이션 설정
- [ ] 백업 자동화 및 백업 파일 암호화
- [ ] 모니터링 및 알림 설정

### 10.5 데이터베이스 보안
- [ ] PostgreSQL 접근 제한 (IP 화이트리스트)
- [ ] 데이터베이스 사용자 권한 최소화
- [ ] 정기적인 데이터베이스 백업
- [ ] SSL 연결 사용 (선택사항)

### 10.6 파일 시스템 보안
- [ ] 민감한 파일 권한 설정 (600 for configs, 700 for scripts)
- [ ] 업로드 디렉토리 실행 권한 제거
- [ ] 로그 디렉토리 권한 제한

---

## 부록

### A. 유용한 명령어 모음

#### 서비스 관리
```bash
# 서비스 상태 확인
sudo systemctl status dhsport

# 서비스 재시작
sudo systemctl restart dhsport

# 로그 실시간 확인
journalctl -u dhsport -f
```

#### 로그 분석
```bash
# 에러 로그만 확인
journalctl -u dhsport -p err

# 지난 1시간 로그
journalctl -u dhsport --since "1 hour ago"

# 특정 날짜 로그
journalctl -u dhsport --since "2024-01-01" --until "2024-01-02"
```

#### 성능 확인
```bash
# CPU/메모리 사용량
top -p $(pgrep -f DhSport.API)

# 네트워크 연결 수
ss -s

# 디스크 I/O
iostat -x 1
```

### B. 긴급 복구 절차

애플리케이션에 문제가 발생한 경우:

1. **이전 버전으로 롤백**
```bash
# 백업 디렉토리 확인
ls -lht /opt/dhsport/backups/

# 최신 백업으로 복원
cd /opt/dhsport/backups
cp -r [backup_timestamp]/* /opt/dhsport/app/

# 서비스 재시작
sudo systemctl restart dhsport
```

2. **데이터베이스 복구**
```bash
# 백업에서 복원
psql -h 172.30.1.51 -U dhsport -d dhsports_prd < /opt/dhsport/backups/[timestamp]/database.sql
```

3. **로그 확인 및 분석**
```bash
# 최근 에러 로그
journalctl -u dhsport -p err --since "10 minutes ago"

# Nginx 에러 로그
tail -100 /var/log/nginx/dhsport_error.log
```

### C. 연락처 및 지원

- **프로젝트 저장소**: [GitHub Repository URL]
- **이슈 리포트**: [GitHub Issues URL]
- **문서**: [Documentation URL]

---

**마지막 업데이트**: 2024-12-30
**버전**: 1.0.0
