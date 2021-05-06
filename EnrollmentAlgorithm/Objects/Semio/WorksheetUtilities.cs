using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Semio.ClientService.OpenXml.Excel
{
    /// <summary>
    /// </summary>
    public static class WorksheetUtilities
    {
        /// <summary>
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="rgbColor"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public static ConditionalFormattingRule CreateDataBarRule(int minValue, int maxValue, string rgbColor, int priority)
        {
            var dataBar = new DataBar(new OpenXmlElement[]
                                      {
                                          new ConditionalFormatValueObject
                                          {
                                              Type = ConditionalFormatValueObjectValues.Number,
                                              Val = minValue.ToString(),
                                          },
                                          new ConditionalFormatValueObject
                                          {
                                              Type = ConditionalFormatValueObjectValues.Max,
                                              Val = maxValue.ToString(),
                                          },
                                          new Color { Rgb = rgbColor },
                                      });
            var dataBarRule = new ConditionalFormattingRule(dataBar)
            {
                Type = ConditionalFormatValues.DataBar,
                Priority = priority,
            };
            return dataBarRule;
        }

        /// <summary>
        /// </summary>
        /// <param name="failValue"></param>
        /// <param name="targetValueCellRef"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public static ConditionalFormattingRule CreateIconSetRule(int failValue, string targetValueCellRef, int priority)
        {
            var iconSet = new IconSet(new OpenXmlElement[]
                                      {
                                          new ConditionalFormatValueObject
                                          {
                                              Type = ConditionalFormatValueObjectValues.Number,
                                              Val = failValue.ToString(),
                                          },
                                          new ConditionalFormatValueObject
                                          {
                                              Type = ConditionalFormatValueObjectValues.Number,
                                              Val = targetValueCellRef,
                                          },
                                          new ConditionalFormatValueObject
                                          {
                                              Type = ConditionalFormatValueObjectValues.Number,
                                              Val = targetValueCellRef,
                                          },
                                      }
                );
            var iconSetRule = new ConditionalFormattingRule(iconSet)
            {
                Type = ConditionalFormatValues.IconSet,
                Priority = priority,
            };
            return iconSetRule;
        }
    }
}