using System;
using System.Windows.Input;

using Spark.Dialogs;
using Spark.Input;

namespace Spark.ViewModels;

public enum DialogButtons
{
    OK = 0,
    OKCancel = 1,
    YesNo = 2
}

public class DialogViewModel : WorkspaceViewModel
{
    private string title;
    private string message;
    private string messageHint;
    private string positiveButtonTitle;
    private string negativeButtonTitle;
    private bool isPositiveButtonVisible;
    private bool isNegativeButtonVisible;

    private ICommand positiveButtonCommand;
    private ICommand negativeButtonCommand;

    public event EventHandler PositiveButtonClicked;
    public event EventHandler NegativeButtonClicked;

    #region Properties
    public virtual string Title
    {
        get { return title; }
        set { SetProperty(ref title, value); }
    }

    public virtual string Message
    {
        get { return message; }
        set { SetProperty(ref message, value); }
    }

    public virtual string MessageHint
    {
        get { return messageHint; }
        set { SetProperty(ref messageHint, value); }
    }

    public virtual string PositiveButtonTitle
    {
        get { return positiveButtonTitle; }
        set { SetProperty(ref positiveButtonTitle, value); }
    }

    public virtual string NegativeButtonTitle
    {
        get { return negativeButtonTitle; }
        set { SetProperty(ref negativeButtonTitle, value); }
    }

    public virtual bool IsPositiveButtonVisible
    {
        get { return isPositiveButtonVisible; }
        set { SetProperty(ref isPositiveButtonVisible, value); }
    }

    public virtual bool IsNegativeButtonVisible
    {
        get { return isNegativeButtonVisible; }
        set { SetProperty(ref isNegativeButtonVisible, value); }
    }

    public ICommand PositiveButtonCommand
    {
        get
        {
            // Lazy-initialized
            if (positiveButtonCommand == null)
                positiveButtonCommand = new DelegateCommand(x => OnPositiveButtonClicked());

            return positiveButtonCommand;
        }
    }

    public ICommand NegativeButtonCommand
    {
        get
        {
            // Lazy-initialized
            if (negativeButtonCommand == null)
                negativeButtonCommand = new DelegateCommand(x => OnNegativeButtonClicked());

            return negativeButtonCommand;
        }
    }
    #endregion

    public DialogViewModel(string title, string message, string messageHint = null, DialogButtons buttons = DialogButtons.OK, IDialogService dialogService = null)
        : base(title, dialogService)
    {
        Title = title;
        Message = message;
        MessageHint = messageHint;

        if (buttons == DialogButtons.OK || buttons == DialogButtons.OKCancel)
        {
            PositiveButtonTitle = "_OK";
            NegativeButtonTitle = "_Cancel";
            IsPositiveButtonVisible = true;
            IsNegativeButtonVisible = (buttons == DialogButtons.OKCancel);
        }
        else if (buttons == DialogButtons.YesNo)
        {
            PositiveButtonTitle = "_Yes";
            NegativeButtonTitle = "_No";
            IsPositiveButtonVisible = true;
            IsNegativeButtonVisible = true;
        }
    }

    protected virtual void OnPositiveButtonClicked()
    {
        var handler = PositiveButtonClicked;

        if (handler != null)
            handler(this, EventArgs.Empty);
    }

    protected virtual void OnNegativeButtonClicked()
    {
        var handler = NegativeButtonClicked;

        if (handler != null)
            handler(this, EventArgs.Empty);
    }
}