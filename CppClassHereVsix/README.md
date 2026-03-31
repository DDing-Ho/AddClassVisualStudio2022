# Add C++ Class Here

Visual Studio 2022 VSIX that adds `Add C++ Class Here...` to Solution Explorer for `vcxproj` projects.

## Behavior

- Physical folders: opens `Add New Item` scoped to the selected folder.
- VC filters: opens `Add New Item` and falls back to the project root.
- Project root: opens `Add New Item` at the project root.
- Files and non-`vcxproj` projects: command stays hidden.

## Build

Use the Visual Studio 2022 MSBuild that ships with the installed SDK:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" .\CppClassHereVsix\CppClassHereVsix.csproj /p:Configuration=Release /p:Platform=AnyCPU /t:Build /m:1 /nologo /v:minimal
```

The VSIX output is expected at `CppClassHereVsix\bin\Release\CppClassHereVsix.vsix`.

## Install

1. Build the project in `Release`.
2. Double-click the generated `.vsix` or install it from `Extensions -> Manage Extensions`.
3. Restart Visual Studio.

## Usage

1. In Solution Explorer, right-click a C++ project node, physical folder, or VC filter.
2. Choose `Add C++ Class Here...`.
3. Pick `C++ Class Pair` in the `Add New Item` dialog if it is not already selected.
4. Enter the class name and confirm.
