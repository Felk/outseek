using System.Collections.Generic;
using Python.Runtime;

namespace Outseek.AvaloniaClient.Utils;

public class OpenTimelineIO
{
    private readonly dynamic _otio;
    public OpenTimelineIO(dynamic otio) => _otio = otio;

    public void SaveSegments(string mediaReference, string targetFilename, IEnumerable<Range> segments)
    {
        const int rate = 30; // TODO get from media

        using (Py.GIL())
        {
            dynamic tl = _otio.schema.Timeline(name: "Example timeline");
            dynamic tr = _otio.schema.Track(name: "Example track");
            tl.tracks.append(tr);

            dynamic range = _otio.opentime.TimeRange(
                start_time: _otio.opentime.RationalTime(0, 24),
                duration: _otio.opentime.RationalTime(100, 24)
            );

            dynamic media_reference = _otio.schema.ExternalReference(
                target_url: mediaReference,
                available_range: range
            );

            int num = 1;
            foreach (Range segment in segments)
            {
                string name = $"Clip{num++}";
                dynamic cl = _otio.schema.Clip(
                    name: name,
                    media_reference: media_reference,
                    source_range: _otio.opentime.TimeRange(
                        start_time: _otio.opentime.RationalTime(
                            segment.From,
                            rate
                        ),
                        duration: _otio.opentime.RationalTime(
                            (segment.To - segment.From),
                            rate
                        )
                    )
                );

                tr.append(cl);
            }

            _otio.adapters.write_to_file(tl, targetFilename);
        }
    }

}
