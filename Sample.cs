using System;
using System.Threading.Tasks;
using OmniPixGather.NET;
using OmniPixGather.NET.Models;
using OmniPixGather.NET.Providers;

namespace OmniPixGatherDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Initializing OmniPixGather.NET Engine...\n");

            var engine = new OmniAggregatorEngine();
            engine.RegisterProvider(new WallhavenProvider());
            engine.RegisterProvider(new DanbooruProvider());
            engine.RegisterProvider(new WaifuImProvider());

            // --- TEST CASE 1: Search by Term ---
            Console.WriteLine("=== 1. Search by Term: 'cyberpunk' ===");
            var queryTerm = new SearchQuery { Term = "cyberpunk", NsfwOnly = false, LimitPerProvider = 3 };
            await RunAndPrint(engine, queryTerm);

            // --- TEST CASE 2: Search by Single Tag ---
            Console.WriteLine("=== 2. Search by Tag: 'landscape' ===");
            var queryTag = new SearchQuery();
            queryTag.Tags.Add("landscape");
            queryTag.NsfwOnly = false;
            queryTag.LimitPerProvider = 3;
            await RunAndPrint(engine, queryTag);

            // --- TEST CASE 3: Search by Multiple Tags ---
            Console.WriteLine("=== 3. Search by Multiple Tags: ['neon', 'city'] ===");
            var queryMultiTags = new SearchQuery();
            queryMultiTags.Tags.AddRange(new[] { "neon", "city" });
            queryMultiTags.NsfwOnly = false;
            queryMultiTags.LimitPerProvider = 3;
            await RunAndPrint(engine, queryMultiTags);

            // --- TEST CASE 4: Search by Artist ---
            Console.WriteLine("=== 4. Search by Artist: 'zaba' ===");
            var queryArtist = new SearchQuery();
            queryArtist.Artists.Add("zaba");
            queryArtist.LimitPerProvider = 3;
            await RunAndPrint(engine, queryArtist);

            // --- TEST CASE 5: Search by Multiple Artists ---
            Console.WriteLine("=== 5. Search by Multiple Artists ===");
            var queryMultiArtists = new SearchQuery();
            queryMultiArtists.Artists.AddRange(new[] { "artistA", "artistB" });
            queryMultiArtists.LimitPerProvider = 3;
            await RunAndPrint(engine, queryMultiArtists);

            // --- TEST CASE 6: Exact Resolution & Near Resolution (Tolerance) ---
            Console.WriteLine("=== 6. Search near Resolution: 1920x1080 (Tolerance: 50px) ===");
            var queryResolution = new SearchQuery
            {
                Term = "wallpaper",
                TargetWidth = 1920,
                TargetHeight = 1080,
                ResolutionTolerance = 50,
                NsfwOnly = false,
                LimitPerProvider = 3
            };
            await RunAndPrint(engine, queryResolution);

            // --- TEST CASE 7: Excluding Tags ---
            Console.WriteLine("=== 7. Search excluding tags: excluding 'nsfw', 'sketch' ===");
            var queryExclude = new SearchQuery();
            queryExclude.Term = "art";
            queryExclude.ExcludeTags.AddRange(new[] { "sketch", "monochrome" });
            queryExclude.NsfwOnly = false;
            queryExclude.LimitPerProvider = 3;
            await RunAndPrint(engine, queryExclude);

            // --- TEST CASE 8: SFW Only vs NSFW Only ---
            Console.WriteLine("=== 8. SFW Only Search ===");
            var querySfw = new SearchQuery { Term = "cute", NsfwOnly = false, LimitPerProvider = 3 };
            await RunAndPrint(engine, querySfw);
        }

        private static async Task RunAndPrint(OmniAggregatorEngine engine, SearchQuery query)
        {
            var results = await engine.GatherAsync(query);
            int count = 0;
            foreach (var img in results)
            {
                count++;
                Console.WriteLine($"  [{count}] [{img.ProviderName}] ID: {img.Id} | Size: {img.Width}x{img.Height} | NSFW: {img.IsNsfw}");
                Console.WriteLine($"      URL: {img.UrlOriginal}");
            }
            if (count == 0) Console.WriteLine("  No results found for this criteria.");
            Console.WriteLine();
        }
    }
}