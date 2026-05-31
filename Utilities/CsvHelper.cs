using System.Globalization;
using System.Text;

namespace Pivot.Utilities;

// RFC 4180'a yakın basit CSV writer/parser. Kütüphane bağımlılığı yok.
// Tüm sayı/decimal/tarih dönüşümleri InvariantCulture (period decimal separator).
public static class CsvHelper
{
    public static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public static string Escape(object? value)
    {
        if (value == null) return string.Empty;
        var s = value switch
        {
            bool b => b ? "true" : "false",
            decimal d => d.ToString("0.####", Culture),
            double d => d.ToString("0.####", Culture),
            float f => f.ToString("0.####", Culture),
            DateOnly date => date.ToString("yyyy-MM-dd"),
            DateTime dt => dt.ToString("yyyy-MM-ddTHH:mm:ss", Culture),
            Enum e => e.ToString(),
            _ => value.ToString() ?? string.Empty
        };
        // Eğer içerik ; , " veya satır sonu içeriyorsa quote'a sar; içerdeki " ikiye katla.
        if (s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
        {
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
        return s;
    }

    public static void WriteRow(StreamWriter w, params object?[] fields)
    {
        for (int i = 0; i < fields.Length; i++)
        {
            if (i > 0) w.Write(',');
            w.Write(Escape(fields[i]));
        }
        w.Write('\n');
    }

    // Tüm dosyayı satır satır, her satırı alan listesi olarak verir. Çoklu satırlı (quoted)
    // alanlar tek field olarak çözülür. Header satırı dahil edilir; çağıran filtreler.
    public static IEnumerable<string[]> ParseRows(TextReader reader)
    {
        var sb = new StringBuilder();
        var current = new List<string>();
        bool inQuotes = false;
        int ch;

        void FlushField()
        {
            current.Add(sb.ToString());
            sb.Clear();
        }
        bool FlushRow(out string[] row)
        {
            if (sb.Length > 0 || current.Count > 0) FlushField();
            row = current.ToArray();
            current.Clear();
            return row.Length > 0;
        }

        while ((ch = reader.Read()) != -1)
        {
            char c = (char)ch;
            if (inQuotes)
            {
                if (c == '"')
                {
                    // İçerdeki "" -> tek " literal; aksi halde quote bitti.
                    if (reader.Peek() == '"') { sb.Append('"'); reader.Read(); }
                    else inQuotes = false;
                }
                else
                {
                    sb.Append(c);
                }
            }
            else
            {
                if (c == '"' && sb.Length == 0) inQuotes = true;
                else if (c == ',') FlushField();
                else if (c == '\r') { /* skip; \n satır sonu */ }
                else if (c == '\n')
                {
                    if (FlushRow(out var r)) yield return r;
                }
                else sb.Append(c);
            }
        }
        if (FlushRow(out var last)) yield return last;
    }

    // Sık kullanılan parse helper'ları (nullable destekli).
    public static int ParseInt(string s) => int.Parse(s, Culture);
    public static int? ParseIntOrNull(string s) => string.IsNullOrEmpty(s) ? null : int.Parse(s, Culture);
    public static decimal? ParseDecimalOrNull(string s) => string.IsNullOrEmpty(s) ? null : decimal.Parse(s, Culture);
    public static bool ParseBool(string s) => s.Equals("true", StringComparison.OrdinalIgnoreCase) || s == "1";
    public static DateOnly? ParseDateOrNull(string s) => string.IsNullOrEmpty(s) ? null : DateOnly.Parse(s, Culture);
    public static T ParseEnum<T>(string s) where T : struct, Enum => Enum.Parse<T>(s, ignoreCase: true);
    public static string? NullIfEmpty(string s) => string.IsNullOrEmpty(s) ? null : s;
}
