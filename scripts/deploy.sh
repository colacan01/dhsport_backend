#!/bin/bash
#
# DhSport Backend 배포 스크립트
# Git을 이용한 자동 배포, 빌드, 서비스 재시작
#

set -e  # 에러 발생 시 스크립트 중단

# 색상 정의
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 설정 변수
APP_DIR="/opt/dhsport/app"
PUBLISH_DIR="/opt/dhsport/app/publish"
BACKUP_DIR="/opt/dhsport/backups/$(date +%Y%m%d_%H%M%S)"
REPO_URL="https://github.com/your-org/dhsport_backend.git"  # 실제 저장소 URL로 변경
BRANCH="main"
SERVICE_NAME="dhsport"
PROJECT_FILE="src/DhSport.API/DhSport.API.csproj"

# 로그 함수
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# 에러 처리
error_exit() {
    log_error "$1"
    exit 1
}

# 사용자 확인
if [ "$EUID" -eq 0 ]; then
    log_error "이 스크립트는 root 권한으로 실행하지 마세요."
    exit 1
fi

# 시작
echo "========================================"
echo "  DhSport Backend 배포"
echo "========================================"
echo ""

# 1. 디렉토리 확인
log_info "애플리케이션 디렉토리 확인: $APP_DIR"
if [ ! -d "$APP_DIR" ]; then
    error_exit "애플리케이션 디렉토리가 존재하지 않습니다: $APP_DIR"
fi

cd $APP_DIR

# 2. Git 상태 확인
log_info "Git 저장소 상태 확인"
if [ ! -d ".git" ]; then
    error_exit "Git 저장소가 초기화되지 않았습니다."
fi

# 3. 현재 브랜치 및 커밋 정보
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)
CURRENT_COMMIT=$(git rev-parse --short HEAD)
log_info "현재 브랜치: $CURRENT_BRANCH"
log_info "현재 커밋: $CURRENT_COMMIT"

# 4. 변경사항 확인
if [ -n "$(git status --porcelain)" ]; then
    log_warn "작업 디렉토리에 커밋되지 않은 변경사항이 있습니다."
    read -p "계속 진행하시겠습니까? (y/n) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        log_info "배포를 취소했습니다."
        exit 0
    fi
fi

# 5. 최신 코드 가져오기
log_info "최신 코드를 가져옵니다 (Git pull)..."
git fetch origin
git reset --hard origin/$BRANCH
git pull origin $BRANCH

NEW_COMMIT=$(git rev-parse --short HEAD)
if [ "$CURRENT_COMMIT" = "$NEW_COMMIT" ]; then
    log_info "변경사항이 없습니다. 현재 버전: $CURRENT_COMMIT"
else
    log_info "업데이트됨: $CURRENT_COMMIT -> $NEW_COMMIT"
fi

# 6. 백업 생성
log_info "현재 버전 백업 중..."
mkdir -p $BACKUP_DIR

# DLL 파일만 백업 (전체 파일은 용량이 클 수 있음)
if [ -f "$APP_DIR/DhSport.API.dll" ]; then
    cp $APP_DIR/*.dll $BACKUP_DIR/ 2>/dev/null || true
    cp $APP_DIR/*.json $BACKUP_DIR/ 2>/dev/null || true
    cp $APP_DIR/*.deps.json $BACKUP_DIR/ 2>/dev/null || true
    log_info "백업 완료: $BACKUP_DIR"
else
    log_warn "기존 DLL 파일이 없습니다. 첫 배포로 간주합니다."
fi

# 7. 의존성 복원
log_info "NuGet 패키지 복원 중..."
dotnet restore || error_exit "패키지 복원 실패"

# 8. 빌드 및 퍼블리시
log_info "릴리스 빌드 및 퍼블리시 중..."
dotnet publish $PROJECT_FILE \
    -c Release \
    -o $PUBLISH_DIR \
    --self-contained false \
    --no-restore \
    || error_exit "빌드 실패"

# 9. appsettings.Production.json 보존
log_info "프로덕션 설정 파일 확인..."
if [ -f "$APP_DIR/appsettings.Production.json" ]; then
    cp $APP_DIR/appsettings.Production.json $PUBLISH_DIR/
    log_info "프로덕션 설정 파일 복사 완료"
else
    log_warn "appsettings.Production.json이 없습니다. 생성해야 합니다."
fi

# 10. 새 파일 배포
log_info "새 버전 배포 중..."
cp -r $PUBLISH_DIR/* $APP_DIR/

# 11. 권한 설정
chmod 600 $APP_DIR/appsettings*.json 2>/dev/null || true

# 12. 데이터베이스 마이그레이션 (선택사항)
log_info "데이터베이스 마이그레이션을 수행하시겠습니까? (y/n)"
read -p "> " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    log_info "데이터베이스 마이그레이션 실행 중..."
    dotnet ef database update \
        --project src/DhSport.Infrastructure/DhSport.Infrastructure.csproj \
        --startup-project src/DhSport.API/DhSport.API.csproj \
        || log_warn "마이그레이션 실패 (계속 진행)"
else
    log_info "마이그레이션을 건너뜁니다."
fi

# 13. 서비스 재시작
log_info "서비스 재시작 중..."
sudo systemctl restart $SERVICE_NAME || error_exit "서비스 재시작 실패"

# 14. 서비스 상태 확인
log_info "서비스 상태 확인 중..."
sleep 3
if sudo systemctl is-active --quiet $SERVICE_NAME; then
    log_info "서비스가 정상적으로 실행 중입니다."
    sudo systemctl status $SERVICE_NAME --no-pager -l
else
    log_error "서비스 시작 실패!"
    sudo journalctl -u $SERVICE_NAME -n 50 --no-pager
    exit 1
fi

# 15. 완료
echo ""
echo "========================================"
log_info "배포 완료!"
echo "========================================"
echo ""
log_info "배포 정보:"
echo "  - 이전 커밋: $CURRENT_COMMIT"
echo "  - 현재 커밋: $NEW_COMMIT"
echo "  - 백업 위치: $BACKUP_DIR"
echo "  - 서비스 상태: $(sudo systemctl is-active $SERVICE_NAME)"
echo ""

# 16. 이전 백업 정리 (30일 이상 된 백업 삭제)
log_info "30일 이상 된 백업 정리 중..."
find /opt/dhsport/backups -type d -mtime +30 -exec rm -rf {} + 2>/dev/null || true
log_info "백업 정리 완료"

echo ""
log_info "로그 확인: journalctl -u $SERVICE_NAME -f"
log_info "백업 복원: cp -r $BACKUP_DIR/* $APP_DIR/"
