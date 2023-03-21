using DataVisualizer.Graph.Net;
using DataVisualizer.Graph.Net.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace DataVisualizer.Net.Examples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CandlestickGraph graph = new();

        public MainWindow()
        {
            InitializeComponent();

            graph.Quotes = LoadSamples();
            graph.End = graph.Quotes.Count;
            Screen.Content = graph;
        }

        public List<Quote> LoadSamples()
        {
            var result = new List<Quote>();

            for(int i = 1; i <= 1; i++)
            {
                var data = File.ReadAllLines(@$"Samples\BtcusdtQuotes\BTCUSDT_2023-03-{i:00}.csv");
                foreach(var d in data)
                {
                    var e = d.Split(',');
                    var quote = new Quote
                    {
                        Time = DateTime.Parse(e[0]),
                        Open = decimal.Parse(e[1]),
                        High = decimal.Parse(e[2]),
                        Low = decimal.Parse(e[3]),
                        Close = decimal.Parse(e[4]),
                        Volume = decimal.Parse(e[5])
                    };
                    result.Add(quote);
                }
            }

            return result;
        }
    }
}
