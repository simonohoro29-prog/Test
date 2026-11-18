using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;

namespace HelloSwAddin
{
    /// <summary>
    /// Minimal SOLIDWORKS add-in: adds a "Hello SW Add-in" menu with one command.
    /// SOLIDWORKS finds it through the registry keys written in <see cref="Register"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid(AddinGuid)]
    [ProgId("HelloSwAddin.SwAddin")]
    public class SwAddin : ISwAddin
    {
        // Change this GUID if you copy this project to make a different add-in.
        public const string AddinGuid = "00c0efad-5915-478e-b275-6e69e66fa6d2";
        private const string AddinTitle = "Hello SW Add-in";
        private const string AddinDescription = "Sample SOLIDWORKS add-in skeleton";

        // IDs SOLIDWORKS uses to remember the command group and its items.
        private const int MainCmdGroupId = 5;
        private const int HelloCmdId = 0;

        private ISldWorks _swApp;
        private ICommandManager _cmdMgr;
        private int _addinCookie;

        #region ISwAddin

        public bool ConnectToSW(object ThisSW, int Cookie)
        {
            _swApp = (ISldWorks)ThisSW;
            _addinCookie = Cookie;

            // Tells SOLIDWORKS to call public methods on this object by name (menu callbacks).
            _swApp.SetAddinCallbackInfo2(0, this, _addinCookie);

            _cmdMgr = _swApp.GetCommandManager(_addinCookie);
            AddCommands();
            return true;
        }

        public bool DisconnectFromSW()
        {
            RemoveCommands();

            if (_cmdMgr != null) Marshal.ReleaseComObject(_cmdMgr);
            if (_swApp != null) Marshal.ReleaseComObject(_swApp);
            _cmdMgr = null;
            _swApp = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return true;
        }

        #endregion

        #region Commands

        private void AddCommands()
        {
            int[] knownIds = { HelloCmdId };

            // If the command list changed since SOLIDWORKS last saw it, discard its cached layout.
            bool ignorePrevious = false;
            if (_cmdMgr.GetGroupDataFromRegistry(MainCmdGroupId, out object registryIds)
                && !SameIds(registryIds as int[], knownIds))
            {
                ignorePrevious = true;
            }

            int errors = 0;
            ICommandGroup group = _cmdMgr.CreateCommandGroup2(
                MainCmdGroupId, AddinTitle, AddinDescription, AddinDescription,
                -1, ignorePrevious, ref errors);

            group.AddCommandItem2(
                "Say Hello",                    // name
                -1,                             // position (-1 = end)
                "Show info about the active document", // hint
                "Say Hello",                    // tooltip
                0,                              // image index (no icons yet)
                nameof(OnHello),                // callback
                nameof(OnHelloEnable),          // enable-state callback
                HelloCmdId,
                (int)swCommandItemType_e.swMenuItem);

            group.HasMenu = true;
            group.HasToolbar = false; // add icons (group.IconList) before turning this on
            group.Activate();
        }

        private void RemoveCommands()
        {
            _cmdMgr?.RemoveCommandGroup(MainCmdGroupId);
        }

        private static bool SameIds(int[] a, int[] b)
        {
            if (a == null || a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (Array.IndexOf(b, a[i]) < 0) return false;
            return true;
        }

        // Callbacks must be public: SOLIDWORKS invokes them by name via COM.

        public void OnHello()
        {
            string msg;
            if (_swApp.ActiveDoc is ModelDoc2 doc)
            {
                string type;
                switch ((swDocumentTypes_e)doc.GetType())
                {
                    case swDocumentTypes_e.swDocPART: type = "Part"; break;
                    case swDocumentTypes_e.swDocASSEMBLY: type = "Assembly"; break;
                    case swDocumentTypes_e.swDocDRAWING: type = "Drawing"; break;
                    default: type = "Document"; break;
                }
                string path = doc.GetPathName();
                msg = $"Hello from {AddinTitle}!\n\n{type}: {doc.GetTitle()}\n" +
                      $"Path: {(string.IsNullOrEmpty(path) ? "(not saved yet)" : path)}";
            }
            else
            {
                msg = $"Hello from {AddinTitle}!\n\nNo document is open.";
            }

            _swApp.SendMsgToUser2(msg,
                (int)swMessageBoxIcon_e.swMbInformation,
                (int)swMessageBoxBtn_e.swMbOk);
        }

        /// <returns>0 = disabled, 1 = enabled, 2 = unchecked, 3 = checked.</returns>
        public int OnHelloEnable() => 1;

        #endregion

        #region COM registration

        private const string AddinsKey = @"SOFTWARE\SolidWorks\Addins\{" + AddinGuid + "}";
        private const string StartupKey = @"Software\SolidWorks\AddInsStartup\{" + AddinGuid + "}";

        [ComRegisterFunction]
        public static void Register(Type t)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(AddinsKey))
                {
                    key.SetValue(null, 0); // 0 = not loaded by default in the Add-Ins dialog
                    key.SetValue("Title", AddinTitle);
                    key.SetValue("Description", AddinDescription);
                }
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(StartupKey))
                {
                    key.SetValue(null, 1); // 1 = load at startup for this user
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to register {AddinTitle}: {ex.Message}");
                throw;
            }
        }

        [ComUnregisterFunction]
        public static void Unregister(Type t)
        {
            try
            {
                Registry.LocalMachine.DeleteSubKey(AddinsKey, false);
                Registry.CurrentUser.DeleteSubKey(StartupKey, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to unregister {AddinTitle}: {ex.Message}");
            }
        }

        #endregion
    }
}
