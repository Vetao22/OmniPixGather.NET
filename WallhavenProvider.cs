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
    /// Image provider implementation for Wallhaven API.
    /// </summary>
    public sealed class WallhavenProvider : BaseHttpProvider, IImageProvider
    {
        public string ProviderName => "Wallhaven";

        public async Task<IEnumerable<ImageMetadata>> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default)
        {
            var results = new List<ImageMetadata>();
            var sb = new StringBuilder("https://wallhaven.cc/api/v1/search?");

            // Build query parameters
            string qParam = query.Term ?? string.Join(" ", query.Tags);
            if (query.Artists.Count > 0) qParam += " " + string.Join(" ", query.Artists);
            sb.Append($"q={Uri.EscapeDataString(qParam)}");

            // Purity filters: 100 = sfw, 010 = sketch/nsfw, 001 = nsfw
            if (query.NsfwOnly.HasValue)
            {
                sb.Append(query.NsfwOnly.Value ? "&purity=001" : "&purity=100");
            }

            sb.Append($"&per_page={Math.Clamp(query.LimitPerProvider, 1, 100)}");

            try
            {
                HttpResponseMessage response = await SharedHttpClient.GetAsync(sb.ToString(), cancellationToken);
                if (!response.IsSuccessStatusCode) return results;

                string json = await response.Content.ReadAsStringAsync(cancellationToken);
                using JsonDocument doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("data", out JsonElement dataArray))
                {
                    foreach (JsonElement item in dataArray.EnumerateArray())
                    {
                        string purity = item.GetProperty("purity").GetString();
                        bool isNsfw = purity != "sfw";

                        var meta = new ImageMetadata
                        {
                            ProviderName = ProviderName,
                            Id = item.GetProperty("id").GetString(),
                            UrlOriginal = item.GetProperty("path").GetString(),
                            UrlThumbnail = item.GetProperty("thumbs").GetProperty("small").GetString(),
                            Width = item.GetProperty("dimension_x").GetInt32(),
                            Height = item.GetProperty("dimension_y").GetInt32(),
                            FileSize = item.GetProperty("file_size").GetInt64(),
                            IsNsfw = isNsfw,
                            SourcePageUrl = item.GetProperty("url").GetString()
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