using System.Text;

using Microsoft.Windows.ApplicationModel.Resources;

namespace Aoe4OverlayWinUI3.Helpers;

/// <summary>
/// 把 Aoe4World 返回的英文标识（地图、文明、对局类型、对局结果）翻译成当前语言的显示文本。
/// 翻译键统一为 “GamesList_类别_标识”，例如 dry_river -> GamesList_Map_DryRiver；
/// 资源文件中没有对应键时回退为格式化后的原始文本，保证新增内容仍有可读的显示文本。
/// </summary>
public static class GameContentLocalizer
{
    private static readonly ResourceLoader _resourceLoader = new();

    // 同一批对战数据里标识会重复出现，缓存查询结果，避免每一行都重新查资源
    private static readonly Dictionary<string, string> _cache = [];

    // 胜负结果：win / loss
    public static string LocalizeResult(string? result) => Localize("GamesList_Result_", result, value => value.ToUpperInvariant());

    // 地图：dry_river / Dry River
    public static string LocalizeMap(string? map) => Localize("GamesList_Map_", map, FormatIdentifier);

    // 文明：english / holy_roman_empire
    public static string LocalizeCivilization(string? civilization) => Localize("GamesList_Civ_", civilization, FormatIdentifier);

    // 对局类型：rm_1v1 / rm_team
    public static string LocalizeKind(string? kind) => Localize("GamesList_Kind_", kind, FormatIdentifier);

    private static string Localize(string keyPrefix, string? rawValue, Func<string, string> fallback)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return string.Empty;
        }

        var key = keyPrefix + ToResourceKeySuffix(rawValue);
        if (!_cache.TryGetValue(key, out var localized))
        {
            localized = TryGetString(key) ?? fallback(rawValue.Trim());
            _cache[key] = localized;
        }

        return localized;
    }

    private static string? TryGetString(string key)
    {
        try
        {
            var localized = _resourceLoader.GetString(key);
            return string.IsNullOrEmpty(localized) ? null : localized;
        }
        catch
        {
            // 资源文件中没有这个键时 ResourceLoader 会抛异常，按“无翻译”处理
            return null;
        }
    }

    // "holy_roman_empire" -> "HolyRomanEmpire"
    private static string ToResourceKeySuffix(string rawValue)
    {
        var builder = new StringBuilder();
        foreach (var part in SplitIdentifier(rawValue))
        {
            builder.Append(Capitalize(part));
        }
        return builder.ToString();
    }

    // "holy_roman_empire" -> "Holy Roman Empire"
    private static string FormatIdentifier(string rawValue) => string.Join(' ', SplitIdentifier(rawValue).Select(Capitalize));

    // 统一按小写切分，兼容 API 返回的 snake_case（dry_river）与已格式化的文本（Dry River）
    private static string[] SplitIdentifier(string rawValue)
        => rawValue.ToLowerInvariant().Split([' ', '_', '-', '\''], StringSplitOptions.RemoveEmptyEntries);

    private static string Capitalize(string part) => char.ToUpperInvariant(part[0]) + part[1..];
}
