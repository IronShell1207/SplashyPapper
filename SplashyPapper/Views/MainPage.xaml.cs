using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Animations;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using SplashyPapper.Services;
using SplashyPapper.ViewModels;

namespace SplashyPapper.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        ViewModel.SelectedImageChanged += ViewModel_SelectedImageChanged;

        ViewModel.GalleryAlwaysStateChanged += ViewModel_GalleryAlwaysStateChanged;

        if (!ViewModel.IsAlwaysShowGallery)
            PointerMoved += MainPage_GalleryVisibilityChanger_OnPointerMoved;

        ToastService.ToastRequested += ToastService_ToastRequested;

        PointerMoved += MainPage_PointerMoved;
    }

    private bool IsCursorAtBottom { get; set; } = false;

    private void MainPage_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        IsCursorAtBottom = CheckPositionIsBottom(e);
    }

    private void ViewModel_GalleryAlwaysStateChanged(bool obj)
    {
        if (obj)
        {
            PointerMoved += MainPage_GalleryVisibilityChanger_OnPointerMoved;
        }
        else
        {
            GalleryGrid.Visibility = Visibility.Visible;
            PointerMoved -= MainPage_GalleryVisibilityChanger_OnPointerMoved;
        }
    }

    private async void ToastService_ToastRequested(ToastService obj)
    {
        ToastGrid.Visibility = Visibility.Visible;
        ToastText.Text = obj.Message;

        await Task.Delay(obj.Duration);

        ToastGrid.Visibility = Visibility.Collapsed;
    }

    private void ViewModel_SelectedImageChanged(int obj)
    {
        if (obj > 0)
            Image1Source.Source = new BitmapImage(new Uri(ViewModel.Images[obj].Path));
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Image1Source.Visibility = Visibility.Visible;
        Image2Source.Visibility = Visibility.Collapsed;
        ViewModel.LoadPicture();
    }

    private void Image1Source_OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.IsInContact)
        {
            var x1 = e.GetCurrentPoint(ContentArea);
            newPoint = x1.Position.X - startPos.Position.X;
            ImageMover.TranslateX = newPoint;
            Image2Source.Visibility = Visibility.Visible;

            if (newPoint < -1)
            {
                DirectionChanging = true;
                Image2Mover.TranslateX = Image2Source.DesiredSize.Width + newPoint;
                if (ViewModel.SelectedPictureIndex + 1 < ViewModel.Images.Count)
                    ViewModel.NextImage = ViewModel.Images[ViewModel.SelectedPictureIndex + 1];
            }
            else
            {
                DirectionChanging = false;
                Image2Mover.TranslateX = newPoint - Image2Source.DesiredSize.Width;
            }
        }
    }

    private bool DirectionChanging = false;

    private double newPoint = 0;

    private PointerPoint startPos
    {
        get;
        set;
    }

    private void Image1Source_OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        startPos = e.GetCurrentPoint(ContentArea);
        Image2Source.Visibility = Visibility.Visible;
        Image2Mover.TranslateX = Image2Source.RenderSize.Width;
        newPoint = 0;
    }

    private void Image1Source_OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (Math.Abs(newPoint) > Image1Source.DesiredSize.Width / 3)
        {
            ImageMover.TranslateX = 0;
            Image2Mover.TranslateX = Image2Source.DesiredSize.Width;
            Image2Source.Visibility = Visibility.Collapsed;
            (Image1Source.Source, Image2Source.Source) = (Image2Source.Source, Image1Source.Source);
            if (DirectionChanging && (ViewModel.SelectedPictureIndex + 1 < ViewModel.Images.Count))
                ViewModel.SelectedPictureIndex++;
            else if (ViewModel.SelectedPictureIndex - 1 > -1) ViewModel.SelectedPictureIndex--;
        }
        else
        {
            ImageMover.TranslateX = 0;
            Image2Source.Visibility = Visibility.Collapsed;
        }
    }

    private void SearchButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogsFrame.Visibility = Visibility.Visible;
        DialogsFrame.Navigate(typeof(SearchWindow), ViewModel);
    }

    private bool GalleryShowing { get; set; } = true;

    private async void MainPage_GalleryVisibilityChanger_OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (ViewModel.IsAlwaysShowGallery)
        {
            GalleryGrid.Visibility = Visibility.Visible;
            PointerMoved -= MainPage_GalleryVisibilityChanger_OnPointerMoved;
        }
        bool isAtBottom = CheckPositionIsBottom(e);
        if (isAtBottom && !GalleryShowing)
        {
            GalleryGrid.Visibility = Visibility.Visible;
            GalleryShowing = true;
        }
        else if (!_galleryHideTimer?.IsEnabled == true)
        {
            _galleryHideTimer = new DispatcherTimer();
            _galleryHideTimer.Interval = TimeSpan.FromMilliseconds(1200);
            _galleryHideTimer.Tick += _galleryHideTimer_Tick;
            _galleryHideTimer.Start();
        }
    }

    private void _galleryHideTimer_Tick(object? sender, object e)
    {
        if (!IsCursorAtBottom)
        {
            GalleryGrid.Visibility = Visibility.Collapsed;
            _galleryHideTimer.Stop();
            _galleryHideTimer.Tick -= _galleryHideTimer_Tick;
        }
    }

    private DispatcherTimer _galleryHideTimer;

    private bool CheckPositionIsBottom(PointerRoutedEventArgs e)
    {
        var position = e?.GetCurrentPoint(ContentArea);
        if (position.Position.Y > (ContentArea.DesiredSize.Height - ContentArea.DesiredSize.Height / 3.5))
        {
            return true;
        }

        return false;
    }

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogsFrame.Visibility = Visibility.Visible;
        DialogsFrame.Navigate(typeof(SettingsPage), ViewModel);
    }
}