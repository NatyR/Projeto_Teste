namespace Portal.API.Dto.Report
{
    public class PainelGerencialDto
    {
        public long groupId { get; set; }
        public long shopId { get; set; }
        public string shopName { get; set; }
        public string shopDocument { get; set; }
        public long userCount { get; set; }
        public long changeLimitCount { get; set; }
        public long globalLimitCount { get; set; }
        public long cardRequestCount { get; set; }
        public long cardReissueCount { get; set; }
        public long dismissalCount { get; set; }
        public long unblockCount { get; set; }
        public long accountUpdateCount { get; set; }
        public long totalRequestsCount { get; set;}
    }
}
