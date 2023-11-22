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
    public string Title
    {
        get => title;
        set => SetProperty(ref title, value);
    }

    public string Message
    {
        get => message;
        set => SetProperty(ref message, value);
    }

    public string MessageHint
    {
        get => messageHint;
        set => SetProperty(ref messageHint, value);
    }

    public string PositiveButtonTitle
    {
        get => positiveButtonTitle;
        set => SetProperty(ref positiveButtonTitle, value);
    }

    public string NegativeButtonTitle
    {
        get => negativeButtonTitle;
        set => SetProperty(ref negativeButtonTitle, value);
    }

    public bool IsPositiveButtonVisible
    {
        get => isPositiveButtonVisible;
        set => SetProperty(ref isPositiveButtonVisible, value);
    }

    public bool IsNegativeButtonVisible
    {
        get => isNegativeButtonVisible;
        set => SetProperty(ref isNegativeButtonVisible, value);
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

        if (buttons is DialogButtons.OK or DialogButtons.OKCancel)
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

    private void OnPositiveButtonClicked()
    {
        var handler = PositiveButtonClicked;

        handler?.Invoke(this, EventArgs.Empty);
    }

    private void OnNegativeButtonClicked()
    {
        var handler = NegativeButtonClicked;

        handler?.Invoke(this, EventArgs.Empty);
    }
}