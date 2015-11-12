// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Input;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for EditTagsView.xaml
    /// </summary>
    public partial class EditTagsView : Window
    {
        #region Constructors

        public EditTagsView()
        {
            InitializeComponent();
            KeyDown += EditTagsViewKeyDown;
        }

        #endregion

        #region Event Handling

        private void EditTagsViewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                KeyDown -= EditTagsViewKeyDown;
                Close();
            }
            if (e.Key == Key.Enter)
            {
                KeyDown -= EditTagsViewKeyDown;
                // нажмем кнопку программно
                var peer = new ButtonAutomationPeer(buttonSave);
                var invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                if (invokeProv != null)
                {
                    invokeProv.Invoke();
                }
            }
        }

        #endregion
    }
}
