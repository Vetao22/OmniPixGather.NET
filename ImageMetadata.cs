using System;
using System.Collections.Generic;

namespace OmniPixGather.NET.Models
{
    /// <summary>
    /// Represents normalized metadata extracted from any image provider.
    /// </summary>
    public sealed class ImageMetadata
    {
        public string ProviderName { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string UrlOriginal { get; set; }
        public string UrlThumbnail { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public long FileSize { get; set; }
        public string SourcePageUrl { get; set; }
        public bool IsNsfw { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public Dictionary<string, object> ExtraAttributes { get; set; } = new Dictionary<string, object>();
    }
}