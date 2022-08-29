using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using System.Drawing;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.Extensions.Primitives;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using RestSharp;
using SplashyPapper.Constants;
using SplashyPapper.Core.Constants;
using SplashyPapper.Core.Services;
using SplashyPapper.Helpers;
using SplashyPapper.Services;

namespace SplashyPapper.ViewModels;

public class MainViewModel : ObservableRecipient
{
    private bool _isLoading = false;

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            SetProperty(ref _isLoading, value);
            DownloadMoreCommand?.NotifyCanExecuteChanged();
        }
    }

    private bool _isCancelTipVisible = false;

    public bool IsCancelTipVisible
    {
        get => _isCancelTipVisible;
        set
        {
            SetProperty(ref _isCancelTipVisible, value);
            LocalSettingsHelper.SaveValue(SettingsName.CANCEL_TIP_SHOWED, true);
        }
    }

    private bool _isAlwaysShowGallery = false;

    public event Action<bool> GalleryAlwaysStateChanged;

    public bool IsAlwaysShowGallery
    {
        get => _isAlwaysShowGallery;
        set
        {
            SetProperty(ref _isAlwaysShowGallery, value);
            LocalSettingsHelper.SaveValue(SettingsName.ALWAYS_SHOW_GALLERY, value);
            GalleryAlwaysStateChanged?.Invoke(value);
        }
    }

    public event Action<int> SelectedImageChanged;

    private int _selectedImageIndex = -1;
    private int _nextImageIndex = 0;

    public int SelectedPictureIndex
    {
        get => _selectedImageIndex;
        set
        {
            SetProperty(ref _selectedImageIndex, value);
            SelectedImageChanged?.Invoke(value);
        }
    }

    public int NextPictureIndex
    {
        get => _nextImageIndex;
        set
        {
            SetProperty(ref _nextImageIndex, value);
        }
    }

    private StorageFile _selectedImage;

    public StorageFile SelectedImage
    {
        get => _selectedImage;
        set
        {
            SetProperty(ref _selectedImage, value);
        }
    }

    private StorageFile _nextImage;

    public StorageFile NextImage
    {
        get => _nextImage;
        set
        {
            SetProperty(ref _nextImage, value);
        }
    }

    public CancellationTokenSource CancellationToken
    {
        get; set;
    }

    public async void LoadPicture()
    {
        if (!IsLoading)
        {
            IsLoading = true;
            if (!LocalSettingsHelper.IsCancelTipShowed) IsCancelTipVisible = true;
            CancellationToken = new CancellationTokenSource();
            CancelCommand.NotifyCanExecuteChanged();
            string link = "https://source.unsplash.com/random/" + Resolutions[SelectedResolution] + "?" + KeyWords;
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(DownloadTimeout);
            try
            {
                var response = await httpClient.GetAsync(link, CancellationToken.Token);
                CancelCommand.NotifyCanExecuteChanged();
                var imagesFolder = await GetPicsFolder();
                await using var responseStream = await response.Content.ReadAsStreamAsync();
                var memoryStream = new MemoryStream();
                await responseStream.CopyToAsync(memoryStream);
                await imagesFolder.WriteBytesToFileAsync(memoryStream.ToArray(),
                    "image_" + DateTime.Now.ToString("MM-dd-yy_hh-mm-ss") + ".jpg",
                    CreationCollisionOption.GenerateUniqueName);

                await LoadAllPictures();
            }
            catch (TaskCanceledException ex)
            {
                var toast = new ToastService($"Can't download picture! {ex.Message}", TimeSpan.FromSeconds(5));
                toast.RequestToast();
            }

            CancellationToken = null;
            CancelCommand.NotifyCanExecuteChanged();
            IsLoading = false;
        }
        else
        {
            var toast = new ToastService($"Download already started", TimeSpan.FromSeconds(3));
            toast.RequestToast();
        }
    }

    #region DownloadMoreCommand

    public RelayCommand DownloadMoreCommand
    {
        get;
    }

    #endregion DownloadMoreCommand

    public async Task<StorageFolder> GetPicsFolder()
    {
        IsLoading = true;
        var folder = ApplicationData.Current.LocalFolder;
        if (!Directory.Exists(folder.Path + @"\Images"))
        {
            var imagesFolder = await folder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);
            await SaveTempImages(imagesFolder);
        }

        IsLoading = false;
        return await folder.GetFolderAsync("Images");
    }

    public int _downloadTimeout = 25;

    public int DownloadTimeout
    {
        get => _downloadTimeout;
        set
        {
            SetProperty(ref _downloadTimeout, value);
            LocalSettingsHelper.SaveValue(SettingsName.TIMEOUT_DURATION, value);
        }
    }

    public bool _autoLoadNext = true;

    public bool AutoLoadNext
    {
        get => _autoLoadNext;
        set
        {
            SetProperty(ref _autoLoadNext, value);
            LocalSettingsHelper.SaveValue(SettingsName.AUTO_LOAD_NEXT, value);
        }
    }

    public RelayCommand CancelCommand
    {
        get; set;
    }

    public void Cancel()
    {
        CancellationToken.Cancel();
    }

    public bool CanCancel() => CancellationToken != null && !CancellationToken.IsCancellationRequested;

    public async Task SaveTempImages(StorageFolder folder)
    {
        var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/temp1.jpg"));
        var file2 = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/temp2.jpg"));
        await file.CopyAsync(folder, "aTemp1.jpg");
        await file2.CopyAsync(folder, "aTemp2.jpg");
        Images.Add(file);
        Images.Add(file2);
    }

    public async Task LoadAllPictures()
    {
        var imagesFolder = await GetPicsFolder();
        var filesInFolder = await imagesFolder.GetFilesAsync();
        foreach (var imageFile in filesInFolder)
        {
            if (!SasesExtensions.IsContainsFile(Images, imageFile))
                Images.Add(imageFile);
        }

        if (SelectedPictureIndex < 0)
        {
            SelectedPictureIndex = 0;
            NextPictureIndex = 1;
        }
    }

    public ObservableCollection<StorageFile> Images { get; set; } = new();

    private bool CanDownloadMoreExecute() => !IsLoading;

    public MainViewModel()
    {
        LoadAllPictures();
        SetWallpaperCommand = new RelayCommand(OnSetWallpaperCommandExecuting);
        CancelCommand = new RelayCommand(Cancel, CanCancel);
        DownloadMoreCommand = new RelayCommand(LoadPicture, CanDownloadMoreExecute);

        SelectedImageChanged += MainViewModel_SelectedImageChanged;
        LoadSettings();
    }

    private void LoadSettings()
    {
        var autoLoadValue = LocalSettingsHelper.TryGetValue(SettingsName.AUTO_LOAD_NEXT);
        if (autoLoadValue != null) AutoLoadNext = (bool)autoLoadValue;

        var keyWordsValue = LocalSettingsHelper.TryGetValue(SettingsName.KEY_WORDS);
        if (keyWordsValue != null) KeyWords = (string)keyWordsValue;

        var timeoutValue = LocalSettingsHelper.TryGetValue(SettingsName.TIMEOUT_DURATION);
        if (timeoutValue != null) DownloadTimeout = (int)timeoutValue;

        var resolutionIdValue = LocalSettingsHelper.TryGetValue(SettingsName.SELECTED_RESOLUTION_ID);
        if (resolutionIdValue != null) SelectedResolution = (int)resolutionIdValue;
    }

    private void MainViewModel_SelectedImageChanged(int obj)
    {
        if (AutoLoadNext && obj + 1 >= Images.Count)
        {
            LoadPicture();
        }

        SelectedImage = Images[SelectedPictureIndex];
        NextImage = Images[NextPictureIndex];
    }

    public List<string> Resolutions => PhotosResolutions.TypicalPhotosResolutions;
    private int _selectedResolution = 0;

    public int SelectedResolution
    {
        get => _selectedResolution;
        set
        {
            SetProperty(ref _selectedResolution, value);
            LocalSettingsHelper.SaveValue(SettingsName.SELECTED_RESOLUTION_ID, value);
        }
    }

    #region SetWallpaperCommand

    public RelayCommand SetWallpaperCommand
    {
        get;
    }

    private async void OnSetWallpaperCommandExecuting()
    {
        WallpaperChanger.Set(Images[SelectedPictureIndex].Path, WallpaperChanger.Style.Stretched);
        var toast = new ToastService($"Wallpaper have been changed!", TimeSpan.FromSeconds(3));
        toast.RequestToast();
    }

    #endregion SetWallpaperCommand

    private bool _searchWindowVisible = false;

    public bool SearchWindowVisible
    {
        get => _searchWindowVisible;
        set => SetProperty(ref _searchWindowVisible, value);
    }

    private string _keyWords = "Cars";

    public string KeyWords
    {
        get => _keyWords;
        set
        {
            SetProperty(ref _keyWords, value);
            LocalSettingsHelper.SaveValue(SettingsName.KEY_WORDS, value);
        }
    }
}