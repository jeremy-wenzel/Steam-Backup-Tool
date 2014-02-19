﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SevenZip;

namespace steamBackup
{
    public partial class BackupUserCtrl : Form
    {
        public BackupUserCtrl()
        {
            InitializeComponent();
        }

        public BackupTask backupTask = new BackupTask();
        
        public bool canceled = true;

        private void BackupUserCtrl_Load(object sender, EventArgs e)
        {

            backupTask.steamDir = Settings.steamDir;
            backupTask.backupDir = Settings.backupDir;

            backupTask.list.Clear();
            backupTask.scan();
            updCheckBoxList();

            if (Settings.useLzma2 && Utilities.getSevenZipRelease() > 64)
            {
                tbarThread.Maximum = Environment.ProcessorCount;
                tbarThread.Value = Settings.lzma2Threads;
                backupTask.threadCount = Settings.lzma2Threads;
            }
            else
            {
                tbarThread.Maximum = 4;
                tbarThread.Value = Settings.threadsBup;
                backupTask.threadCount = Settings.threadsBup;
            }

            cBoxLzma2.Checked = Settings.useLzma2;

            threadText();

            tbarComp.Value = Settings.compresion;
            backupTask.setCompLevel((CompressionLevel)Settings.compresion);
            compresionText();

            ramUsage();
        }

        private void BackupUserCtrl_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.compresion = (int)backupTask.getCompLevel();
            Settings.useLzma2 = cBoxLzma2.Checked;

            if (Settings.useLzma2)
                Settings.lzma2Threads = tbarThread.Value;
            else
                Settings.threadsBup = tbarThread.Value;
            Settings.save();
        }

        private void updCheckBoxList()
        {
            chkList.BeginUpdate();
            chkList.Items.Clear();
            foreach (Job job in backupTask.list)
            {
                bool enabled = false;
                if (job.status == JobStatus.WAITING)
                    enabled = true;

                chkList.Items.Add(job.name, enabled);
            }
            chkList.EndUpdate();
        }

        private void btnBupAll_Click(object sender, EventArgs e)
        {
            disableButtons(false);

            cBoxDelBup.Enabled = true;
            cBoxDelBup.Checked = false;

            backupTask.setEnableAll();
            updCheckBoxList();

            disableButtons(true);
        }

        private void btnBupNone_Click(object sender, EventArgs e)
        {
            disableButtons(false);

            cBoxDelBup.Enabled = true;
            cBoxDelBup.Checked = false;

            backupTask.setEnableNone();
            updCheckBoxList();

            disableButtons(true);
        }

        private void btnUpdBup_Click(object sender, EventArgs e)
        {
            disableButtons(false);
  
            cBoxDelBup.Enabled = false;
            cBoxDelBup.Checked = false;

            this.Cursor = Cursors.WaitCursor;
            backupTask.setEnableUpd(chkList, true);
            this.Cursor = Cursors.Arrow;

            disableButtons(true);
        }

        private void btnUpdLib_Click(object sender, EventArgs e)
        {
            disableButtons(false);

            cBoxDelBup.Enabled = false;
            cBoxDelBup.Checked = false;

            this.Cursor = Cursors.WaitCursor;
            backupTask.setEnableUpd(chkList, false);
            this.Cursor = Cursors.Arrow;

            disableButtons(true);
        }

        private void disableButtons(bool disableBool)
        {
            btnBupAll.Enabled = disableBool;
            btnBupNone.Enabled = disableBool;
            btnUpdBup.Enabled = disableBool;
            btnUpdLib.Enabled = disableBool;
        }

        private void btnStartBup_Click(object sender, EventArgs e)
        {
            if (Utilities.isSteamRunning())
            {
                MessageBox.Show("Please exit Steam before backing up. To continue, exit Steam and then click the 'Backup' button again. Do Not start Steam until the backup process is finished.", "Steam Is Running", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                canceled = false;

                backupTask.setup();

                this.Close();
            }
        }

        private void btnCancelBup_Click(object sender, EventArgs e)
        {
            canceled = true;
            
            this.Close();
        }

        private void tbarThread_Scroll(object sender, EventArgs e)
        {
            backupTask.threadCount = tbarThread.Value;

            threadText();

            ramUsage();
        }

        private void threadText()
        {
            if (cBoxLzma2.Checked && Utilities.getSevenZipRelease() > 64)
                lblThread.Text = "Number Of Threads: \r\n" + tbarThread.Value.ToString();
            else
                lblThread.Text = "Number Of Instances:\r\n" + tbarThread.Value.ToString();
        }

        private void tbarComp_Scroll(object sender, EventArgs e)
        {
            backupTask.setCompLevel((CompressionLevel)tbarComp.Value);
            
            compresionText();

            ramUsage();
        }

        private void compresionText()
        {
            if ((int)backupTask.getCompLevel() == 5)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Ultra";
            else if ((int)backupTask.getCompLevel() == 4)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Maximum";
            else if ((int)backupTask.getCompLevel() == 3)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Normal";
            else if ((int)backupTask.getCompLevel() == 2)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Fast";
            else if ((int)backupTask.getCompLevel() == 1)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Fastest";
            else if ((int)backupTask.getCompLevel() == 0)
                lblComp.Text = "Compression Level:" + Environment.NewLine + "Copy";
            else
                lblComp.Text = "Compression Level:" + Environment.NewLine + "N/A";
        }

        private void ramUsage()
        {
            int ram = backupTask.ramUsage(cBoxLzma2.Checked);

            lblRamBackup.Text = "Max Ram Usage: " + ram.ToString() + "MB";

            if (ram >= 1500)
                lblRamBackup.ForeColor = Color.Red;
            else if (ram >= 750)
                lblRamBackup.ForeColor = Color.Orange;
            else
                lblRamBackup.ForeColor = Color.Black;
        }

        private void chkList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            foreach (Job job in backupTask.list)
            {
                CheckedListBox chkList = (CheckedListBox)sender;

                if (chkList.Items[e.Index].ToString().Equals(job.name))
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        backupTask.enableJob(job);
                    }
                    else
                    {
                        backupTask.disableJob(job);
                    }
                    break;
                }
            }
        }

        private void cBoxDelBup_CheckedChanged(object sender, EventArgs e)
        {
            if (cBoxDelBup.Checked)
                cBoxDelBup.ForeColor = Color.Red;
            else
                cBoxDelBup.ForeColor = Color.Black;

            backupTask.deleteAll = cBoxDelBup.Checked;
        }

        public Task getTask()
        {
            return backupTask;
        }

        private void controls_MouseLeave(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"Hover your mouse over the controls to get further information.");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void btnStartBup_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"Starts the backup procedure with the above parameters");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void btnCancelBup_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"Cancels the backup procedure and navigates back to the main menu.");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void cBoxDelBup_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"This will delete \b EVERYTHING \b0 in the 'Backup Directory'. Make sure that there are no valuable files in there!");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void lblComp_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"This will change how small the backup files are. Higher compression levels will use more ram and take longer but will result in far better compression.");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void lblThread_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            
            if (cBoxLzma2.Checked && Utilities.getSevenZipRelease() > 64)
            {
                sb.Append(@"{\rtf1\ansi ");
                sb.Append(@"This will change how many threads are used. It is recommended that you set the slider to \b 'core_count' \b0 for best performance.");
                sb.Append(@" Dramatically increases ram usage when also using high compression rates.");
                sb.Append(@" }");
            }
            else
            {
                sb.Append(@"{\rtf1\ansi ");
                sb.Append(@"This will change how many instances of 7zip are used, Each instance creates two threads. It is recommended that you set the slider to \b 'core_count/2' \b0 for best performance.");
                sb.Append(@" Dramatically increases ram usage when also using high compression rates.");
                sb.Append(@" }");
            }
            
            
            

            infoBox.Rtf = sb.ToString();
        }

        private void chkList_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"Customise your selection of games to backup. Older games that utilize Valve's Source Engine share resources between each other. For this reason they cannot be separated and have to be backed up together.");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void btnBupAll_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"Click to \b select \b0 all games for backup.");
            sb.Append(@" The selection can be modified in the check box list.");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void btnBupNone_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"Click to \b deselect \b0 all games for backup.");
            sb.Append(@" The selection can be modified in the check box list.");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void btnUpdBup_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"Click to select all games that have been changed since the last backup, \b Excluding \b0 games that have not been backed up yet.");
            sb.Append(@" The selection can be modified in the check box list.");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void btnUpdLib_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"Click to select all games that have been changed since the last backup, \b Including \b0 games that have not been backed up yet.");
            sb.Append(@" The selection can be modified in the check box list.");
            sb.Append(@" }");
            
            infoBox.Rtf = sb.ToString();
        }

        private void cBoxLzma2_MouseHover(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi ");
            sb.Append(@"This will use multithreaded compression and reduce concurrent compression instances to 1.");

            if (Utilities.getSevenZipRelease() <= 64)
                sb.Append(@" Uses \b all \b0 cpu cores for compression.");

            sb.Append(@" The compressed archives have similar sizes compared to LZMA compression.");
            sb.Append(@" }");

            infoBox.Rtf = sb.ToString();
        }

        private void cBoxLzma2_CheckStateChanged(object sender, EventArgs e)
        {
            int sevenZipRelease = Utilities.getSevenZipRelease();
            if (cBoxLzma2.Checked)
            {
                backupTask.setCompMethod(CompressionMethod.Lzma2);

                if (sevenZipRelease > 64)
                {
                    tbarThread.Maximum = Environment.ProcessorCount;
                    tbarThread.Value = Settings.lzma2Threads;
                    backupTask.threadCount = Settings.lzma2Threads;
                    threadText();

                    tbarThreadLbl.Text = "Choose the number of instances to run.\nRecommended: One instance for every CPU core.";
                }
                else
                    tbarThread.Enabled = false;
            }
            else
            {
                backupTask.setCompMethod(CompressionMethod.Lzma);

                if (sevenZipRelease > 64)
                {
                    tbarThread.Maximum = 4;
                    tbarThread.Value = Settings.threadsBup;
                    backupTask.threadCount = Settings.threadsBup;
                    threadText();

                    tbarThreadLbl.Text = "Choose the number of instances to run.\nRecommended: One instance for every two CPU cores.";
                }
                else
                    tbarThread.Enabled = true;
            }

            ramUsage();
        }
    }
}
