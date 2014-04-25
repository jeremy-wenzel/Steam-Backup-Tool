﻿/*
 *-------------------------------------------------------------------------------------------------------------------------*
 * --==--This program is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.--==--
 *                           --==--http://creativecommons.org/licenses/by-nc-sa/3.0/--==--
 *-------------------------------------------------------------------------------------------------------------------------*
 */

namespace steamBackup
{
    using Microsoft.Win32;
    using steamBackup.AppServices;
    using steamBackup.AppServices.Errors;
    using steamBackup.AppServices.Jobs;
    using steamBackup.AppServices.Tasks;
    using steamBackup.Forms;
    using steamBackup.Properties;
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Threading;
    using System.Windows.Forms;

    public partial class Main : Form
    {
        readonly string _versionNum = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        bool _cancelJob;
        bool _pauseJob;
        int _threadDone;

        Task _task;

        readonly Job[] _currJobs = new Job[4];
        readonly Thread[] _threadList = new Thread[4];

        private void main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Save(); // save Main form settings then close
        }

        // Save Main form settings
        private void Save()
        {
            Settings.BackupDir = tbxBackupDir.Text;
            Settings.SteamDir = tbxSteamDir.Text;
            Settings.Save();
        }

        public Main()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;

            // Load Main form settings
            Settings.Load();
            tbxSteamDir.Text = Settings.SteamDir;
            tbxBackupDir.Text = Settings.BackupDir;

            lblStarted.Text = null;
            lbl0.Text = string.Format(Resources.VersionStr, _versionNum);
        }

        private void btnBrowseSteam_Click(object sender, EventArgs e)
        {
            var fdlg = new FolderBrowserDialog();
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                tbxSteamDir.Text = fdlg.SelectedPath;
            }
        }

        private void btnBrowseBackup_Click(object sender, EventArgs e)
        {
            var fdlg = new FolderBrowserDialog();
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                tbxBackupDir.Text = fdlg.SelectedPath;
            }

        }

        // Check to see if a steam install directory is valid
        private bool IsValidSteamFolder()
        {
            return File.Exists(tbxSteamDir.Text + "\\config\\config.vdf");
        }

        // Check to see if a backup directory is valid
        private bool IsValidBackupFolder()
        {
            if(File.Exists(tbxBackupDir.Text + "\\config.sbt"))
            {
                // Valid Archiver Version 2
                return true;
            }

            return Directory.Exists(tbxBackupDir.Text + "\\common\\") && 
                   File.Exists(tbxBackupDir.Text + "\\games.7z") &&
                   File.Exists(tbxBackupDir.Text + "\\steamapps.7z");
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("Steam");
            if (pname.Length != 0 && Settings.CheckSteamRun)
            {
                MessageBox.Show(Resources.BackupSteamRunningText, Resources.SteamRunningTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else if (!IsValidSteamFolder())
            {
                MessageBox.Show(string.Format(Resources.NotValidSteamDirectory, tbxSteamDir.Text));
            }
            else
            {
                Save();

                // Open Backup User Control Window
                var backupUserCtrl = new BackupUserCtrl();
                backupUserCtrl.ShowDialog(this);

                if (backupUserCtrl.Canceled)
                    return;

                // create folders if needed
                if (!Directory.Exists(tbxBackupDir.Text))
                    Directory.CreateDirectory(tbxBackupDir.Text);
                if (!Directory.Exists(tbxBackupDir.Text + "\\common"))
                    Directory.CreateDirectory(tbxBackupDir.Text + "\\common");
                if (!Directory.Exists(tbxBackupDir.Text + "\\acf"))
                    Directory.CreateDirectory(tbxBackupDir.Text + "\\acf");

                _task = backupUserCtrl.GetTask();
                Start();
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("Steam");
            if (pname.Length != 0 && Settings.CheckSteamRun)
            {
                MessageBox.Show(Resources.RestoreSteamRunningText, Resources.SteamRunningTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else if (!IsValidSteamFolder())
            {
                MessageBox.Show(string.Format(Resources.NotValidSteamDirectory, tbxSteamDir.Text));
            }
            else if (!IsValidBackupFolder())
            {
                MessageBox.Show(string.Format(Resources.NotValidSteamBackupDirectory, tbxBackupDir.Text));
            }
            else
            {
                Save();

                // Open Backup User Control Window
                var restoreUserCtrl = new RestoreUserCtrl();
                restoreUserCtrl.ShowDialog(this);

                if (restoreUserCtrl.Canceled)
                    return;

                _task = restoreUserCtrl.GetTask();
                Start();

            }
        }

        private void Start()
        {
            // set UI to starting values
            pgsBarAll.Value = 0;
            pgsBarAll.Maximum = _task.JobsToDoCount;

            btnBackup.Visible = false;
            btnRestore.Visible = false;

            btnBrowseSteam.Enabled = false;
            btnFindSteam.Enabled = false;
            btnBrowseBackup.Enabled = false;
            tbxSteamDir.Enabled = false;
            tbxBackupDir.Enabled = false;

            lblStarted.Text = string.Format(Resources.ProcessingStarted, DateTime.Now.ToString("H:mm.ss dd/MM/yyyy"));
            _cancelJob = false;
            _pauseJob = false;
            _threadDone = 0;

            btnCancel.Visible = true;
            btnPause.Visible = true;
            btnShowLog.Visible = true;
            btnUpdWiz.Visible = false;

            timer.Start();

            // Launch task threads
            StartThreads();
        }

        private void StartThreads()
        {
            // setup the UI for each thread that is running
            if (_task.ThreadCount >= 1)
            {
                _threadList[0] = new Thread(() => DoWork(0))
                {
                    Priority = ThreadPriority.Lowest,
                    Name = string.Format(Resources.JobThreadText, 1),
                    IsBackground = true
                };
                _threadList[0].Start();

                lbl0.Text = string.Format(Resources.InstanceNumText, 1);
                lbl0Info.Text = Resources.WaitingText;
                lbl0SpeedEta.Text = string.Empty;
                this.Size = new Size(400, 482);
                lbl1.Text = string.Format(Resources.VersionStr, _versionNum);
            }
            if (_task.ThreadCount >= 2 && !Settings.UseLzma2)
            {
                _threadList[1] = new Thread(() => DoWork(1))
                {
                    Priority = ThreadPriority.Lowest,
                    Name = string.Format(Resources.JobThreadText, 2),
                    IsBackground = true
                };
                _threadList[1].Start();

                lbl1.Text = string.Format(Resources.InstanceNumText, 2);
                lbl1Info.Text = Resources.WaitingText;
                lbl1SpeedEta.Text = string.Empty;
                this.Size = new Size(400, 562);
                lbl2.Text = string.Format(Resources.VersionStr, _versionNum);
            }
            if (_task.ThreadCount >= 3 && !Settings.UseLzma2)
            {
                _threadList[2] = new Thread(() => DoWork(2))
                {
                    Priority = ThreadPriority.Lowest,
                    Name = string.Format(Resources.JobThreadText, 3),
                    IsBackground = true
                };
                _threadList[2].Start();

                lbl2.Text = string.Format(Resources.InstanceNumText, 3);
                lbl2Info.Text = Resources.WaitingText;
                lbl2SpeedEta.Text = string.Empty;
                this.Size = new Size(400, 642);
                lbl3.Text = string.Format(Resources.VersionStr, _versionNum);
            }
            if (_task.ThreadCount >= 4 && !Settings.UseLzma2)
            {
                _threadList[3] = new Thread(() => DoWork(3))
                {
                    Priority = ThreadPriority.Lowest,
                    Name = string.Format(Resources.JobThreadText, 4),
                    IsBackground = true
                };
                _threadList[3].Start();

                lbl3.Text = string.Format(Resources.InstanceNumText, 4);
                lbl3Info.Text = Resources.WaitingText;
                lbl3SpeedEta.Text = string.Empty;
                this.Size = new Size(400, 722);
                lbl4.Text = string.Format(Resources.VersionStr, _versionNum);
            }
        }

        private void DoWork(int thread)
        {
            Thread.Sleep(1000 * thread);

            ProgressBar pgsBar = null;
            Label lblJobTitle = null;
            Label lblJobFile = null;
            Label lblJobSpeedEta = null;

            switch (thread)
            {
                case 0:
                    pgsBar = pgsBar0;
                    lblJobTitle = lbl0;
                    lblJobFile = lbl0Info;
                    lblJobSpeedEta = lbl0SpeedEta;
                    break;
                case 1:
                    pgsBar = pgsBar1;
                    lblJobTitle = lbl1;
                    lblJobFile = lbl1Info;
                    lblJobSpeedEta = lbl1SpeedEta;
                    break;
                case 2:
                    pgsBar = pgsBar2;
                    lblJobTitle = lbl2;
                    lblJobFile = lbl2Info;
                    lblJobSpeedEta = lbl2SpeedEta;
                    break;
                case 3:
                    pgsBar = pgsBar3;
                    lblJobTitle = lbl3;
                    lblJobFile = lbl3Info;
                    lblJobSpeedEta = lbl3SpeedEta;
                    break;
            }

            while (_task.JobsAnalysed < _task.JobCount && !_cancelJob)
            {
                Job job = _task.GetNextJob();
                if (job == null)
                    break;

                _currJobs[thread] = job;
                if (pgsBar != null) 
                    pgsBar.Value = 0;
                pgsBarAll.Value = _task.JobsDone;
                lblProgress.Text = _task.ProgressText();
                job.Status = JobStatus.Working;
                UpdateList();

                if (lblJobFile != null)
                    lblJobFile.Text = Resources.FindingFilesText;

                if (lblJobSpeedEta != null) 
                    lblJobSpeedEta.Text = string.Empty;

                job.Start();

                if(job.GetJobType() == JobType.Backup)
                    CopyAcfToBackup(job);
                else
                    CopyAcfToRestore(job);
                
                UpdateList();

                if (lblJobFile != null) 
                    lblJobFile.Text = Resources.FinishedJobText;

                if(_cancelJob)
                    job.Status = JobStatus.Canceled;
                _currJobs[thread] = null;
            }

            if (pgsBar != null) 
                pgsBar.Value = 0;

            if (lblJobTitle != null) 
                lblJobTitle.Text = string.Format(Resources.InstanceFinishedText, (thread + 1));

            if (lblJobFile != null) 
                lblJobFile.Text = Resources.NoJobsText;

            if (lblJobSpeedEta != null) 
                lblJobSpeedEta.Text = string.Empty;

            JobsFinished();
        }

        // Used to update the UI at each tick
        private void timer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                if (_currJobs[i] != null)
                {
                    UpdateStats(i, _currJobs[i]);
                }
            }
        }

        // updates the UI
        private void UpdateStats(int thread, Job job)
        {
            ProgressBar pgsBar = null;
            Label lblJobTitle = null;
            Label lblJobFile = null;
            Label lblJobSpeedEta = null;

            switch (thread)
            {
                case 0:
                    pgsBar = pgsBar0;
                    lblJobTitle = lbl0;
                    lblJobFile = lbl0Info;
                    lblJobSpeedEta = lbl0SpeedEta;
                    break;
                case 1:
                    pgsBar = pgsBar1;
                    lblJobTitle = lbl1;
                    lblJobFile = lbl1Info;
                    lblJobSpeedEta = lbl1SpeedEta;
                    break;
                case 2:
                    pgsBar = pgsBar2;
                    lblJobTitle = lbl2;
                    lblJobFile = lbl2Info;
                    lblJobSpeedEta = lbl2SpeedEta;
                    break;
                case 3:
                    pgsBar = pgsBar3;
                    lblJobTitle = lbl3;
                    lblJobFile = lbl3Info;
                    lblJobSpeedEta = lbl3SpeedEta;
                    break;
            }

            string name = job.Name;
            if (job.Name.Length >= 28)
                name = job.Name.Substring(0, 25) + "...";

            if (lblJobTitle != null)
                lblJobTitle.Text = string.Format(Resources.InstanceProcessing, (thread + 1), job.Status, name);

            if (pgsBar != null) 
                pgsBar.Value = job.GetPercDone();

            if (lblJobSpeedEta != null) 
                lblJobSpeedEta.Text = job.GetSpeedEta();

            if (string.IsNullOrEmpty(job.GetCurFileStr())) return;

            if (lblJobFile != null) 
                lblJobFile.Text = job.GetCurFileStr();
        }

        // Copies ACF files from the steam install to Backup
        private void CopyAcfToBackup(Job job)
        {
            if (!String.IsNullOrEmpty(job.AcfFiles))
            {
                string[] acfId = job.AcfFiles.Split('|');

                foreach(string id in acfId)
                {
                    string src = job.AcfDir + "\\appmanifest_" + id + ".acf";
                    string dst = tbxBackupDir.Text + "\\acf";

                    if (!Directory.Exists(dst))
                        Directory.CreateDirectory(dst);

                    var fi = new FileInfo(src);
                    StreamReader reader = fi.OpenText();

                    string acf = reader.ReadToEnd();
                    string gameCommonFolder = Utilities.UpDirLvl(job.GetSteamDir());
                    acf = acf.Replace(gameCommonFolder, "|DIRECTORY-STD|");
                    acf = acf.Replace(gameCommonFolder.ToLower(), "|DIRECTORY-LOWER|");
                    acf = acf.Replace(gameCommonFolder.ToLower().Replace("\\", "\\\\"), "|DIRECTORY-ESCSLASH-LOWER|");

                    File.WriteAllText(dst + "\\appmanifest_" + id + ".acf", acf);
                    reader.Close();
                }
            }
        }

        // Copies ACF files from the Backup to steam install
        private void CopyAcfToRestore(Job job)
        {
            if (String.IsNullOrEmpty(job.AcfFiles)) return;

            string[] acfId = job.AcfFiles.Split('|');

            foreach (string id in acfId)
            {
                string src = tbxBackupDir.Text + "\\acf\\appmanifest_" + id + ".acf";
                string dst = job.AcfDir;

                if (!Directory.Exists(dst))
                    Directory.CreateDirectory(dst);

                var fi = new FileInfo(src);
                StreamReader reader = fi.OpenText();

                string acf = reader.ReadToEnd();
                string gameCommonFolder = job.AcfDir + "common\\";
                acf = acf.Replace("|DIRECTORY-STD|", gameCommonFolder);
                acf = acf.Replace("|DIRECTORY-LOWER|", gameCommonFolder.ToLower());
                acf = acf.Replace("|DIRECTORY-ESCSLASH-LOWER|", gameCommonFolder.ToLower().Replace("\\", "\\\\"));

                File.WriteAllText(dst + "\\appmanifest_" + id + ".acf", acf);
                reader.Close();
            }
        }

        // Runs after each job is done.
        private void JobsFinished()
        {
            _threadDone++;
            if ((_task.ThreadCount == _threadDone && !Settings.UseLzma2) || Settings.UseLzma2)
            {
                timer.Stop();
                
                btnBrowseSteam.Enabled = true;
                btnFindSteam.Enabled = true;
                btnBrowseBackup.Enabled = true;
                tbxSteamDir.Enabled = true;
                tbxBackupDir.Enabled = true;
                btnBackup.Visible = true;
                btnRestore.Visible = true;
                btnCancel.Visible = false;
                btnPause.Visible = false;
                btnPause.Text = Resources.PauseText;
                btnShowLog.Visible = false;
                btnUpdWiz.Visible = true;
                this.Size = new Size(400, 402);
                lbl0.Text = string.Format(Resources.VersionStr, _versionNum);


                if (ErrorList.HasErrors())
                {
                    MessageBox.Show(string.Format(Resources.FinishedWithErrorsText, _task.JobsDone, _task.JobsToDoCount), Resources.FinishedWithErrorsTitle,MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(string.Format(Resources.FinishedText, _task.JobsDone, _task.JobsToDoCount), Resources.FinishedTitle);
                }

                ErrorList.Clear();
                lblProgress.Text = _task.ProgressText();
                _task = null;
            }
            else
            {
                lblProgress.Text = _task.ProgressText();
            }
        }

        private void tbxSteamDir_Enter(object sender, EventArgs e)
        {
            if (tbxSteamDir.Text == Resources.SteamInstallDir)
                tbxSteamDir.Text = "";
        }

        private void tbxBackupDir_Enter(object sender, EventArgs e)
        {
            if (tbxBackupDir.Text == Resources.BackupDir)
                tbxBackupDir.Text = "";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Resources.CancelQueryText, Resources.CancelQueryTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                _cancelJob = true;
                _pauseJob = false;
                btnPause.Visible = false;

                for (int i = 0; i < 4; i++)
                {
                    if (_currJobs[i] != null)
                        _currJobs[i].Status = JobStatus.Canceled;
                }
            }
            else
            {
                _cancelJob = true;
                _pauseJob = false;
                btnPause.Visible = false;
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (_pauseJob)
            {
                _pauseJob = false;
                btnPause.Text = Resources.PauseText;

                for (int i = 0; i < 4; i++)
                {
                    if (_currJobs[i] != null)
                        _currJobs[i].Status = JobStatus.Working;
                }
            }
            else
            {
                _pauseJob = true;
                btnPause.Text = Resources.ResumeText;

                for (int i = 0; i < 4; i++)
                {
                    if (_currJobs[i] != null)
                        _currJobs[i].Status = JobStatus.Paused;
                }
            }
        }

        // Uses steam reg keys to determine where steam is installed 
        private void btnFindSteam_Click(object sender, EventArgs e)
        {
            const string keyStr = @"Software\Valve\Steam";

            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(keyStr, false);
                if (key != null)
                    tbxSteamDir.Text = Utilities.GetFileSystemCasing((string) key.GetValue("SteamPath"));
                else
                {
                    key = Registry.LocalMachine.OpenSubKey(keyStr, false);
                    if (key != null)
                        tbxSteamDir.Text = (string) key.GetValue("InstallPath");
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(Resources.SteamFolderNotFound);
            }
            catch (SecurityException)
            {
                MessageBox.Show(Resources.SteamFolderNotFound);
            }
        }

        private void btnShowList_Click(object sender, EventArgs e)
        {
            if (Size.Width == 400)
            {
                this.Size = new Size(Size.Width + 600, Size.Height);
                listView.Size = new Size(listView.Size.Width, this.Size.Height - 50);
                btnShowLog.Text = Resources.JobListHideText;
                UpdateList();
            }
            else
            {
                this.Size = new Size(400, Size.Height);
                btnShowLog.Text = Resources.JobListShowText;
            }
        }

        private void UpdateList()
        {
            if (Size.Width != 400)
            {
                listView.Items.Clear();
                int i = 0;

                listView.BeginUpdate();
                foreach (Job job in _task.JobList)
                {
                    i++;
                    ListViewItem listItem = listView.Items.Add(i.ToString(CultureInfo.InvariantCulture));
                    listItem.SubItems.Add(job.Name);
                    listItem.SubItems.Add("");
                    listItem.SubItems.Add(job.Status.ToString());
                    listItem.SubItems.Add("");
                    listItem.SubItems.Add(job.AcfFiles);

                    switch (job.Status)
                    {
                        case JobStatus.Paused:
                            listItem.ForeColor = Color.Blue;
                            break;
                        case JobStatus.Waiting:
                            listItem.ForeColor = Color.Green;
                            break;
                        case JobStatus.Working:
                            listItem.ForeColor = Color.BlueViolet;
                            break;
                        case JobStatus.Skipped:
                            listItem.ForeColor = Color.DarkOrange;
                            break;
                        case JobStatus.Error:
                            listItem.ForeColor = Color.Red;
                            break;
                        case JobStatus.Canceled:
                            listItem.ForeColor = Color.Orange;
                            break;
                        case JobStatus.Finished:
                            listItem.ForeColor = Color.DarkBlue;
                            break;
                        default:
                            listItem.ForeColor = Color.Black;
                            break;
                    }
                }
                listView.EndUpdate();
            }
        }

        private void title_Click(object sender, EventArgs e)
        {
            var about = new AboutBox();
            about.ShowDialog();
        }

        private void btnUpdWiz_Click(object sender, EventArgs e)
        {
            var updater = new UpdateWizard();
            updater.ShowDialog();
        }
    }
}