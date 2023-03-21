using DataVisualizer.Graph.Net.Enums;
using DataVisualizer.Graph.Net.Interfaces;
using DataVisualizer.Graph.Net.Models;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataVisualizer.Graph.Net
{
    /// <summary>
    /// CandlestickGraph.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CandlestickGraph : UserControl, IGraph
    {
        public List<Quote> Quotes { get; set; } = new();
        public List<Signal> Signals = new();

        private Pen bullPen = new(new SolidColorBrush(Color.FromRgb(59, 207, 134)), 1.0);
        private Pen bearPen = new(new SolidColorBrush(Color.FromRgb(237, 49, 97)), 1.0);
        private Brush bullBrush = new SolidColorBrush(Color.FromRgb(59, 207, 134));
        private Brush bearBrush = new SolidColorBrush(Color.FromRgb(237, 49, 97));

        public int candleMargin { get; set; } = 1;
        public int Start { get; set; } = 0;
        public int End { get; set; } = 0;
        public int ViewCountMin { get; set; } = 10;
        public int ViewCountMax { get; set; } = 1000;
        public int ViewCount => End - Start;
        public int TotalCount => Quotes.Count;
        public Point CurrentMousePosition { get; set; }

        public CandlestickGraph()
        {
            InitializeComponent();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if(Quotes.Count == 0)
            {
                return;
            }

            var itemWidth = ActualWidth / (ViewCount - 1);
            var max = Quotes.Skip(Start).Take(ViewCount).Max(x => x.High);
            var min = Quotes.Skip(Start).Take(ViewCount).Min(x => x.Low);

            for (int i = Start; i < End - 1; i++)
            {
                var quote = Quotes[i];
                var viewIndex = i - Start;

                // Draw Candlestick
                drawingContext.DrawLine(
                    quote.Open < quote.Close ? bullPen : bearPen,
                    new Point(itemWidth * (viewIndex + 0.5), ActualHeight * (double)(1.0m - (quote.High - min) / (max - min))),
                    new Point(itemWidth * (viewIndex + 0.5), ActualHeight * (double)(1.0m - (quote.Low - min) / (max - min))));
                drawingContext.DrawRectangle(
                    quote.Open < quote.Close ? bullBrush : bearBrush,
                    quote.Open < quote.Close ? bullPen : bearPen,
                    new Rect(
                    new Point(itemWidth * viewIndex + candleMargin, ActualHeight * (double)(1.0m - (quote.Open - min) / (max - min))),
                    new Point(itemWidth * (viewIndex + 1) - candleMargin, ActualHeight * (double)(1.0m - (quote.Close - min) / (max - min)))
                    ));

                // Draw Buy/Sell Signal Arrow
                var signal = Signals.Find(x => x.Time.Equals(quote.Time));
                if (signal != null)
                {
                    if (signal.OrderType == OrderType.Buy)
                    {
                        drawingContext.DrawLine(bullPen,
                    new Point(itemWidth * (viewIndex + 0.5), ActualHeight * (double)(1.0m - (quote.Low - min) / (max - min)) + 24),
                    new Point(itemWidth * (viewIndex + 0.5), ActualHeight * (double)(1.0m - (quote.Low - min) / (max - min)) + 10));
                        drawingContext.DrawLine(bullPen,
                    new Point(itemWidth * (viewIndex + 0.25), ActualHeight * (double)(1.0m - (quote.Low - min) / (max - min)) + 15),
                    new Point(itemWidth * (viewIndex + 0.5), ActualHeight * (double)(1.0m - (quote.Low - min) / (max - min)) + 10));
                        drawingContext.DrawLine(bullPen,
                    new Point(itemWidth * (viewIndex + 0.75), ActualHeight * (double)(1.0m - (quote.Low - min) / (max - min)) + 15),
                    new Point(itemWidth * (viewIndex + 0.5), ActualHeight * (double)(1.0m - (quote.Low - min) / (max - min)) + 10));
                    }
                    else
                    {
                        drawingContext.DrawLine(bearPen,
                    new Point(itemWidth * (viewIndex + 0.5), ActualHeight * (double)(1.0m - (quote.High - min) / (max - min)) - 24),
                    new Point(itemWidth * (viewIndex + 0.5), ActualHeight * (double)(1.0m - (quote.High - min) / (max - min)) - 10));
                        drawingContext.DrawLine(bearPen,
                    new Point(itemWidth * (viewIndex + 0.25), ActualHeight * (double)(1.0m - (quote.High - min) / (max - min)) - 15),
                    new Point(itemWidth * (viewIndex + 0.5), ActualHeight * (double)(1.0m - (quote.High - min) / (max - min)) - 10));
                        drawingContext.DrawLine(bearPen,
                    new Point(itemWidth * (viewIndex + 0.75), ActualHeight * (double)(1.0m - (quote.High - min) / (max - min)) - 15),
                    new Point(itemWidth * (viewIndex + 0.5), ActualHeight * (double)(1.0m - (quote.High - min) / (max - min)) - 10));
                    }
                }
            }
        }

        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scaleUnit = Math.Max(1, ViewCount * Math.Abs(e.Delta) / 2000);
            if (e.Delta > 0) // Zoom-in
            {
                if (ViewCount <= ViewCountMin)
                {
                    return;
                }

                Start = Math.Min(TotalCount - ViewCountMin, Start + scaleUnit);
                End = Math.Max(ViewCountMin, End - scaleUnit);
            }
            else // Zoom-out
            {
                if (ViewCount >= ViewCountMax)
                {
                    return;
                }

                Start = Math.Max(0, Start - scaleUnit);
                End = Math.Min(TotalCount, End + scaleUnit);
            }

            InvalidateVisual();
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            Vector diff = e.GetPosition(Parent as Window) - CurrentMousePosition;
            if (IsMouseCaptured)
            {
                var moveUnit = (int)(diff.X * ViewCount / 18000);
                if (diff.X > 0) // Graph Move Left
                {
                    if (Start > moveUnit)
                    {
                        Start -= moveUnit;
                        End -= moveUnit;
                        InvalidateVisual();
                    }
                }
                else if (diff.X < 0) // Graph Move Right
                {
                    if (End < TotalCount + moveUnit)
                    {
                        Start -= moveUnit;
                        End -= moveUnit;
                        InvalidateVisual();
                    }
                }
            }
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CurrentMousePosition = e.GetPosition(Parent as Window);
            CaptureMouse();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
        }
    }
}
