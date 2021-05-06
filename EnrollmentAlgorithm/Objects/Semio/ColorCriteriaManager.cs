
using Semio.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;

namespace Semio.ClientService.Data.Intelligence
{
    public class ColorCriteriaManager
    {
        public static readonly string V1 = "V1";
        public static readonly string V2 = "V2";
        public static readonly string Vx = "Vx";  //no color coding

        public static readonly Dictionary<int, ColorScore> ColorScoring = new Dictionary<int, ColorScore>
            {
                {100, new ColorScore {Id=100, ScoreSet=V1, Score=0, DisplayText="0", DisplayOrder=1, IsRangeOption=true, IsTierColor=true, TierDisplayText="3", TierDisplayOrder=3, ColorText="Red", ColorResourceName="FRangeRed", NotIncludedColorResourceName = "FRangeLightRed", StyleId=ColorStyle.RedFill}},
                {101, new ColorScore {Id=101, ScoreSet=V1, Score=1, DisplayText="1", DisplayOrder=2, IsRangeOption=true, IsTierColor=true, TierDisplayText="2", TierDisplayOrder=2, ColorText="Yellow", ColorResourceName="FRangeYellow" , NotIncludedColorResourceName = "FRangeLightYellow", StyleId=ColorStyle.YellowFill}},
                {102, new ColorScore {Id=102, ScoreSet=V1, Score=2, DisplayText="2", DisplayOrder=3, IsRangeOption=true, IsTierColor=true, TierDisplayText="1", TierDisplayOrder=1, ColorText="Green", ColorResourceName="FRangeGreen", NotIncludedColorResourceName = "FRangeLightGreen", StyleId=ColorStyle.GreenFill}},
                {103, new ColorScore {Id=103, ScoreSet=V1, Score=-1, DisplayText="-1", DisplayOrder=4, IsRangeOption=false, IsTierColor=true, TierDisplayText="-1", TierDisplayOrder=4, ColorText="Gray", ColorResourceName="FRangeLightGray", StyleId=ColorStyle.LightGrayFill}},

                {200, new ColorScore {Id=200, ScoreSet=V2, Score=0, DisplayText="0", DisplayOrder=2, IsRangeOption=true, IsTierColor=false, TierDisplayText="", TierDisplayOrder=0, ColorText="Gray", ColorResourceName="FRangeLightGray", StyleId=ColorStyle.LightGrayFill}},
                {201, new ColorScore {Id=201, ScoreSet=V2, Score=1, DisplayText="1", DisplayOrder=3, IsRangeOption=true, IsTierColor=true, TierDisplayText="3", TierDisplayOrder=3, ColorText="Red", ColorResourceName="FRangeRed", NotIncludedColorResourceName = "FRangeLightRed", StyleId=ColorStyle.RedFill}},
                {202, new ColorScore {Id=202, ScoreSet=V2, Score=2, DisplayText="2", DisplayOrder=4, IsRangeOption=true, IsTierColor=false, TierDisplayText="", TierDisplayOrder=0, ColorText="Orange", ColorResourceName="FRangeOrange", StyleId=ColorStyle.OrangeFill}},
                {203, new ColorScore {Id=203, ScoreSet=V2, Score=3, DisplayText="3", DisplayOrder=5, IsRangeOption=true, IsTierColor=true, TierDisplayText="2", TierDisplayOrder=2, ColorText="Yellow", ColorResourceName="FRangeYellow" , NotIncludedColorResourceName = "FRangeLightYellow",StyleId=ColorStyle.YellowFill}},
                {204, new ColorScore {Id=204, ScoreSet=V2, Score=4, DisplayText="4", DisplayOrder=6, IsRangeOption=true, IsTierColor=true, TierDisplayText="1", TierDisplayOrder=1, ColorText="Green", ColorResourceName="FRangeGreen", NotIncludedColorResourceName = "FRangeLightGreen", StyleId=ColorStyle.GreenFill}},
                {299, new ColorScore {Id=299, ScoreSet=V2, Score=0, DisplayText="Fail", DisplayOrder=1, IsRangeOption=true, IsTierColor=true, TierDisplayText="Fail", TierDisplayOrder=4, ColorText="DarkGray", ColorResourceName="FRangeDarkGray", StyleId=ColorStyle.DarkGrayFill}},

                {999, new ColorScore {Id=999, ScoreSet=Vx, Score=0, DisplayText="Neutral", DisplayOrder=1, IsRangeOption=false, IsTierColor=false, TierDisplayText="Neutral", TierDisplayOrder=4, ColorText="White", ColorResourceName="FRangeWhite", StyleId=ColorStyle.WhiteFill}}
            };

        private static readonly FacetBucketScoring[] FACETS_WITH_BUCKETED_SCORING = new[]
            {
                new FacetBucketScoring
                {
                    Id = "EnrollmentFactorMedian",
                    DefaultValue = 100,
                    MinAndResult = new Dictionary<double, double>
                                   {
                                       {0.25, 0},
                                       {0.50, 25},
                                       {0.75, 50},
                                       {1.00, 75},
                                   }
                },
            };

        ////private Func<Facet, bool> _validFacetPredicate = f => !f.IsDataSourceFacet && f.IsChecked;
        ////private static Func<Facet, bool> _excludeFacetForCodingPredicate = f => !f.IsDataSourceFacet && !f.IsTierScoreFacet && f.IsChecked;
        ////public static Func<Facet, bool> ExcludeFacetForCodingPredicate = _excludeFacetForCodingPredicate;
        ////private Dictionary<string, Facet> _criteriaControl = new Dictionary<string, Facet>();

        /////// <summary>
        /////// Initialize the ColorCriteriaManager
        /////// </summary>
        /////// <param name="savedCriteria"></param>
        ////public void Initialize(FacetCollection savedCriteria)
        ////{
        ////    _criteriaControl.Clear();

        ////    var validFacets = savedCriteria.Facets.Where(_validFacetPredicate).ToArray();
        ////    //setting criteria must also store them sorted.
        ////    //do this once now for efficiency
        ////    for (int i = 0; i < validFacets.Count(); i++)
        ////    {
        ////        _criteriaControl.Add(validFacets[i].Id,
        ////           new Facet
        ////           {
        ////               Id = validFacets[i].Id,
        ////               Values = new ObservableCollection<FacetValue>(validFacets[i].Values.OrderByDescending(fv => Convert.ToDouble(fv.Value)).ToList()),
        ////               ScoreWeight = validFacets[i].ScoreWeight,
        ////               IsChecked = validFacets[i].IsChecked,
        ////               Display = validFacets[i].Display
        ////           });
        ////    }
        ////}

        ///// <summary>
        ///// Color code and Rank countries
        ///// </summary>
        ///// <param name="colorCodedItems"></param>
        //public void CalculateColors(IEnumerable<IColorCoded> colorCodedItems)
        //{
        //    var colorCodedList = colorCodedItems.ToList();

        //    var checkedFacetsExceptDataSourceAndTier = _criteriaControl.Values.Where(_excludeFacetForCodingPredicate).ToList();

        //    foreach (var item in colorCodedList)
        //    {
        //        item.FacetScores.Clear();
        //        item.ClearVals();

        //        var propertyDictionary = CreatePropertyValuesDictionary(item, checkedFacetsExceptDataSourceAndTier);

        //        foreach (var savedCriteriaFacet in checkedFacetsExceptDataSourceAndTier)
        //        {
        //            //// TODO: Do not get this through reflection...
        //            //var itemPropertyValue = GetPropertyValue(item, savedCriteriaFacet.Value.Id);

        //            var itemPropertyValue = GetItemValue(item, savedCriteriaFacet.Id, propertyDictionary);

        //            //note new savedcriteria should contain a -1 range for nulls
        //            var criteriaFacetValueMatch = savedCriteriaFacet.Values.FirstOrDefault(v => Double.Parse(itemPropertyValue) >= Convert.ToDouble(v.IdRounded));
        //            var scoreId = (criteriaFacetValueMatch != null)
        //                    ? criteriaFacetValueMatch.ScoreId
        //                    : ColorScoring.First(s => s.Value.Score == -1).Value.Id;  //backup to -1 scoreid if no criteria is used

        //            double colorVal = ColorScoring[scoreId].Score;

        //            item.FacetScores.Add(
        //                    new Facet
        //                    {
        //                        Id = savedCriteriaFacet.Id,
        //                        ScoreWeight = savedCriteriaFacet.ScoreWeight,
        //                        ScoreId = scoreId,
        //                        Display = savedCriteriaFacet.Display
        //                    });

        //            PropertyInfo valPropertyInfo = item.GetType().GetProperty(savedCriteriaFacet.Id + "Val", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        //            //some properties may not have a related val field
        //            if (valPropertyInfo != null)
        //            {
        //                valPropertyInfo.SetValue(item, Convert.ChangeType(colorVal, valPropertyInfo.PropertyType), null);
        //            }
        //        }
        //        CalculateColorSummaryValues(item);
        //    }

        //    //rank'em
        //    var rank = 1;
        //    var rankDictionary = colorCodedList.GroupBy(i => i.TotalScore).OrderByDescending(g => g.Key).ToDictionary(colorCoded => colorCoded.Key, colorCoded => rank++);
        //    foreach (var item in colorCodedList)
        //    {
        //        item.TotalRank = rankDictionary[item.TotalScore];
        //    }
        //}

        //public void CalculateColorSummaryValues(IColorCoded item)
        //{
        //    double totalScore = 0d;
        //    int count = 0;
        //    double weightedTotalScore = 0d;
        //    double sumOfRatioValues = 0d;
        //    bool hasFailure = false;
        //    int failureScoreId = 0;

        //    foreach (var facet in item.FacetScores)
        //    {
        //        double scoreValue = 0;

        //        ColorScore colorScore = ColorScoring[facet.ScoreId];

        //        //check for percentile scoring
        //        if (facet.ScoreId == 999)
        //        {
        //            var firstOrDefault = facet.Values.FirstOrDefault();
        //            if (firstOrDefault != null && firstOrDefault.IdRounded.HasValue)
        //                scoreValue = firstOrDefault.IdRounded.Value;
        //        }
        //        else
        //        {
        //            scoreValue = colorScore.Score;
        //        }

        //        if (scoreValue != -1)
        //        {
        //            //int colorVal = colorScore.Score;
        //            totalScore = totalScore + scoreValue;
        //            weightedTotalScore = weightedTotalScore + (scoreValue * facet.ScoreWeight);
        //            count++;
        //            sumOfRatioValues = sumOfRatioValues + facet.ScoreWeight;
        //        }
        //        if (colorScore.DisplayText == "Fail")
        //        {
        //            hasFailure = true;
        //            failureScoreId = colorScore.Id;
        //        }
        //    }

        //    var avgScore = (count > 0 && sumOfRatioValues > 0)
        //        ? CalculateAvgColor(totalScore / count)
        //        : -1;
        //    var weightedAvgScore = (count > 0 && sumOfRatioValues > 0)
        //        ? weightedTotalScore / sumOfRatioValues
        //        : -1;

        //    item.AvgScore = avgScore.ToString(CultureInfo.InvariantCulture);
        //    item.TotalScore = (int)totalScore;
        //    item.ScoredCount = count;

        //    FacetValue tierScoreFacetValueMatch =
        //        _criteriaControl[FacetHelper.TierScoreFacetName].Values.First(
        //            v => Math.Round(weightedAvgScore, 2, MidpointRounding.AwayFromZero) >= Convert.ToDouble(v.Value));
        //    int tierScoreId = tierScoreFacetValueMatch.ScoreId;
        //    if (hasFailure)
        //    {
        //        tierScoreId = failureScoreId;
        //    }
        //    item.WeightedTierScore = new Facet
        //    {
        //        Id = FacetHelper.TierScoreFacetName,
        //        ScoreId = tierScoreId,
        //        IsChecked = true,
        //        Values = new ObservableCollection<FacetValue>
        //        {
        //            new FacetValue {Id = weightedAvgScore.ToString(), Value = weightedAvgScore.ToString()}
        //            //store the actual value
        //        }
        //    };
        //}

        //public int CalculateAvgColor(double? avg)
        //{
        //    //todo: upgrade this
        //    if (avg < .75)
        //        return 0;

        //    if (avg >= .75 && avg < 1.25)
        //        return 1;

        //    if (avg >= 1.25)
        //        return 2;

        //    return -1;
        //}

        ///// <summary>
        ///// Loads a dictionary with properties with a corresponding facet keyed on the
        ///// property name lowercased
        ///// </summary>
        ///// <param name="item"></param>
        ///// <param name="facets"></param>
        //private static Dictionary<string, object> CreatePropertyValuesDictionary(
        //    IColorCoded item,
        //    IEnumerable<Facet> facets)
        //{
        //    var propertyDictionary = new Dictionary<string, object>();

        //    var properties =
        //        item.GetType()
        //            .GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
        //            .ToDictionary(p => p.Name.ToLower(), p => p);

        //    foreach (var x in facets)
        //    {
        //        var facetId = x.Id.ToLower();
        //        if (properties.ContainsKey(facetId))
        //        {
        //            var propValue = properties[facetId].GetValue(item, null);

        //            var returnValue = RoundToTwoDecimalPlaces(propValue);

        //            propertyDictionary.Add(properties[facetId].Name.ToLower(), returnValue);
        //        }
        //    }
        //    return propertyDictionary;
        //}

        //private static object RoundToTwoDecimalPlaces(object value)
        //{
        //    double outValue;
        //    bool tryParse = double.TryParse(Convert.ToString(value), out outValue);
        //    if (tryParse)
        //    {
        //        return Convert.ToString(Math.Round(outValue, 2, MidpointRounding.AwayFromZero));
        //    }
        //    return value;
        //}

        //public void CalculatePercentiles(IEnumerable<IColorCoded> colorCodedItems, IEnumerable<Facet> sourceFacets)
        //{
        //    var colorCodedList = colorCodedItems.ToList();
        //    var facetDictionary = new Dictionary<string, Facet>();

        //    var checkedFacetsExceptDataSourceAndTier = _criteriaControl.Values.Where(_excludeFacetForCodingPredicate).ToList();

        //    facetDictionary.AddRange(sourceFacets.Select(f => new KeyValuePair<string, Facet>(f.Id.ToLower(), f)));
        //    Parallel.ForEach(
        //        colorCodedList,
        //        item =>
        //    {
        //        lock (item)
        //        {
        //            var localItem = item;
        //            localItem.FacetScores.Clear();
        //            localItem.ClearVals();

        //            var propertyDictionary = CreatePropertyValuesDictionary(localItem, checkedFacetsExceptDataSourceAndTier);

        //            foreach (var savedCriteriaFacet in checkedFacetsExceptDataSourceAndTier)
        //            {
        //                var facetId = savedCriteriaFacet.Id.ToLower();
        //                if (!facetDictionary.ContainsKey(facetId))
        //                {
        //                    continue;
        //                }

        //                var sourceFacet = facetDictionary[facetId];

        //                if (sourceFacet == null)
        //                {
        //                    continue;
        //                }

        //                //get property value of item
        //                ////var itemPropertyValue = GetPropertyValue(localItem, savedCriteriaFacet.Id);

        //                var itemPropertyValue = GetItemValue(item, savedCriteriaFacet.Id, propertyDictionary);
        //                var itemPropertyAsDouble = Convert.ToDouble(itemPropertyValue);

        //                var percentile = itemPropertyValue != "-1"
        //                    ? sourceFacet.EspPercentile(itemPropertyAsDouble)
        //                    : 0;

        //                var bucketScoreDefinition = FACETS_WITH_BUCKETED_SCORING.FirstOrDefault(b => b.Id == savedCriteriaFacet.Id);
        //                if (bucketScoreDefinition != null)
        //                {
        //                    percentile = bucketScoreDefinition.GetScoreBucket(itemPropertyAsDouble);
        //                }

        //                var newFacet = new Facet
        //                {
        //                    Id = savedCriteriaFacet.Id,
        //                    ScoreWeight = savedCriteriaFacet.ScoreWeight,
        //                    ScoreId = 999,
        //                    Display = savedCriteriaFacet.Display,
        //                    Values =
        //                        new ObservableCollection<FacetValue>
        //                        {
        //                            new FacetValue {Id = percentile.ToString(), Value = percentile.ToString()}
        //                        }
        //                };

        //                localItem.FacetScores.Add(newFacet);
        //            }

        //            CalculateColorSummaryValues(localItem);
        //        }
        //    });

        //    //rank'em
        //    var rank = 1;
        //    var rankDictionary = colorCodedList.GroupBy(i => i.WeightedTierScore.FirstFacetValue).OrderByDescending(g => g.Key).ToDictionary(colorCoded => colorCoded.Key, colorCoded => rank++);
        //    foreach (var item in colorCodedList)
        //    {
        //        item.TotalRank = rankDictionary[item.WeightedTierScore.FirstFacetValue];
        //    }
        //}

        //private string GetItemValue(IColorCoded item, string savedCriteriaFacetId, Dictionary<string, object> propertyDictionary)
        //{
        //    object obj;

        //    propertyDictionary.TryGetValue(savedCriteriaFacetId.ToLower(), out obj);

        //    var itemPropertyValue = obj != null && !string.IsNullOrEmpty(Convert.ToString(obj)) ? Convert.ToString(obj) : "-1";

        //    if (itemPropertyValue == "-1" && item.CustomValues != null)
        //    {
        //        // search the custom values
        //        var customValues = item.CustomValues;
        //        var facet = customValues.FirstOrDefault(v => v.Id == savedCriteriaFacetId);
        //        if (facet != null)
        //        {
        //            var value = facet.Values.FirstOrDefault();
        //            if (value != null)
        //            {
        //                itemPropertyValue = value.Value;
        //            }
        //        }
        //    }
        //    return itemPropertyValue;
        //}

        public class FacetBucketScoring
        {
            public string Id { get; set; }
            public Dictionary<double, double> MinAndResult { get; set; }
            public double DefaultValue { get; set; }

            public double GetScoreBucket(double value)
            {
                var match = MinAndResult.OrderBy(mar => mar.Key)
                                        .FirstOrDefault(mar => value < mar.Key);

                if (!match.Equals(default(KeyValuePair<double, double>)))
                    return match.Value;

                return DefaultValue;
            }

            public FacetBucketScoring()
            {
                MinAndResult = new Dictionary<double, double>();
            }
        }
    }
}