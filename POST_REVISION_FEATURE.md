# Post Revision 기능 문서

## 개요

게시물의 버전 관리 기능으로, 게시물 수정 시 이전 내용을 자동으로 저장하고 필요 시 복원할 수 있습니다.

## 주요 기능

### 1. 자동 버전 저장
- 게시물 업데이트 시 이전 내용을 자동으로 `post_revision` 테이블에 저장
- JSON key-value 레벨에서 실제 변경이 발생한 경우에만 리비전 생성
- 최대 10개의 리비전 유지 (자동으로 오래된 리비전 삭제)

### 2. 버전 비교
- 두 리비전 간의 차이를 필드 단위로 비교
- 추가된 필드 (AddedFields)
- 수정된 필드 (ModifiedFields - from/to 값 포함)
- 삭제된 필드 (RemovedFields)

### 3. 버전 복원
- 작성자 또는 관리자가 이전 버전으로 되돌리기 가능
- 복원 전 현재 내용을 백업 리비전으로 저장
- 관리자가 다른 사용자의 게시물을 복원하면 작성자에게 알림 전송

### 4. 권한 관리
- 작성자: 자신의 게시물 리비전 조회/복원 가능
- 관리자: 모든 게시물의 리비전 조회/복원 가능

## 아키텍처

### Clean Architecture + DDD 패턴

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                             │
│  - PostsController: 리비전 관련 엔드포인트                     │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│  - Queries: GetPostRevisions, GetPostRevisionById, Diff     │
│  - Commands: UpdatePost (revision 생성), RestorePostRevision│
│  - DTOs: PostRevisionDto, PostRevisionDiffDto               │
│  - Helpers: JsonComparer (JSON 비교 로직)                   │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                             │
│  - Entities: Post, PostRevision                             │
│  - Navigation: Post.Revisions, Post.Author                  │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                         │
│  - Configurations: PostConfiguration, PostRevisionConfig    │
│  - Database: PostgreSQL + EF Core                           │
└─────────────────────────────────────────────────────────────┘
```

## API 엔드포인트

### 1. 리비전 목록 조회
```http
GET /api/posts/{id}/revisions
Authorization: Bearer {token}
```

**응답 예시:**
```json
[
  {
    "id": "uuid",
    "postId": "uuid",
    "postContent": "{...}",
    "revisionNote": "자동 저장",
    "revisionUserId": "uuid",
    "revisionUserNm": "홍길동",
    "createDttm": "2026-01-02T10:00:00Z"
  }
]
```

### 2. 특정 리비전 조회
```http
GET /api/posts/{id}/revisions/{revisionId}
Authorization: Bearer {token}
```

**응답 예시:**
```json
{
  "id": "uuid",
  "postId": "uuid",
  "postContent": "{\"title\":\"제목\",\"body\":\"내용\"}",
  "revisionNote": "자동 저장",
  "revisionUserId": "uuid",
  "revisionUserNm": "홍길동",
  "createDttm": "2026-01-02T10:00:00Z"
}
```

### 3. 리비전 간 차이 비교
```http
GET /api/posts/{id}/revisions/{fromRevisionId}/diff/{toRevisionId}
Authorization: Bearer {token}
```

**응답 예시:**
```json
{
  "fromRevisionId": "uuid",
  "toRevisionId": "uuid",
  "addedFields": {
    "newField": "새로운 값"
  },
  "modifiedFields": {
    "title": {
      "from": "이전 제목",
      "to": "새로운 제목"
    }
  },
  "removedFields": {
    "oldField": "삭제된 값"
  }
}
```

### 4. 리비전 복원
```http
POST /api/posts/{id}/revisions/{revisionId}/restore
Authorization: Bearer {token}
```

**응답 예시:**
```json
{
  "id": "uuid",
  "boardId": "uuid",
  "postTypeId": "uuid",
  "authorId": "uuid",
  "title": "복원된 제목",
  "postContent": {...},
  "isNotice": false,
  "isSecret": false,
  "viewCnt": 100,
  "likeCnt": 10,
  "commentCnt": 5,
  "createDttm": "2026-01-01T10:00:00Z",
  "updateDttm": "2026-01-02T10:00:00Z"
}
```

### 5. 게시물 수정 (리비전 자동 생성)
```http
PUT /api/posts/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "수정된 제목",
  "postContent": {...},
  "postTypeId": "uuid",
  "isNotice": false,
  "isSecret": false,
  "revisionNote": "사용자 입력 노트 (선택사항)"
}
```

**동작:**
- `postContent`가 실제로 변경된 경우에만 리비전 생성
- `revisionNote`가 없으면 "자동 저장"으로 저장
- 10개를 초과하는 리비전은 자동 삭제 (오래된 순)

## 데이터베이스 스키마

### post_revision 테이블
```sql
CREATE TABLE post_revision (
    id UUID PRIMARY KEY,
    post_id UUID NOT NULL REFERENCES post(id) ON DELETE CASCADE,
    post_content TEXT NOT NULL,  -- JSON string
    revision_note VARCHAR(500) NOT NULL,
    revision_user_id UUID NOT NULL,
    create_dttm TIMESTAMP NOT NULL,
    
    INDEX idx_post_revision_post_id (post_id),
    INDEX idx_post_revision_create_dttm (create_dttm)
);
```

## 핵심 구현 로직

### 1. JSON 비교 로직 (JsonComparer)

**위치:** `src/DhSport.Application/Common/Helpers/JsonComparer.cs`

```csharp
// 두 JSON 객체가 동일한지 비교 (재귀적)
public static bool AreEqual(object? obj1, object? obj2)

// 두 리비전 간 차이점 추출
public static PostRevisionDiffDto GetDifferences(
    Guid fromRevisionId, 
    Guid toRevisionId, 
    string fromContent, 
    string toContent)
```

**특징:**
- Object, Array, String, Number, Boolean, Null 타입 모두 지원
- 중첩된 객체 재귀적 비교
- 배열 요소 순서 및 내용 비교

### 2. UpdatePostCommandHandler 로직

**위치:** `src/DhSport.Application/Features/Posts/Commands/UpdatePost/UpdatePostCommandHandler.cs`

```csharp
// 1. 트랜잭션 시작
await using var transaction = await _unitOfWork.BeginTransactionAsync();

// 2. 원본 내용 저장
var originalContent = post.PostContent;

// 3. 게시물 업데이트
if (request.Dto.PostContent != null)
{
    // JSON 비교로 실제 변경 확인
    if (!JsonComparer.AreEqual(originalContent, request.Dto.PostContent))
    {
        contentChanged = true;
        post.PostContent = request.Dto.PostContent;
    }
}

// 4. 변경이 있을 경우에만 리비전 생성
if (contentChanged)
{
    var revision = new PostRevision
    {
        PostId = post.Id,
        PostContent = JsonSerializer.Serialize(originalContent),
        RevisionNote = request.Dto.RevisionNote ?? "자동 저장",
        RevisionUserId = request.UserId,
        CreateDttm = DateTime.UtcNow
    };
    
    await _unitOfWork.Repository<PostRevision>().AddAsync(revision);
    
    // 5. 10개 초과 시 오래된 리비전 삭제
    var revisionCount = await _unitOfWork.Repository<PostRevision>()
        .GetQueryable()
        .CountAsync(r => r.PostId == post.Id);
        
    if (revisionCount >= 10)
    {
        var revisionsToDelete = await _unitOfWork.Repository<PostRevision>()
            .GetQueryable()
            .Where(r => r.PostId == post.Id)
            .OrderBy(r => r.CreateDttm)
            .Take(revisionCount - 9)
            .ToListAsync();
            
        foreach (var oldRevision in revisionsToDelete)
        {
            await _unitOfWork.Repository<PostRevision>().DeleteAsync(oldRevision);
        }
    }
}

// 6. 트랜잭션 커밋
await _unitOfWork.SaveChangesAsync();
await transaction.CommitAsync();
```

### 3. RestorePostRevisionCommandHandler 로직

**위치:** `src/DhSport.Application/Features/Posts/Commands/RestorePostRevision/RestorePostRevisionCommandHandler.cs`

```csharp
// 1. 권한 검증
if (!request.IsAdmin && post.AuthorId != request.UserId)
    throw new UnauthorizedAccessException();

// 2. 현재 내용을 백업 리비전으로 저장
var currentContentRevision = new PostRevision
{
    PostId = post.Id,
    PostContent = JsonSerializer.Serialize(post.PostContent),
    RevisionNote = $"복원 전 백업 (Revision ID: {request.RevisionId})",
    RevisionUserId = request.UserId,
    CreateDttm = DateTime.UtcNow
};
await _unitOfWork.Repository<PostRevision>().AddAsync(currentContentRevision);

// 3. 리비전에서 내용 복원
var revisionContentObject = JsonSerializer.Deserialize<object>(revision.PostContent);
if (!JsonComparer.AreEqual(post.PostContent, revisionContentObject))
{
    post.PostContent = revisionContentObject;
    post.UpdateDttm = DateTime.UtcNow;
    post.UpdateUserId = request.UserId;
    await _unitOfWork.Repository<Post>().UpdateAsync(post);
}

// 4. 관리자가 다른 사용자 게시물 복원 시 알림
if (request.IsAdmin && post.AuthorId != request.UserId)
{
    var notification = new Notification
    {
        UserId = post.AuthorId,
        NotificationType = "POST_RESTORED",
        NotificationTitle = "게시물이 복원되었습니다",
        NotificationContent = $"{adminName}님이 회원님의 게시물 '{post.Title}'을 이전 버전으로 복원했습니다.",
        IsRead = false,
        CreateDttm = DateTime.UtcNow
    };
    await _unitOfWork.Repository<Notification>().AddAsync(notification);
}

// 5. 10개 초과 리비전 정리
// ... (UpdatePostCommandHandler와 동일)
```

## 사용 시나리오

### 시나리오 1: 게시물 수정 및 리비전 자동 생성

```bash
# 1. 게시물 작성
POST /api/posts
{
  "title": "첫 번째 게시물",
  "postContent": {"body": "초기 내용"}
}

# 2. 게시물 수정 (첫 번째 리비전 생성)
PUT /api/posts/{id}
{
  "title": "첫 번째 게시물",
  "postContent": {"body": "수정된 내용"}
}
# → 이전 내용 {"body": "초기 내용"}이 리비전으로 저장됨

# 3. 다시 수정 (두 번째 리비전 생성)
PUT /api/posts/{id}
{
  "title": "첫 번째 게시물",
  "postContent": {"body": "다시 수정된 내용"}
}
# → 이전 내용 {"body": "수정된 내용"}이 리비전으로 저장됨
```

### 시나리오 2: 리비전 조회 및 비교

```bash
# 1. 리비전 목록 조회
GET /api/posts/{id}/revisions
# → 시간 역순으로 정렬된 리비전 목록 반환

# 2. 두 리비전 비교
GET /api/posts/{id}/revisions/{revision1}/diff/{revision2}
# → 추가/수정/삭제된 필드 정보 반환
```

### 시나리오 3: 이전 버전으로 복원

```bash
# 1. 복원할 리비전 선택
GET /api/posts/{id}/revisions
# → 리비전 목록에서 복원할 revision_id 확인

# 2. 복원 실행
POST /api/posts/{id}/revisions/{revisionId}/restore
# → 현재 내용이 백업 리비전으로 저장되고, 선택한 리비전으로 복원
```

### 시나리오 4: 관리자가 다른 사용자 게시물 복원

```bash
# 관리자 토큰으로 요청
POST /api/posts/{id}/revisions/{revisionId}/restore
Authorization: Bearer {admin_token}

# 결과:
# 1. 게시물이 이전 버전으로 복원됨
# 2. 작성자에게 알림 전송:
#    "관리자님이 회원님의 게시물 'XXX'을 이전 버전으로 복원했습니다."
```

## 성능 최적화

### 1. Include 사용
```csharp
var revisions = await _unitOfWork.Repository<PostRevision>()
    .GetQueryable()
    .Include(r => r.Post)
    .ThenInclude(p => p.Author)
    .Where(r => r.PostId == request.PostId)
    .OrderByDescending(r => r.CreateDttm)
    .ToListAsync(cancellationToken);
```
- N+1 쿼리 문제 방지
- 한 번의 조인으로 필요한 데이터 모두 조회

### 2. 인덱스 활용
```sql
-- post_id로 빠른 조회
INDEX idx_post_revision_post_id (post_id)

-- 생성 시간 정렬 최적화
INDEX idx_post_revision_create_dttm (create_dttm)
```

### 3. 트랜잭션 사용
- 리비전 생성과 게시물 업데이트를 원자적으로 처리
- 실패 시 자동 롤백으로 데이터 일관성 보장

## 보안 고려사항

### 1. 권한 검증
- 모든 리비전 관련 API는 인증 필수 (`[Authorize]`)
- 작성자 또는 관리자만 접근 가능
```csharp
if (!request.IsAdmin && request.RequestUserId != post.AuthorId)
    return new List<PostRevisionDto>(); // 빈 리스트 반환
```

### 2. JSONB 타입 사용
- PostgreSQL의 JSONB 타입으로 PostContent 저장
- SQL Injection 방지
- JSON 유효성 자동 검증

### 3. 입력 검증
- RevisionNote 최대 길이 제한 (500자)
- PostContent JSON 형식 검증

## 제한사항

### 1. 리비전 개수 제한
- 게시물당 최대 10개의 리비전 유지
- 오래된 리비전 자동 삭제
- **이유:** 스토리지 비용 및 성능 고려

### 2. 파일 변경 추적 안 함
- PostRevision은 `post_content` (JSONB) 필드만 추적
- 첨부 파일(`post_file` 테이블)의 변경은 추적하지 않음

### 3. 리비전 삭제 불가
- 사용자가 직접 리비전을 삭제할 수 없음
- 자동 삭제 정책에 따라서만 삭제됨

## 향후 개선 사항

### 1. 리비전 보관 정책 개선
- [ ] 중요 리비전 영구 보관 기능
- [ ] 시간 기반 보관 정책 (예: 30일 이상된 리비전 삭제)
- [ ] 관리자가 보관 정책 설정 가능

### 2. 리비전 UI 개선
- [ ] 사이드바이사이드 비교 뷰
- [ ] 변경 사항 하이라이트
- [ ] 리비전 타임라인 시각화

### 3. 성능 최적화
- [ ] 리비전 데이터 압축
- [ ] 대용량 게시물에 대한 청크 단위 저장
- [ ] 리비전 조회 캐싱

### 4. 감사 기능 강화
- [ ] 누가 언제 어떤 리비전을 조회했는지 로깅
- [ ] 복원 이력 추적
- [ ] 리비전 비교 이력

## 문제 해결

### Q: 리비전이 생성되지 않습니다
**A:** 다음을 확인하세요:
1. `postContent` 필드가 실제로 변경되었는지 확인
2. JSON 형식이 올바른지 확인
3. 트랜잭션 로그에서 에러 메시지 확인

### Q: 10개 이상의 리비전이 있습니다
**A:** 버그가 아닙니다:
- 정확히는 10개까지 유지하는 정책
- 11번째 리비전 생성 시 가장 오래된 1개가 삭제됨
- 복원 시 생성되는 백업 리비전도 카운트에 포함

### Q: 복원 후 알림이 전송되지 않습니다
**A:** 다음을 확인하세요:
1. 관리자가 자신의 게시물을 복원한 경우 알림 미전송 (정상)
2. Notification 테이블에 레코드가 생성되었는지 확인
3. 알림 서비스가 제대로 동작하는지 확인

## 관련 파일

### Domain Layer
- `src/DhSport.Domain/Entities/Content/Post.cs`
- `src/DhSport.Domain/Entities/Content/PostRevision.cs`

### Application Layer
- `src/DhSport.Application/DTOs/Post/PostRevisionDto.cs`
- `src/DhSport.Application/DTOs/Post/PostRevisionDiffDto.cs`
- `src/DhSport.Application/DTOs/Post/UpdatePostDto.cs`
- `src/DhSport.Application/Common/Helpers/JsonComparer.cs`
- `src/DhSport.Application/Features/Posts/Queries/GetPostRevisions/`
- `src/DhSport.Application/Features/Posts/Queries/GetPostRevisionById/`
- `src/DhSport.Application/Features/Posts/Queries/GetPostRevisionDiff/`
- `src/DhSport.Application/Features/Posts/Commands/UpdatePost/UpdatePostCommandHandler.cs`
- `src/DhSport.Application/Features/Posts/Commands/RestorePostRevision/`
- `src/DhSport.Application/Mappings/PostProfile.cs`

### Infrastructure Layer
- `src/DhSport.Infrastructure/Data/Configurations/Content/PostConfiguration.cs`
- `src/DhSport.Infrastructure/Data/Configurations/Content/PostRevisionConfiguration.cs`
- `src/DhSport.Infrastructure/Migrations/YYYYMMDDHHMMSS_AddPostAuthorRelationship.cs`

### API Layer
- `src/DhSport.API/Controllers/PostsController.cs`

## 테스트 체크리스트

### 단위 테스트
- [ ] JsonComparer.AreEqual() - 다양한 JSON 타입 테스트
- [ ] JsonComparer.GetDifferences() - 추가/수정/삭제 케이스
- [ ] UpdatePostCommandHandler - 리비전 생성 조건
- [ ] RestorePostRevisionCommandHandler - 권한 검증

### 통합 테스트
- [ ] 게시물 수정 → 리비전 자동 생성
- [ ] 10개 초과 시 자동 삭제
- [ ] 리비전 목록 조회 (권한별)
- [ ] 리비전 비교 API
- [ ] 리비전 복원 (작성자/관리자)
- [ ] 관리자 복원 시 알림 전송

### 성능 테스트
- [ ] 대용량 JSON 처리 (10MB+)
- [ ] 동시 업데이트 처리
- [ ] 리비전 조회 성능 (Include 최적화)

## 버전 히스토리

### v1.0.0 (2026-01-02)
- ✅ 초기 구현 완료
- ✅ 자동 리비전 생성
- ✅ 리비전 조회/비교/복원 API
- ✅ JSON 레벨 비교 로직
- ✅ 10개 리비전 제한
- ✅ 관리자 복원 알림

---

**문서 작성일:** 2026년 1월 2일  
**마지막 업데이트:** 2026년 1월 2일  
**작성자:** DhSport Backend Team
