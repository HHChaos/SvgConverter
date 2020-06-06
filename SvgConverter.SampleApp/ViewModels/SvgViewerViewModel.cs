using HHChaosToolkit.UWP.Controls;
using HHChaosToolkit.UWP.Mvvm;
using HHChaosToolkit.UWP.Services.Navigation;
using SvgConverter.SampleApp.Controls;
using SvgConverter.SampleApp.Views;
using SvgConverter.SvgParse;
using SvgConverter.SvgParseForWin2D;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Xaml.Navigation;

namespace SvgConverter.SampleApp.ViewModels
{
    public class SvgViewerViewModel : ViewModelBase
    {
        private string _fileName;
        private bool _showBackHomeBtn;
        private SvgElement _svgElement;
        private AnimationPlayer _animationPlayer;

        public async void InitPlayer(AnimationPlayer animationPlayer)
        {
            _animationPlayer = animationPlayer;
            if (_svgElement != null)
            {
                await _animationPlayer.SetAnimationItem(_svgElement);
            }
        }

        public bool ShowBackHomeBtn
        {
            get => _showBackHomeBtn;
            set => Set(ref _showBackHomeBtn, value);
        }

        public string FileName
        {
            get => _fileName;
            set => Set(ref _fileName, value);
        }


        public ICommand PickFileCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    var openPicker = new FileOpenPicker
                    {
                        ViewMode = PickerViewMode.Thumbnail,
                        SuggestedStartLocation = PickerLocationId.Desktop
                    };
                    openPicker.FileTypeFilter.Add(".svg");
                    var file = await openPicker.PickSingleFileAsync();
                    if (file == null || !file.IsAvailable)
                        return;
                    await UpdateAnimationItem(file);
                });
            }
        }

        public ICommand BackHomeCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    NavigationServiceList.Instance.Default.Navigate(typeof(ShellViewModel).FullName);
                });
            }
        }

        public ICommand FeedbackCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9pcn16mmnhpn"));
                });
            }
        }

        public ICommand SavePngCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (_svgElement == null)
                    {
                        var toast = new Toast("Empty SVG File!");
                        toast.Show();
                        return;
                    }

                    var picker = new FileSavePicker
                    {
                        SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                        SuggestedFileName = $"SvgLab_{DateTime.Now.ToFileTime()}"
                    };
                    picker.FileTypeChoices.Add("PNG File", new List<string> {".png"});
                    var file = await picker.PickSaveFileAsync();
                    if (file != null)
                    {
                        using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            await Win2DSvgElement.RenderImage(stream, _svgElement);
                        }
                    }
                });
            }
        }

        public override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _svgElement = null;
            if (e.Parameter is IStorageFile file)
            {
                await UpdateAnimationItem(file);
            }
            else
            {
                var defaultFile =
                    await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/sample.svg"));
                await UpdateAnimationItem(defaultFile);
            }

            ShowBackHomeBtn = false;
            await Task.Delay(100);
            ShowBackHomeBtn = !(NavigationServiceList.Instance.Default.Frame?.Content is ShellPage);
        }

        public override void OnNavigatedFrom(NavigatedFromEventArgs e)
        {
            _animationPlayer = null;
            base.OnNavigatedFrom(e);
        }

        private async Task UpdateAnimationItem(IStorageFile file)
        {
            if (file == null)
                return;
            var waitingDialog = new WaitingDialog("Loading SVG file...");
            waitingDialog.Show();
            FileName = file.Name;
            try
            {
                _svgElement = await SvgElement.LoadFromFile(file);
                if (_animationPlayer != null)
                {
                    await _animationPlayer.SetAnimationItem(_svgElement);
                }
            }
            catch (Exception exception)
            {
                var toast = new Toast(exception.Message);
                toast.Show();
            }
            finally
            {
                waitingDialog.Close();
            }
        }
    }
}
