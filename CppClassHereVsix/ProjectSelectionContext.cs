using System;
using System.IO;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CppClassHereVsix
{
    internal sealed class ProjectSelectionContext
    {
        private static readonly string LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CppClassHereVsix.log");

        private ProjectSelectionContext(Project dteProject, ProjectItem targetProjectItem, string defaultLocation)
        {
            DteProject = dteProject;
            TargetProjectItem = targetProjectItem;
            DefaultLocation = defaultLocation;
        }

        public Project DteProject { get; }

        public ProjectItem TargetProjectItem { get; }

        public string DefaultLocation { get; }

        public static ProjectSelectionContext TryCreate(IVsMonitorSelection selectionService)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (selectionService == null)
            {
                AppendLog("TryCreate: selection service is null.");
                return null;
            }

            IntPtr hierarchyPointer = IntPtr.Zero;
            IntPtr selectionContainerPointer = IntPtr.Zero;

            try
            {
                uint itemId;
                IVsMultiItemSelect multiItemSelect;

                int hr = selectionService.GetCurrentSelection(
                    out hierarchyPointer,
                    out itemId,
                    out multiItemSelect,
                    out selectionContainerPointer);

                AppendLog("TryCreate: GetCurrentSelection hr=" + hr + ", hierarchyPointer=" + hierarchyPointer + ", itemId=" + itemId + ", multiItemSelect=" + (multiItemSelect != null));

                if (ErrorHandler.Failed(hr) || hierarchyPointer == IntPtr.Zero || multiItemSelect != null)
                {
                    AppendLog("TryCreate: current selection rejected early.");
                    return null;
                }

                IVsHierarchy hierarchy = Marshal.GetObjectForIUnknown(hierarchyPointer) as IVsHierarchy;
                IVsProject vsProject = hierarchy as IVsProject;
                AppendLog("TryCreate: hierarchy type=" + hierarchy?.GetType().FullName + ", isProject=" + (vsProject != null));
                if (hierarchy == null || vsProject == null)
                {
                    AppendLog("TryCreate: hierarchy/project cast failed.");
                    return null;
                }

                string projectFilePath = GetProjectFilePath(hierarchy);
                AppendLog("TryCreate: projectFilePath='" + projectFilePath + "'");
                if (string.IsNullOrEmpty(projectFilePath) || !projectFilePath.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase))
                {
                    AppendLog("TryCreate: project file path is not vcxproj.");
                    return null;
                }

                string projectDirectory = Path.GetDirectoryName(projectFilePath);
                AppendLog("TryCreate: projectDirectory='" + projectDirectory + "'");
                if (string.IsNullOrEmpty(projectDirectory) || !Directory.Exists(projectDirectory))
                {
                    AppendLog("TryCreate: project directory invalid.");
                    return null;
                }

                Project dteProject = GetHierarchyProperty(hierarchy, VSConstants.VSITEMID_ROOT, __VSHPROPID.VSHPROPID_ExtObject) as Project;
                AppendLog("TryCreate: dteProject=" + dteProject?.UniqueName);
                if (dteProject == null)
                {
                    AppendLog("TryCreate: root EnvDTE.Project unavailable.");
                    return null;
                }

                if (itemId == VSConstants.VSITEMID_ROOT)
                {
                    AppendLog("TryCreate: root item selected.");
                    return new ProjectSelectionContext(dteProject, null, projectDirectory);
                }

                ProjectItem selectedProjectItem = GetHierarchyProperty(hierarchy, itemId, __VSHPROPID.VSHPROPID_ExtObject) as ProjectItem;
                AppendLog("TryCreate: selectedProjectItem type=" + selectedProjectItem?.GetType().FullName);
                if (selectedProjectItem != null)
                {
                    ProjectSelectionContext fromProjectItem = CreateFromProjectItem(dteProject, selectedProjectItem, itemId, projectDirectory, hierarchy);
                    AppendLog("TryCreate: project item path result=" + (fromProjectItem != null ? fromProjectItem.DefaultLocation : "null"));
                    if (fromProjectItem != null)
                    {
                        return fromProjectItem;
                    }
                }

                string canonicalPath = GetCanonicalName(hierarchy, itemId);
                AppendLog("TryCreate: item canonical='" + canonicalPath + "', isDirectory=" + (!string.IsNullOrEmpty(canonicalPath) && Directory.Exists(canonicalPath)));
                if (!string.IsNullOrEmpty(canonicalPath) && Directory.Exists(canonicalPath))
                {
                    return new ProjectSelectionContext(dteProject, null, canonicalPath);
                }

                AppendLog("TryCreate: no supported selection mapping found.");
                return null;
            }
            finally
            {
                if (hierarchyPointer != IntPtr.Zero)
                {
                    Marshal.Release(hierarchyPointer);
                }

                if (selectionContainerPointer != IntPtr.Zero)
                {
                    Marshal.Release(selectionContainerPointer);
                }
            }
        }

        public void AddFileToProject(string filePath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            ProjectItems targetItems = TargetProjectItem?.ProjectItems;
            if (targetItems != null)
            {
                AppendLog("AddFileToProject: adding to selected item container '" + SafeGetName(TargetProjectItem) + "' => " + filePath);
                targetItems.AddFromFile(filePath);
                return;
            }

            ProjectItems rootItems = DteProject?.ProjectItems;
            if (rootItems == null)
            {
                throw new InvalidOperationException(LocalizedStrings.ProjectItemsUnavailableMessage);
            }

            AppendLog("AddFileToProject: adding to project root => " + filePath);
            rootItems.AddFromFile(filePath);
        }

        private static ProjectSelectionContext CreateFromProjectItem(Project dteProject, ProjectItem projectItem, uint itemId, string projectDirectory, IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string kind = SafeGetKind(projectItem);
            string fullPath = TryGetPhysicalFolderPath(projectItem);
            AppendLog("CreateFromProjectItem: kind='" + kind + "', name='" + SafeGetName(projectItem) + "', fullPath='" + fullPath + "'");

            if (StringComparer.OrdinalIgnoreCase.Equals(kind, EnvDTE.Constants.vsProjectItemKindPhysicalFolder))
            {
                if (!string.IsNullOrEmpty(fullPath) && Directory.Exists(fullPath))
                {
                    return new ProjectSelectionContext(dteProject, projectItem, fullPath);
                }

                string canonicalPath = GetCanonicalName(hierarchy, itemId);
                if (!string.IsNullOrEmpty(canonicalPath) && Directory.Exists(canonicalPath))
                {
                    return new ProjectSelectionContext(dteProject, projectItem, canonicalPath);
                }

                return new ProjectSelectionContext(dteProject, projectItem, projectDirectory);
            }

            if (StringComparer.OrdinalIgnoreCase.Equals(kind, EnvDTE.Constants.vsProjectItemKindVirtualFolder))
            {
                return new ProjectSelectionContext(dteProject, projectItem, projectDirectory);
            }

            if (StringComparer.OrdinalIgnoreCase.Equals(kind, EnvDTE.Constants.vsProjectItemKindSubProject) ||
                StringComparer.OrdinalIgnoreCase.Equals(kind, EnvDTE.Constants.vsProjectItemKindPhysicalFile))
            {
                return null;
            }

            return null;
        }

        private static string GetProjectFilePath(IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string canonicalName = GetCanonicalName(hierarchy, VSConstants.VSITEMID_ROOT);
            if (!string.IsNullOrEmpty(canonicalName))
            {
                return canonicalName;
            }

            string saveName = GetHierarchyProperty(hierarchy, VSConstants.VSITEMID_ROOT, __VSHPROPID.VSHPROPID_SaveName) as string;
            if (!string.IsNullOrEmpty(saveName) && Path.IsPathRooted(saveName))
            {
                return saveName;
            }

            return saveName;
        }

        private static object GetHierarchyProperty(IVsHierarchy hierarchy, uint itemId, __VSHPROPID propertyId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            object value;
            return ErrorHandler.Succeeded(hierarchy.GetProperty(itemId, (int)propertyId, out value)) ? value : null;
        }

        private static string GetCanonicalName(IVsHierarchy hierarchy, uint itemId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string canonicalName;
            return ErrorHandler.Succeeded(hierarchy.GetCanonicalName(itemId, out canonicalName)) ? canonicalName : null;
        }

        private static string SafeGetKind(ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                return projectItem.Kind;
            }
            catch (COMException)
            {
                return string.Empty;
            }
        }

        private static string SafeGetName(ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                return projectItem.Name;
            }
            catch (COMException)
            {
                return string.Empty;
            }
        }

        private static string TryGetPhysicalFolderPath(ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string fullPathProperty = TryGetProjectItemProperty(projectItem, "FullPath");
            if (!string.IsNullOrEmpty(fullPathProperty))
            {
                return fullPathProperty;
            }

            try
            {
                string fileName = projectItem.FileNames[1];
                return string.IsNullOrEmpty(fileName) ? null : fileName;
            }
            catch (COMException)
            {
                return null;
            }
        }

        private static string TryGetProjectItemProperty(ProjectItem projectItem, string propertyName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (projectItem.Properties == null)
            {
                return null;
            }

            try
            {
                Property property = projectItem.Properties.Item(propertyName);
                return property == null ? null : property.Value as string;
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (COMException)
            {
                return null;
            }
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
