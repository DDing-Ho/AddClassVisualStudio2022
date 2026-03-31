using System;
using System.Globalization;

namespace CppClassHereVsix
{
    internal static class LocalizedStrings
    {
        private static bool IsKorean => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ko", StringComparison.OrdinalIgnoreCase);

        public static string CommandText => IsKorean ? "여기에 C++ 클래스 추가..." : "Add C++ Class Here...";
        public static string DialogWindowTitle => IsKorean ? "C++ 클래스 추가" : "Add C++ Class";
        public static string DialogHeaderTitle => IsKorean ? "클래스 추가" : "Add Class";
        public static string ClassNameLabel => IsKorean ? "클래스 이름(&L)" : "Class name(&N)";
        public static string HeaderFileLabel => IsKorean ? ".h 파일(&F)" : ".h file(&H)";
        public static string SourceFileLabel => IsKorean ? ".cpp 파일(&P)" : ".cpp file(&C)";
        public static string BaseClassLabel => IsKorean ? "기본 클래스(&B)" : "Base class(&B)";
        public static string AccessLabel => IsKorean ? "액세스(&A)" : "Access(&A)";
        public static string OtherOptionsLabel => IsKorean ? "기타 옵션:" : "Other options:";
        public static string InlineLabel => IsKorean ? "인라인(&I)" : "Inline(&I)";
        public static string OkButtonText => IsKorean ? "확인" : "OK";
        public static string CancelButtonText => IsKorean ? "취소" : "Cancel";
        public static string HeaderBrowseTitle => IsKorean ? "헤더 파일 선택" : "Select Header File";
        public static string SourceBrowseTitle => IsKorean ? "소스 파일 선택" : "Select Source File";
        public static string HeaderFileFilter => IsKorean ? "헤더 파일 (*.h)|*.h|모든 파일 (*.*)|*.*" : "Header Files (*.h)|*.h|All Files (*.*)|*.*";
        public static string SourceFileFilter => IsKorean ? "소스 파일 (*.cpp)|*.cpp|모든 파일 (*.*)|*.*" : "Source Files (*.cpp)|*.cpp|All Files (*.*)|*.*";
        public static string SelectionNotRecognizedMessage => IsKorean
            ? "현재 선택을 인식할 수 없습니다. vcxproj 프로젝트 노드, 실제 폴더 또는 VC 필터를 선택하세요."
            : "The current selection was not recognized. Use a vcxproj project node, physical folder, or VC filter.";
        public static string ErrorTitle => IsKorean ? "C++ 클래스 추가 오류" : "Add C++ Class Error";
        public static string DuplicateHeaderFileMessage => IsKorean ? "대상 폴더에 같은 이름의 헤더 파일이 이미 있습니다." : "A header file with the same name already exists in the target folder.";
        public static string HeaderSourceMustDifferMessage => IsKorean ? "헤더 파일과 소스 파일 이름은 서로 달라야 합니다." : "The header and source file names must be different.";
        public static string DuplicateSourceFileMessage => IsKorean ? "대상 폴더에 같은 이름의 소스 파일이 이미 있습니다." : "A source file with the same name already exists in the target folder.";
        public static string EnterClassNameMessage => IsKorean ? "클래스 이름을 입력하세요." : "Enter a class name.";
        public static string InvalidClassNameMessage => IsKorean ? "올바른 C++ 식별자를 입력하세요. 예: PlayerController" : "Enter a valid C++ identifier. Example: PlayerController";
        public static string EnterFileNameMessage => IsKorean ? "파일 이름을 입력하세요." : "Enter a file name.";
        public static string FileNamesMustNotContainPathMessage => IsKorean ? "헤더 파일과 소스 파일에는 경로가 아닌 파일 이름만 입력하세요." : "Enter only file names for the header and source files, not paths.";
        public static string InvalidFileNameCharactersMessage => IsKorean ? "파일 이름에 사용할 수 없는 문자가 포함되어 있습니다." : "The file name contains invalid characters.";
        public static string BaseClassSingleLineMessage => IsKorean ? "기반 클래스 이름은 한 줄로 입력해야 합니다." : "The base class name must be a single line.";
        public static string EnterHeaderFileNameMessage => IsKorean ? "헤더 파일 이름을 입력하세요." : "Enter a header file name.";
        public static string EnterSourceFileNameMessage => IsKorean ? "소스 파일 이름을 입력하세요." : "Enter a source file name.";
        public static string ProjectItemsUnavailableMessage => IsKorean ? "현재 선택에서 프로젝트 항목 컬렉션을 찾을 수 없습니다." : "Unable to locate the project item collection for the current selection.";
        public static string ManifestDisplayName => IsKorean ? "여기에 C++ 클래스 추가" : "Add C++ Class Here";
        public static string ManifestDescription => IsKorean
            ? "선택한 vcxproj 폴더나 VC 필터 기준으로 .h와 .cpp 파일을 만들고 프로젝트에 바로 추가합니다."
            : "Creates folder-aware .h and .cpp files for the selected vcxproj folder or VC filter and adds them to the project immediately.";
    }
}
