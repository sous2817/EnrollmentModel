namespace Semio.ClientService.Data.Intelligence
{
    public class CountryInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IsoCode { get; set; }
        public string Iso3Code { get; set; }
        public string IrbType { get; set; }
        public decimal? SiteStartUpMedian { get; set; }
        public decimal? EnrollmentRateMedian { get; set; }
        public decimal? RegulatoryDocumentCycleMedian { get; set; }
        public decimal? SiteContractCycleMedian { get; set; }
    }
}