# CppClassHereVsix

`CppClassHereVsix`는 Visual Studio 2022용 C++ 확장 프로그램이다.

기본 `추가 -> 클래스` 동작은 솔루션 탐색기에서 특정 폴더나 필터를 우클릭해도 실제 `.h`, `.cpp` 파일이 프로젝트 루트에 생성되는 경우가 많다. 이 확장은 그 문제를 보완하기 위해 `Add C++ Class Here...` 명령을 추가하고, 사용자가 클릭한 위치를 기준으로 클래스 파일을 생성한 뒤 프로젝트에 바로 연결한다.

## 목적

- C++ 클래스 생성 위치를 사용자가 우클릭한 위치 기준으로 제어
- `.h`, `.cpp` 파일 생성과 프로젝트 추가를 한 번에 처리
- 기본 VC 클래스 추가 위저드 대신 더 단순한 전용 다이얼로그 제공

## 설치 대상

- Visual Studio 2022
- `vcxproj` 기반 C++ 프로젝트

## 바로 설치

빌드 없이 바로 설치하려면 아래 파일을 실행하면 된다.

- `Installer\CppClassHereVsix-1.3.2.vsix`

설치 절차:

1. Visual Studio 2022를 종료한다.
2. `Installer\CppClassHereVsix-1.3.2.vsix`를 실행한다.
3. 설치가 끝나면 Visual Studio를 다시 실행한다.

## 메뉴 위치

확장 설치 후 아래 위치에서 명령을 사용할 수 있다.

- 솔루션 탐색기에서 프로젝트/폴더/필터 우클릭
- `추가` 하위 메뉴의 `Add C++ Class Here...`
- `도구` 메뉴의 `Add C++ Class Here...`

명령 텍스트는 Visual Studio UI 언어에 따라 한국어/영어로 표시된다.

## 지원되는 선택 대상

아래 선택 상태에서 동작한다.

- `vcxproj` 프로젝트 루트 노드
- 실제 물리 폴더 노드
- VC 필터 노드

아래 경우는 지원하지 않는다.

- 비 `vcxproj` 프로젝트
- 파일 노드 선택 상태
- 다중 선택 상태
- 현재 선택 위치를 실제 프로젝트 위치로 해석할 수 없는 경우

지원하지 않는 상태에서 실행하면 오류 메시지가 표시되고 파일은 생성되지 않는다.

## 선택 위치별 동작 사양

### 1. 실제 폴더를 우클릭한 경우

- 파일 생성 위치: 우클릭한 실제 디스크 폴더
- 프로젝트 추가 위치: 우클릭한 해당 폴더 아래
- 결과: 물리 경로와 솔루션 탐색기 구조가 일치한다

예:

- `Source\Gameplay` 폴더를 우클릭하고 `Player` 생성
- 실제 생성 파일: `Source\Gameplay\Player.h`, `Source\Gameplay\Player.cpp`
- 프로젝트에서도 `Gameplay` 폴더 아래에 추가

### 2. VC 필터를 우클릭한 경우

- 파일 생성 위치: 프로젝트 루트 디렉터리
- 프로젝트 추가 위치: 우클릭한 필터 아래
- 결과: 솔루션 탐색기에서는 선택한 필터에 보이지만, 실제 디스크 파일은 프로젝트 루트에 생성된다

이 동작은 의도된 현재 사양이다.

예:

- `Gameplay`라는 VC 필터를 우클릭하고 `Player` 생성
- 실제 생성 파일: `<프로젝트 루트>\Player.h`, `<프로젝트 루트>\Player.cpp`
- 프로젝트에서는 `Gameplay` 필터 아래에 추가

### 3. 프로젝트 루트를 우클릭한 경우

- 파일 생성 위치: 프로젝트 루트 디렉터리
- 프로젝트 추가 위치: 프로젝트 루트

## 생성 다이얼로그 사양

기본 VC 클래스 추가 위저드가 아니라, 확장에서 제공하는 커스텀 다이얼로그를 사용한다.

입력 항목:

- 클래스 이름
- `.h` 파일 이름
- `.cpp` 파일 이름
- 기본 클래스
- 액세스 지정자
- 기타 옵션: `인라인`

다이얼로그 특징:

- Visual Studio 현재 테마 색을 따라간다
- Visual Studio UI 언어에 따라 한국어/영어로 표시된다
- 커스텀 무제목(titleless) UI를 사용한다
- 탭 이동 순서는 `클래스 이름 -> .h 파일 -> .cpp 파일 -> 기본 클래스 -> 액세스` 기준으로 맞춰져 있다

## 파일 이름 동작 규칙

### 클래스 이름과 파일 이름 동기화

초기 상태에서는 클래스 이름을 입력하면 파일 이름이 자동으로 아래처럼 맞춰진다.

- `Player` 입력 시 `Player.h`, `Player.cpp`

동기화 규칙:

- 사용자가 `.h` 또는 `.cpp` 파일명을 직접 수정하면 자동 동기화가 내부적으로 해제된다
- 그 뒤 클래스 이름을 다시 수정하면 동기화가 다시 활성화되면서 파일명이 클래스 이름 기준으로 다시 맞춰진다
- 이 동기화 상태는 UI에 별도 옵션으로 노출되지 않는다

### 파일 이름 검증

- 파일 이름에는 경로를 넣을 수 없다
- 잘못된 파일명 문자는 허용되지 않는다
- 확장자를 직접 입력하지 않으면 `.h`, `.cpp`가 자동으로 붙는다
- 헤더 파일명과 소스 파일명은 서로 같을 수 없다
- 대상 위치에 같은 이름의 파일이 이미 있으면 생성이 실패한다

## 클래스 생성 규칙

### 클래스 이름 검증

클래스 이름은 C++ 식별자 규칙에 맞아야 한다.

허용 예:

- `Player`
- `PlayerController`
- `_InternalHelper`

허용되지 않는 예:

- `1Player`
- `Player Controller`
- `Player-Controller`

### 기본 클래스와 액세스 지정자

기본 클래스를 입력하지 않으면 상속 구문 없이 생성된다.
기본 클래스를 입력하면 선택한 액세스 지정자를 사용해 상속 구문이 만들어진다.

예:

- 클래스 이름: `Player`
- 기본 클래스: `Actor`
- 액세스: `public`

생성 결과:

```cpp
#pragma once

class Player : public Actor
{
};
```

### 생성되는 기본 파일 내용

일반 모드:

- 헤더 파일: `#pragma once` + 클래스 선언
- 소스 파일: 해당 헤더를 include 하는 한 줄

예:

```cpp
#include "Player.h"
```

생성자/소멸자 자동 생성 기능은 현재 지원하지 않는다.

## Inline 옵션 사양

`인라인` 옵션을 체크하면 `.cpp` 파일을 만들지 않고 헤더 파일만 생성한다.

동작:

- `.cpp` 파일 입력칸과 선택 버튼이 비활성화된다
- 실제 생성 결과는 `.h` 파일 하나뿐이다
- 프로젝트에도 헤더 파일만 추가된다

## 프로젝트 반영 방식

파일 생성 후 확장은 즉시 해당 파일을 프로젝트에 추가한다.

- 실제 폴더 선택 시: 그 폴더의 `ProjectItems` 아래에 추가
- VC 필터 선택 시: 그 필터의 `ProjectItems` 아래에 추가
- 프로젝트 루트 선택 시: 프로젝트 루트의 `ProjectItems` 아래에 추가

헤더 파일 생성이 끝나면 생성된 헤더 파일을 Visual Studio에서 자동으로 연다.

## 언어 및 테마 지원

### UI 언어

Visual Studio UI 언어를 따라 다음 항목이 한국어/영어로 바뀐다.

- 명령 이름
- 다이얼로그 라벨
- 오류 메시지
- 확장 프로그램 이름/설명

### 테마

다이얼로그는 Visual Studio 현재 테마 색을 읽어 다음 요소에 반영한다.

- 창 배경
- 입력 컨트롤 배경
- 텍스트 색
- 버튼 색
- 테두리 색

다이얼로그가 열린 상태에서 Visual Studio 테마가 바뀌면, 가능한 범위에서 다시 테마 색을 적용한다.

## 제약 및 현재 사양

- 대상은 `vcxproj` 프로젝트만 지원한다
- CMake / Open Folder 프로젝트는 지원하지 않는다
- 파일 노드에서 직접 실행하는 흐름은 지원하지 않는다
- VC 필터를 선택한 경우 실제 디스크 생성 위치는 필터가 아니라 프로젝트 루트다
- 네임스페이스 자동 생성 기능은 없다
- 생성자/소멸자 자동 생성 기능은 없다
- PCH include 자동 삽입 기능은 없다
- 기존 Visual Studio 기본 클래스 추가 UI를 재사용하지 않는다

## 저장소 구성

- `Installer/`: 바로 설치 가능한 VSIX 파일
- `CppClassHereVsix/`: 확장 프로그램 소스 코드
- `CppClassHereVsix/ScreenShot/`: UI 비교용 참고 이미지
- `CppClassHereVsix.sln`: 솔루션 파일

## 소스 빌드

소스를 직접 수정하거나 새 버전을 빌드하려면 Visual Studio 2022 SDK가 필요하다.

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" .\CppClassHereVsix.sln /p:Configuration=Release /t:Build /m:1 /nologo /v:minimal
```

출력 파일:

- `CppClassHereVsix\bin\Release\CppClassHereVsix.vsix`
- `CppClassHereVsix\bin\Release\CppClassHereVsix-1.3.2.vsix`
