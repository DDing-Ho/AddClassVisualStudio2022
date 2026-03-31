# CppClassHereVsix v1.3.2

`CppClassHereVsix` is a Visual Studio 2022 extension for `vcxproj` projects.
It adds a folder-aware `Add C++ Class Here...` command so C++ class files are created in the folder you actually clicked, instead of always being placed at the project root.

## Highlights

- Adds `Add C++ Class Here...` to the C++ project context menu.
- Creates `.h` and `.cpp` files in the selected physical folder.
- Resolves VC filters and adds generated files back into the project.
- Uses a custom class creation dialog instead of the default VC wizard.
- Supports Korean and English UI based on the Visual Studio UI language.
- Adapts the dialog colors to the current Visual Studio theme.
- Supports `Inline` mode to generate only the header file.

## Release Asset

- `CppClassHereVsix-1.3.2.vsix`

## Installation

1. Close Visual Studio 2022.
2. Download `CppClassHereVsix-1.3.2.vsix` from this release.
3. Run the `.vsix` installer.
4. Restart Visual Studio.

## Notes

- Target: Visual Studio 2022
- Project type: `vcxproj`
- UI languages: English, Korean
