﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Spark.Themes.Templates;

internal partial class SparkWindow
{
    #region Drag to Move

    private void TitleBarDragDelta(object sender, DragDeltaEventArgs e)
    {
        var thumb = sender as Thumb;
        var window = thumb.TemplatedParent as Window;

        if (window != null)
        {
            window.Left += e.HorizontalChange;
            window.Top += e.VerticalChange;
        }
    }
    #endregion

    #region Control Box Button Handlers

    private void minimizeButtonClicked(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var window = button.TemplatedParent as Window;

        if (window != null)
            window.WindowState = WindowState.Minimized;     
    }

    private void maximizeButtonClicked(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var window = button.TemplatedParent as Window;

        if (window != null)
        {
            if (window.WindowState != WindowState.Maximized)
                window.WindowState = WindowState.Maximized;
            else
                window.WindowState = WindowState.Normal;
        }
    }

    private void closeButtonClicked(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var window = button.TemplatedParent as Window;

        window?.Close();
    }
    #endregion
}