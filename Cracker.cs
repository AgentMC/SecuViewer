using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SecuViewer
{
    public partial class Cracker : Form
    {
        private string _dic;
        private int _chars;
        private double _max;
        private double _maxFactor;
        private double _done;
        private bool _run;
        private MemoryStream _ms;
        private Thread _worker;
        private DateTime _started;
        private volatile string _last;
        private int[] _indices;
        private int _pointer;
        private StringBuilder _sb;
        private ManualResetEventSlim _synch = new ManualResetEventSlim(true);

        public Cracker()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _dic = textBox2.Text;
            _chars = (int) numericUpDown1.Value;
            _max = Math.Pow(textBox2.TextLength, _chars);
            _maxFactor = _max/1000;
            progressBar1.Value = 0;
            progressBar1.Maximum = (int)Math.Ceiling(_max/_maxFactor);
            _done = 0;
            _run = true;

            _ms = new MemoryStream();
            using (var rdr = new FileStream(textBox1.Text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                rdr.CopyTo(_ms);
            }

            _worker = new Thread(ThreadProc);
            _worker.Start();
        }

        private void ThreadProc()
        {
            _started = DateTime.Now;
            _sb = new StringBuilder(new string(_dic[0], _chars));
            _indices = new int[_chars];
            _pointer = _chars - 1;
            do
            {
                _last = _sb.ToString();
                if (TestPassword(_last)) break;
                _done++;
            } while (UpdatePassword() && _run);
            if (_run)
            {
                _synch.Reset();
                _synch.Wait();
                _run = false;
            }
        }

        private bool UpdatePassword()
        {
            if (_pointer < 0) return false;
            _indices[_pointer]++;
            if (_indices[_pointer] < _dic.Length)
            {
                _sb[_pointer] = _dic[_indices[_pointer]];
            }
            else
            {
                _pointer--;
                if(!UpdatePassword()) return false;
                char def = _dic[0];
                while (_pointer < _chars - 1)
                {
                    _pointer++;
                    _indices[_pointer] = 0;
                    _sb[_pointer] = def;
                }
            }
            return true;
        }

        private bool TestPassword(string psw)
        {
            _ms.Position = 0;
            var response = Crypter.Decrypt(new Crypter.CryptoData(psw), _ms);
            return response.Contains("parter");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(!_run) return;
            progressBar1.Value = (int) (_done/_maxFactor);
            var speed = _done/(DateTime.Now - _started).TotalSeconds;
            label5.Text = speed.ToString("F");
            label7.Text = SecondsToTimespan((_max - _done)/speed);
            label9.Text = _last;
            label11.Text = _done.ToString("F0");
            _synch.Set();
        }

        private readonly Dictionary<string, int> _measures = new Dictionary<string, int>
        {
            {"seconds", 1},
            {"minutes", 60},
            {"hours", 60},
            {"days", 24},
            {"months", 30},
            {"years", 12},
            {"decades", 10},
            {"centuries", 10},
            {"milleniums", 10},
            {"10ks!", 10},
        };

        private string SecondsToTimespan(double units)
        {

            string suffix = null;
            foreach (var measure in _measures)
            {
                if (units > measure.Value)
                {
                    units /= measure.Value;
                    suffix = measure.Key;
                }
                else
                {
                    break;
                }
            }
            return units.ToString("F") + " " + suffix;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _synch.Wait();
            _run = false;
        }

        private void Cracker_FormClosed(object sender, FormClosedEventArgs e)
        {
            button2_Click(sender, e);
        }
    }
}
