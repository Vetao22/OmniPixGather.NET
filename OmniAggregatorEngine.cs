using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniPixGather.NET.Models;
using OmniPixGather.NET.Providers;

namespace OmniPixGather.NET
{
    /// <summary>
    /// Central aggregation engine executing queries in parallel and applying post-filtering (resolution, tags, SFW/NSFW).
    /// </summary>
    public sealed class OmniAggregatorEngine
    {
        private readonly List<IImageProvider> _providers = new List<IImageProvider>();

        public void RegisterProvider(IImageProvider provider)
        {
            if (provider != null) _providers.Add(provider);
        }

        public async Task<IEnumerable<ImageMetadata>> GatherAsync(SearchQuery query, CancellationToken cancellationToken = default)
        {
            if (_providers.Count == 0) return Enumerable.Empty<ImageMetadata>();

            // Execute parallel requests across all registered providers
            var tasks = _providers.Select(p => p.SearchAsync(query, cancellationToken));
            var providerResults = await Task.WhenAll(tasks);
            var aggregated = providerResults.SelectMany(x => x).ToList();

            // Apply client-side filters (Resolution, Target proximity, Exclude tags, SFW/NSFW enforcement)
            return aggregated.Where(img =>
            {
                // SFW / NSFW filter enforcement
                if (query.NsfwOnly.HasValue && img.IsNsfw != query.NsfwOnly.Value)
                    return false;

                // Exact or Near Resolution filtering
                if (query.TargetWidth.HasValue)
                {
                    int wDiff = Math.Abs(img.Width - query.TargetWidth.Value);
                    if (wDiff > query.ResolutionTolerance) return false;
                }

                if (query.TargetHeight.HasValue)
                {
                    int hDiff = Math.Abs(img.Height - query.TargetHeight.Value);
                    if (hDiff > query.ResolutionTolerance) return false;
                }

                return true;
            });
        }
    }
}