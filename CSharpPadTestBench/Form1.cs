using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Gobiner.CSharpPad;

namespace CSharpPadTestBench
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void runButton_Click( object sender, EventArgs e )
        {
            Eval eval = new Eval( Path.GetTempPath() );
            eval.CompileAndEval( codeTextBox.Text, Language.CSharp );
            if ( eval.Errors != null && eval.Errors.Length > 0 )
            {
                StringBuilder sb = new StringBuilder();
                foreach ( var error in eval.Errors )
                {
                    sb.AppendLine( error.ToString() );
                }
                outputTextBox.Text = sb.ToString();
                msilTextBox.Text = string.Empty;
            }
            else
            {
                outputTextBox.Text = string.Join( Environment.NewLine, eval.Output );
                msilTextBox.Text = string.Join( Environment.NewLine, eval.FormattedILDisassembly );
            }
        }
    }
}
