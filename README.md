# CppClassHereVsix

Visual Studio 2022 VSIX extension for `vcxproj` projects.

It adds a folder-aware class creation command to Solution Explorer so `.h` and `.cpp` files are created in the folder you actually clicked, not always at the project root.

## Features

- Adds `Add C++ Class Here...` to the C++ project context menu.
- Uses a custom class dialog instead of the default VC wizard.
- Creates files in the selected physical folder or the resolved folder behind a VC filter.
- Supports Korean and English UI based on the Visual Studio UI language.
- Adapts dialog colors to the current Visual Studio theme.
- Supports `Inline` generation to create only the header file.

## Repository Layout

- `CppClassHereVsix.sln`: solution file
- `CppClassHereVsix/`: VSIX project source
- `CppClassHereVsix/ScreenShot/`: reference screenshots for UI matching

## Build

Build with the Visual Studio 2022 MSBuild that ships with the installed SDK.

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" .\CppClassHereVsix.sln /p:Configuration=Release /t:Build /m:1 /nologo /v:minimal
```

Output:

- `CppClassHereVsix\bin\Release\CppClassHereVsix.vsix`
- `CppClassHereVsix\bin\Release\CppClassHereVsix-1.3.2.vsix`

## Install

1. Close Visual Studio.
2. Build the project in `Release`.
3. Install the generated `.vsix`.
4. Restart Visual Studio.

## Usage

1. In Solution Explorer, right-click a `vcxproj` project, folder, or VC filter.
2. Choose `Add C++ Class Here...`.
3. Enter the class information in the custom dialog.
4. Confirm to generate the files and add them to the project.
