using System.Text.RegularExpressions;

namespace BlogApi.Helper
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;

            //lowercase
            var slug = title.ToLowerInvariant();

            //remove invalid characters
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

            //convert multiple spaces into one hyphen
            slug = Regex.Replace(slug, @"\s+", "-");

            //trim hyphens
            slug = slug.Trim('-');

            return slug;
        }
    }

}