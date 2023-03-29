using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MongoDB.Driver.Core.Operations;

namespace MongolianBarbecue.Tests;

public class BetterStopwatch
{
    readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    readonly List<Lap> _laps = new List<Lap>();

    public void RecordLap(string label, decimal? countOrNull = null)
    {
        _laps.Add(new Lap(_stopwatch.Elapsed, label, countOrNull));
        _stopwatch.Restart();
    }

    public IEnumerable<Lap> Laps => _laps.ToList();

    public class Lap
    {
        readonly decimal? _countOrNull;

        public TimeSpan Duration { get; }

        public string Label { get; }

        public string Rate => _countOrNull == null || _countOrNull.Value == 0
            ? ""
            : $"{(double) _countOrNull.Value / Duration.TotalSeconds:0.00} /s";

        public Lap(TimeSpan duration, string label, decimal? countOrNull)
        {
            _countOrNull = countOrNull;
            Duration = duration;
            Label = label;
        }
    }
}