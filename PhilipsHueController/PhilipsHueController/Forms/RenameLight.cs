﻿using PhilipsHueController.Extensions;
using PhilipsHueController.Helpers;
using System.Windows.Forms;

namespace PhilipsHueController
{
    public partial class RenameLight : Form
    {
        private readonly string LightName = string.Empty;
        private readonly string LightId = string.Empty;

        public RenameLight(object selectedLight)
        {
            InitializeComponent();

            LightId = selectedLight.GetObjectPropertyByName("Id");
            LightName = selectedLight.GetObjectPropertyByName("LightName");

            txtRename.Text = LightName;
        }

        private async void btnSaveChange_Click(object sender, System.EventArgs e)
        {
            await HueLightHelpers.RenameLightById(LightId, txtRename.Text);
            Close();
        }
    }
}
