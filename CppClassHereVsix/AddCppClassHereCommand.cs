using System;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CppClassHereVsix
{
    internal sealed class AddCppClassHereCommand
    {
        public const int ContextCommandId = 0x0100;
        public const int ToolsCommandId = 0x0101;
        public static readonly Guid CommandSet = new Guid("5db1127e-437e-4997-8553-fa1b59866fd9");
        private static readonly string LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CppClassHereVsix.log");
        private static readonly Regex ValidClassNamePattern = new Regex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        private readonly Package package;

        private AddCppClassHereCommand(Package package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            commandService.AddCommand(CreateCommand(ContextCommandId));
            commandService.AddCommand(CreateCommand(ToolsCommandId));
        }

        public static void Initialize(Package package)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            OleMenuCommandService commandService = ((IServiceProvider)package).GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService == null)
            {
                throw new InvalidOperationException("OleMenuCommandService를 가져올 수 없습니다.");
            }

            _ = new AddCppClassHereCommand(package, commandService);
        }

        private OleMenuCommand CreateCommand(int commandId)
        {
            OleMenuCommand command = new OleMenuCommand(Execute, new CommandID(CommandSet, commandId));
            command.Text = LocalizedStrings.CommandText;
            command.BeforeQueryStatus += OnBeforeQueryStatus;
            return command;
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand command)
            {
                command.Text = LocalizedStrings.CommandText;
            }
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                AppendLog("Execute start. Assembly=" + Assembly.GetExecutingAssembly().GetName().Version);

                IVsMonitorSelection selectionService = ((IServiceProvider)package).GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
                AppendLog("Selection service acquired: " + (selectionService != null));

                ProjectSelectionContext context = ProjectSelectionContext.TryCreate(selectionService);
                AppendLog("Selection context acquired: " + (context != null));
                if (context == null)
                {
                    MessageBox.Show(
                        LocalizedStrings.SelectionNotRecognizedMessage,
                        LocalizedStrings.DialogWindowTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                AddCppClassDialogResult dialogResult = AddCppClassDialog.ShowDialog(context.DefaultLocation);
                AppendLog("Prompt result='" + (dialogResult?.ClassName ?? "<cancel>") + "', inline=" + dialogResult?.IsInline);
                if (dialogResult == null)
                {
                    return;
                }

                ValidateClassName(dialogResult.ClassName);
                string headerFileName = NormalizeFileName(dialogResult.HeaderFileName, ".h");
                string sourceFileName = dialogResult.IsInline ? string.Empty : NormalizeFileName(dialogResult.SourceFileName, ".cpp");
                string baseClassName = NormalizeBaseClassName(dialogResult.BaseClassName);

                string headerPath = Path.Combine(context.DefaultLocation, headerFileName);
                string sourcePath = dialogResult.IsInline ? string.Empty : Path.Combine(context.DefaultLocation, sourceFileName);
                AppendLog("Creating files: " + headerPath + (dialogResult.IsInline ? " | <inline>" : " | " + sourcePath));

                if (File.Exists(headerPath))
                {
                    throw new InvalidOperationException(LocalizedStrings.DuplicateHeaderFileMessage);
                }

                if (!dialogResult.IsInline)
                {
                    if (string.Equals(headerPath, sourcePath, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(LocalizedStrings.HeaderSourceMustDifferMessage);
                    }

                    if (File.Exists(sourcePath))
                    {
                        throw new InvalidOperationException(LocalizedStrings.DuplicateSourceFileMessage);
                    }
                }

                Directory.CreateDirectory(context.DefaultLocation);
                File.WriteAllText(headerPath, BuildHeaderContent(dialogResult.ClassName, baseClassName, dialogResult.InheritanceAccess));
                context.AddFileToProject(headerPath);

                if (!dialogResult.IsInline)
                {
                    File.WriteAllText(sourcePath, BuildSourceContent(headerFileName));
                    context.AddFileToProject(sourcePath);
                }

                AppendLog("Files added to project.");
                VsShellUtilities.OpenDocument(package, headerPath);
                AppendLog("Header document opened.");
            }
            catch (Exception ex)
            {
                AppendLog("Execute failure: " + ex);
                MessageBox.Show(
                    ex.Message,
                    LocalizedStrings.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void ValidateClassName(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                throw new InvalidOperationException(LocalizedStrings.EnterClassNameMessage);
            }

            if (!ValidClassNamePattern.IsMatch(className))
            {
                throw new InvalidOperationException(LocalizedStrings.InvalidClassNameMessage);
            }
        }

        private static string NormalizeFileName(string fileName, string defaultExtension)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new InvalidOperationException(LocalizedStrings.EnterFileNameMessage);
            }

            string trimmed = fileName.Trim();
            if (!string.Equals(trimmed, Path.GetFileName(trimmed), StringComparison.Ordinal))
            {
                throw new InvalidOperationException(LocalizedStrings.FileNamesMustNotContainPathMessage);
            }

            if (trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new InvalidOperationException(LocalizedStrings.InvalidFileNameCharactersMessage);
            }

            if (string.IsNullOrEmpty(Path.GetExtension(trimmed)))
            {
                trimmed += defaultExtension;
            }

            return trimmed;
        }

        private static string NormalizeBaseClassName(string baseClassName)
        {
            if (string.IsNullOrWhiteSpace(baseClassName))
            {
                return string.Empty;
            }

            string trimmed = baseClassName.Trim();
            if (trimmed.Contains("\r") || trimmed.Contains("\n"))
            {
                throw new InvalidOperationException(LocalizedStrings.BaseClassSingleLineMessage);
            }

            return trimmed;
        }

        private static string BuildHeaderContent(string className, string baseClassName, string inheritanceAccess)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("#pragma once");
            builder.AppendLine();
            builder.Append("class ").Append(className);
            if (!string.IsNullOrEmpty(baseClassName))
            {
                builder.Append(" : ").Append(inheritanceAccess).Append(' ').Append(baseClassName);
            }

            builder.AppendLine();
            builder.AppendLine("{");
            builder.AppendLine("};");
            return builder.ToString();
        }

        private static string BuildSourceContent(string headerFileName)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("#include \"").Append(Path.GetFileName(headerFileName)).AppendLine("\"");
            return builder.ToString();
        }

        private static void AppendLog(string message)
        {
            try
            {
                File.AppendAllText(LogPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + message + Environment.NewLine);
            }
            catch
            {
            }
        }
    }
}

