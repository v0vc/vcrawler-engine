using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Input;
using Crawler.ViewModels;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for AddNewTagView.xaml
    /// </summary>
    public partial class AddNewTagView : Window
    {
        public AddNewTagView()
        {
            InitializeComponent();
            KeyDown += AddNewTag_KeyDown;
        }

        private void AddNewTag_KeyDown(object sender, KeyEventArgs e)
        {
            KeyDown -= AddNewTag_KeyDown;
            if (e.Key == Key.Escape)
            {
                Close();
            }
            if (e.Key == Key.Enter)
            {
                // нажмем кнопку программно
                var peer = new ButtonAutomationPeer(ButtonOk);
                var invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                if (invokeProv != null)
                {
                    invokeProv.Invoke();
                }
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var mv = DataContext as MainWindowViewModel;
            if (mv == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(mv.Model.NewTag))
            {
                var tf = mv.Model.BaseFactory.CreateTagFactory();
                var tag = tf.CreateTag();
                tag.Title = mv.Model.NewTag;
                if (!mv.Model.Tags.Select(x => x.Title).Contains(tag.Title))
                {
                    mv.Model.Tags.Add(tag);
                    Close();
                }
            }
        }

        private void AddNewTagView_OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBoxTag.Focus();
        }
    }
}
