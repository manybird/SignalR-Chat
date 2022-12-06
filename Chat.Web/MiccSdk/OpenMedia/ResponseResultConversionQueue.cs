namespace Chat.Web.MiccSdk.OpenMedia
{
    public class ResponseResultConversionQueue : ResponseResult
    {
        public string Count { get; set; }
        public override ResponseLinks _links { get; set; }
        public override bool IsSuccess => IsSuccessStatusCode && _embedded != null;
        public ResponseResultEmbedded<ResponseResultConversionQueueItem> _embedded { get; set; }

        
    }

    public class ResponseResultConversionQueueItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Reporting { get; set; }
    }
}
