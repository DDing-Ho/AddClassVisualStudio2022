# CppClassHereVsix v1.3.2

`CppClassHereVsix`는 `vcxproj` 프로젝트용 Visual Studio 2022 확장이다.
우클릭한 폴더 기준으로 `Add C++ Class Here...` 명령을 실행해 C++ 클래스 파일을 생성할 수 있으며, 기본 동작처럼 항상 프로젝트 루트에 파일이 만들어지는 문제를 피할 수 있다.

## 주요 내용

- C++ 프로젝트 컨텍스트 메뉴에 `Add C++ Class Here...` 명령 추가
- 선택한 실제 폴더에 `.h`, `.cpp` 파일 생성
- VC 필터를 해석해 생성한 파일을 프로젝트에 다시 추가
- 기본 VC 위저드 대신 커스텀 클래스 생성 다이얼로그 제공
- Visual Studio UI 언어에 따라 한국어/영어 UI 지원
- Visual Studio 현재 테마에 맞춰 다이얼로그 색상 적용
- `Inline` 옵션으로 헤더 파일만 생성 가능

## 릴리스 자산

- `CppClassHereVsix-1.3.2.vsix`

## 설치 방법

1. Visual Studio 2022를 종료한다.
2. 이 릴리스에서 `CppClassHereVsix-1.3.2.vsix`를 다운로드한다.
3. `.vsix` 설치 파일을 실행한다.
4. Visual Studio를 다시 실행한다.

## 참고

- 대상 버전: Visual Studio 2022
- 대상 프로젝트 형식: `vcxproj`
- 지원 UI 언어: 한국어, 영어
