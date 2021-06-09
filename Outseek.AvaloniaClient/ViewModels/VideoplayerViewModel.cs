using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
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
        public MediaState MediaState { get; }

        private Media? _currentMedia;

        [Reactive] public ReactiveCommand<Unit, Unit> PlayOrPause { get; set; }
        [Reactive] public ReactiveCommand<Unit, Unit> OpenFileDialog { get; set; }

        public VideoplayerViewModel() : this(new TimelineState(), new MediaState())
        {
            // the default constructor is only used by the designer
        }

        private void Play(string? filename)
        {
            _mediaPlayer?.Stop();
            if (filename == null) return;
            Uri mediaUri = new(filename);
            _currentMedia = new Media(_libVlc, mediaUri);
            _mediaPlayer?.Play(_currentMedia);
        }

        public VideoplayerViewModel(TimelineState timelineState, MediaState mediaState)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: Assume a 64-bit VLC player is installed in the default location
                Core.Initialize("C:/Program Files/VideoLAN/VLC");
            }
            else
            {
                // Linux: Requires libvlc to be available, e.g. by installing the 'vlc' and 'libvlc-dev' packages
                Core.Initialize();
            }

            _libVlc = new LibVLC();
            TimelineState = timelineState;
            MediaState = mediaState;
            PlayOrPause = ReactiveCommand.Create(() =>
            {
                if (_mediaPlayer == null) return;
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.SetPause(true);
                }
                else if (_mediaPlayer.Position < _mediaPlayer.Length)
                {
                    _mediaPlayer.SetPause(false);
                }
                else
                {
                    Play(MediaState.Filename);
                    _mediaPlayer.Time = (long) (TimelineState.PlaybackPosition * 1000);
                }
            });
            OpenFileDialog = ReactiveCommand.Create(() =>
            {
                Debug.Assert(Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime);
                Window mainWindow = ((IClassicDesktopStyleApplicationLifetime) Application.Current.ApplicationLifetime)
                    .MainWindow;
                var fileDialog = new OpenFileDialog
                {
                    AllowMultiple = false,
                    Title = "Choose video file",
                };
                fileDialog.ShowAsync(mainWindow).ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        var messagebox = MessageBoxManager.GetMessageBoxStandardWindow(
                            "Could not open file", task.Exception.ToString(), ButtonEnum.Ok, Icon.Error);
                        messagebox.Show();
                    }
                    else
                    {
                        var filename = task.Result[0];
                        MediaState.Filename = filename;
                    }
                });
            });
            MediaState.WhenAnyValue(m => m.Filename).Subscribe(Play);
        }

        public void InitPlayer(VideoView videoView)
        {
            _mediaPlayer = new MediaPlayer(_libVlc);
            _mediaPlayer.LengthChanged += (sender, args) =>
            {
                double durationSeconds = args.Length / 1000d;
                TimelineState.End = durationSeconds;
            };
            // There doesn't seem to be easy control over the sender object for reactive properties,
            // so this bool is used to avoid time change events originating from libvlc to cause a feedback loop. 
            bool isNextTimeChangeFromVlc = false;
            _mediaPlayer.TimeChanged += (_, args) =>
            {
                double positionSeconds = args.Time / 1000d;
                if (!Equals(TimelineState.PlaybackPosition, positionSeconds))
                {
                    isNextTimeChangeFromVlc = true;
                    TimelineState.PlaybackPosition = positionSeconds;
                }
            };
            _mediaPlayer.EndReached += (_, _) =>
            {
                MediaState.IsPlaying = false;
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    _mediaPlayer.Media = _currentMedia;
                    TimelineState.PlaybackPosition = 0;
                });
            };
            _mediaPlayer.Paused += (_, _) => MediaState.IsPlaying = false;
            _mediaPlayer.Stopped += (_, _) => MediaState.IsPlaying = false;
            _mediaPlayer.Playing += (_, _) => MediaState.IsPlaying = true;
            TimelineState.WhenAnyValue(t => t.PlaybackPosition)
                .Where(_ =>
                {
                    if (!isNextTimeChangeFromVlc) return true;
                    isNextTimeChangeFromVlc = false;
                    return false;
                })
                // This throttles user input when scrubbing through the timeline
                // (The ReactiveUI Throttle method is actually a de-bouncer, which is not useful here),
                // but actually results in a smoother scrubbing experience because changing the playback position too
                // frequently makes libvlc freeze for short periods of time.
                .Buffer(TimeSpan.FromMilliseconds(200))
                .Subscribe(bufferedPositions =>
                {
                    if (!bufferedPositions.Any()) return;
                    double ms = bufferedPositions.Last() * 1000;
                    _mediaPlayer.Time = (int) ms;
                });

            videoView.MediaPlayer = _mediaPlayer;
        }
    }
}
