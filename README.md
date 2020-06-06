# SvgConverter
SvgConverter is a library for UWP platform to convert and display SVG files.

It provides the following features:

1. Parse/render SVG files, hand-drawn SVG animation.

2. Parsing text path, hand-drawn text animation.

## Using SvgConverter

```csharp
    var svgElement = await SvgElement.LoadFromFile(file);
    var win2DSvg = await Win2DSvgElement.Parse(resourceCreator, svgElement);
```


## Download

UWP : download from [Nuget](https://www.nuget.org/packages/SvgConverter/)

## Sample App

### SvgLab

SvgLab is an application for rendering SVG and drawing hand-drawn animation.

SvgLab is based on the SvgConverter library.

Download from [Microsoft Store](https://www.microsoft.com/store/apps/9PCN16MMNHPN).

#### Screenshot:
 ![Screenshot](Screenshot/Screenshot1.png)

**GIF:**
![Screenshot](Screenshot/Screenshot2.gif)