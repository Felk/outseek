using System;
using System.Diagnostics;
using System.Reactive;
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
            Core.Initialize("C:/Program Files/VideoLAN/VLC");
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
            bool expectTimeChange = false;
            _mediaPlayer.TimeChanged += (sender, args) =>
            {
                expectTimeChange = true;
                double positionSeconds = args.Time / 1000d;
                TimelineState.PlaybackPosition = positionSeconds;
            };
            _mediaPlayer.EndReached += (_, _) =>
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    _mediaPlayer.Media = _currentMedia;
                    TimelineState.PlaybackPosition = 0;
                });
            };
            TimelineState.WhenAnyValue(t => t.PlaybackPosition)
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
