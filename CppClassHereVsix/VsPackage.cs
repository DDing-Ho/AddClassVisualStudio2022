using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace CppClassHereVsix
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideMenuResource("Menus.CTMENU", 1)]
    [Guid(PackageGuidString)]
    public sealed class VsPackage : Package
    {
        public const string PackageGuidString = "f0434c21-891e-4dd4-b097-ce71dda1cd08";
        private static readonly string LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CppClassHereVsix.Package.log");

        protected override void Initialize()
        {
            try
            {
                AppendLog("Initialize start");
                AddCppClassHereCommand.Initialize(this);
                AppendLog("Initialize success");
            }
            catch (Exception ex)
            {
                AppendLog("Initialize failure: " + ex);
                throw;
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
