using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniPixGather.NET.Models;

namespace OmniPixGather.NET.Providers
{
    /// <summary>
    /// Defines the contract for all image provider implementations.
    /// </summary>
    public interface IImageProvider
    {
        string ProviderName { get; }
        Task<IEnumerable<ImageMetadata>> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default);
    }
}