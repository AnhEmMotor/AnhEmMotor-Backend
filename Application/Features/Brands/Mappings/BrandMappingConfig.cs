using Application.ApiContracts.Brand.Responses;
using Application.Features.Brands.Commands.CreateBrand;
using Application.Features.Brands.Commands.UpdateBrand;
using Mapster;
using System.Globalization;
using System.Text.Json;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.Brands.Mappings;

public class BrandMappingConfig : IRegister
{
    private static string GetCurrentLanguage()
    {
        return CultureInfo.CurrentCulture.Name.StartsWith("vi", StringComparison.OrdinalIgnoreCase) ? "vi" : "en";
    }

    private static string? ResolveLocalizedText(string? json, string lang)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (dict == null)
                return null;
            dict.TryGetValue(lang, out var value);
            if (string.IsNullOrEmpty(value))
                dict.TryGetValue("vi", out value);
            return value;
        } catch
        {
            return null;
        }
    }

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateBrandCommand, BrandEntity>();
        config.NewConfig<BrandEntity, BrandResponse>().AfterMapping((src, dest) => ApplyLocalization(src, dest));
        config.NewConfig<UpdateBrandCommand, BrandEntity>().IgnoreNullValues(true);
    }

    private static void ApplyLocalization(BrandEntity src, BrandResponse dest)
    {
        if (dest == null || src == null)
            return;
        var lang = GetCurrentLanguage();
        dest.Name = ResolveLocalizedText(src.NameJson, lang) ?? src.Name;
        dest.Description = ResolveLocalizedText(src.DescriptionJson, lang) ?? src.Description;
    }
}
