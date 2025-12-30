#!/bin/bash
#
# DhSport Backend 백업 스크립트
# 애플리케이션 파일 및 데이터베이스 백업
#

set -e

# 색상 정의
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

# 설정
BACKUP_ROOT="/opt/dhsport/backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="$BACKUP_ROOT/$TIMESTAMP"
APP_DIR="/opt/dhsport/app"
UPLOADS_DIR="/opt/dhsport/uploads"
LOGS_DIR="/opt/dhsport/logs"

# 데이터베이스 설정
DB_HOST="172.30.1.51"
DB_PORT="5432"
DB_NAME="dhsports_prd"
DB_USER="dhsport"
# 주의: 비밀번호는 ~/.pgpass 파일에 설정하거나 환경 변수로 전달

# 보관 기간 (일)
RETENTION_DAYS=30

# 로그 함수
log_info() {
    echo -e "${GREEN}[INFO]${NC} $(date '+%Y-%m-%d %H:%M:%S') - $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $(date '+%Y-%m-%d %H:%M:%S') - $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $(date '+%Y-%m-%d %H:%M:%S') - $1"
}

# 시작
echo "========================================"
echo "  DhSport Backend 백업"
echo "========================================"
log_info "백업 시작: $TIMESTAMP"
echo ""

# 백업 디렉토리 생성
log_info "백업 디렉토리 생성: $BACKUP_DIR"
mkdir -p $BACKUP_DIR

# 1. 애플리케이션 파일 백업
log_info "애플리케이션 파일 백업 중..."
if [ -d "$APP_DIR" ]; then
    tar -czf $BACKUP_DIR/app.tar.gz -C $APP_DIR \
        --exclude='*.log' \
        --exclude='logs' \
        --exclude='uploads' \
        --exclude='.git' \
        . 2>/dev/null || log_warn "일부 파일 백업 실패"
    log_info "애플리케이션 백업 완료: app.tar.gz"
else
    log_warn "애플리케이션 디렉토리가 없습니다: $APP_DIR"
fi

# 2. 업로드 파일 백업
log_info "업로드 파일 백업 중..."
if [ -d "$UPLOADS_DIR" ] && [ "$(ls -A $UPLOADS_DIR 2>/dev/null)" ]; then
    tar -czf $BACKUP_DIR/uploads.tar.gz -C $UPLOADS_DIR . 2>/dev/null || log_warn "업로드 파일 백업 실패"
    log_info "업로드 파일 백업 완료: uploads.tar.gz"
else
    log_info "업로드 파일이 없습니다."
fi

# 3. 설정 파일 백업
log_info "설정 파일 백업 중..."
if [ -f "$APP_DIR/appsettings.Production.json" ]; then
    cp $APP_DIR/appsettings.Production.json $BACKUP_DIR/
    log_info "프로덕션 설정 파일 백업 완료"
else
    log_warn "프로덕션 설정 파일이 없습니다."
fi

# 4. 데이터베이스 백업
log_info "데이터베이스 백업 중..."

# PGPASSWORD 환경 변수 또는 ~/.pgpass 파일 사용
if command -v pg_dump &> /dev/null; then
    # 스키마 백업
    pg_dump -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME \
        --schema-only \
        > $BACKUP_DIR/schema.sql 2>/dev/null && \
        log_info "데이터베이스 스키마 백업 완료: schema.sql" || \
        log_warn "데이터베이스 스키마 백업 실패"

    # 데이터 백업
    pg_dump -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME \
        --data-only \
        > $BACKUP_DIR/data.sql 2>/dev/null && \
        log_info "데이터베이스 데이터 백업 완료: data.sql" || \
        log_warn "데이터베이스 데이터 백업 실패"

    # 전체 백업 (스키마 + 데이터)
    pg_dump -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME \
        > $BACKUP_DIR/full_backup.sql 2>/dev/null && \
        log_info "데이터베이스 전체 백업 완료: full_backup.sql" || \
        log_warn "데이터베이스 전체 백업 실패"

    # 압축
    gzip $BACKUP_DIR/*.sql 2>/dev/null && log_info "SQL 파일 압축 완료" || true
else
    log_warn "pg_dump를 찾을 수 없습니다. 데이터베이스 백업을 건너뜁니다."
    log_warn "PostgreSQL 클라이언트를 설치하세요: sudo apt install postgresql-client"
fi

# 5. 백업 정보 파일 생성
log_info "백업 정보 파일 생성 중..."
cat > $BACKUP_DIR/backup_info.txt << EOF
======================================
DhSport Backend 백업 정보
======================================

백업 시간: $(date '+%Y-%m-%d %H:%M:%S')
백업 디렉토리: $BACKUP_DIR

애플리케이션 버전:
$(cd $APP_DIR && git rev-parse --short HEAD 2>/dev/null || echo "N/A")

백업 파일:
$(ls -lh $BACKUP_DIR)

시스템 정보:
OS: $(uname -a)
.NET: $(dotnet --version 2>/dev/null || echo "N/A")

디스크 사용량:
$(df -h $BACKUP_ROOT)
EOF

log_info "백업 정보 파일 생성 완료: backup_info.txt"

# 6. 백업 크기 확인
BACKUP_SIZE=$(du -sh $BACKUP_DIR | cut -f1)
log_info "백업 크기: $BACKUP_SIZE"

# 7. 오래된 백업 정리
log_info "오래된 백업 정리 중 (${RETENTION_DAYS}일 이상)..."
DELETED_COUNT=0
while IFS= read -r -d '' backup; do
    rm -rf "$backup"
    ((DELETED_COUNT++))
done < <(find $BACKUP_ROOT -maxdepth 1 -type d -mtime +$RETENTION_DAYS -print0 2>/dev/null)

if [ $DELETED_COUNT -gt 0 ]; then
    log_info "삭제된 백업: $DELETED_COUNT개"
else
    log_info "삭제할 백업이 없습니다."
fi

# 8. 백업 목록 표시
log_info "최근 백업 목록 (최대 5개):"
ls -lt $BACKUP_ROOT | grep ^d | head -5 | awk '{print "  - " $9 " (" $6 " " $7 " " $8 ")"}'

# 완료
echo ""
echo "========================================"
log_info "백업 완료!"
echo "========================================"
echo ""
echo "백업 위치: $BACKUP_DIR"
echo "백업 크기: $BACKUP_SIZE"
echo ""

# 복원 방법 안내
echo "복원 방법:"
echo "  애플리케이션: tar -xzf $BACKUP_DIR/app.tar.gz -C $APP_DIR"
echo "  업로드 파일: tar -xzf $BACKUP_DIR/uploads.tar.gz -C $UPLOADS_DIR"
echo "  데이터베이스: gunzip < $BACKUP_DIR/full_backup.sql.gz | psql -h $DB_HOST -U $DB_USER -d $DB_NAME"
echo ""

exit 0
