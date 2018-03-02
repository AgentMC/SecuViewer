using System;
using System.Windows.Forms;

namespace SecuViewer
{
    public partial class Password : Form
    {
        public Password()
        {
            InitializeComponent();
        }

        private bool _showConfirmBox;

        public DialogResult ShowDialog(bool showConfirmBox, IWin32Window owner)
        {
            _showConfirmBox = showConfirmBox;
            if (!showConfirmBox)
            {
                textBox2.Visible = false;
                Height -= 30;
            }
            else
            {
                textBox2.TextChanged += TextBoxTextChanged;
            }
            var result = ShowDialog(owner);
            if (!showConfirmBox)
            {
                textBox2.Visible = true;
                Height += 30;
            }
            else
            {
                textBox2.TextChanged -= TextBoxTextChanged;
            }
            return result;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void TextBoxTextChanged(object sender, EventArgs e)
        {
            var isInputOk = textBox1.TextLength > 5;
            if (_showConfirmBox)
            {
                isInputOk &= textBox1.Text == textBox2.Text;
            }
            OkButton.Enabled = isInputOk;
        }
        
        private void Password_Layout(object sender, LayoutEventArgs e)
        {
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            CancelButtn.Focus();
            textBox1.Focus();
        }
    }
}
