using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AllergyScan.Models;

namespace AllergyScan.Services
{
    public interface IHistoryRescanService
    {
        Task RescanAllHistoryAsync(IEnumerable<Allergy> allergies, CancellationToken cancellationToken = default);
    }

    public sealed class HistoryRescanService : IHistoryRescanService
    {
        private readonly IScanDataService scanDataService;
        private readonly IAllergenDetectionService allergenDetectionService;

        public HistoryRescanService(
            IScanDataService scanDataService,
            IAllergenDetectionService allergenDetectionService)
        {
            this.scanDataService = scanDataService;
            this.allergenDetectionService = allergenDetectionService;
        }

        public async Task RescanAllHistoryAsync(IEnumerable<Allergy> allergies, CancellationToken cancellationToken = default)
        {
            // Get all history scans (paged)
            int page = 1;
            const int pageSize = 50;
            bool hasMore = true;

            while (hasMore && !cancellationToken.IsCancellationRequested)
            {
                var result = await scanDataService.GetHistoryAsync(page, pageSize, cancellationToken);
                
                if (result.Items.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                // Re-scan each history entry with current allergens
                foreach (var scan in result.Items)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    // Detect allergens in the stored text
                    // For history scans, we only have the original text (no translated text)
                    // So we use the same text for both original and translated
                    var detected = allergenDetectionService.Detect(
                        scan.Text, 
                        scan.Text, 
                        allergies);
                    
                    var mayContain = allergenDetectionService.DetectMayContain(
                        scan.Text, 
                        scan.Text, 
                        allergies);
                    
                    var mayContainTraces = allergenDetectionService.DetectMayContainTraces(
                        scan.Text, 
                        scan.Text, 
                        allergies);

                    // Combine all results
                    var allMatches = detected
                        .Concat(mayContain)
                        .Concat(mayContainTraces)
                        .ToList();

                    // Determine the overall flag (most severe)
                    var newFlag = allMatches.Count > 0
                        ? allMatches.Max(m => m.Flag)
                        : ScanFlag.NoAllergen;

                    // Only update if the flag changed
                    if (scan.Flag != newFlag)
                    {
                        try
                        {
                            await scanDataService.UpdateAsync(scan.Id, scan.Text, newFlag, cancellationToken);
                        }
                        catch
                        {
                            // If update fails, continue with next scan
                        }
                    }
                }

                // Check if there are more pages
                hasMore = result.Items.Count >= pageSize;
                page++;
            }
        }
    }
}
