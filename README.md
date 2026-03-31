# CppClassHereVsix

`vcxproj` 프로젝트를 위한 Visual Studio 2022 VSIX 확장이다.

이 확장은 솔루션 탐색기에서 우클릭한 폴더 기준으로 C++ 클래스를 생성할 수 있게 해준다. 기본 동작처럼 항상 프로젝트 루트에 파일이 생성되는 문제를 피하고, 선택한 폴더나 필터에 맞춰 `.h`, `.cpp` 파일을 만들고 프로젝트에도 바로 추가한다.

## 주요 기능

- C++ 프로젝트 컨텍스트 메뉴에 `Add C++ Class Here...` 명령 추가
- 선택한 실제 폴더 기준으로 `.h`, `.cpp` 파일 생성
- VC 필터를 선택한 경우 대상 폴더를 해석해 파일 생성 후 프로젝트에 추가
- 기본 VC 위저드 대신 커스텀 클래스 생성 다이얼로그 사용
- Visual Studio UI 언어에 따라 한국어/영어 UI 지원
- Visual Studio 현재 테마에 맞춰 다이얼로그 색상 적용
- `Inline` 옵션으로 헤더 파일만 생성 가능

## 저장소 구성

- `CppClassHereVsix.sln`: 솔루션 파일
- `CppClassHereVsix/`: VSIX 프로젝트 소스
- `CppClassHereVsix/ScreenShot/`: UI 비교용 참고 스크린샷

## 빌드

설치된 Visual Studio 2022의 MSBuild로 빌드한다.

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" .\CppClassHereVsix.sln /p:Configuration=Release /t:Build /m:1 /nologo /v:minimal
```

출력 파일:

- `CppClassHereVsix\bin\Release\CppClassHereVsix.vsix`
- `CppClassHereVsix\bin\Release\CppClassHereVsix-1.3.2.vsix`

## 설치

1. Visual Studio를 종료한다.
2. `Release` 구성으로 빌드한다.
3. 생성된 `.vsix` 파일을 설치한다.
4. Visual Studio를 다시 실행한다.

## 사용 방법

1. 솔루션 탐색기에서 `vcxproj` 프로젝트, 실제 폴더, 또는 VC 필터를 우클릭한다.
2. `Add C++ Class Here...`를 선택한다.
3. 커스텀 다이얼로그에서 클래스 정보를 입력한다.
4. 확인하면 파일이 생성되고 프로젝트에 추가된다.

## 릴리스

현재 릴리스 패키지:

- `CppClassHereVsix\bin\Release\CppClassHereVsix-1.3.2.vsix`

권장 릴리스 절차:

1. `Release`로 빌드한다.
2. 생성된 VSIX가 Visual Studio 2022에서 정상 설치되는지 확인한다.
3. 릴리스 버전에 맞는 태그를 만든다.
4. `.vsix` 파일을 Git 호스팅 서비스의 Release 또는 내부 배포 채널에 업로드한다.

## Git 정보

- 기본 브랜치: `main`
- 저장소 루트: `AddClassVisualStudio2022`
