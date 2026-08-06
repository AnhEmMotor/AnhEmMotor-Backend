using Application.Features.Ai;
using FluentAssertions;

namespace UnitTests;

public class AiSearchRuleParserTests
{
    private static readonly List<string> Brands = ["Honda", "Yamaha", "Suzuki", "Piaggio"];
    private static readonly List<string> Categories = ["Xe máy", "Phụ tùng", "Phụ kiện"];
    private static readonly List<string> VehicleTypes = ["Xe ga", "Xe số", "Xe côn tay"];

    [Fact(DisplayName = "AISEARCH_01 - Khớp đúng brand + khoảng giá dạng 'duoi X trieu'")]
    public void TryParse_BrandAndMaxPrice_ReturnsResult()
    {
        var result = AiSearchRuleParser.TryParse("Honda dưới 40 triệu", Brands, Categories, VehicleTypes);
        result.Should().NotBeNull();
        result!.Brand.Should().Be("Honda");
        result.PriceMax.Should().Be(40_000_000);
    }

    [Fact(DisplayName = "AISEARCH_02 - Gõ sai brand vẫn khớp được nhờ fuzzy match")]
    public void TryParse_TypoBrand_StillMatchesViaFuzzy()
    {
        var result = AiSearchRuleParser.TryParse("Hnoda xe ga", Brands, Categories, VehicleTypes);
        result.Should().NotBeNull();
        result!.Brand.Should().Be("Honda");
        result.VehicleType.Should().Be("Xe ga");
    }

    [Fact(DisplayName = "AISEARCH_03 - Khớp khoảng giá 'tu X den Y trieu'")]
    public void TryParse_PriceRange_ReturnsMinAndMax()
    {
        var result = AiSearchRuleParser.TryParse("xe số từ 20 đến 30 triệu", Brands, Categories, VehicleTypes);
        result.Should().NotBeNull();
        result!.VehicleType.Should().Be("Xe số");
        result.PriceMin.Should().Be(20_000_000);
        result.PriceMax.Should().Be(30_000_000);
    }

    [Fact(DisplayName = "AISEARCH_04 - Từ khóa sản phẩm ngắn, không khớp gì vẫn trả về (không cần AI)")]
    public void TryParse_ShortPlainKeyword_ReturnsPassthrough()
    {
        var result = AiSearchRuleParser.TryParse("SH 150i", Brands, Categories, VehicleTypes);
        result.Should().NotBeNull();
        result!.Keyword.Should().Be("SH 150i");
        result.Brand.Should().BeEmpty();
    }

    [Fact(DisplayName = "AISEARCH_05 - Câu mô tả dài, không khớp entity nào -> fallback null cho AI xử lý")]
    public void TryParse_LongUnclearSentence_ReturnsNullForLlmFallback()
    {
        var result = AiSearchRuleParser.TryParse(
            "xe đi làm hằng ngày cần bền tiết kiệm xăng dễ đi trong phố đông đúc",
            Brands,
            Categories,
            VehicleTypes);
        result.Should().BeNull();
    }

    [Fact(DisplayName = "AISEARCH_06 - Khớp màu sắc trong câu")]
    public void TryParse_Color_AddsToColorsList()
    {
        var result = AiSearchRuleParser.TryParse("Yamaha màu đỏ", Brands, Categories, VehicleTypes);
        result.Should().NotBeNull();
        result!.Brand.Should().Be("Yamaha");
        result.Colors.Should().Contain("đỏ");
    }

    [Fact(DisplayName = "AISEARCH_07 - Khớp mốc giá tối thiểu 'tren X trieu'")]
    public void TryParse_MinPriceOnly_ReturnsPriceMin()
    {
        var result = AiSearchRuleParser.TryParse("Suzuki trên 25 triệu", Brands, Categories, VehicleTypes);
        result.Should().NotBeNull();
        result!.Brand.Should().Be("Suzuki");
        result.PriceMin.Should().Be(25_000_000);
    }

    [Fact(DisplayName = "AISEARCH_08 - Đơn vị 'k' (nghìn) được quy đổi đúng")]
    public void TryParse_ThousandUnit_ConvertsCorrectly()
    {
        var result = AiSearchRuleParser.TryParse("mũ bảo hiểm dưới 500k", Brands, Categories, VehicleTypes);
        result.Should().NotBeNull();
        result!.PriceMax.Should().Be(500_000);
    }

    [Fact(DisplayName = "AISEARCH_09 - Khớp nhiều màu trong cùng câu")]
    public void TryParse_MultipleColors_AddsAllToColorsList()
    {
        var result = AiSearchRuleParser.TryParse("Honda đỏ đen", Brands, Categories, VehicleTypes);
        result.Should().NotBeNull();
        result!.Brand.Should().Be("Honda");
        result.Colors.Should().BeEquivalentTo(["đỏ", "đen"]);
    }

    [Fact(DisplayName = "AISEARCH_10 - Đúng 3 từ thừa (biên MaxUnmatchedWords) vẫn xử lý được, không cần AI")]
    public void TryParse_ExactlyThreeUnmatchedWords_ReturnsPassthrough()
    {
        var result = AiSearchRuleParser.TryParse("áo mưa xịn", Brands, Categories, VehicleTypes);
        result.Should().NotBeNull();
        result!.Keyword.Should().Be("áo mưa xịn");
    }

    [Fact(DisplayName = "AISEARCH_11 - 4 từ thừa vượt biên -> fallback AI")]
    public void TryParse_FourUnmatchedWords_ReturnsNullForLlmFallback()
    {
        var result = AiSearchRuleParser.TryParse("áo mưa xịn đẹp", Brands, Categories, VehicleTypes);
        result.Should().BeNull();
    }

    [Fact(DisplayName = "AISEARCH_12 - Không phân biệt chữ hoa/thường khi khớp brand")]
    public void TryParse_UpperCaseInput_StillMatchesBrand()
    {
        var result = AiSearchRuleParser.TryParse("HONDA DƯỚI 30 TRIỆU", Brands, Categories, VehicleTypes);
        result.Should().NotBeNull();
        result!.Brand.Should().Be("Honda");
        result.PriceMax.Should().Be(30_000_000);
    }

    [Fact(DisplayName = "AISEARCH_13 - Màu đen vẫn nhận đúng khi không có ngữ cảnh 'từ...đến' (regression)")]
    public void TryParse_ColorDenWithoutRangeContext_StillMatchesAsColor()
    {
        var result = AiSearchRuleParser.TryParse("Honda màu đen", Brands, Categories, VehicleTypes);
        result.Should().NotBeNull();
        result!.Brand.Should().Be("Honda");
        result.Colors.Should().Contain("đen");
        result.PriceMin.Should().Be(0);
    }

    [Fact(DisplayName = "AISEARCH_14 - 'xe tay ga' (nói phổ biến) quy đổi đúng về VehicleType 'Xe ga' (regression)")]
    public void TryParse_XeTayGaColloquialism_MatchesVehicleTypeXeGa()
    {
        var result = AiSearchRuleParser.TryParse("xe tay ga nữ dưới 40 triệu", Brands, Categories, VehicleTypes);
        result.Should().NotBeNull();
        result!.VehicleType.Should().Be("Xe ga");
        result.Category.Should().BeEmpty();
        result.PriceMax.Should().Be(40_000_000);
        result.Keyword.Should().Be("nữ");
    }

    [Fact(
        DisplayName = "AISEARCH_15 - 'xe tay' (không có 'ga' theo sau) không bị nhận nhầm thành category 'Xe máy' (regression)")]
    public void TryParse_XeTayWithoutGa_DoesNotFalselyMatchCategory()
    {
        var result = AiSearchRuleParser.TryParse("xe tay đẹp", Brands, Categories, VehicleTypes);
        result.Should().NotBeNull();
        result!.Category.Should().BeEmpty();
        result.Keyword.Should().Be("xe tay đẹp");
    }
}
