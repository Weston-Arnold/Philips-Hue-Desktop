﻿using PhilipsHueController.Extensions;
using PhilipsHueController.Forms;
using PhilipsHueController.Helpers;
using Q42.HueApi.Models.Groups;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhilipsHueController
{
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private async void Dashboard_Load(object sender, System.EventArgs e)
        {
            pnlContinueSetup.Visible = false;

            var isApplicationRegistered = HueConnectionManager.IsApplicationRegistered();
            if (!isApplicationRegistered)
            {
                pnlContinueSetup.Visible = true;
                LaunchSetup(sender, e);

                return;
            }

            HueConnectionManager.LoadConfiguredBridge();

            btnRenameLight.Enabled = false;
            btnEditGroup.Enabled = false;

            pnlContinueSetup.Visible = false;
            txtAdditionalInformation.Text = "Select a light or group to show additional information...";
            txtBridgeInfo.Text = await HueConnectionManager.GetConnectedBridgeFooterInformation();

            ToggleLightActionControls(false);

            await LoadLightListBox();
            await LoadGroupListBox();
        }

        private async Task LoadLightListBox()
        {
            lbLights.Items.Clear();

            var lightList = await HueLightManager.GetAllLights();

            foreach (var light in lightList.OrderBy(x => x.Name))
            {
                lbLights.Items.Add(new
                {
                    Id = light.Id,
                    LightName = light.Name
                });
            }

            lbLights.DisplayMember = "LightName";
        }

        private async Task LoadGroupListBox()
        {
            lbLightGroups.Items.Clear();

            var groupList = await HueGroupManager.GetAllGroups(GroupType.Room);

            foreach (var group in groupList.OrderBy(x => x.Name))
            {
                lbLightGroups.Items.Add(new
                {
                    Id = group.Id,
                    GroupName = group.Name
                });
            }

            lbLightGroups.DisplayMember = "GroupName";
        }

        private void LaunchSetup(object sender, System.EventArgs e)
        {
            var setupWindow = new Setup();

            setupWindow.ShowDialog();

            var isApplicationRegisteredAfterWindowClose = HueConnectionManager.IsApplicationRegistered();
            if (isApplicationRegisteredAfterWindowClose)
            {
                Dashboard_Load(sender, e);
            }
        }

        private void btnCompleteSetup_Click(object sender, System.EventArgs e)
        {
            LaunchSetup(sender, e);
        }

        private void btnRenameLight_Click(object sender, System.EventArgs e)
        {
            OpenLightEditDialogue();
        }

        private void btnEditGroup_Click(object sender, System.EventArgs e)
        {
            OpenGroupEditDialogue();
        }

        private async void OpenLightEditDialogue()
        {
            var currentlySelectedLightIndex = lbLights.SelectedIndex;
            var renameLightWindow = new RenameLight(lbLights.SelectedItem);

            renameLightWindow.ShowDialog();

            txtAdditionalInformation.Text = await HueLightManager.GetSelectedLightInformation(lbLights.SelectedItem);
            await LoadLightListBox();

            lbLights.SelectedIndex = currentlySelectedLightIndex;
        }

        private async void OpenGroupEditDialogue()
        {
            var currentlySelectedGroupIndex = lbLightGroups.SelectedIndex;
            var renameGroupWindow = new EditGroup(lbLightGroups.SelectedItem);

            renameGroupWindow.ShowDialog();

            txtAdditionalInformation.Text = await HueGroupManager.GetSelectedGroupInformation(lbLightGroups.SelectedItem);
            await LoadGroupListBox();

            lbLightGroups.SelectedIndex = currentlySelectedGroupIndex;
        }

        private void lbLights_MouseDoubleClick(object sender, System.EventArgs e)
        {
            OpenLightEditDialogue();
        }

        private void lbLightGroups_MouseDoubleClick(object sender, System.EventArgs e)
        {
            OpenGroupEditDialogue();
        }

        private async void lbLightGroups_OnClick(object sender, System.EventArgs e)
        {
            lbLights.SelectedItem = null;

            if (lbLightGroups.SelectedItem != null)
            {
                btnEditGroup.Enabled = true;
                btnRenameLight.Enabled = false;

                ToggleLightActionControls(true);

                txtAdditionalInformation.Text = await HueGroupManager.GetSelectedGroupInformation(lbLightGroups.SelectedItem);
            }
            else
            {
                btnEditGroup.Enabled = false;
            }
        }

        private async void lbLights_OnClick(object sender, System.EventArgs e)
        {
            lbLightGroups.SelectedItem = null;

            if (lbLights.SelectedItem != null)
            {
                btnRenameLight.Enabled = true;
                btnEditGroup.Enabled = false;

                ToggleLightActionControls(true);

                txtAdditionalInformation.Text = await HueLightManager.GetSelectedLightInformation(lbLights.SelectedItem);
                tbBrightness.Value = (int)(await HueLightManager.GetLightBrightness(lbLights.SelectedItem) / 25.4);

                await HueLightManager.BlipSelectedLight(lbLights.SelectedItem);
            }
            else
            {
                btnRenameLight.Enabled = false;
            }
        }

        private void btnDisconnect_Click(object sender, System.EventArgs e)
        {
            var disconnectWindow = new Disconnect();
            disconnectWindow.ShowDialog();

            var appKey = ConfigurationHelpers.GetSettingByKey("AppKey");
            if (string.IsNullOrEmpty(appKey))
            {
                pnlContinueSetup.Visible = true;
            }
        }

        private async void btnLightsOn_Click(object sender, System.EventArgs e)
        {
            await ToggleButtonPower(lbLights.SelectedItem, lbLightGroups.SelectedItem, true);
        }

        private async void btnLightsOff_Click(object sender, System.EventArgs e)
        {
            await ToggleButtonPower(lbLights.SelectedItem, lbLightGroups.SelectedItem, false);
        }

        public async static Task ToggleButtonPower(object selectedLightId, object groupId, bool turningOn)
        {
            if (selectedLightId != null)
            {
                await HueLightManager.SetLightOnOff(selectedLightId.GetObjectPropertyByName("Id"), turningOn);
                return;
            }

            if (groupId != null)
            {
                await HueGroupManager.SetGroupLightsOnOff(groupId.GetObjectPropertyByName("Id"), turningOn);
                return;
            }
        }

        private async void btnChangeColor_Click(object sender, System.EventArgs e)
        {
            var currentlySelectedLight = lbLights.SelectedItem;
            var currentlySelectedGroup = lbLightGroups.SelectedItem;

            dlgChangeColor.ShowDialog();
            var selectedColor = dlgChangeColor.Color;

            if (currentlySelectedLight != null)
            {
                await HueLightManager.SetLightColor(selectedColor, currentlySelectedLight.GetObjectPropertyByName("Id"));
                txtAdditionalInformation.Text = await HueLightManager.GetSelectedLightInformation(lbLights.SelectedItem);
                return;
            }

            if (currentlySelectedGroup != null)
            {
                await HueGroupManager.SetGroupLightColor(selectedColor, currentlySelectedGroup.GetObjectPropertyByName("Id"));
                txtAdditionalInformation.Text = await HueGroupManager.GetSelectedGroupInformation(lbLightGroups.SelectedItem);
                return;
            }
        }

        private async void tbBrightness_Scroll(object sender, System.EventArgs e)
        {
            var currentlySelectedLight = lbLights.SelectedItem;
            var currentlySelectedGroup = lbLightGroups.SelectedItem;

            var selectedBrightness = tbBrightness.Value * 25.4;
            if(currentlySelectedLight != null)
            {
                await HueLightManager.SetSingleLightBrightness(currentlySelectedLight.GetObjectPropertyByName("Id"), (byte)selectedBrightness);
                txtAdditionalInformation.Text = await HueLightManager.GetSelectedLightInformation(lbLights.SelectedItem);
                return;
            }

            if (currentlySelectedGroup != null)
            {
                await HueGroupManager.SetGroupLightBrightness(currentlySelectedGroup.GetObjectPropertyByName("Id"), (byte)selectedBrightness);
                txtAdditionalInformation.Text = await HueGroupManager.GetSelectedGroupInformation(lbLightGroups.SelectedItem);
                return;
            }
        }

        private void ToggleLightActionControls(bool toggleOn)
        {
            btnChangeColor.Enabled = toggleOn;
            btnLightsOn.Enabled = toggleOn;
            btnLightsOff.Enabled = toggleOn;

            tbBrightness.Enabled = toggleOn;
        }

        private void lblLightRooms_Click(object sender, System.EventArgs e)
        {

        }

        private void lblContinueSetupPrompt_Click(object sender, System.EventArgs e)
        {

        }

        private void lblLightListHeader_Click(object sender, System.EventArgs e)
        {

        }

        private void lblAdditionalLightInformation_Click(object sender, System.EventArgs e)
        {

        }
    }
}
