﻿using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Spark.Input;

public sealed class DelegateCommand : ICommand
{
    private readonly Action<object> onExecute;
    private readonly Predicate<object> onCanExecute;

    public DelegateCommand(Action<object> onExecute)
        : this(onExecute, null) { }

    public DelegateCommand(Action<object> onExecute, Predicate<object> onCanExecute)
    {
        if (onExecute == null)
            throw new ArgumentNullException("onExecute");

        this.onExecute = onExecute;
        this.onCanExecute = onCanExecute;
    }

    #region ICommand
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public void Execute(object parameter)
    {
        onExecute(parameter);
    }

    [DebuggerStepThrough]
    public bool CanExecute(object parameter)
    {
        return onCanExecute != null ? onCanExecute(parameter) : true;
    }
    #endregion
}