using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SecuViewer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(string[] args)
        {
            InitializeComponent();
            _args = args;
        }

        private readonly string[] _args;

        #region Standard edit

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Undo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Paste();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.SelectAll();
        }

        #endregion

        #region Options

        private void chooseFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fontDialog.Font = textBox1.Font;
            if (fontDialog.ShowDialog() != DialogResult.OK)
                return;
            textBox1.Font = fontDialog.Font;
            Properties.Settings.Default.TextBoxFont = textBox1.Font;
            Properties.Settings.Default.Save();
        }

        private void wrapLinesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.WordWrap = wrapLinesToolStripMenuItem.Checked;
            Properties.Settings.Default.WrapLines = wrapLinesToolStripMenuItem.Checked;
            Properties.Settings.Default.Save();
        }

        #endregion

        private string _lastPath;
        private bool _saveNeeded;

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (QuerySaveIfNeeded()) textBox1.Clear();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (QuerySaveIfNeeded()) Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Properties.Resources.AboutText, Properties.Resources.AboutTitle);
        }

        private bool LoadPrologue()
        {
            return QuerySaveIfNeeded() && Ofd.ShowDialog(this) == DialogResult.OK;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (LoadPrologue()) LoadEncrypted(Ofd.FileName);
        }

        private void LoadEncrypted(string path)
        {
            var psw = new Password();
            if (psw.ShowDialog(false, this) != DialogResult.OK)
                return;
            var vocabulary = GetVocabulary(psw);
            var globalHash = (char) vocabulary.GetHashCode() & 0xFFFF;
            var builder = new StringBuilder();
            int i = 0, iMax = vocabulary.Length;
            int overhead = 0;
            using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var breader = new BinaryReader(reader, Encoding.Default))
            {
                while (builder.Length < (reader.Length - overhead*2)/8)
                {
                    char x = default(char);
                    for (int j = 0; j < 4; j++)
                    {
                        var z = breader.ReadInt16();
                        var y = (char) (z ^ vocabulary[i++] ^ globalHash);
                        x |= y;
                        if (i == iMax) i = 0;
                    }
                    if (x%3 == 0)
                    {
                        breader.ReadInt16();
                        overhead++;
                    }
                    if (x%2 == 0)
                    {
                        breader.ReadInt16();
                        overhead++;
                    }
                    builder.Append(x);
                }
            }
            textBox1.Clear();
            textBox1.Text = builder.ToString();
            _lastPath = path;
            _saveNeeded = false;
        }

        private void loadFromToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!LoadPrologue())
                return;
            using (var reader = new StreamReader(Ofd.FileName, Encoding.Default, true))
            {
                textBox1.Text = reader.ReadToEnd();
            }
            _lastPath = Ofd.FileName;
        }

        private static string GetVocabulary(Password psw)
        {
            var builder = new StringBuilder();
            int k = 0;
            var source = psw.textBox1.Text;
            for (int j = 0; j < 5; j++)
            {
                foreach (var t in source)
                {
                    builder.Append((char) (t ^ (0x579D579D >> k++) ^ (source.GetHashCode())));
                    if (k == 16) k = 0;
                }
            }
            return builder.ToString();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveConditional();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private bool QuerySaveIfNeeded()
        {
            if (!_saveNeeded) return true;
            var q = MessageBox.Show(Properties.Resources.DoYouWantToSaveDox, Properties.Resources.AppTitle,
                                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
            if (q == DialogResult.No)
                return true;
            if (q == DialogResult.Cancel)
                return false;
            return SaveConditional();
        }

        private bool SaveConditional()
        {
            return _lastPath == null ? SaveAs() : Save(_lastPath);
        }

        private bool SaveAs()
        {
            return Sfd.ShowDialog(this) == DialogResult.OK && Save(Sfd.FileName);
        }

        private bool Save(string path)
        {
            var psw = new Password();
            if (psw.ShowDialog(true, this) != DialogResult.OK)
                return false;
            var vocabulary = GetVocabulary(psw);
            var globalHash = (char) vocabulary.GetHashCode() & 0xFFFF;
            int i = 0, iMax = vocabulary.Length;
            var source = textBox1.Text;
            var rnd = new Random();
            int overhead = 0;
            using (var writer = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            using (var briter = new BinaryWriter(writer, Encoding.Default))
            {
                foreach (char t in source)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        var ch = (ushort) ((t & (0xF << (4*j))) ^ vocabulary[i++] ^ globalHash);
                        briter.Write(ch);
                        if (i == iMax) i = 0;
                    }
                    if (t%3 == 0)
                    {
                        briter.Write((short) rnd.Next(1023, 65535));
                        overhead++;
                    }
                    if (t%2 == 0)
                    {
                        briter.Write((short) rnd.Next(1023, 65535));
                        overhead++;
                    }
                }
                writer.SetLength(source.Length*8 + overhead*2);
            }
            _lastPath = path;
            _saveNeeded = false;
            return true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            _saveNeeded = textBox1.TextLength != 0;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !QuerySaveIfNeeded();
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            //settings
            textBox1.Font = Properties.Settings.Default.TextBoxFont;
            wrapLinesToolStripMenuItem.Checked = Properties.Settings.Default.WrapLines;

            //args
            string error;
            if (_args != null)
            {
                var commands = new Dictionary<Command, string>();
                for (int i = 0; i < _args.Length; i++)
                {
                    if (_args[i].ToLower() == "/decrypt")
                    {
                        if (i < _args.Length - 1 && File.Exists(_args[i + 1]))
                        {
                            commands.Add(Command.Decrypt, _args[++i]);
                        }
                        else
                        {
                            error = Properties.Resources.DecryptError;
                            goto ErrorAndClose;
                        }
                    }
                }
                if (commands.ContainsKey(Command.Decrypt))
                {
                    LoadEncrypted(commands[Command.Decrypt]);
                }
            }
            return;

            ErrorAndClose:
            MessageBox.Show(error, Properties.Resources.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }

        private enum Command
        {
            Decrypt
        }
    }
}
