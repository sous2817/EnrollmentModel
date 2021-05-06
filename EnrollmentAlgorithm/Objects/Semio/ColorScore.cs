namespace Semio.ClientService.Data.Intelligence
{
    public class ColorScore
    {
        public int Id { get; set; }
        public int Score { get; set; }
        public string ScoreSet { get; set; }
        public string DisplayText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRangeOption { get; set; }
        public bool IsTierColor { get; set; }
        public string TierDisplayText { get; set; }
        public int TierDisplayOrder { get; set; }
        public string ColorText { get; set; }
        public string ColorResourceName { get; set; }
        public ColorStyle StyleId { get; set; }
        public string NotIncludedColorResourceName { get; set; }
        public bool IsFailed { get; set; }
    }
}