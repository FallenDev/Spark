﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Spark.Common;
using Spark.Dialogs;

namespace Spark.ViewModels;

public abstract class ViewModelBase : Observable, IDisposable
{
    private bool isDisposed;
    private string displayName;
    private bool throwOnInvalidPropertyName;
    private IDialogService dialogService;

    #region Properties
    public virtual string DisplayName
    {
        get { return displayName; }
        set { SetProperty(ref displayName, value); }
    }

    public virtual bool ThrowOnInvalidPropertyName
    {
        get { return throwOnInvalidPropertyName; }
        set { SetProperty(ref throwOnInvalidPropertyName, value); }
    }

    public virtual IDialogService DialogService
    {
        get { return dialogService; }
        set { SetProperty(ref dialogService, value); }
    }
    #endregion

    protected ViewModelBase(string displayName = null, IDialogService dialogService = null)
    {
        DisplayName = displayName;
        DialogService = dialogService;
    }

    #region IDisposable Methods
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool isDisposing)
    {
        if (isDisposed)
            return;

        if (isDisposing)
        {
            // Dispose of managed resources here
        }

        isDisposed = true;
    }
    #endregion

    #region INotifyPropertyChanged Overrides
    protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        VerifyPropertyName(propertyName);
        base.OnPropertyChanged(propertyName);
    }
    #endregion

    #region INotifyPropertyChanging Overrides
    protected override void OnPropertyChanging([CallerMemberName] string propertyName = "")
    {
        VerifyPropertyName(propertyName);
        base.OnPropertyChanging(propertyName);
    }
    #endregion

    [Conditional("DEBUG")]
    [DebuggerStepThrough]
    public void VerifyPropertyName(string propertyName)
    {
        if (propertyName == null)
            throw new ArgumentNullException("propertyName");

        // Check if the property exists on this object
        if (TypeDescriptor.GetProperties(this)[propertyName] == null)
        {
            var message = string.Format("Invalid property name: {0}", propertyName);

            if (ThrowOnInvalidPropertyName)
                throw new Exception(message);
            else
                Debug.Fail(message);
        }
    }
}