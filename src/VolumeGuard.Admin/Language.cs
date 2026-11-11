using System.Globalization;

namespace VolumeGuard.Admin;

internal static class L
{
    private static readonly bool Spanish = ResolveLanguage() == "es";
    public static string T(string spanish, string english) => Spanish ? spanish : english;

    private static string ResolveLanguage()
    {
        try
        {
            string language = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "language.txt")).Trim();
            if (language is "es" or "en") return language;
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "es" ? "es" : "en";
    }
}
