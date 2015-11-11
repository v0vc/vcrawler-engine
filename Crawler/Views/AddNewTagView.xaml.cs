// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Input;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for AddNewTagView.xaml
    /// </summary>
    public partial class AddNewTagView : Window
    {
        #region Constructors

        public AddNewTagView()
        {
            InitializeComponent();
            KeyDown += AddNewTagKeyDown;
        }

        #endregion

        #region Event Handling

        private void AddNewTagKeyDown(object sender, KeyEventArgs e)
        {
            KeyDown -= AddNewTagKeyDown;
            if (e.Key == Key.Escape)
            {
                Close();
            }
            if (e.Key == Key.Enter)
            {
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
