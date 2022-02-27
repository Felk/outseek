using System.Linq;
using System.Text.RegularExpressions;
using Outseek.API;

namespace Outseek.Backend.Processors;

public class FilterReactionsParams : TimelineProcessorParams
{
}

public class FilterReactions : ITimelineProcessor<TimelineObject.TimedText, TimelineObject.TimedText, FilterReactionsParams>
{
    public string Name => "filter reactions";

    private readonly Regex _reactionRegex = new(string.Join('|', new[]
    {
        @"\b(c|k)li+p(p?ed)?\b", // clip it, clip that, clipped, etc.
        @"\bti+ming\b",          // that timing, perfect timing, etc.
        @"lulw?\b",              // LUL (the emote) and variations ending in lul (e.g. lulw, omegalul, custom emotes)
        @"\blo+l[ol]*\b",        // lol and variations with more o's and l's
        @"\bl+m+f*a+o+\b",       // lmao, lmfao, lmaooooo, etc.
        @"\bh+[ae][hae]+\b",     // haha, hehe, hhaaaahaaa, etc.
        @"\bhi youtube\b",       // hi youtube
        @"\bx+d[xd]+\b",         // xD, XDDXDDXDD etc.
        @"pepelaugh",
        @"pog(champ)?u?\b",      // PogChamp, Pog, PogU
    }), RegexOptions.IgnoreCase);

    public TimelineObject.TimedText Process(ITimelineProcessContext context, TimelineObject.TimedText input, FilterReactionsParams parameters) =>
        new(() => input.Entries().Where(t => _reactionRegex.IsMatch(t.Text)));
}
