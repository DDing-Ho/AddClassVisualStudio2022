# CppClassHereVsix v1.3.2

`CppClassHereVsix`는 `vcxproj` 프로젝트용 Visual Studio 2022 확장 프로그램입니다.
우클릭한 폴더 기준으로 `Add C++ Class Here...` 명령을 실행해 C++ 클래스 파일을 생성할 수 있으며, 기본 동작처럼 항상 프로젝트 루트에 파일이 만들어지는 문제를 피할 수 있습니다.

## 주요 내용

- C++ 프로젝트 컨텍스트 메뉴에 `Add C++ Class Here...` 명령을 추가합니다.
- 선택한 실제 폴더에 `.h`, `.cpp` 파일을 생성합니다.
- VC 필터를 해석해 생성한 파일을 프로젝트에 다시 추가합니다.
- 기본 VC 위저드 대신 커스텀 클래스 생성 다이얼로그를 제공합니다.
- Visual Studio UI 언어에 따라 한국어/영어 UI를 지원합니다.
- Visual Studio 현재 테마에 맞춰 다이얼로그 색상을 적용합니다.
- `Inline` 옵션으로 헤더 파일만 생성할 수 있습니다.

## 릴리스 자산

- `Installer\CppClassHereVsix-1.3.2.vsix`

## 설치 방법

1. Visual Studio 2022를 종료합니다.
2. 저장소 또는 릴리스 자산에서 `CppClassHereVsix-1.3.2.vsix`를 받습니다.
3. `.vsix` 설치 파일을 실행합니다.
4. Visual Studio를 다시 실행합니다.

## 참고

- 빌드 없이 설치할 수 있습니다.
- 소스 빌드에는 Visual Studio 2022 SDK가 필요합니다.
- 대상 버전은 Visual Studio 2022입니다.
- 대상 프로젝트 형식은 `vcxproj`입니다.
- 지원 UI 언어는 한국어, 영어입니다.
