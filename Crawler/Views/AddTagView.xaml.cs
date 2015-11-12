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
    ///     Interaction logic for AddTagView.xaml
    /// </summary>
    public partial class AddTagView : Window
    {
        #region Constructors

        public AddTagView()
        {
            InitializeComponent();
            KeyDown += AddTagKeyDown;
        }

        #endregion

        #region Event Handling

        private void AddTagKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                KeyDown -= AddTagKeyDown;
                Close();
            }
            if (e.Key == Key.Enter)
            {
                KeyDown -= AddTagKeyDown;
                // нажмем кнопку программно
                var peer = new ButtonAutomationPeer(buttonOk);
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
