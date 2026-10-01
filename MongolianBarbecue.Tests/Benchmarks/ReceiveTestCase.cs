using System.Collections.Generic;
using System.Linq;

namespace MongolianBarbecue.Tests.Benchmarks;

public record ReceiveIndexTestCase(int MessageCount, int ConcumerCount, params IReadOnlyList<string> IndexFields)
{
    public override string ToString() => $"MessageCount: {MessageCount}, ConsumerCount: {ConcumerCount}, Index: [{string.Join(", ", IndexFields.Select(f => $"'{f}'"))}]";
}