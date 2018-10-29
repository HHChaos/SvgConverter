using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Navigation;
using HHChaosToolkit.UWP.Controls;
using HHChaosToolkit.UWP.Mvvm;

namespace SvgConverter.SampleApp.ViewModels
{
    public class ImageToSvgViewModel : ViewModelBase
    {
        private StorageFile _backImageFile;

        public StorageFile BackImageFile
        {
            get => _backImageFile;
            set => Set(ref _backImageFile, value);
        }

        public ICommand PickImageCommand
        {
            get { return new RelayCommand(async () => { await PickBackImageFile(); }); }
        }

        public ICommand SaveSvgCommand
        {
            get
            {
                return new RelayCommand<string>(async svgContent =>
                {
                    var picker = new FileSavePicker
                    {
                        SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                        SuggestedFileName = $"SvgLab_{DateTime.Now.ToFileTime()}"
                    };
                    picker.FileTypeChoices.Add("Svg Picture", new List<string> {".svg"});
                    var file = await picker.PickSaveFileAsync();
                    if (file != null)
                    {
                        await FileIO.WriteBytesAsync(file,
                            Encoding.UTF8.GetBytes(svgContent));
                        ShellViewModel.NavigationService?.Navigate(typeof(SvgViewerViewModel).FullName, file);
                    }
                });
            }
        }

        public override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (BackImageFile == null)
                await PickBackImageFile();
            if (BackImageFile == null)
            {
                var toast = new Toast("You must first select a valid picture!");
                toast.Show();
                if (ShellViewModel.NavigationService?.CanGoBack == true) ShellViewModel.NavigationService?.GoBack();
            }
        }

        private async Task PickBackImageFile()
        {
            var waitingDialog = new WaitingDialog("Pick a picture...");
            waitingDialog.Show();
            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            var file = await openPicker.PickSingleFileAsync();
            if (file != null) BackImageFile = file;
            waitingDialog.Close();
        }
    }
}
