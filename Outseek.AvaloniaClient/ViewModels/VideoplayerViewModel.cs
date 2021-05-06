using System;
using System.Reactive;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using Outseek.AvaloniaClient.SharedViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class VideoplayerViewModel : ViewModelBase
    {
        private readonly LibVLC _libVlc;
        private MediaPlayer? _mediaPlayer;
        
        public TimelineState TimelineState { get; }

        [Reactive] public ReactiveCommand<Unit, Unit> PlayOrPause { get; set; }

        public VideoplayerViewModel() : this(new TimelineState())
        {
            // the default constructor is only used by the designer
        }

        public VideoplayerViewModel(TimelineState timelineState)
        {
            Core.Initialize();
            _libVlc = new LibVLC();
            TimelineState = timelineState;
            PlayOrPause = ReactiveCommand.Create(() =>
            {
                if (_mediaPlayer == null) return;
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.SetPause(true);
                }
                else if (_mediaPlayer.Media != null && !_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.SetPause(false);
                }
                else
                {
                    Uri mediaUri = new("/path/to/file");
                    Media media = new(_libVlc, mediaUri);
                    _mediaPlayer.Play(media);
                }
            });
        }

        public void InitPlayer(VideoView videoView)
        {
            _mediaPlayer = new MediaPlayer(_libVlc);
            _mediaPlayer.LengthChanged += (sender, args) =>
            {
                double durationSeconds = args.Length / 1000d;
                TimelineState.End = durationSeconds;
            };
            bool expectTimeChange = false;
            _mediaPlayer.TimeChanged += (sender, args) =>
            {
                expectTimeChange = true;
                double positionSeconds = args.Time / 1000d;
                TimelineState.Position = positionSeconds;
            };
            TimelineState.WhenAnyValue(t => t.Position)
                .Subscribe(position =>
                {
                    double ms = position * 1000;
                    if (expectTimeChange)
                    {
                        expectTimeChange = false;
                        return;
                    }
                    _mediaPlayer.Time = (int) ms;
                });

            videoView.MediaPlayer = _mediaPlayer;
        }
    }
}
