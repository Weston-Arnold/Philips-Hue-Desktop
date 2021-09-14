﻿using System;
using System.Windows.Forms;

namespace PhilipsHueController
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MainWindow_Load(object sender, EventArgs e)
        {
            await HueConnectionHelpers.ConfigureBridge();
        }
    }
}