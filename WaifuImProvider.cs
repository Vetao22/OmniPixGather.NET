using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OmniPixGather.NET.Models;

namespace OmniPixGather.NET.Providers
{
    /// <summary>
    /// Image provider implementation for Waifu.im API.
    /// </summary>
    public sealed class WaifuImProvider : BaseHttpProvider, IImageProvider
    {
        public string ProviderName => "Waifu.im";

        public async Task<IEnumerable<ImageMetadata>> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default)
        {
            var results = new List<ImageMetadata>();
            string tag = query.Tags.Count > 0 ? query.Tags[0] : (query.Term ?? "waifu");
            string nsfwParam = query.NsfwOnly.HasValue && query.NsfwOnly.Value ? "true" : "false";

            string url = $"https://api.waifu.im/search?included_tags={Uri.EscapeDataString(tag)}&is_nsfw={nsfwParam}&limit={Math.Clamp(query.LimitPerProvider, 1, 30)}";

            try
            {
                HttpResponseMessage response = await SharedHttpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode) return results;

                string json = await response.Content.ReadAsStringAsync(cancellationToken);
                using JsonDocument doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("images", out JsonElement imagesArray))
                {
                    foreach (JsonElement item in imagesArray.EnumerateArray())
                    {
                        bool isNsfw = item.TryGetProperty("is_nsfw", out JsonElement nsfwProp) && nsfwProp.GetBoolean();
                        var meta = new ImageMetadata
                        {
                            ProviderName = ProviderName,
                            Id = item.GetProperty("image_id").GetInt32().ToString(),
                            UrlOriginal = item.GetProperty("url").GetString(),
                            UrlThumbnail = item.GetProperty("url").GetString(),
                            Width = item.GetProperty("width").GetInt32(),
                            Height = item.GetProperty("height").GetInt32(),
                            IsNsfw = isNsfw,
                            SourcePageUrl = item.TryGetProperty("source", out JsonElement src) && src.ValueKind == JsonValueKind.String ? src.GetString() : ""
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