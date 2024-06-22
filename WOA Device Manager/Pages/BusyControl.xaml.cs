using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WOADeviceManager.Pages
{
    public sealed partial class BusyControl : UserControl
    {
        public BusyControl()
        {
            InitializeComponent();
        }

        public void SetStatus(string? Message = null, uint? Percentage = null, string? Text = null, string? SubMessage = null, string? Title = null, string? SubTitle = null, string? Emoji = null)
        {
            _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                if (Message == null && Percentage == null && Text == null && SubMessage == null && Title == null && SubTitle == null && Emoji == null)
                {
                    // Hide
                    return;
                }

                if (Emoji != null)
                {
                    ProgressEmoji.Text = Emoji;
                    ProgressEmoji.Visibility = Visibility.Visible;
                }
                else
                {
                    ProgressEmoji.Visibility = Visibility.Collapsed;
                }

                if (Title != null)
                {
                    ProgressTitle.Text = Title;
                    ProgressTitle.Visibility = Visibility.Visible;
                }
                else
                {
                    ProgressTitle.Visibility = Visibility.Collapsed;
                }

                if (SubTitle != null)
                {
                    ProgressSubTitle.Text = SubTitle;
                    ProgressSubTitle.Visibility = Visibility.Visible;
                }
                else
                {
                    ProgressSubTitle.Visibility = Visibility.Collapsed;
                }

                if (Message != null)
                {
                    ProgressMessage.Text = Message;
                    ProgressMessage.Visibility = Visibility.Visible;
                }
                else
                {
                    ProgressMessage.Visibility = Visibility.Collapsed;
                }

                if (Percentage != null)
                {
                    LoadingRing.Visibility = Visibility.Collapsed;

                    ProgressPercentageBar.Maximum = 100;
                    ProgressPercentageBar.Minimum = 0;
                    ProgressPercentageBar.Value = (int)Percentage;

                    ProgressPercentageBar.Visibility = Visibility.Visible;
                }
                else
                {
                    ProgressPercentageBar.Visibility = Visibility.Collapsed;
                    LoadingRing.Visibility = Visibility.Visible;
                }

                if (Text != null)
                {
                    ProgressText.Text = Text;
                    ProgressText.Visibility = Visibility.Visible;
                }
                else
                {
                    ProgressText.Visibility = Visibility.Collapsed;
                }

                if (SubMessage != null)
                {
                    ProgressSubMessage.Text = SubMessage;
                    ProgressSubMessage.Visibility = Visibility.Visible;
                }
                else
                {
                    ProgressSubMessage.Visibility = Visibility.Collapsed;
                }
            });
        }
    }
}
