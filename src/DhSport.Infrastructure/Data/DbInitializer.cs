using DhSport.Domain.Entities.Content;
using DhSport.Domain.Entities.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DhSport.Infrastructure.Data;

public class DbInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(ApplicationDbContext context, ILogger<DbInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Ensure database is created
            await _context.Database.MigrateAsync();

            // Check if data already exists
            if (await _context.Set<Role>().AnyAsync())
            {
                _logger.LogInformation("Database already seeded");
                return;
            }

            _logger.LogInformation("Starting database seeding...");

            // Seed Roles
            var adminRole = new Role
            {
                Id = Guid.NewGuid(),
                RoleNm = "관리자",
                RoleDesc = "시스템 관리자 권한",
                IsActive = true,
                CreateDttm = DateTime.UtcNow
            };

            var memberRole = new Role
            {
                Id = Guid.NewGuid(),
                RoleNm = "회원",
                RoleDesc = "일반 회원 권한",
                IsActive = true,
                CreateDttm = DateTime.UtcNow
            };

            await _context.Set<Role>().AddRangeAsync(adminRole, memberRole);

            // Seed Admin User
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                LogonId = "admin",
                Passwd = BCrypt.Net.BCrypt.HashPassword("admin123!"),
                UserNm = "시스템관리자",
                Email = "admin@dhsport.com",
                Tel = "010-0000-0000",
                IsActive = true,
                CreateDttm = DateTime.UtcNow
            };

            await _context.Set<User>().AddAsync(adminUser);

            // Assign admin role
            var adminUserRole = new UserRoleMap
            {
                Id = Guid.NewGuid(),
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                IsActive = true,
                CreateDttm = DateTime.UtcNow
            };

            await _context.Set<UserRoleMap>().AddAsync(adminUserRole);

            // Seed Board Types
            var generalBoardType = new BoardType
            {
                Id = Guid.NewGuid(),
                BoardTypeNm = "일반게시판",
                BoardTypeDesc = "일반적인 게시판",
                IsActive = true,
                CreateDttm = DateTime.UtcNow
            };

            var noticeBoardType = new BoardType
            {
                Id = Guid.NewGuid(),
                BoardTypeNm = "공지게시판",
                BoardTypeDesc = "공지사항 전용 게시판",
                IsActive = true,
                CreateDttm = DateTime.UtcNow
            };

            await _context.Set<BoardType>().AddRangeAsync(generalBoardType, noticeBoardType);

            // Seed Post Types
            var generalPostType = new PostType
            {
                Id = Guid.NewGuid(),
                PostTypeNm = "일반",
                PostTypeDesc = "일반 게시물",
                IsActive = true,
                CreateDttm = DateTime.UtcNow
            };

            var noticePostType = new PostType
            {
                Id = Guid.NewGuid(),
                PostTypeNm = "공지",
                PostTypeDesc = "공지사항",
                IsActive = true,
                CreateDttm = DateTime.UtcNow
            };

            var eventPostType = new PostType
            {
                Id = Guid.NewGuid(),
                PostTypeNm = "이벤트",
                PostTypeDesc = "이벤트 게시물",
                IsActive = true,
                CreateDttm = DateTime.UtcNow
            };

            await _context.Set<PostType>().AddRangeAsync(generalPostType, noticePostType, eventPostType);

            // Seed Boards
            var noticeBoard = new Board
            {
                Id = Guid.NewGuid(),
                BoardTypeId = noticeBoardType.Id,
                BoardNm = "공지사항",
                BoardDesc = "동협스포츠 공지사항",
                BoardSlug = "notice",
                DisplayOrder = 1,
                IsActive = true,
                CreateDttm = DateTime.UtcNow
            };

            var newsBoard = new Board
            {
                Id = Guid.NewGuid(),
                BoardTypeId = generalBoardType.Id,
                BoardNm = "새소식",
                BoardDesc = "동협스포츠 새소식 및 업데이트",
                BoardSlug = "news",
                DisplayOrder = 2,
                IsActive = true,
                CreateDttm = DateTime.UtcNow
            };

            var communityBoard = new Board
            {
                Id = Guid.NewGuid(),
                BoardTypeId = generalBoardType.Id,
                BoardNm = "커뮤니티",
                BoardDesc = "회원들의 자유 소통 공간",
                BoardSlug = "community",
                DisplayOrder = 3,
                IsActive = true,
                CreateDttm = DateTime.UtcNow
            };

            await _context.Set<Board>().AddRangeAsync(noticeBoard, newsBoard, communityBoard);

            // Seed Sample Posts
            var welcomePost = new Post
            {
                Id = Guid.NewGuid(),
                BoardId = noticeBoard.Id,
                PostTypeId = noticePostType.Id,
                AuthorId = adminUser.Id,
                Title = "동협스포츠에 오신 것을 환영합니다",
                PostContent = new { content = "동협스포츠 웹사이트를 방문해 주셔서 감사합니다. 다양한 스포츠 용품과 서비스를 만나보세요." },
                PostSlug = "welcome",
                IsNotice = true,
                IsPublished = true,
                PublishDttm = DateTime.UtcNow,
                ViewCnt = 0,
                LikeCnt = 0,
                CommentCnt = 0,
                CreateDttm = DateTime.UtcNow
            };

            var samplePost = new Post
            {
                Id = Guid.NewGuid(),
                BoardId = newsBoard.Id,
                PostTypeId = generalPostType.Id,
                AuthorId = adminUser.Id,
                Title = "2025년 신제품 소식",
                PostContent = new { content = "새해를 맞아 다양한 신제품을 준비했습니다. 많은 관심 부탁드립니다." },
                PostSlug = "new-products-2025",
                IsPublished = true,
                PublishDttm = DateTime.UtcNow,
                ViewCnt = 0,
                LikeCnt = 0,
                CommentCnt = 0,
                CreateDttm = DateTime.UtcNow
            };

            await _context.Set<Post>().AddRangeAsync(welcomePost, samplePost);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
