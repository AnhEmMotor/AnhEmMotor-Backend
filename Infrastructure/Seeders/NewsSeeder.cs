using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class NewsSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var predefinedNews = new List<News>
        {
            Create(
            "Kinh nghiệm chọn xe máy phù hợp nhu cầu đi lại hằng ngày",
            "kinh-nghiem-chon-xe-may-phu-hop-nhu-cau-di-lai",
            1,
            "Những tiêu chí thực tế về quãng đường, tư thế lái, mức tiêu hao nhiên liệu và chi phí sử dụng giúp bạn chọn xe dễ dàng hơn.",
            now.AddDays(-2)),
            Create(
            "5 hạng mục cần kiểm tra trước một chuyến đi xa",
            "5-hang-muc-can-kiem-tra-truoc-chuyen-di-xa",
            2,
            "Lốp, phanh, dầu máy, hệ thống điện và giấy tờ là năm nhóm cần được kiểm tra để chuyến đi an toàn và chủ động.",
            now.AddDays(-5)),
            Create(
            "So sánh xe tay ga và xe số cho người mới đi làm",
            "so-sanh-xe-tay-ga-va-xe-so-cho-nguoi-moi-di-lam",
            4,
            "So sánh chi phí mua xe, mức tiêu hao, khả năng chứa đồ và trải nghiệm vận hành trong điều kiện đô thị.",
            now.AddDays(-8)),
            Create(
            "Lịch bảo dưỡng xe máy theo từng mốc kilomet",
            "lich-bao-duong-xe-may-theo-tung-moc-kilomet",
            2,
            "Theo dõi các mốc thay dầu, kiểm tra phanh, lốp và truyền động để xe luôn vận hành ổn định.",
            now.AddDays(-11)),
            Create(
            "Cách đọc thông số lốp xe máy trước khi thay mới",
            "cach-doc-thong-so-lop-xe-may-truoc-khi-thay-moi",
            2,
            "Giải thích dễ hiểu về kích thước, tải trọng, tốc độ và thời gian sản xuất được in trên thành lốp.",
            now.AddDays(-14)),
            Create(
            "Những giấy tờ cần chuẩn bị khi mua xe trả góp",
            "nhung-giay-to-can-chuan-bi-khi-mua-xe-tra-gop",
            1,
            "Danh sách giấy tờ và các bước cơ bản giúp khách hàng rút ngắn thời gian hoàn thiện hồ sơ mua xe trả góp.",
            now.AddDays(-17)),
            Create(
            "Showroom Anh Em Motor cập nhật khu vực tiếp nhận dịch vụ",
            "showroom-anh-em-motor-cap-nhat-khu-vuc-tiep-nhan-dich-vu",
            3,
            "Khu vực tiếp nhận được sắp xếp lại để khách hàng dễ theo dõi tiến độ kiểm tra, bảo dưỡng và bàn giao xe.",
            now.AddDays(-20)),
            Create(
            "Mẹo giữ lớp sơn xe bền màu trong mùa nắng nóng",
            "meo-giu-lop-son-xe-ben-mau-trong-mua-nang-nong",
            2,
            "Các thói quen rửa xe, che phủ và lựa chọn vị trí đỗ giúp hạn chế bạc màu và xuống cấp bề mặt sơn.",
            now.AddDays(-23)),
            Create(
            "Chọn dung tích động cơ nào khi thường xuyên đi trong phố",
            "chon-dung-tich-dong-co-nao-khi-thuong-xuyen-di-trong-pho",
            1,
            "Phân tích nhu cầu tăng tốc, mức tiêu hao và khả năng xoay trở để chọn dung tích động cơ phù hợp.",
            now.AddDays(-26)),
            Create(
            "Phanh ABS và CBS khác nhau như thế nào",
            "phanh-abs-va-cbs-khac-nhau-nhu-the-nao",
            4,
            "Tìm hiểu nguyên lý cơ bản, tình huống phát huy hiệu quả và lưu ý sử dụng của hai hệ thống phanh phổ biến.",
            now.AddDays(-29)),
            Create(
            "Hướng dẫn chăm sóc ắc quy xe máy đúng cách",
            "huong-dan-cham-soc-ac-quy-xe-may-dung-cach",
            2,
            "Nhận biết dấu hiệu ắc quy yếu và những cách sử dụng giúp hệ thống điện duy trì tuổi thọ tốt hơn.",
            now.AddDays(-32)),
            Create(
            "Checklist nhận xe mới tại showroom",
            "checklist-nhan-xe-moi-tai-showroom",
            1,
            "Kiểm tra ngoại hình, phụ kiện, giấy tờ, số khung số máy và hướng dẫn vận hành trước khi hoàn tất bàn giao.",
            now.AddDays(-35),
            isPublished: false)
        };
        var slugs = predefinedNews.Select(item => item.Slug).ToList();
        var existingSlugs = await context.News
            .Where(item => slugs.Contains(item.Slug))
            .Select(item => item.Slug)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var missingNews = predefinedNews
            .Where(item => !existingSlugs.Contains(item.Slug))
            .ToList();
        if (missingNews.Count == 0)
        {
            return;
        }
        context.News.AddRange(missingNews);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static News Create(
        string title,
        string slug,
        int categoryId,
        string description,
        DateTimeOffset createdAt,
        bool isPublished = true)
    {
        return new News
        {
            Title = title,
            Slug = slug,
            CategoryId = categoryId,
            AuthorName = "Ban biên tập Anh Em Motor",
            MetaTitle = title.Length <= 100 ? title : title[..100],
            MetaDescription = description,
            Content = $"<p>{description}</p>",
            IsPublished = isPublished,
            PublishedDate = isPublished ? createdAt : null,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }
}
