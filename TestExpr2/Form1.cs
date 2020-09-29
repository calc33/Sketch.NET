using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sketch;

namespace TestExpr2 {
    public partial class Form1: Form {
        public Form1() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            //Expression expr = new Expression(this, textBox1.Text);
            ////expr.Formula = textBox1.Text;
            //textBox2.Text = expr.ToDebugString();
        }

        public void OnDependencyAdded(EventHandler<DependencyAddedEevntArgs> e) { }
    }
}
