# ColorChooser

ColorChooser is a lightweight, user-friendly desktop utility for Windows designed to help you pick, visualize, and copy colors.

-----

## Features

  * **Color Picker:** Uses dropdowns for named colors, an eyedropper, and a custom colors box.
  * **Embed Code:** Provides the hex value and how to embed it in a wide variety of programming languages.
  * **Named-Color Finder:** Users can find the nearest named color to any valid RGB hex value.
  * **PNG Export:** Allows for saving a PNG in the chosen color.

-----

## Requirements

The pre-compiled application is designed to run on **64-bit Windows (Windows 10 and newer)**.

There are **no external dependencies or prerequisites**. The executable is a **self-contained** application, which means it includes the .NET runtime and does not require you to have .NET installed on your system.

-----

## How to Use

1.  Navigate to the [**Releases** page](https://github.com/ianrastall/ColorChooser/releases) of this repository.
2.  Under the latest release, download the `ColorChooser.exe` file from the **Assets** section.
3.  Run the downloaded file. No installation is necessary.

-----

## Building from Source

If you prefer to compile the application yourself, you will need the **.NET 8 SDK** or newer installed.

### Using the .NET CLI (Recommended for a Single Executable)

This is the recommended method for creating a single, portable, and self-contained executable, identical to the one provided in the releases.

1.  Clone the repository:
    ```shell
    git clone https://github.com/ianrastall/ColorChooser.git
    ```
2.  Navigate to the repository's root directory.
3.  Run the following `dotnet publish` command:
    ```shell
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
    ```
    This command compiles the application in `Release` mode, packages it for 64-bit Windows, bundles the .NET runtime, and compresses everything into a single `.exe` file. The output will be located in the `bin/Release/net8.0-windows/win-x64/publish` folder.

### Using Visual Studio 2022

1.  Clone the repository.
2.  Open the `ColorChooser.sln` solution file in Visual Studio.
3.  Set the solution configuration to **Release**.
4.  From the menu, select **Build \> Build Solution**. The output will be located in the `bin/Release` folder inside the project directory.

-----

## License

This project is licensed under the MIT License.
