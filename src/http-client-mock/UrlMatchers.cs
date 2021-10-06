using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace HttpClientMock
{
    public static class UrlMatchers
    {
        [PublicAPI]
        public static Func<string, bool> Regex(string regexPattern)
        {
            return url =>
                {
                    var match = System.Text.RegularExpressions.Regex.Match(url, regexPattern, RegexOptions.IgnoreCase);
                    return match.Success;
                };
        }

        [PublicAPI]
        public static Func<string, bool> Is(string urlPattern)
        {
            return
                pathAndQuery =>
                {
                    var lowerStr = urlPattern.ToLower();
                    return pathAndQuery.ToLower() == (
                                        Uri.IsWellFormedUriString(lowerStr, UriKind.Absolute)
                                            ? new Uri(lowerStr).PathAndQuery
                                            : lowerStr
                                        );
                };
        }

        [PublicAPI]
        public static Func<string, bool> Wildcard(string wildCardPattern)
        {
            return s => new Regex($"^{System.Text.RegularExpressions.Regex.Escape(wildCardPattern)}$"
                .Replace("\\*", ".*")
                .Replace("\\?","."), RegexOptions.IgnoreCase).Match(s).Success;
        }
    }
}