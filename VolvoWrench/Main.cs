﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using VolvoWrench.Netdec;

namespace VolvoWrench
{
    public partial class Main : Form
    {
        public string CurrentFile;
        public DemoFile CurrentDemoFile;

        public Main()
        {
            InitializeComponent();
            HotkeyTimer.Start();
            string DropFile = (Environment.GetCommandLineArgs().Any(x=>Path.GetExtension(x) == ".dem")) ? Environment.GetCommandLineArgs().First(x=>Path.GetExtension(x) == ".dem"):null;
            if (DropFile == null)
            {
                toolsToolStripMenuItem.Enabled = false;
                richTextBox1.Text = "^ Use File->Open to open a correct \".dem\" file!" + "\n" + 
                                    "No file dropped!";
            }
            else
            {
                if (Path.GetExtension(DropFile) == ".dem")
                {
                    CurrentFile = DropFile;
                    Stream cfs = File.Open(CurrentFile, FileMode.Open);
                    CurrentDemoFile = new DemoFile(cfs);
                    cfs.Close();
                    PrintSetails(CurrentDemoFile);
                    toolsToolStripMenuItem.Enabled = true;
                }
                else
                {
                    toolsToolStripMenuItem.Enabled = false;
                    richTextBox1.Text = "^ Use File->Open to open a correct \".dem\" file!" + "\n" +
                                    "No file dropped!";
                }
            }
        }

        public void PrintSetails(DemoFile d)
        {
            richTextBox1.Text = "Analyzed source engine demo file:" + "\n"
                      + "----------------------------------------------------------" + "\n"
                      + "Demo protocol: " + CurrentDemoFile.Info.DemoProtocol + "\n"
                      + "Net protocol: " + CurrentDemoFile.Info.NetProtocol + "\n"
                      + "Server name: " + CurrentDemoFile.Info.ServerName + "\n"
                      + "Client name: " + CurrentDemoFile.Info.ClientName + "\n"
                      + "Map name: " + CurrentDemoFile.Info.MapName + "\n"
                      + "Game directory: " + CurrentDemoFile.Info.GameDirectory + "\n"
                      + "Length in seconds: " + CurrentDemoFile.Info.Seconds + "\n"
                      + "Tick count: " + CurrentDemoFile.Info.TickCount + "\n"
                      + "Frame count: " + CurrentDemoFile.Info.FrameCount + "\n"
                      + "----------------------------------------------------------" + "\n";
            foreach (var f in CurrentDemoFile.Info.Flags)
            {
                if (f.Name == "#SAVE#")
                {
                    richTextBox1.Text += $"#SAVE# flag at Tick: {f.Tick} -> {f.Time}s" + "\n";
                    HighlightLastLine(richTextBox1,Color.Yellow);
                }
                if (f.Name == "autosave")
                {
                    richTextBox1.Text += $"Autosave at Tick: {f.Tick} -> {f.Time}s" + "\n";
                    HighlightLastLine(richTextBox1, Color.DarkOrange);
                }
            }
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (About a = new About())
            {
                a.ShowDialog();
            }
        }

        public void HighlightLastLine(RichTextBox TextControl, Color HighlightColor)
        {
            TextControl.Text = TextControl.Text.Trim();
            TextControl.SelectionStart = 0;
            TextControl.SelectionLength = 0;
            TextControl.SelectionColor = Color.Black;
            string LastLineText = TextControl.Lines[richTextBox1.Lines.Count() - 1];
            int LastLineStartIndex = richTextBox1.Text.LastIndexOf(LastLineText);
            TextControl.SelectionStart = LastLineStartIndex;
            TextControl.SelectionLength = TextControl.Text.Length - 1;
            TextControl.SelectionColor = HighlightColor;
            TextControl.DeselectAll();
            TextControl.Select(TextControl.Text.Length, 0);
        }

        private void checkForUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Update a = new Update())
            {
                a.ShowDialog();
            }
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (FontDialog fd = new FontDialog())
                {
                    if (fd.ShowDialog() == DialogResult.OK)
                    {
                        richTextBox1.Font = fd.Font;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void HotkeyTimer_Tick(object sender, EventArgs e)
        {
            if (CurrentDemoFile != null)
            {
                int keyInput = KeyInputApi.GetKeyState(0x70);
                if ((keyInput & 0x8000) != 0)
                {
                    MessageBox.Show("Demo protocol: " + CurrentDemoFile.Info.DemoProtocol + "\n"
                      + "Net protocol: " + CurrentDemoFile.Info.NetProtocol + "\n"
                      + "Server name: " + CurrentDemoFile.Info.ServerName + "\n"
                      + "Client name: " + CurrentDemoFile.Info.ClientName + "\n"
                      + "Map name: " + CurrentDemoFile.Info.MapName + "\n"
                      + "Game directory: " + CurrentDemoFile.Info.GameDirectory + "\n"
                      + "Length in seconds: " + CurrentDemoFile.Info.Seconds + "\n"
                      + "Tick count: " + CurrentDemoFile.Info.TickCount + "\n"
                      + "Frame count: " + CurrentDemoFile.Info.FrameCount);
                } //TODO: Add flags

            }
        }

        private void copyAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox1.Text);
        }

        private void netdecodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFile != null)
            {
                if (Path.GetExtension(CurrentFile) == ".dem")
                {
                    using (Stream cfs = File.Open(CurrentFile, FileMode.Open))
                    using (DemoDecoder nd = new DemoDecoder(cfs))
                    {
                        nd.ShowDialog();
                    }
                }
            }
        }

        private void sourcerunsWikiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://wiki.sourceruns.org");
        }

        private void sourcerunsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://sourceruns.org");
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog of = new OpenFileDialog())
            {
                if (of.ShowDialog() == DialogResult.OK)
                {
                    if (Path.GetExtension(of.FileName) == ".dem")
                    {
                        CurrentFile = of.FileName;
                        Stream cfs = File.Open(CurrentFile, FileMode.Open);
                        CurrentDemoFile = new DemoFile(cfs);
                        cfs.Close();
                        PrintSetails(CurrentDemoFile);
                        toolsToolStripMenuItem.Enabled = true;
                    }
                }
            }
        }

        private void rescanFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFile != null)
            {
                if (Path.GetExtension(CurrentFile) == ".dem")
                {
                    Stream cfs = File.Open(CurrentFile, FileMode.Open);
                    CurrentDemoFile = new DemoFile(cfs);
                    cfs.Close();
                    PrintSetails(CurrentDemoFile);
                    toolsToolStripMenuItem.Enabled = true;
                }
            }
        }
    }
}