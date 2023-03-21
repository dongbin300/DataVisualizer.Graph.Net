using System.Collections.Generic;
using System.Windows;

namespace DataVisualizer.Graph.Net.Interfaces
{
    public interface IGraph
    {
        int Start { get; set; }
        int End { get; set; }
        int ViewCount => End - Start;
        int ViewCountMin { get; set; }
        int ViewCountMax { get; set; }
        int TotalCount { get; }
        Point CurrentMousePosition { get; set; }
    }
}
