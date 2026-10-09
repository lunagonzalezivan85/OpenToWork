using OpenToWork.Core.Services;

namespace OpenToWork.Tests;

/// <summary>Noticias: Markdown -> HTML seguro, enlaces de video y direcciones (logica pura, sin API ni BD).</summary>
public class NewsTextTests
{
    [Fact]
    public void Markdown_HtmlEscritoAMano_SaleComoTexto()
    {
        var html = NewsMarkdown.ToHtml("Hola <script>alert('x')</script> <img src=x onerror=alert(1)>");
        Assert.DoesNotContain("<script", html);
        Assert.DoesNotContain("<img", html);
        Assert.Contains("&lt;script&gt;", html);
    }

    [Fact]
    public void Markdown_Formato_Basico()
    {
        var html = NewsMarkdown.ToHtml("## Consejos\n\nUn **gran** dia y *calma*.\nSegunda linea\n\n- uno\n- dos\n\n1. primero\n2. segundo");
        Assert.Equal(
            "<h2>Consejos</h2><p>Un <strong>gran</strong> dia y <em>calma</em>.<br>Segunda linea</p>" +
            "<ul><li>uno</li><li>dos</li></ul><ol><li>primero</li><li>segundo</li></ol>", html);
    }

    [Fact]
    public void Markdown_Enlaces_SoloHttpYMailto()
    {
        Assert.Contains("<a href=\"https://tratodirecto.es\" target=\"_blank\" rel=\"noopener nofollow\">web</a>",
            NewsMarkdown.ToHtml("[web](https://tratodirecto.es)"));
        // javascript: no se convierte en enlace.
        Assert.DoesNotContain("<a", NewsMarkdown.ToHtml("[clic](javascript:alert(1))"));
    }

    [Fact]
    public void Markdown_ComillasEnLaUrl_NoCierranElAtributo()
    {
        var html = NewsMarkdown.ToHtml("[x](https://a.com/\"onmouseover=\"alert(1))");
        Assert.DoesNotContain("\"onmouseover", html);
    }

    [Fact]
    public void Markdown_Vacio_DevuelveVacio()
    {
        Assert.Equal(string.Empty, NewsMarkdown.ToHtml(null));
        Assert.Equal(string.Empty, NewsMarkdown.ToHtml("   \n\n "));
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "youtube", "dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/watch?feature=share&v=dQw4w9WgXcQ", "youtube", "dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?t=10", "youtube", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/dQw4w9WgXcQ", "youtube", "dQw4w9WgXcQ")]
    [InlineData("https://vimeo.com/76979871", "vimeo", "76979871")]
    [InlineData("https://player.vimeo.com/video/76979871", "vimeo", "76979871")]
    public void Video_EnlacesValidos(string url, string provider, string id)
    {
        Assert.Equal((provider, id), NewsVideo.Parse(url));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("https://www.dailymotion.com/video/x7tgad0")]
    [InlineData("https://evil.com/?u=youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("javascript:alert(1)")]
    public void Video_EnlacesNoValidos(string? url)
    {
        Assert.Null(NewsVideo.Parse(url));
    }

    [Fact]
    public void Video_EmbedSinCookies()
    {
        Assert.StartsWith("https://www.youtube-nocookie.com/embed/abc", NewsVideo.EmbedUrl("youtube", "abc"));
        Assert.Contains("dnt=1", NewsVideo.EmbedUrl("vimeo", "123"));
    }

    [Theory]
    [InlineData("¡Consejos para tu 1ª entrevista!", "consejos-para-tu-1a-entrevista")]
    [InlineData("  Camarero/a en Málaga — año 2026 ", "camarero-a-en-malaga-ano-2026")]
    [InlineData("!!!", "")]
    public void Slug_DesdeTitulo(string title, string slug)
    {
        Assert.Equal(slug, NewsSlug.From(title));
    }

    [Fact]
    public void Slug_SeCortaSinGuionFinal()
    {
        var slug = NewsSlug.From(new string('a', 79) + " bbbb");
        Assert.True(slug.Length <= NewsSlug.MaxLength);
        Assert.False(slug.EndsWith('-'));
    }
}
