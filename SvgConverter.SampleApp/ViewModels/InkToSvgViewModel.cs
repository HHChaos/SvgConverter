using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using HHChaosToolkit.UWP.Mvvm;

namespace SvgConverter.SampleApp.ViewModels
{
    public class InkToSvgViewModel : ViewModelBase
    {
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
    }
}
