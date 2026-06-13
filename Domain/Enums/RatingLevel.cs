namespace Domain.Enums;

public static class RatingLevel
{
    public const int Te = 1;
    public const int ChuaTot = 2;
    public const int BinhThuong = 3;
    public const int Tot = 4;
    public const int RatTot = 5;

    public static readonly int[] All = [Te, ChuaTot, BinhThuong, Tot, RatTot];
}
