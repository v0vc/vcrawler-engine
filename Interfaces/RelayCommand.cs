// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Windows.Input;

namespace Interfaces
{
    // http://stackoverflow.com/questions/6634777/what-is-the-actual-task-of-canexecutechanged-and-commandmanager-requerysuggested

    /// <summary>
    ///     A command whose sole purpose is to relay its functionality to other
    ///     objects by invoking delegates. The default return value for the
    ///     CanExecute method is 'true'.
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Static and Readonly Fields

        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        #endregion

        #region Constructors

        /// <summary>
        ///     Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<object> execute) : this(execute, null)
        {
        }

        /// <summary>
        ///     Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion

        #region ICommand Members

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameters)
        {
            return _canExecute == null || _canExecute(parameters);
        }

        public void Execute(object parameters)
        {
            _execute(parameters);
        }

        #endregion
    }
}
