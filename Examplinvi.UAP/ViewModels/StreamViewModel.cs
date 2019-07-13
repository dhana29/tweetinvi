﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Examplinvi.UniversalApp;
using Tweetinvi;

namespace Examplinvi.UAP.ViewModels
{
    public class StreamViewModel : INotifyPropertyChanged
    {
        private ITwitterClient _client;

        /// <summary>
        /// Informational message to the user.
        /// </summary>
        public string Message { get; private set; }

        private string streamingText;

        /// <summary>
        /// Tweets from the streaming API.
        /// </summary>
        public string StreamingText
        {
            get { return this.streamingText; }
            set
            {
                this.streamingText = value;
                NotifyPropertyChanged(nameof(this.StreamingText));
            }
        }

        public StreamViewModel()
        {
            Authenticate();
        }

        private async Task Authenticate()
        {
            TwitterConfig.InitApp(); // Initializing credentials -> Auth.SetUserCredentials

            if (Auth.Credentials == null ||
                string.IsNullOrEmpty(Auth.Credentials.ConsumerKey) ||
                string.IsNullOrEmpty(Auth.Credentials.ConsumerSecret) ||
                string.IsNullOrEmpty(Auth.Credentials.AccessToken) ||
                string.IsNullOrEmpty(Auth.Credentials.AccessTokenSecret) ||
                Auth.Credentials.AccessToken == "ACCESS_TOKEN")
            {
                Message = "Please enter your credentials in the StreamViewModel.cs file";
            }
            else
            {
                _client = new TwitterClient(Auth.Credentials);

                var user = await User.GetAuthenticatedUser();
                Message = $"Hi '{user.Name}'. Welcome on board with Windows 10 Universal App!";                
            }
        }

        public async Task PublishTweet()
        {
            await _client.Tweets.PublishTweet("Check out #tweetinvi, the best c# library!");
        }

        private string _buffer;

        public void RunSampleStream()
        {
            var uiDispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;

            var s = Stream.CreateSampleStream();

            var i = 0;

            s.TweetReceived += async (o, args) =>
            {
                _buffer += $"{args.Tweet.Text}\r\n";

                ++i;

                if (i == 10) // Every 10 tweets so that it makes it easier to handle for raspberry pi.
                {
                    i = 0;

                    await uiDispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        StreamingText = _buffer;
                    });

                    _buffer = string.Empty;
                }
            };

            s.StartStream();
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
