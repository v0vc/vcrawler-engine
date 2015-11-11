// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Interfaces.Models;
using Models.Factories;

namespace Models.BO
{
    public sealed class Tag : ITag, INotifyPropertyChanged
    {
        #region Static and Readonly Fields

        private readonly TagFactory tf;

        #endregion

        #region Fields

        private bool isChecked;

        #endregion

        #region Constructors

        public Tag(TagFactory tf)
        {
            this.tf = tf;
        }

        private Tag()
        {
        }

        #endregion

        #region Methods

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ITag Members

        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
                OnPropertyChanged();
            }
        }

        public string Title { get; set; }

        public async Task DeleteTagAsync()
        {
            // return ServiceLocator.TagFactory.DeleteTagAsync(Title);
            await tf.DeleteTagAsync(Title);
        }

        public async Task InsertTagAsync()
        {
            await tf.InsertTagAsync(this);
        }

        #endregion
    }
}
