using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OmniPixGather.NET.Models;

namespace OmniPixGather.NET.Providers
{
    /// <summary>
    /// Image provider implementation for Danbooru API.
    /// </summary>
    public sealed class DanbooruProvider : BaseHttpProvider, IImageProvider
    {
        public string ProviderName => "Danbooru";

        public async Task<IEnumerable<ImageMetadata>> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default)
        {
            var results = new List<ImageMetadata>();
            var tagsList = new List<string>();

            if (!string.IsNullOrEmpty(query.Term)) tagsList.Add(query.Term);
            foreach (var t in query.Tags) tagsList.Add(t);
            foreach (var a in query.Artists) tagsList.Add($"artist:{a}");
            foreach (var et in query.ExcludeTags) tagsList.Add($"-{et}");

            if (query.NsfwOnly.HasValue)
            {
                tagsList.Add(query.NsfwOnly.Value ? "rating:e" : "rating:g");
            }

            string tagQuery = string.Join(" ", tagsList);
            string url = $"https://danbooru.donmai.us/posts.json?tags={Uri.EscapeDataString(tagQuery)}&limit={Math.Clamp(query.LimitPerProvider, 1, 50)}";

            try
            {
                HttpResponseMessage response = await SharedHttpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode) return results;

                string json = await response.Content.ReadAsStringAsync(cancellationToken);
                using JsonDocument doc = JsonDocument.Parse(json);

                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in doc.RootElement.EnumerateArray())
                    {
                        if (!item.TryGetProperty("file_url", out JsonElement fileUrlProp)) continue;

                        string rating = item.TryGetProperty("rating", out JsonElement rat) ? rat.GetString() : "g";
                        bool isNsfw = rating == "e" || rating == "q";

                        var meta = new ImageMetadata
                        {
                            ProviderName = ProviderName,
                            Id = item.GetProperty("id").GetInt64().ToString(),
                            Author = item.TryGetProperty("tag_string_artist", out JsonElement artist) ? artist.GetString() : "",
                            UrlOriginal = fileUrlProp.GetString(),
                            UrlThumbnail = item.TryGetProperty("preview_file_url", out JsonElement thumb) ? thumb.GetString() : fileUrlProp.GetString(),
                            Width = item.TryGetProperty("image_width", out JsonElement w) ? w.GetInt32() : 0,
                            Height = item.TryGetProperty("image_height", out JsonElement h) ? h.GetInt32() : 0,
                            IsNsfw = isNsfw,
                            SourcePageUrl = $"https://danbooru.donmai.us/posts/{item.GetProperty("id").GetInt64()}"
                        };
                        results.Add(meta);
                    }
                }
            }
            catch (Exception) { }

            return results;
        }
    }
}