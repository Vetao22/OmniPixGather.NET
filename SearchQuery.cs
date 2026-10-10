using System.Collections.Generic;

namespace OmniPixGather.NET.Models
{
    /// <summary>
    /// Encapsulates advanced search parameters across multiple providers.
    /// </summary>
    public sealed class SearchQuery
    {
        public string Term { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> ExcludeTags { get; set; } = new List<string>();
        public List<string> Artists { get; set; } = new List<string>();
        public int? TargetWidth { get; set; }
        public int? TargetHeight { get; set; }
        public int ResolutionTolerance { get; set; } = 150; // Pixel variance margin for near-resolution matching
        public bool? NsfwOnly { get; set; } // true = nsfw only, false = sfw only, null = any
        public int LimitPerProvider { get; set; } = 20;
    }
}