using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenToWork.Core.Services;

/// <summary>
/// Markdown minimo de las Noticias -> HTML seguro. Se escapa TODO el HTML antes de dar formato, asi que nada
/// escrito a mano (ni &lt;script&gt;) llega a ejecutarse. Soporta: parrafos (linea en blanco), "## " y "### "
/// subtitulos, listas "- " / "1. ", **negrita**, *cursiva* y [texto](https://...). Lo demas sale como texto.
/// </summary>
public static class NewsMarkdown
{
    private static readonly Regex Bold = new(@"\*\*(.+?)\*\*", RegexOptions.Compiled);
    private static readonly Regex Italic = new(@"(?<!\*)\*(?!\s)(.+?)(?<!\s)\*(?!\*)", RegexOptions.Compiled);
    // El texto ya esta escapado: las comillas de la URL vienen como &quot; y no pueden cerrar el atributo.
    private static readonly Regex Link = new(@"\[([^\]]+)\]\(((?:https?://|mailto:)[^\s)]+)\)", RegexOptions.Compiled);
    private static readonly Regex Ordered = new(@"^\d+\.\s+", RegexOptions.Compiled);

    public static string ToHtml(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return string.Empty;

        var blocks = Regex.Split(markdown.Replace("\r\n", "\n").Trim(), @"\n\s*\n");
        var html = new StringBuilder();
        foreach (var raw in blocks)
        {
            var lines = raw.Split('\n').Select(l => l.TrimEnd()).Where(l => l.Length > 0).ToList();
            if (lines.Count == 0) continue;

            if (lines.Count == 1 && lines[0].StartsWith("### "))
                html.Append("<h3>").Append(Inline(lines[0][4..])).Append("</h3>");
            else if (lines.Count == 1 && lines[0].StartsWith("## "))
                html.Append("<h2>").Append(Inline(lines[0][3..])).Append("</h2>");
            else if (lines.All(l => l.StartsWith("- ") || l.StartsWith("* ")))
                html.Append("<ul>").Append(string.Concat(lines.Select(l => $"<li>{Inline(l[2..])}</li>"))).Append("</ul>");
            else if (lines.All(l => Ordered.IsMatch(l)))
                html.Append("<ol>").Append(string.Concat(lines.Select(l => $"<li>{Inline(Ordered.Replace(l, ""))}</li>"))).Append("</ol>");
            else
                html.Append("<p>").Append(string.Join("<br>", lines.Select(Inline))).Append("</p>");
        }
        return html.ToString();
    }

    private static string Inline(string text)
    {
        var s = WebUtility.HtmlEncode(text.Trim());
        s = Link.Replace(s, m => $"<a href=\"{m.Groups[2].Value}\" target=\"_blank\" rel=\"noopener nofollow\">{m.Groups[1].Value}</a>");
        s = Bold.Replace(s, "<strong>$1</strong>");
        s = Italic.Replace(s, "<em>$1</em>");
        return s;
    }
}

/// <summary>Enlaces de video admitidos en Noticias: YouTube y Vimeo.</summary>
public static class NewsVideo
{
    private static readonly Regex YouTube = new(@"^https?://(?:www\.|m\.)?(?:youtube\.com/(?:watch\?(?:.*&)?v=|shorts/|embed/)|youtu\.be/)([A-Za-z0-9_-]{11})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex Vimeo = new(@"^https?://(?:www\.|player\.)?vimeo\.com/(?:video/)?(\d{6,12})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>("youtube"|"vimeo", id) o null si no es un enlace valido de esos dos.</summary>
    public static (string Provider, string Id)? Parse(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        url = url.Trim();
        var yt = YouTube.Match(url);
        if (yt.Success) return ("youtube", yt.Groups[1].Value);
        var vm = Vimeo.Match(url);
        if (vm.Success) return ("vimeo", vm.Groups[1].Value);
        return null;
    }

    /// <summary>Reproductor sin cookies de seguimiento; solo se carga al pulsar Reproducir.</summary>
    public static string EmbedUrl(string provider, string id) => provider == "youtube"
        ? $"https://www.youtube-nocookie.com/embed/{id}?autoplay=1&rel=0"
        : $"https://player.vimeo.com/video/{id}?autoplay=1&dnt=1";
}

public static class NewsSlug
{
    public const int MaxLength = 80;

    /// <summary>"¡Consejos para tu 1ª entrevista!" -> "consejos-para-tu-1a-entrevista".</summary>
    public static string From(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var sb = new StringBuilder();
        foreach (var c in text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            var ch = c switch { 'ª' => 'a', 'º' => 'o', 'ñ' => 'n', _ => c };
            sb.Append(ch is >= 'a' and <= 'z' or >= '0' and <= '9' ? ch : '-');
        }
        var slug = Regex.Replace(sb.ToString(), "-{2,}", "-").Trim('-');
        return slug.Length > MaxLength ? slug[..MaxLength].TrimEnd('-') : slug;
    }
}
