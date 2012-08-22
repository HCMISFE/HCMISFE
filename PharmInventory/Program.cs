using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using PharmInventory.Forms.Modals;
using System.Deployment.Application;
using System.ComponentModel;
using Microsoft.Win32;

namespace PharmInventory
{
    static class Program
    {
        public const string RegKey = "Software\\JSI\\HCMIS\\Configuration";
        public const string PrevConnectionStringKey = "Software\\JSI\\HCMIS\\Configuration\\ConnectionStringManager\\History";

        public static ConnectionStringManager.ConnectionStringManager ConnStringManager;

        public static ApplicationDeployment HCMIS;

        public static bool UpdateRunning, RestartRequired;


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            DevExpress.UserSkins.OfficeSkins.Register();
            DevExpress.UserSkins.BonusSkins.Register();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ConnStringManager = new ConnectionStringManager.ConnectionStringManager(RegKey, PrevConnectionStringKey);

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                HCMIS = ApplicationDeployment.CurrentDeployment;
                HCMIS.CheckForUpdateCompleted += new CheckForUpdateCompletedEventHandler(HCMIS_CheckForUpdateCompleted);
                HCMIS.UpdateCompleted += new System.ComponentModel.AsyncCompletedEventHandler(HCMIS_UpdateCompleted);
            }

            //If the user opens the application while holding down the shift key, we want to open the configuration options before the login form.
            if (Control.ModifierKeys == Keys.Shift)
            {
                Application.Run(new LoginForm(true));
            }

            Application.Run(new LoginForm());
        }

        public static string HCMISVersionString
        {
            get
            {
                string versionInfo = "";
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    if (HCMIS == null)
                        HCMIS = ApplicationDeployment.CurrentDeployment;

                    versionInfo = HCMIS.CurrentVersion.Major.ToString();
                    versionInfo += "." + HCMIS.CurrentVersion.Minor;
                    versionInfo += "." + HCMIS.CurrentVersion.Build;
                    versionInfo += "." + HCMIS.CurrentVersion.MinorRevision;
                }
                else
                {
                    versionInfo = Application.ProductVersion;
                }
                return versionInfo;
            }
        }

        public static void SaveHCMISVersionToRegistry()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(RegKey);
            string versionString = "";

            if (key == null)
            {
                key = Registry.CurrentUser.CreateSubKey(RegKey);
            }
            try
            {
                versionString = key.GetValue("ApplicationVersion").ToString();
            }
            catch
            {
                key = Registry.CurrentUser.CreateSubKey(RegKey);
                key.SetValue("ApplicationVersion", "");
            }
            
            HelperClasses.RegistrationHelper.UpdateApplicationVersionInfo();
            RegistryKey regKey = Registry.CurrentUser.CreateSubKey(RegKey);
            regKey.SetValue("ApplicationVersion", HCMISVersionString);
        }

        public static void ShowHCMISVersionInfoMessageBox()
        {
            XtraMessageBox.Show(string.Format("HCMIS Version: {0}", HCMISVersionString), "Version Information",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void UpdateDatabase()
        {
            PharmInventory.HelperClasses.DatabaseHelpers.RunScriptsOnDatabase();
        }

        public static void UpdateApplication()
        {
            if (!ApplicationDeployment.IsNetworkDeployed)
            {
                XtraMessageBox.Show("The application is not network deployed and can't be updated online.",
                                    "Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (RestartRequired)
            {
                XtraMessageBox.Show("The application needs to be restarted!", "Restart", MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                return;
            }

            if (UpdateRunning)
                return;

            UpdateRunning = true;
            HCMIS.CheckForUpdateAsync();
        }

        private static void HCMIS_CheckForUpdateCompleted(object sender, CheckForUpdateCompletedEventArgs e)
        {
            UpdateRunning = false;

            if (e.Error != null)
            {
                XtraMessageBox.Show("An Error occurred:\n" + e.Error.Message, "Error", MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                return;
            }

            if (!e.UpdateAvailable)
            {
                XtraMessageBox.Show("No updates found!", "HCMIS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (e.Cancelled)
            {
                XtraMessageBox.Show("Update was cancelled!", "Update Cancelled", MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                return;
            }

            if (e.UpdateAvailable)
            {
                if (e.IsUpdateRequired)
                {
                    XtraMessageBox.Show("There is a mandatory update available. Click OK to start installation", "Update Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateRunning = true;
                    HCMIS.UpdateAsync();
                    ShowUpdatingSignOnMainWindow();
                }

                if (XtraMessageBox.Show("Update found! Would you like to download and install the update now?", "Update Found", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    UpdateRunning = true;
                    HCMIS.UpdateAsync();
                    ShowUpdatingSignOnMainWindow();
                }
            }
        }

        private static void ShowUpdatingSignOnMainWindow()
        {
            MainWindow.Instance.mnuCheckForUpdates.Text = "Updating...";
        }

        private static void HCMIS_UpdateCompleted(object sender, AsyncCompletedEventArgs e)
        {
            UpdateRunning = false;

            if (e.Cancelled)
            {
                XtraMessageBox.Show("Update cancelled!", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (e.Error != null)
            {
                XtraMessageBox.Show("Couldn't Install application! Reason: \n" + e.Error.Message);
            }

            //If it reaches here, a restart is required for the update to be applied.
            RestartRequired = true;
            if (XtraMessageBox.Show("Update Completed! Update will not take effect until you restart HCMIS.  Restart HCMIS now?", "Success", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Restart();
            }

            HideUpdatingSignOnMainWindow();
        }

        private static void HideUpdatingSignOnMainWindow()
        {
            MainWindow.Instance.mnuCheckForUpdates.Text = "Check for Updates";
        }





    }
}