namespace AllergyScan.BLL.Options
{
    public class ScanOptions
    {
        public int MaxImageBytes { get; set; } = 10 * 1024 * 1024;

        public int MaxTextLength { get; set; } = 100_000;

        public int DefaultPageSize { get; set; } = 20;

        public int MaxPageSize { get; set; } = 100;
    }
}
