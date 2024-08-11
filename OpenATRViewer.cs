#region Using declarations
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;

namespace NinjaTrader.NinjaScript.Indicators
{
    public class ATRInfo
    {
        public string Name { get; set; }
        public double ATR { get; set; }
    }

    public class OpenATRViewer : Indicator
    {
        public const string GROUP_NAME_GENERAL = "General";
        public const string GROUP_NAME = "Open Auto ATR Viewer";

        private List<ATRInfo> _atrInfos;
        private Brush _atrBlockBackgroundColor;
        private Brush _atrBlockTextColor;

        #region General Properties

        [NinjaScriptProperty]
        [Display(Name = "Version", Description = "Open Auto ATR Viewer version.", Order = 0, GroupName = GROUP_NAME_GENERAL)]
        [ReadOnly(true)]
        public string Version
        {
            get { return "1.0.0"; }
            set { }
        }

        #endregion

        [NinjaScriptProperty]
        [Display(Name = "ATR", Description = "The ATR period.", Order = 0, GroupName = GROUP_NAME)]
        public int ATRPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "TotalATRView", Description = "The total ATR you want to view. This is use for dynamically calculating the block height.", Order = 1, GroupName = GROUP_NAME)]
        public int TotalATRView { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "ATR Block Top Offset", Description = "The offset for the ATR block from the top.", Order = 2, GroupName = GROUP_NAME)]
        public float ATRBlockTopOffset { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "ATR Block Left Offset", Description = "The offset for the ATR block from the left.", Order = 3, GroupName = GROUP_NAME)]
        public float ATRBlockLeftOffset { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "ATR Block Background Color", Description = "The background color for the ATR block.", Order = 4, GroupName = GROUP_NAME)]
        public Brush ATRBlockBackgroundColor
        {
            get { return _atrBlockBackgroundColor; }
            set { _atrBlockBackgroundColor = value; }
        }

        [Browsable(false)]
        public string ATRBlockBackgroundColorSerialize
        {
            get { return Serialize.BrushToString(_atrBlockBackgroundColor); }
            set { _atrBlockBackgroundColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "ATR Block Text Color", Description = "The text color for the ATR block.", Order = 5, GroupName = GROUP_NAME)]
        public Brush ATRBlockTextColor
        {
            get { return _atrBlockTextColor; }
            set { _atrBlockTextColor = value; }
        }

        [Browsable(false)]
        public string ATRBlockTextColorSerialize
        {
            get { return Serialize.BrushToString(_atrBlockTextColor); }
            set { _atrBlockTextColor = Serialize.StringToBrush(value); }
        }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"View the ATR for various data series.";
                Name = "OpenATRViewer";
                Calculate = Calculate.OnEachTick;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;

                ATRPeriod = 14;
                // This should equal the amount of data series in the configure section
                TotalATRView = 5;
                ATRBlockTopOffset = 10;
                ATRBlockLeftOffset = 10;
                ATRBlockBackgroundColor = Brushes.Black;
                ATRBlockTextColor = Brushes.White;
            }
            else if (State == State.Configure)
            {
                // The first added data series should be the lowest
                AddDataSeries(Data.BarsPeriodType.Tick, 200);
                AddDataSeries(Data.BarsPeriodType.Tick, 500);
                AddDataSeries(Data.BarsPeriodType.Tick, 1000);
                AddDataSeries(Data.BarsPeriodType.Second, 30);
                AddDataSeries(Data.BarsPeriodType.Minute, 1);
            }
            else if (State == State.DataLoaded)
            {
                _atrInfos = new List<ATRInfo>();
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBars[0] < ATRPeriod)
            {
                return;
            }

            /*if (!_initialATRInfosAdded)
            {
                // Don't add the data series for the current chart
                for (int i = 1; i <= TotalATRView; i++)
                {
                    _atrInfos.Add(
                        new ATRInfo()
                        {
                            Index = i,
                            Name = BarsArray[i].BarsPeriod.BarsPeriodType + " " + BarsArray[i].BarsPeriod.Value,
                            ATR = Math.Round(ATR(BarsArray[i], ATRPeriod)[0], 2)
                        }
                    );

                    if (i == TotalATRView + 1)
                    {
                        _initialATRInfosAdded = true;
                    }
                }
            }*/

            if (BarsInProgress == 1 && IsFirstTickOfBar)
            {
                _atrInfos = new List<ATRInfo>();

                for (int i = 1; i <= TotalATRView; i++)
                {
                    _atrInfos.Add(
                        new ATRInfo()
                        {
                            Name = BarsArray[i].BarsPeriod.BarsPeriodType + "  " + BarsArray[i].BarsPeriod.Value,
                            ATR = Math.Round(ATR(BarsArray[i], ATRPeriod)[0], 2)
                        }
                    );
                }
            }
        }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            if (CurrentBars[0] < ATRPeriod)
            {
                return;
            }

            DrawATRBlock(chartControl, chartScale);
        }

        private SharpDX.Color ConvertToDxColor(Brush brush, byte alpha)
        {
            var color = ((SolidColorBrush)brush).Color;
            return new SharpDX.Color(color.R, color.G, color.B, alpha);
        }

        private void DrawATRBlock(ChartControl chartControl, ChartScale chartScale)
        {
            float x = ATRBlockLeftOffset;
            float y = ATRBlockTopOffset;
            float width = 135;
            float height = TotalATRView * 17;
            float padding = 5;

            var backgroundBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ConvertToDxColor(ATRBlockBackgroundColor, 255));
            var rect = new SharpDX.RectangleF(x, y, width, height);

            RenderTarget.FillRectangle(rect, backgroundBrush);

            var textFormat = new SharpDX.DirectWrite.TextFormat(new SharpDX.DirectWrite.Factory(), "Arial", SharpDX.DirectWrite.FontWeight.Normal, SharpDX.DirectWrite.FontStyle.Normal, 12)
            {
                TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading,
                ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Near
            };

            var textBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ConvertToDxColor(ATRBlockTextColor, 255));

            // Construct the text string by looping through _atrInfos
            var textBuilder = new System.Text.StringBuilder();
            foreach (var atrInfo in _atrInfos)
            {
                textBuilder.AppendLine($"{atrInfo.Name}:   {atrInfo.ATR}");
            }
            var text = textBuilder.ToString();

            var textLayout = new SharpDX.DirectWrite.TextLayout(new SharpDX.DirectWrite.Factory(), text, textFormat, width - 2 * padding, height - 2 * padding);

            RenderTarget.DrawTextLayout(new SharpDX.Vector2(x + padding, y + padding), textLayout, textBrush);

            backgroundBrush.Dispose();
            textBrush.Dispose();
            textFormat.Dispose();
            textLayout.Dispose();
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OpenATRViewer[] cacheOpenATRViewer;
		public OpenATRViewer OpenATRViewer(string version, int aTRPeriod, int totalATRView, float aTRBlockTopOffset, float aTRBlockLeftOffset, Brush aTRBlockBackgroundColor, Brush aTRBlockTextColor)
		{
			return OpenATRViewer(Input, version, aTRPeriod, totalATRView, aTRBlockTopOffset, aTRBlockLeftOffset, aTRBlockBackgroundColor, aTRBlockTextColor);
		}

		public OpenATRViewer OpenATRViewer(ISeries<double> input, string version, int aTRPeriod, int totalATRView, float aTRBlockTopOffset, float aTRBlockLeftOffset, Brush aTRBlockBackgroundColor, Brush aTRBlockTextColor)
		{
			if (cacheOpenATRViewer != null)
				for (int idx = 0; idx < cacheOpenATRViewer.Length; idx++)
					if (cacheOpenATRViewer[idx] != null && cacheOpenATRViewer[idx].Version == version && cacheOpenATRViewer[idx].ATRPeriod == aTRPeriod && cacheOpenATRViewer[idx].TotalATRView == totalATRView && cacheOpenATRViewer[idx].ATRBlockTopOffset == aTRBlockTopOffset && cacheOpenATRViewer[idx].ATRBlockLeftOffset == aTRBlockLeftOffset && cacheOpenATRViewer[idx].ATRBlockBackgroundColor == aTRBlockBackgroundColor && cacheOpenATRViewer[idx].ATRBlockTextColor == aTRBlockTextColor && cacheOpenATRViewer[idx].EqualsInput(input))
						return cacheOpenATRViewer[idx];
			return CacheIndicator<OpenATRViewer>(new OpenATRViewer(){ Version = version, ATRPeriod = aTRPeriod, TotalATRView = totalATRView, ATRBlockTopOffset = aTRBlockTopOffset, ATRBlockLeftOffset = aTRBlockLeftOffset, ATRBlockBackgroundColor = aTRBlockBackgroundColor, ATRBlockTextColor = aTRBlockTextColor }, input, ref cacheOpenATRViewer);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OpenATRViewer OpenATRViewer(string version, int aTRPeriod, int totalATRView, float aTRBlockTopOffset, float aTRBlockLeftOffset, Brush aTRBlockBackgroundColor, Brush aTRBlockTextColor)
		{
			return indicator.OpenATRViewer(Input, version, aTRPeriod, totalATRView, aTRBlockTopOffset, aTRBlockLeftOffset, aTRBlockBackgroundColor, aTRBlockTextColor);
		}

		public Indicators.OpenATRViewer OpenATRViewer(ISeries<double> input , string version, int aTRPeriod, int totalATRView, float aTRBlockTopOffset, float aTRBlockLeftOffset, Brush aTRBlockBackgroundColor, Brush aTRBlockTextColor)
		{
			return indicator.OpenATRViewer(input, version, aTRPeriod, totalATRView, aTRBlockTopOffset, aTRBlockLeftOffset, aTRBlockBackgroundColor, aTRBlockTextColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OpenATRViewer OpenATRViewer(string version, int aTRPeriod, int totalATRView, float aTRBlockTopOffset, float aTRBlockLeftOffset, Brush aTRBlockBackgroundColor, Brush aTRBlockTextColor)
		{
			return indicator.OpenATRViewer(Input, version, aTRPeriod, totalATRView, aTRBlockTopOffset, aTRBlockLeftOffset, aTRBlockBackgroundColor, aTRBlockTextColor);
		}

		public Indicators.OpenATRViewer OpenATRViewer(ISeries<double> input , string version, int aTRPeriod, int totalATRView, float aTRBlockTopOffset, float aTRBlockLeftOffset, Brush aTRBlockBackgroundColor, Brush aTRBlockTextColor)
		{
			return indicator.OpenATRViewer(input, version, aTRPeriod, totalATRView, aTRBlockTopOffset, aTRBlockLeftOffset, aTRBlockBackgroundColor, aTRBlockTextColor);
		}
	}
}

#endregion
