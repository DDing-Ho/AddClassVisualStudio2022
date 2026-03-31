# Add C++ Class Here

`vcxproj` 프로젝트용 Visual Studio 2022 VSIX 확장이다.

이 확장은 솔루션 탐색기에서 선택한 위치를 기준으로 C++ 클래스를 생성할 수 있게 해준다. 실제 폴더를 선택하면 그 폴더에 파일이 생성되고, VC 필터를 선택한 경우에는 해석된 대상 위치에 파일을 만든 뒤 프로젝트에 바로 추가한다.

## 동작 방식

- 실제 폴더: 선택한 폴더에 파일 생성 후 프로젝트에 추가
- VC 필터: 대상 위치를 해석해 파일 생성 후 프로젝트에 추가
- 프로젝트 루트: 프로젝트 기준 위치에 파일 생성
- 지원하지 않는 선택 상태: 명령이 동작하지 않음

## 설치용 파일

빌드 없이 바로 설치하려면 루트의 아래 파일을 사용한다.

- `..\Installer\CppClassHereVsix-1.3.2.vsix`

## 빌드

소스를 직접 수정하거나 새 버전을 빌드하려면 Visual Studio 2022 SDK가 필요하다.
설치된 Visual Studio 2022의 MSBuild로 빌드한다.

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" .\CppClassHereVsix\CppClassHereVsix.csproj /p:Configuration=Release /p:Platform=AnyCPU /t:Build /m:1 /nologo /v:minimal
```

출력 VSIX:

- `CppClassHereVsix\bin\Release\CppClassHereVsix.vsix`

## 사용 방법

1. 솔루션 탐색기에서 C++ 프로젝트, 실제 폴더, 또는 VC 필터를 우클릭한다.
2. `Add C++ Class Here...`를 선택한다.
3. 다이얼로그에서 클래스 이름, 파일명, 기본 클래스 등을 입력한다.
4. 확인하면 선택한 위치 기준으로 파일이 생성되고 프로젝트에 추가된다.
