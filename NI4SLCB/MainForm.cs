using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 * Nanoleaf Integration for SLCB: Websocket client
 * Author:  CyberHumi
 *          https://github.com/CyberHumi/
 * Licence: MIT
 */

namespace NI4SLCB {
    public partial class MainForm : Form {
        private SLCB conn;
        private NanoleafDevice[] devices;
        private EffectChange[] alerts;
        private EffectChange[] chatCmds;
        private ArrayList DisvoveredDevices;
        private IContainer notifyIcon_components;
        private ContextMenu notifyIcon_contextMenu;
        private MenuItem notifyIcon_menuItem;
        private Boolean closeApp;

        public MainForm() {
            closeApp = false;
            InitializeComponent();
            tabControl2_settings.DrawItem += new DrawItemEventHandler(TabControl2_DrawItem);
            InitializeWebsocket();
            devices = new NanoleafDevice[4];
            alerts = new EffectChange[Enum.GetNames(typeof(EffectChange.Type)).Length];
            chatCmds = new EffectChange[Enum.GetNames(typeof(EffectChange.ChatCmd)).Length];
            NanoleafEventHandler.Instance.SetMainForm(this);
            LoadSettings();
            UpdateEffectList();             /* Nanoleaf effect list */
            AddCheckedChanged();            /* Alerts */
            AddCheckedChanged2();           /* Chat Cmds */
            AddFormEvents();
            AddNotifyIconEventsAndMenu();
        }

        private void TabControl2_DrawItem(object sender, DrawItemEventArgs e) {
            Graphics g = e.Graphics;
            Brush _textBrush;

            // Get the item from the collection
            TabPage _tabPage = tabControl2_settings.TabPages[e.Index];

            // Get the real bounds for the tab rectangle
            Rectangle _tabBounds = tabControl2_settings.GetTabRect(e.Index);

            if (e.State == DrawItemState.Selected) {
                // Draw a different background color, and don't paint a focus rectangle
                _textBrush = new SolidBrush(Color.White);
                g.FillRectangle(Brushes.Gray, e.Bounds);
            } else {
                _textBrush = new System.Drawing.SolidBrush(e.ForeColor);
                e.DrawBackground();
            }

            // Set font
            Font _tabFont = new Font("Arial", 14.0f, FontStyle.Bold, GraphicsUnit.Pixel);

            // Draw string, center the text
            StringFormat _stringFlags = new StringFormat {
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));
        }

        private void AddFormEvents() {
            FormClosing += new FormClosingEventHandler(MainForm_AvoidClosing);
            Resize += new EventHandler(MainForm_Resize);
        }

        private void AddNotifyIconEventsAndMenu() {
            notifyIcon.DoubleClick += new EventHandler(NotifyIcon_DoubleClick);

            notifyIcon_components = new Container();
            notifyIcon_contextMenu = new ContextMenu();
            notifyIcon_menuItem = new MenuItem();

            // Initialize notifyIcon_contextMenu
            notifyIcon_contextMenu.MenuItems.AddRange(new MenuItem[] { notifyIcon_menuItem });

            // Initialize notifyIcon_menuItem
            notifyIcon_menuItem.Index = 0;
            notifyIcon_menuItem.Text = "E&xit";
            notifyIcon_menuItem.Click += new System.EventHandler(MenuItemExit_Click);

            // Add notifyIcon_contextMenu to notifyIcon
            notifyIcon.ContextMenu = notifyIcon_contextMenu;
        }

        private async void InitializeWebsocket() {
            conn = new SLCB(this);
            if (conn != null)
                await conn.ConnectAsync();
        }

        private void DiscordSupportENDEToolStripMenuItem_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("https://discordapp.com/invite/UYpvv55");
        }

        private void InstallationGuideToolStripMenuItem_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/CyberHumi/NI4SLCB/wiki/Installation");
        }

        private void ReportIssueToolStripMenuItem_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/CyberHumi/NI4SLCB/issues");
        }

        private void TwitchtvCyberHumiToolStripMenuItem_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("https://www.twitch.tv/CyberHumi");
        }

        private void TwittercomCyberHumiDEToolStripMenuItem_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("https://www.twitter.com/CyberHumiDE");
        }

        private void GithubcomCyberHumiToolStripMenuItem_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/CyberHumi");
        }

        private void ManuelLopezemey87deviantartcomToolStripMenuItem_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("https://www.deviantart.com/emey87");
        }

        private void StreamlabsChatbotToolStripMenuItem_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("https://streamlabs.com/chatbot");
        }

        private void CheckForNI4SLCBUpdate_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/CyberHumi/NI4SLCB/");
        }

        private async void ToolStripButton1_ClickAsync(object sender, EventArgs e) {
            if (conn == null)
                return;
            if (!conn.IsConnected())
                await conn.ConnectAsync();
            else if (!conn.IsAborted()) {
                try {
                    await conn.DisconnectAsync();
                } catch (Exception) {
                }
            } else {
                toolStripStatusLabel1.Text = "Connection will be closed";
                toolStripStatusLabel1.Image = NI4SLCB.Properties.Resources.Badge_multiply;
            }
        }

        public void AddListViewEventsItem(JSONSLCBEvent SLCBEvent) {
            string eventname = SLCBEvent.@event.Substring(SLCBEvent.@event.IndexOf("_") + 1);
            string information = "";
            switch (eventname) {
                case "FOLLOW":
                case "MX_FOLLOW":
                    information = "User: " + SLCBEvent.EventData.display_name;
                    break;
                case "SUB":
                case "MX_SUB":
                case "YT_SUB":
                    information = "User: " + SLCBEvent.EventData.display_name + (SLCBEvent.EventData.gift_target != null ? ", gifted to " + SLCBEvent.EventData.gift_target : "") + ", tier: " + SLCBEvent.EventData.tier + ", months: " + SLCBEvent.EventData.months;
                    break;
                case "DONATION":
                    information = "User: " + SLCBEvent.EventData.display_name + ", " + SLCBEvent.EventData.amount + SLCBEvent.EventData.currency;
                    break;
                case "CHEER":
                    information = "User: " + SLCBEvent.EventData.display_name + ", bits: " + SLCBEvent.EventData.bits + ", total: " + SLCBEvent.EventData.total_bits;
                    break;
                case "HOST":
                case "MX_HOST":
                    information = "User: " + SLCBEvent.EventData.display_name + ", Viewers: " + SLCBEvent.EventData.viewers;
                    break;
                case "RAID":
                    information = "User: " + SLCBEvent.EventData.display_name + ", Viewers: " + SLCBEvent.EventData.viewers;
                    break;
                case "CHATCMD":
                    information = "User: " + SLCBEvent.EventData.display_name;
                    break;
            }
            AddListViewEventsItem(SLCBEvent.Date, eventname, information);
        }

        public void AddListViewEventsItem(DateTime date, string eventname, string information) {
            if (listViewEvents.InvokeRequired) {
                MethodInvoker del = delegate { AddListViewEventsItem(date, eventname, information); };
                listViewEvents.Invoke(del);
                return;
            }
            listViewEvents.Items.Add(new System.Windows.Forms.ListViewItem(new[] {
                //date.ToString("g", CultureInfo.CurrentCulture),
                date.ToString("T", CultureInfo.CurrentCulture),
                eventname,
                information
            }, 0));
            for (int i = 0; i < listViewEvents.Items.Count - 300; i++)
                listViewEvents.Items[i].Remove();
            listViewEvents.Items[listViewEvents.Items.Count - 1].EnsureVisible();
        }

        public void SetConnctStatus() {
            toolStripStatusLabel1.Text = "Connected to SLCB";
            toolStripStatusLabel1.Image = NI4SLCB.Properties.Resources.Badge_tick;
            toolStripButton1.Text = "Disconnect from SLCB";
            toolStripButton1.Image = NI4SLCB.Properties.Resources.Electric_interruptor;
        }
        public void SetDisconnctStatus() {
            toolStripStatusLabel1.Text = "Disconnected from SLCB";
            toolStripStatusLabel1.Image = NI4SLCB.Properties.Resources.Badge_multiply;
            toolStripButton1.Text = "Connect to SLCB";
            toolStripButton1.Image = NI4SLCB.Properties.Resources.Electric_interruptor;
        }
        public void UpdateTabpage21_websocket(string url, string apiKey) {
            textBox211_wsurl.Text = url;
            textBox212_apikey.Text = apiKey;
        }

        private void Button221_authtokengenerate_Click(object sender, EventArgs e) {
            string token = Nanoleaf.GenerateToken(textBox221_location.Text);
            if (token != null)
                textBox221_authtoken.Text = token;
            if (textBox221_location.Text.Length > 0 && textBox221_authtoken.Text.Length > 0 && !textBox221_authtoken.Text.StartsWith("---")) {
                devices[0] = new NanoleafDevice(textBox221_location.Text, textBox221_authtoken.Text);
                UpdateEffectList();
            }
        }

        private void Button222_authtokengenerate_Click(object sender, EventArgs e) {
            string token = Nanoleaf.GenerateToken(textBox222_location.Text);
            if (token != null)
                textBox221_authtoken.Text = token;
            if (textBox222_location.Text.Length > 0 && textBox222_authtoken.Text.Length > 0 && !textBox222_authtoken.Text.StartsWith("---")) {
                devices[1] = new NanoleafDevice(textBox222_location.Text, textBox222_authtoken.Text);
                UpdateEffectList();
            }
        }

        private void Button223_authtokengenerate_Click(object sender, EventArgs e) {
            string token = Nanoleaf.GenerateToken(textBox223_location.Text);
            if (token != null)
                textBox221_authtoken.Text = token;
            if (textBox223_location.Text.Length > 0 && textBox223_authtoken.Text.Length > 0 && !textBox223_authtoken.Text.StartsWith("---")) {
                devices[2] = new NanoleafDevice(textBox223_location.Text, textBox223_authtoken.Text);
                UpdateEffectList();
            }
        }

        private void Button224_authtokengenerate_Click(object sender, EventArgs e) {
            string token = Nanoleaf.GenerateToken(textBox224_location.Text);
            if (token != null)
                textBox221_authtoken.Text = token;
            if (textBox224_location.Text.Length > 0 && textBox224_authtoken.Text.Length > 0 && !textBox224_authtoken.Text.StartsWith("---")) {
                devices[3] = new NanoleafDevice(textBox224_location.Text, textBox224_authtoken.Text);
                UpdateEffectList();
            }
        }

        private void Button2_saveSettings_Click(object sender, EventArgs e) {
            /* save Nanoleaf devices */
            SaveObject("settings/NanoleafDevices.save", devices);

            /* save light effects / alerts */
            alerts[(int)EffectChange.Type.DEFAULT] = new EffectChange(
                comboBox231_default.Text,
                checkBox231_default_brightness.Checked,
                (int)numericUpDown231_default_brightness.Value,
                false,
                -1,
                null
                );
            alerts[(int)EffectChange.Type.FOLLOW] = new EffectChange(
                comboBox232_follow.Text,
                checkBox232_follow_brightness.Checked,
                (int)numericUpDown232_follow_brightness.Value,
                checkBox232_follow_duration.Checked,
                (int)numericUpDown232_follow_duration.Value,
                new Boolean[] { checkBox232_follow_device1.Checked, checkBox232_follow_device2.Checked, checkBox232_follow_device3.Checked, checkBox232_follow_device4.Checked }
                );
            alerts[(int)EffectChange.Type.SUB] = new EffectChange(
                comboBox233_sub.Text,
                checkBox233_sub_brightness.Checked,
                (int)numericUpDown233_sub_brightness.Value,
                checkBox233_sub_duration.Checked,
                (int)numericUpDown233_sub_duration.Value,
                new Boolean[] { checkBox233_sub_device1.Checked, checkBox233_sub_device2.Checked, checkBox233_sub_device3.Checked, checkBox233_sub_device4.Checked }
                );
            alerts[(int)EffectChange.Type.DONATION] = new EffectChange(
                comboBox234_donation.Text,
                checkBox234_donation_brightness.Checked,
                (int)numericUpDown234_donation_brightness.Value,
                checkBox234_donation_duration.Checked,
                (int)numericUpDown234_donation_duration.Value,
                new Boolean[] { checkBox234_donation_device1.Checked, checkBox234_donation_device2.Checked, checkBox234_donation_device3.Checked, checkBox234_donation_device4.Checked }
                );
            alerts[(int)EffectChange.Type.CHEER] = new EffectChange(
                comboBox235_cheer.Text,
                checkBox235_cheer_brightness.Checked,
                (int)numericUpDown235_cheer_brightness.Value,
                checkBox235_cheer_duration.Checked,
                (int)numericUpDown235_cheer_duration.Value,
                new Boolean[] { checkBox235_cheer_device1.Checked, checkBox235_cheer_device2.Checked, checkBox235_cheer_device3.Checked, checkBox235_cheer_device4.Checked }
                );
            alerts[(int)EffectChange.Type.HOST] = new EffectChange(
                comboBox236_host.Text,
                checkBox236_host_brightness.Checked,
                (int)numericUpDown236_host_brightness.Value,
                checkBox236_host_duration.Checked,
                (int)numericUpDown236_host_duration.Value,
                new Boolean[] { checkBox236_host_device1.Checked, checkBox236_host_device2.Checked, checkBox236_host_device3.Checked, checkBox236_host_device4.Checked }
                );
            alerts[(int)EffectChange.Type.RAID] = new EffectChange(
                comboBox237_raid.Text,
                checkBox237_raid_brightness.Checked,
                (int)numericUpDown237_raid_brightness.Value,
                checkBox237_raid_duration.Checked,
                (int)numericUpDown237_raid_duration.Value,
                new Boolean[] { checkBox237_raid_device1.Checked, checkBox237_raid_device2.Checked, checkBox237_raid_device3.Checked, checkBox237_raid_device4.Checked }
                );
            SaveObject("settings/Alerts.save", alerts);

            /* save chat commands */
            chatCmds[(int)EffectChange.ChatCmd.MASTER] = new EffectChange(
                null,
                checkBox240_mstr_brightness.Checked,
                (int)numericUpDown240_mstr_brightness.Value,
                checkBox240_mstr_duration.Checked,
                (int)numericUpDown240_mstr_duration.Value,
                new Boolean[] { checkBox240_device1.Checked, checkBox240_device2.Checked, checkBox240_device3.Checked, checkBox240_device4.Checked }
                );
            chatCmds[(int)EffectChange.ChatCmd.CMD1] = new EffectChange(
                comboBox241_effect.Text,
                checkBox241_cmd1_brightness.Checked,
                (int)numericUpDown241_cmd1_brightness.Value,
                checkBox241_cmd1_duration.Checked,
                (int)numericUpDown241_cmd1_duration.Value,
                new Boolean[] { checkBox241_device1.Checked, checkBox241_device2.Checked, checkBox241_device3.Checked, checkBox241_device4.Checked }
                );
            chatCmds[(int)EffectChange.ChatCmd.CMD2] = new EffectChange(
                comboBox242_effect.Text,
                checkBox242_cmd2_brightness.Checked,
                (int)numericUpDown242_cmd2_brightness.Value,
                checkBox242_cmd2_duration.Checked,
                (int)numericUpDown242_cmd2_duration.Value,
                new Boolean[] { checkBox242_device1.Checked, checkBox242_device2.Checked, checkBox242_device3.Checked, checkBox242_device4.Checked }
                );
            chatCmds[(int)EffectChange.ChatCmd.CMD3] = new EffectChange(
                comboBox243_effect.Text,
                checkBox243_cmd3_brightness.Checked,
                (int)numericUpDown243_cmd3_brightness.Value,
                checkBox243_cmd3_duration.Checked,
                (int)numericUpDown243_cmd3_duration.Value,
                new Boolean[] { checkBox243_device1.Checked, checkBox243_device2.Checked, checkBox243_device3.Checked, checkBox243_device4.Checked }
                );
            chatCmds[(int)EffectChange.ChatCmd.CMD4] = new EffectChange(
                comboBox244_effect.Text,
                checkBox244_cmd4_brightness.Checked,
                (int)numericUpDown244_cmd4_brightness.Value,
                checkBox244_cmd4_duration.Checked,
                (int)numericUpDown244_cmd4_duration.Value,
                new Boolean[] { checkBox244_device1.Checked, checkBox244_device2.Checked, checkBox244_device3.Checked, checkBox244_device4.Checked }
                );
            chatCmds[(int)EffectChange.ChatCmd.CMD5] = new EffectChange(
                comboBox245_effect.Text,
                checkBox245_cmd5_brightness.Checked,
                (int)numericUpDown245_cmd5_brightness.Value,
                checkBox245_cmd5_duration.Checked,
                (int)numericUpDown245_cmd5_duration.Value,
                new Boolean[] { checkBox245_device1.Checked, checkBox245_device2.Checked, checkBox245_device3.Checked, checkBox245_device4.Checked }
                );
            chatCmds[(int)EffectChange.ChatCmd.CMD6] = new EffectChange(
                comboBox246_effect.Text,
                checkBox246_cmd6_brightness.Checked,
                (int)numericUpDown246_cmd6_brightness.Value,
                checkBox246_cmd6_duration.Checked,
                (int)numericUpDown246_cmd6_duration.Value,
                new Boolean[] { checkBox246_device1.Checked, checkBox246_device2.Checked, checkBox246_device3.Checked, checkBox246_device4.Checked }
                );
            chatCmds[(int)EffectChange.ChatCmd.CMD7] = new EffectChange(
                comboBox247_effect.Text,
                checkBox247_cmd7_brightness.Checked,
                (int)numericUpDown247_cmd7_brightness.Value,
                checkBox247_cmd7_duration.Checked,
                (int)numericUpDown247_cmd7_duration.Value,
                new Boolean[] { checkBox247_device1.Checked, checkBox247_device2.Checked, checkBox247_device3.Checked, checkBox247_device4.Checked }
                );
            chatCmds[(int)EffectChange.ChatCmd.CMD8] = new EffectChange(
                comboBox248_effect.Text,
                checkBox248_cmd8_brightness.Checked,
                (int)numericUpDown248_cmd8_brightness.Value,
                checkBox248_cmd8_duration.Checked,
                (int)numericUpDown248_cmd8_duration.Value,
                new Boolean[] { checkBox248_device1.Checked, checkBox248_device2.Checked, checkBox248_device3.Checked, checkBox248_device4.Checked }
                );
            chatCmds[(int)EffectChange.ChatCmd.CMD9] = new EffectChange(
                comboBox249_effect.Text,
                checkBox249_cmd9_brightness.Checked,
                (int)numericUpDown249_cmd9_brightness.Value,
                checkBox249_cmd9_duration.Checked,
                (int)numericUpDown249_cmd9_duration.Value,
                new Boolean[] { checkBox249_device1.Checked, checkBox249_device2.Checked, checkBox249_device3.Checked, checkBox249_device4.Checked }
                );
            chatCmds[(int)EffectChange.ChatCmd.CMD10] = new EffectChange(
                comboBox2410_effect.Text,
                checkBox2410_cmd10_brightness.Checked,
                (int)numericUpDown2410_cmd10_brightness.Value,
                checkBox2410_cmd10_duration.Checked,
                (int)numericUpDown2410_cmd10_duration.Value,
                new Boolean[] { checkBox2410_device1.Checked, checkBox2410_device2.Checked, checkBox2410_device3.Checked, checkBox2410_device4.Checked }
                );
            SaveObject("settings/ChatCommands.save", chatCmds);

            /* update NanoleafEventHandler */
            NanoleafEventHandler.Instance.SetNanoleafConfig(devices, alerts, chatCmds);
        }

        private void LoadSettings() {
            /* load Nanoleaf devices */
            string deviceFile = "settings/NanoleafDevices.save";
            if (File.Exists(deviceFile)) {
                devices = (NanoleafDevice[])LoadObject(deviceFile);
                TextBox[] tbAt = { textBox221_authtoken, textBox222_authtoken, textBox223_authtoken, textBox224_authtoken };
                TextBox[] tbHo = { textBox221_location, textBox222_location, textBox223_location, textBox224_location };
                for (int i = 0; i < devices.Length; i++) {
                    if (devices[i] != null && devices[i].GetAuthToken().Length > 0)
                        tbAt[i].Text = devices[i].GetAuthToken();
                    if (devices[i] != null && devices[i].GetLocation().Length > 0)
                        tbHo[i].Text = devices[i].GetLocation();
                }
            }

            /* load light effects / alerts */
            string alertsFile = "settings/Alerts.save";
            if (File.Exists(alertsFile)) {
                alerts = (EffectChange[])LoadObject(alertsFile);
                ComboBox[] effects = { comboBox231_default, comboBox232_follow, comboBox233_sub, comboBox234_donation, comboBox235_cheer, comboBox236_host, comboBox237_raid };
                CheckBox[] isBrightness = { checkBox231_default_brightness, checkBox232_follow_brightness, checkBox233_sub_brightness, checkBox234_donation_brightness, checkBox235_cheer_brightness, checkBox236_host_brightness, checkBox237_raid_brightness };
                NumericUpDown[] brightness = { numericUpDown231_default_brightness, numericUpDown232_follow_brightness, numericUpDown233_sub_brightness, numericUpDown234_donation_brightness, numericUpDown235_cheer_brightness, numericUpDown236_host_brightness, numericUpDown237_raid_brightness };
                CheckBox[] isDuration = { null, checkBox232_follow_duration, checkBox233_sub_duration, checkBox234_donation_duration, checkBox235_cheer_duration, checkBox236_host_duration, checkBox237_raid_duration };
                NumericUpDown[] durations = { null, numericUpDown232_follow_duration, numericUpDown233_sub_duration, numericUpDown234_donation_duration, numericUpDown235_cheer_duration, numericUpDown236_host_duration, numericUpDown237_raid_duration };
                CheckBox[] devcice1 = { null, checkBox232_follow_device1, checkBox233_sub_device1, checkBox234_donation_device1, checkBox235_cheer_device1, checkBox236_host_device1, checkBox237_raid_device1 };
                CheckBox[] devcice2 = { null, checkBox232_follow_device2, checkBox233_sub_device2, checkBox234_donation_device2, checkBox235_cheer_device2, checkBox236_host_device2, checkBox237_raid_device2 };
                CheckBox[] devcice3 = { null, checkBox232_follow_device3, checkBox233_sub_device3, checkBox234_donation_device3, checkBox235_cheer_device3, checkBox236_host_device3, checkBox237_raid_device3 };
                CheckBox[] devcice4 = { null, checkBox232_follow_device4, checkBox233_sub_device4, checkBox234_donation_device4, checkBox235_cheer_device4, checkBox236_host_device4, checkBox237_raid_device4 };
                for (int i = 0; i < Enum.GetNames(typeof(EffectChange.Type)).Length; i++) {
                    if (alerts[i] == null)
                        continue;
                    effects[i].SelectedText = alerts[i].GetEffectName();
                    isBrightness[i].Checked = alerts[i].IsActiveBrightness();
                    brightness[i].Enabled = alerts[i].IsActiveBrightness();
                    brightness[i].Value = alerts[i].GetBrightness();
                    if (i == 0)
                        continue;
                    isDuration[i].Checked = alerts[i].IsActiveDuration();
                    durations[i].Enabled = alerts[i].IsActiveDuration();
                    durations[i].Value = alerts[i].GetDuration();
                    devcice1[i].Checked = alerts[i].GetDevice(0);
                    devcice2[i].Checked = alerts[i].GetDevice(1);
                    devcice3[i].Checked = alerts[i].GetDevice(2);
                    devcice4[i].Checked = alerts[i].GetDevice(3);
                }
            }

            /* load chat commands */
            string chatCmdsFile = "settings/ChatCommands.save";
            if (File.Exists(chatCmdsFile)) {
                chatCmds = (EffectChange[])LoadObject(chatCmdsFile);
                ComboBox[] effects = { null, comboBox241_effect, comboBox242_effect, comboBox243_effect, comboBox244_effect, comboBox245_effect, comboBox246_effect, comboBox247_effect, comboBox248_effect, comboBox249_effect, comboBox2410_effect };
                CheckBox[] isBrightness = { checkBox240_mstr_brightness, checkBox241_cmd1_brightness, checkBox242_cmd2_brightness, checkBox243_cmd3_brightness, checkBox244_cmd4_brightness, checkBox245_cmd5_brightness, checkBox246_cmd6_brightness, checkBox247_cmd7_brightness, checkBox248_cmd8_brightness, checkBox249_cmd9_brightness, checkBox2410_cmd10_brightness };
                NumericUpDown[] brightness = { numericUpDown240_mstr_brightness, numericUpDown241_cmd1_brightness, numericUpDown242_cmd2_brightness, numericUpDown243_cmd3_brightness, numericUpDown244_cmd4_brightness, numericUpDown245_cmd5_brightness, numericUpDown246_cmd6_brightness, numericUpDown247_cmd7_brightness, numericUpDown248_cmd8_brightness, numericUpDown249_cmd9_brightness, numericUpDown2410_cmd10_brightness };
                CheckBox[] isDuration = { checkBox240_mstr_duration, checkBox241_cmd1_duration, checkBox242_cmd2_duration, checkBox243_cmd3_duration, checkBox244_cmd4_duration, checkBox245_cmd5_duration, checkBox246_cmd6_duration, checkBox247_cmd7_duration, checkBox248_cmd8_duration, checkBox249_cmd9_duration, checkBox2410_cmd10_duration };
                NumericUpDown[] durations = { numericUpDown240_mstr_duration, numericUpDown241_cmd1_duration, numericUpDown242_cmd2_duration, numericUpDown243_cmd3_duration, numericUpDown244_cmd4_duration, numericUpDown245_cmd5_duration, numericUpDown246_cmd6_duration, numericUpDown247_cmd7_duration, numericUpDown248_cmd8_duration, numericUpDown249_cmd9_duration, numericUpDown2410_cmd10_duration };
                CheckBox[] devcice1 = { checkBox240_device1, checkBox241_device1, checkBox242_device1, checkBox243_device1, checkBox244_device1, checkBox245_device1, checkBox246_device1, checkBox247_device1, checkBox248_device1, checkBox249_device1, checkBox2410_device1 };
                CheckBox[] devcice2 = { checkBox240_device2, checkBox241_device2, checkBox242_device2, checkBox243_device2, checkBox244_device2, checkBox245_device2, checkBox246_device2, checkBox247_device2, checkBox248_device2, checkBox249_device2, checkBox2410_device2 };
                CheckBox[] devcice3 = { checkBox240_device3, checkBox241_device3, checkBox242_device3, checkBox243_device3, checkBox244_device3, checkBox245_device3, checkBox246_device3, checkBox247_device3, checkBox248_device3, checkBox249_device3, checkBox2410_device3 };
                CheckBox[] devcice4 = { checkBox240_device4, checkBox241_device4, checkBox242_device4, checkBox243_device4, checkBox244_device4, checkBox245_device4, checkBox246_device4, checkBox247_device4, checkBox248_device4, checkBox249_device4, checkBox2410_device4 };
                for (int i = 0; i < Enum.GetNames(typeof(EffectChange.ChatCmd)).Length; i++) {
                    if (chatCmds[i] == null)
                        continue;
                    if (i > 0)
                        effects[i].SelectedText = chatCmds[i].GetEffectName();
                    isBrightness[i].Checked = chatCmds[i].IsActiveBrightness();
                    brightness[i].Enabled = chatCmds[i].IsActiveBrightness();
                    brightness[i].Value = chatCmds[i].GetBrightness();
                    isDuration[i].Checked = chatCmds[i].IsActiveDuration();
                    durations[i].Enabled = chatCmds[i].IsActiveDuration();
                    durations[i].Value = chatCmds[i].GetDuration();
                    devcice1[i].Checked = chatCmds[i].GetDevice(0);
                    devcice2[i].Checked = chatCmds[i].GetDevice(1);
                    devcice3[i].Checked = chatCmds[i].GetDevice(2);
                    devcice4[i].Checked = chatCmds[i].GetDevice(3);
                }
            }

            /* update NanoleafEventHandler */
            NanoleafEventHandler.Instance.SetNanoleafConfig(devices, alerts, chatCmds);
        }

        private void SaveObject(string filename, object obj) {
            string path = Path.GetDirectoryName(filename);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            using (System.IO.FileStream fs = System.IO.File.Create(filename)) {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.Serialize(fs, obj);
            }
        }
        private object LoadObject(string filename) {
            object obj = null;
            try {
                using (System.IO.FileStream fs = System.IO.File.OpenRead(filename)) {
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    obj = bf.Deserialize(fs);
                }
            } catch (Exception) {
                /* do nothing */
            }
            return obj;
        }

        private void AddCheckedChanged() {
            checkBox231_default_brightness.CheckedChanged += CheckBox231_brightness_CheckedChanged;
            checkBox232_follow_brightness.CheckedChanged += CheckBox232_brightness_CheckedChanged;
            checkBox233_sub_brightness.CheckedChanged += CheckBox233_brightness_CheckedChanged;
            checkBox234_donation_brightness.CheckedChanged += CheckBox234_brightness_CheckedChanged;
            checkBox235_cheer_brightness.CheckedChanged += CheckBox235_brightness_CheckedChanged;
            checkBox236_host_brightness.CheckedChanged += CheckBox236_brightness_CheckedChanged;
            checkBox237_raid_brightness.CheckedChanged += CheckBox237_brightness_CheckedChanged;

            checkBox232_follow_duration.CheckedChanged += CheckBox232_duration_CheckedChanged;
            checkBox233_sub_duration.CheckedChanged += CheckBox233_duration_CheckedChanged;
            checkBox234_donation_duration.CheckedChanged += CheckBox234_duration_CheckedChanged;
            checkBox235_cheer_duration.CheckedChanged += CheckBox235_duration_CheckedChanged;
            checkBox236_host_duration.CheckedChanged += CheckBox236_duration_CheckedChanged;
            checkBox237_raid_duration.CheckedChanged += CheckBox237_duration_CheckedChanged;
        }

        private void CheckBox231_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown231_default_brightness.Enabled = checkBox231_default_brightness.Checked;
        }
        private void CheckBox232_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown232_follow_brightness.Enabled = checkBox232_follow_brightness.Checked;
        }
        private void CheckBox233_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown233_sub_brightness.Enabled = checkBox233_sub_brightness.Checked;
        }
        private void CheckBox234_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown234_donation_brightness.Enabled = checkBox234_donation_brightness.Checked;
        }
        private void CheckBox235_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown235_cheer_brightness.Enabled = checkBox235_cheer_brightness.Checked;
        }
        private void CheckBox236_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown236_host_brightness.Enabled = checkBox236_host_brightness.Checked;
        }
        private void CheckBox237_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown237_raid_brightness.Enabled = checkBox237_raid_brightness.Checked;
        }

        private void CheckBox232_duration_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown232_follow_duration.Enabled = checkBox232_follow_duration.Checked;
        }
        private void CheckBox233_duration_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown233_sub_duration.Enabled = checkBox233_sub_duration.Checked;
        }
        private void CheckBox234_duration_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown234_donation_duration.Enabled = checkBox234_donation_duration.Checked;
        }
        private void CheckBox235_duration_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown235_cheer_duration.Enabled = checkBox235_cheer_duration.Checked;
        }
        private void CheckBox236_duration_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown236_host_duration.Enabled = checkBox236_host_duration.Checked;
        }
        private void CheckBox237_duration_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown237_raid_duration.Enabled = checkBox237_raid_duration.Checked;
        }

        private void AddCheckedChanged2() {
            checkBox240_mstr_brightness.CheckedChanged += CheckBox240_brightness_CheckedChanged;
            checkBox241_cmd1_brightness.CheckedChanged += CheckBox241_brightness_CheckedChanged;
            checkBox242_cmd2_brightness.CheckedChanged += CheckBox242_brightness_CheckedChanged;
            checkBox243_cmd3_brightness.CheckedChanged += CheckBox243_brightness_CheckedChanged;
            checkBox244_cmd4_brightness.CheckedChanged += CheckBox244_brightness_CheckedChanged;
            checkBox245_cmd5_brightness.CheckedChanged += CheckBox245_brightness_CheckedChanged;
            checkBox246_cmd6_brightness.CheckedChanged += CheckBox246_brightness_CheckedChanged;
            checkBox247_cmd7_brightness.CheckedChanged += CheckBox247_brightness_CheckedChanged;
            checkBox248_cmd8_brightness.CheckedChanged += CheckBox248_brightness_CheckedChanged;
            checkBox249_cmd9_brightness.CheckedChanged += CheckBox249_brightness_CheckedChanged;
            checkBox2410_cmd10_brightness.CheckedChanged += CheckBox2410_brightness_CheckedChanged;

            checkBox240_mstr_duration.CheckedChanged += CheckBox240_mstr_CheckedChanged;
            checkBox241_cmd1_duration.CheckedChanged += CheckBox241_cmd1_CheckedChanged;
            checkBox242_cmd2_duration.CheckedChanged += CheckBox242_cmd2_CheckedChanged;
            checkBox243_cmd3_duration.CheckedChanged += CheckBox243_cmd3_CheckedChanged;
            checkBox244_cmd4_duration.CheckedChanged += CheckBox244_cmd4_CheckedChanged;
            checkBox245_cmd5_duration.CheckedChanged += CheckBox245_cmd5_CheckedChanged;
            checkBox246_cmd6_duration.CheckedChanged += CheckBox246_cmd6_CheckedChanged;
            checkBox247_cmd7_duration.CheckedChanged += CheckBox247_cmd7_CheckedChanged;
            checkBox248_cmd8_duration.CheckedChanged += CheckBox248_cmd8_CheckedChanged;
            checkBox249_cmd9_duration.CheckedChanged += CheckBox249_cmd9_CheckedChanged;
            checkBox2410_cmd10_duration.CheckedChanged += CheckBox2410_cmd10_CheckedChanged;
        }

        private void CheckBox240_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown240_mstr_brightness.Enabled = checkBox240_mstr_brightness.Checked;
        }
        private void CheckBox241_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown241_cmd1_brightness.Enabled = checkBox241_cmd1_brightness.Checked;
        }
        private void CheckBox242_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown242_cmd2_brightness.Enabled = checkBox242_cmd2_brightness.Checked;
        }
        private void CheckBox243_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown243_cmd3_brightness.Enabled = checkBox243_cmd3_brightness.Checked;
        }
        private void CheckBox244_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown244_cmd4_brightness.Enabled = checkBox244_cmd4_brightness.Checked;
        }
        private void CheckBox245_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown245_cmd5_brightness.Enabled = checkBox245_cmd5_brightness.Checked;
        }
        private void CheckBox246_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown246_cmd6_brightness.Enabled = checkBox246_cmd6_brightness.Checked;
        }
        private void CheckBox247_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown247_cmd7_brightness.Enabled = checkBox247_cmd7_brightness.Checked;
        }
        private void CheckBox248_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown248_cmd8_brightness.Enabled = checkBox248_cmd8_brightness.Checked;
        }
        private void CheckBox249_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown249_cmd9_brightness.Enabled = checkBox249_cmd9_brightness.Checked;
        }
        private void CheckBox2410_brightness_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown2410_cmd10_brightness.Enabled = checkBox2410_cmd10_brightness.Checked;
        }

        private void CheckBox240_mstr_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown240_mstr_duration.Enabled = checkBox240_mstr_duration.Checked;
        }
        private void CheckBox241_cmd1_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown241_cmd1_duration.Enabled = checkBox241_cmd1_duration.Checked;
        }
        private void CheckBox242_cmd2_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown242_cmd2_duration.Enabled = checkBox242_cmd2_duration.Checked;
        }
        private void CheckBox243_cmd3_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown243_cmd3_duration.Enabled = checkBox243_cmd3_duration.Checked;
        }
        private void CheckBox244_cmd4_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown244_cmd4_duration.Enabled = checkBox244_cmd4_duration.Checked;
        }
        private void CheckBox245_cmd5_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown245_cmd5_duration.Enabled = checkBox245_cmd5_duration.Checked;
        }
        private void CheckBox246_cmd6_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown246_cmd6_duration.Enabled = checkBox246_cmd6_duration.Checked;
        }
        private void CheckBox247_cmd7_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown247_cmd7_duration.Enabled = checkBox247_cmd7_duration.Checked;
        }
        private void CheckBox248_cmd8_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown248_cmd8_duration.Enabled = checkBox248_cmd8_duration.Checked;
        }
        private void CheckBox249_cmd9_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown249_cmd9_duration.Enabled = checkBox249_cmd9_duration.Checked;
        }
        private void CheckBox2410_cmd10_CheckedChanged(Object sender, EventArgs e) {
            numericUpDown2410_cmd10_duration.Enabled = checkBox2410_cmd10_duration.Checked;
        }

        private void Button221_search_Click(object sender, EventArgs e) {
            DiscoverDialog(0);
        }
        private void Button222_search_Click(object sender, EventArgs e) {
            DiscoverDialog(1);
        }
        private void Button223_search_Click(object sender, EventArgs e) {
            DiscoverDialog(2);
        }
        private void Buttonl224_search_Click(object sender, EventArgs e) {
            DiscoverDialog(3);

        }
        private void DiscoverDialog(int DeviceNo) {
            Action act = () => {
                NanoleafDiscover.Go(this);
            };
            using (WaitForm wf = new WaitForm(act)) {
                wf.ShowDialog(this);
            }
            using (DeviceChooser wf = new DeviceChooser(this, DeviceNo)) {
                wf.ShowDialog(this);
            }
        }

        private void UpdateEffectList() {
            if (alerts == null || chatCmds == null || devices == null || alerts.Length == 0 || chatCmds.Length == 0 || devices.Length == 0)
                return;
            for (int i = 0; i < devices.Length; i++) {
                if (devices[i] == null)
                    continue;
                string[] el = Nanoleaf.GetEffectList(devices[i]);
                if (el != null)
                    devices[i].SetEffectList(el);
            }

            ArrayList els = new ArrayList();
            for (int i = 0; i < devices.Length; i++) {
                if (devices[i] == null)
                    continue;
                string[] el = devices[i].GetEffectList();
                //els.Add("--- Device #" + i + " ---");
                for (int j = 0; j < el.Length; j++) {
                    els.Add(el[j]);
                }
            }

            ComboBox[] effects = { comboBox231_default, comboBox232_follow, comboBox233_sub, comboBox234_donation, comboBox235_cheer, comboBox236_host, comboBox237_raid, comboBox241_effect, comboBox242_effect, comboBox243_effect, comboBox244_effect, comboBox245_effect, comboBox246_effect, comboBox247_effect, comboBox248_effect, comboBox249_effect, comboBox2410_effect };
            for (int i = 0; i < effects.Length; i++) {
                if (effects[i].Items.Count == 0) {
                    if (i == 0)
                        effects[i].Items.Add(EffectChange.CurrentEffect);
                    else
                        effects[i].Items.Add("");
                }
                foreach (string en in els) {
                    effects[i].Items.Add(en);
                }
                if (i < 7 && alerts[i] != null) {
                    effects[i].SelectedIndex = effects[i].FindStringExact(alerts[i].GetEffectName());
                    effects[i].SelectedText = alerts[i].GetEffectName();
                } else if (i >= 7 && chatCmds[i - 6] != null) {
                    effects[i].SelectedIndex = effects[i].FindStringExact(chatCmds[i - 6].GetEffectName());
                    effects[i].SelectedText = chatCmds[i - 6].GetEffectName();
                }
            }
        }

        public static DialogResult ShowAlert(string text, string title) {
            Alert form = new Alert(title, text);
            return form.ShowDialog();
        }

        public void NewDisvoveredDevices() {
            DisvoveredDevices = new ArrayList();
        }
        public void SetDisvoveredDevices(string Location) {
            DisvoveredDevices.Add(Location);
        }
        public ArrayList GetDisvoveredDevices() {
            return DisvoveredDevices;
        }
        public void SetDeviceLocation(int DevNo, string DeviceLocation) {
            TextBox[] location = { textBox221_location, textBox222_location, textBox223_location, textBox224_location };
            location[DevNo].Text = DeviceLocation;
        }

        private void MainForm_AvoidClosing(Object sender, FormClosingEventArgs e) {
            if (closeApp)
                return;
            e.Cancel = true;
            Hide();
            ShowBallon("Nanoleaf Integration for SLCB: WebSocket client", "Running in the background.");
        }

        private void MainForm_Resize(object sender, EventArgs e) {
            if (WindowState == FormWindowState.Minimized) {
                Hide();
                ShowBallon("Nanoleaf Integration for SLCB: WebSocket client", "Running in the background.");
            }
        }

        private void ShowBallon(string title, string body) {
            if (title != null)
                notifyIcon.BalloonTipTitle = title;

            if (body != null)
                notifyIcon.BalloonTipText = body;

            notifyIcon.ShowBalloonTip(3000);
        }

        private void NotifyIcon_DoubleClick(object Sender, EventArgs e) {
            // Show the form when the user double clicks on the notify icon
            Show();
            WindowState = FormWindowState.Normal;
            // Activate the form
            Activate();
        }

        private void MenuItemExit_Click(object Sender, EventArgs e) {
            closeApp = true;
            notifyIcon.Visible = false;
            Close();
            Application.Exit();
        }
    }
}
