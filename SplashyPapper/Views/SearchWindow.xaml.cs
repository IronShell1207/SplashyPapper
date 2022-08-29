using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using SplashyPapper.ViewModels;

namespace SplashyPapper.Views;

public sealed partial class SearchWindow : Page
{
    private MainViewModel ViewModel
    {
        get; set;
    }

    public SearchWindow()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter != null)
            ViewModel = e.Parameter as MainViewModel;
        base.OnNavigatedTo(e);
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        this.Visibility = Visibility.Collapsed;
    }
}