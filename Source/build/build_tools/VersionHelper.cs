using Nuke.Common.CI.GitLab;
using System;
using System.Linq;

namespace build_tools
{
    internal static class VersionHelper
    {
        public static TagVersion GetTagVersionString(string baseVersion)
        {
            if (string.IsNullOrWhiteSpace(GitLab.Instance?.CommitTag))
                return new TagVersion(baseVersion + ".0", baseVersion + ".0-dev");

            var tag = GitLab.Instance.CommitTag.Split('/').Last();

            if (!tag.StartsWith("v" + baseVersion))
                throw new Exception($"Wrong tag pattern! The release version tags needs to be like 'v{baseVersion}.x'! If you want to release a version higher than '{baseVersion}', you have to change the version in Build.cs.");

            var tagParts = tag.Split(new[] { '-' }, 2);

            var suffix = tagParts.Length == 2
                ? tagParts[1]
                : "";

            var version = tagParts[0].TrimStart('v');
            if (string.IsNullOrEmpty(suffix))
            {
                return new TagVersion(version, version);
            }
            return new TagVersion(version, version + "-" + suffix);
        }

        public static Version GetTagVersion(string baseVersion)
        {
            var tagVersion = GetTagVersionString(baseVersion);
            var version = new Version(tagVersion.Version);
            if (version.Revision == -1)
            {
                return new Version(version.Major, version.Minor, version.Build, 0);
            }
            return version;
        }
    }

    internal record TagVersion(string Version, string PackageVersion);
}