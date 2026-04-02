# CppClassHereVsix v1.3.10

`CppClassHereVsix`는 Visual Studio 2022에서 C++ 클래스를 더 편하게 만들 수 있게 해주는 확장 프로그램입니다.

기본 `추가 -> 클래스` 기능은 솔루션 탐색기에서 폴더나 필터를 우클릭해도 실제 `.h`, `.cpp` 파일이 프로젝트 루트에 생성되는 경우가 많습니다. 이 확장은 그 불편함을 줄이기 위해 `Add C++ Class Here...` 메뉴를 추가하고, 사용자가 선택한 위치를 기준으로 파일을 만들고 프로젝트에 바로 추가해줍니다.

## v1.3.10 변경 사항

- `v1.3.9` 설치본에서 보고된 manifest 버전 불일치 문제를 수정한 패치 릴리스입니다.
- VSIX 패키징 직전에 `source.extension.vsixmanifest`를 intermediate manifest로 강제 동기화해서, `extension.vsixmanifest`, `manifest.json`, `catalog.json`이 같은 버전으로 생성되도록 수정했습니다.
- 결과적으로 증분 빌드 상태에서도 이전 버전 manifest가 재사용되어 설치가 실패하는 문제를 막았습니다.
- DPI 관련 동작은 `v1.3.9`까지의 보정 내용을 그대로 포함합니다.

## 설치하면 무엇이 추가되나요?

확장을 설치하면 아래 위치에서 `Add C++ Class Here...` 메뉴를 사용할 수 있습니다.

- 솔루션 탐색기에서 프로젝트, 폴더, 필터를 우클릭했을 때의 `추가` 메뉴
- 상단 `도구` 메뉴

## 릴리스 자산

- `Installer\CppClassHereVsix-1.3.10.vsix`

## 설치 방법

1. Visual Studio 2022를 종료합니다.
2. 이 릴리스에서 `CppClassHereVsix-1.3.10.vsix`를 다운로드합니다.
3. `.vsix` 설치 파일을 실행합니다.
4. 설치가 끝나면 Visual Studio를 다시 실행합니다.

## 지원 대상

- Visual Studio 2022
- `vcxproj` 기반 C++ 프로젝트
