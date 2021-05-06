using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using NonVisualDrawingProperties = DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualDrawingProperties;
using NonVisualGraphicFrameDrawingProperties = DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameDrawingProperties;
using NonVisualGraphicFrameProperties = DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualGraphicFrameProperties;
using Run = DocumentFormat.OpenXml.Drawing.Run;
using RunProperties = DocumentFormat.OpenXml.Drawing.RunProperties;
using Text = DocumentFormat.OpenXml.Drawing.Text;

namespace Semio.ClientService.OpenXml.Excel
{
    /// <summary>
    /// Helper class for generating Excel Chart content
    /// </summary>
    public class ExcelChartHelper
    {
        public static Title GenerateAxisTitle(string axisTitleText, bool isXAxis)
        {
            Title title = new Title();
            ChartText chartText = new ChartText();

            RichText richText = new RichText();

            BodyProperties bodyProperties = isXAxis ?
                new BodyProperties() :
                new BodyProperties() { Rotation = -5400000, Vertical = TextVerticalValues.Horizontal, Anchor = TextAnchoringTypeValues.Top, AnchorCenter = true };

            ListStyle listStyle = new ListStyle();

            Paragraph paragraph = new Paragraph();

            ParagraphProperties paragraphProperties = new ParagraphProperties();
            DefaultRunProperties defaultRunProperties = new DefaultRunProperties();

            paragraphProperties.Append(defaultRunProperties);

            Run run = new Run();
            RunProperties runProperties = new RunProperties() { Language = "en-US" };

            Text text = new Text();
            text.Text = axisTitleText;

            run.Append(runProperties);
            run.Append(text);

            paragraph.Append(paragraphProperties);
            paragraph.Append(run);

            richText.Append(bodyProperties);
            richText.Append(listStyle);
            richText.Append(paragraph);

            chartText.Append(richText);

            title.ChartText = chartText;
            return title;
        }

        public static TwoCellAnchor GenerateWorksheetDrawingContents(DrawingsPart drawingsPart, ChartPart chartPart, string chartName, int startingRow, int startingColumn, int height, int width)
        {
            // The drawings part of the chart
            var graphicFrame = new DocumentFormat.OpenXml.Drawing.Spreadsheet.GraphicFrame
            {
                Macro = string.Empty,
                NonVisualGraphicFrameProperties = new NonVisualGraphicFrameProperties
                {
                    NonVisualDrawingProperties = new NonVisualDrawingProperties
                    {
                        Id = 2,
                        Name = chartName
                    }
                }
            };

            graphicFrame.NonVisualGraphicFrameProperties.NonVisualGraphicFrameDrawingProperties = new NonVisualGraphicFrameDrawingProperties();

            graphicFrame.Transform = new Transform
            {
                Offset = new Offset { X = 0, Y = 0 },
                Extents = new Extents { Cx = 0, Cy = 0 }
            };

            graphicFrame.Graphic = new Graphic { GraphicData = new GraphicData { Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart" } };
            graphicFrame.Graphic.GraphicData.Append(new ChartReference { Id = drawingsPart.GetIdOfPart(chartPart) });

            var twoCellAnchor = new TwoCellAnchor
            {
                FromMarker = new FromMarker
                {
                    RowId = new RowId(startingRow.ToString()),
                    RowOffset = new RowOffset("0"),
                    ColumnId = new ColumnId(startingColumn.ToString()),
                    ColumnOffset = new ColumnOffset("0")
                },
                ToMarker = new ToMarker
                {
                    RowId = new RowId((startingRow + height).ToString()),
                    RowOffset = new RowOffset("0"),
                    ColumnId = new ColumnId((startingColumn + width).ToString()),
                    ColumnOffset = new ColumnOffset("0")
                }
            };

            twoCellAnchor.Append(graphicFrame);
            twoCellAnchor.Append(new ClientData());

            return twoCellAnchor;
        }

        public static MajorGridlines GenerateMajorGridlines()
        {
            ChartShapeProperties chartShapeProperties = new ChartShapeProperties();
            Outline outline = new Outline();
            SolidFill solidFill = new SolidFill();
            SchemeColor schemeColor = new SchemeColor { Val = SchemeColorValues.Text1 };
            LuminanceModulation luminanceModulation = new LuminanceModulation { Val = 15000 };
            LuminanceOffset luminanceOffset = new LuminanceOffset { Val = 85000 };
            schemeColor.Append(luminanceModulation);
            schemeColor.Append(luminanceOffset);
            solidFill.Append(schemeColor);
            outline.Append(solidFill);
            chartShapeProperties.Append(outline);
            MajorGridlines majorGridlines = new MajorGridlines
            {
                ChartShapeProperties = chartShapeProperties
            };
            return majorGridlines;
        }

        public static ScatterChartSeries GenerateScatterChartSeries(uint index)
        {
            return GenerateScatterChartSeries(index, null, MarkerStyleValues.Diamond);
        }

        public static ScatterChartSeries GenerateScatterChartSeries(uint index, string rgb, MarkerStyleValues markerStyleValue)
        {
            ScatterChartSeries series = new ScatterChartSeries
            {
                Index = new Index { Val = index },
                Order = new Order { Val = index }
            };

            ChartShapeProperties chartShapeProperties = new ChartShapeProperties();
            Outline outline = new Outline();
            if (String.IsNullOrEmpty(rgb))
            {
                outline.Append(new NoFill());
            }
            else
            {
                SolidFill solidFill = new SolidFill();
                RgbColorModelHex rgbColorModelHex = new RgbColorModelHex { Val = rgb };
                solidFill.Append(rgbColorModelHex);
                outline.Append(solidFill);
            }
            chartShapeProperties.Append(outline);
            series.Append(chartShapeProperties);

            Marker marker = new Marker();
            Symbol symbol = new Symbol { Val = markerStyleValue };
            Size size = new Size { Val = 8 };
            marker.Append(symbol);
            marker.Append(size);
            if (!String.IsNullOrEmpty(rgb))
            {
                ChartShapeProperties markerChartShapeProperties = new ChartShapeProperties();
                SolidFill solidFill = new SolidFill();
                RgbColorModelHex rgbColorModelHex = new RgbColorModelHex { Val = rgb };
                solidFill.Append(rgbColorModelHex);
                markerChartShapeProperties.Append(solidFill);
                marker.Append(markerChartShapeProperties);
            }
            series.Append(marker);

            return series;
        }

        public static XValues GenerateScatterXValuesWithStringReferences(string worksheetName, int startingColumn, int endingColumn, int startingRow, int endingRow, IEnumerable<SpreadsheetCell> cells)
        {
            string startingColumnLetter = CellUtilities.ConvertIntToColumnId(startingColumn);
            string endingColumnLetter = CellUtilities.ConvertIntToColumnId(endingColumn);
            XValues xValues = new XValues
            {
                StringReference = new StringReference
                {
                    Formula = new Formula
                    {
                        Text = string.Format("'{0}'!${1}${2}:${3}${4}",
                                             worksheetName,
                                             startingColumnLetter,
                                             startingRow,
                                             endingColumnLetter,
                                             endingRow)
                    },
                    StringCache = new StringCache()
                }
            };

            int i = 0;
            foreach (SpreadsheetCell cell in cells)
            {
                xValues.StringReference.StringCache.Append(new StringPoint
                {
                    Index = (uint)i++,
                    NumericValue = new NumericValue(cell.Data)
                });
            }
            xValues.StringReference.StringCache.PointCount = new PointCount { Val = (uint)(endingRow - startingRow + 1) };

            return xValues;
        }

        public static XValues GenerateScatterXValuesWithNumberReferences(string worksheetName, int startingColumn, int endingColumn, int startingRow, int endingRow, IEnumerable<SpreadsheetCell> cells)
        {
            string startingColumnLetter = CellUtilities.ConvertIntToColumnId(startingColumn);
            string endingColumnLetter = CellUtilities.ConvertIntToColumnId(endingColumn);
            XValues xValues = new XValues
            {
                NumberReference = new NumberReference
                {
                    Formula = new Formula
                    {
                        Text = string.Format("'{0}'!${1}${2}:${3}${4}",
                                             worksheetName,
                                             startingColumnLetter,
                                             startingRow,
                                             endingColumnLetter,
                                             endingRow)
                    },
                    NumberingCache = new NumberingCache()
                }
            };

            int i = 0;
            foreach (SpreadsheetCell cell in cells)
            {
                xValues.NumberReference.NumberingCache.Append(new NumericPoint
                {
                    Index = (uint)i++,
                    NumericValue = new NumericValue(cell.Data)
                });
            }
            xValues.NumberReference.NumberingCache.PointCount = new PointCount { Val = (uint)(endingRow - startingRow + 1) };

            return xValues;
        }

        public static YValues GenerateScatterYValuesWithNumberReferences(string worksheetName, int startingColumn, int endingColumn, int startingRow, int endingRow, IEnumerable<SpreadsheetCell> cells)
        {
            string startingColumnLetter = CellUtilities.ConvertIntToColumnId(startingColumn);
            string endingColumnLetter = CellUtilities.ConvertIntToColumnId(endingColumn);
            YValues yValues = new YValues
            {
                NumberReference = new NumberReference
                {
                    Formula = new Formula
                    {
                        Text = string.Format("'{0}'!${1}${2}:${3}${4}",
                                             worksheetName,
                                             startingColumnLetter,
                                             startingRow,
                                             endingColumnLetter,
                                             endingRow)
                    },
                    NumberingCache = new NumberingCache()
                }
            };

            int i = 0;
            foreach (SpreadsheetCell cell in cells)
            {
                yValues.NumberReference.NumberingCache.Append(new NumericPoint
                {
                    Index = (uint)i++,
                    NumericValue = new NumericValue(cell.Data)
                });
            }
            yValues.NumberReference.NumberingCache.PointCount = new PointCount { Val = (uint)(endingRow - startingRow + 1) };

            return yValues;
        }
    }
}