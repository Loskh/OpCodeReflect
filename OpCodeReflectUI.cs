using Advanced_Combat_Tracker;
using FFXIV_ACT_Plugin.Common;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace OpCodeReflect
{
    class OpCodeReflectUI : UserControl
    {

        private GroupBox _mainGroupBox;
        public Label LabelOpCodePath;

        public Button ButtonStart { get; private set; }
        public Button ButtonLoad { get; private set; }
        private static ListBox lstMessages;
        private static Button cmdClearMessages;
        private static Button cmdCopyProblematic;
        private CheckBox _autoStart;

        public string OpCodePath ="";
        public bool AutoStart => _autoStart.Checked;
        private static readonly string SettingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\OpCodeReflect.config.xml");

        public void InitializeComponent(TabPage pluginScreenSpace) {
            _mainGroupBox = new GroupBox { Location = new Point(12, 12), Text = "日志", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };

            LabelOpCodePath = new Label { AutoSize = true, Text = $"OpCode路径:{OpCodePath}", Location = new Point(10, 20) };
            ButtonStart = new Button { Text = "刷新并显示OpCode", Location = new Point(10, LabelOpCodePath.Location.Y+20), Size = new Size(150, 35) };
            ButtonLoad = new Button { Text = "载入OpCode", Location = new Point(ButtonStart.Location.X+ ButtonStart.Size.Width+20, ButtonStart.Location.Y),Size = new Size(100, 35) };
            _autoStart = new CheckBox { AutoSize = true, Text = "自动启动", Location = new Point(ButtonLoad.Location.X + ButtonLoad.Size.Width + 20, ButtonLoad.Location.Y) };

            lstMessages = new ListBox { Location = new Point(10, ButtonStart.Location.Y+50), FormattingEnabled = true, ScrollAlwaysVisible = true, HorizontalScrollbar = true, Size = new Size(440, 500), };
            cmdClearMessages = new Button { Location = new Point(10, lstMessages.Location.Y + lstMessages.Height + 10), Size = new Size(200, 35), Text = "清空日志", UseVisualStyleBackColor = true };
            cmdCopyProblematic = new Button { Location = new Point(220, lstMessages.Location.Y + lstMessages.Height + 10), Size = new Size(200, 35), Text = "复制到剪贴板", UseVisualStyleBackColor = true };

            _mainGroupBox.Controls.Add(LabelOpCodePath);
            _mainGroupBox.Controls.Add(ButtonStart);
            _mainGroupBox.Controls.Add(ButtonLoad);
            _mainGroupBox.Controls.Add(_autoStart);
            _mainGroupBox.Controls.Add(lstMessages);
            _mainGroupBox.Controls.Add(cmdClearMessages);
            _mainGroupBox.Controls.Add(cmdCopyProblematic);

            pluginScreenSpace.Controls.Add(_mainGroupBox);
            pluginScreenSpace.AutoSize = true;

            cmdCopyProblematic.Click += cmdCopyProblematic_Click;
            cmdClearMessages.Click += cmdClearMessages_Click;

            LoadSettings();

            _mainGroupBox.ResumeLayout(false);
            _mainGroupBox.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        public void cmdCopyProblematic_Click(object sender, EventArgs e) {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (object item in lstMessages.Items) {
                stringBuilder.AppendLine((item ?? "").ToString());
            }
            if (stringBuilder.Length > 0) {
                Clipboard.SetText(stringBuilder.ToString());
            }
        }

        public void cmdClearMessages_Click(object sender, EventArgs e) {
            lstMessages.Items.Clear();
        }

        public void Log(string message) {
            ACTWrapper.RunOnACTUIThread((System.Action)delegate {
                lstMessages.Items.Add(message);
            });
        }

        public void SetPath(string path) {
            OpCodePath = path;
            LabelOpCodePath.Text = $"OpCode路径:{OpCodePath}";
        }

        void LoadSettings() {

            if (File.Exists(SettingsFile)) {
                XmlDocument xdo = new XmlDocument();
                try {
                    xdo.Load(SettingsFile);
                    XmlNode head = xdo.SelectSingleNode("Config");
                    SetPath(head?.SelectSingleNode("Path")?.InnerText);
                    _autoStart.Checked = bool.Parse(head?.SelectSingleNode("AutoStart")?.InnerText ?? "false");
                }
                catch (Exception ex) {
                    Log("配置文件载入异常");
                    File.Delete(SettingsFile);
                    Log("已清除错误的配置文件");
                    Log("设置已被重置");
                }
            }
        }

        public void SaveSettings() {
            FileStream fs = new FileStream(SettingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            XmlTextWriter xWriter = new XmlTextWriter(fs, Encoding.UTF8) { Formatting = Formatting.Indented, Indentation = 1, IndentChar = '\t' };
            xWriter.WriteStartDocument(true);
            xWriter.WriteStartElement("Config");    // <Config>
            xWriter.WriteElementString("Path", OpCodePath);
            xWriter.WriteElementString("AutoStart", _autoStart.Checked.ToString());
            xWriter.WriteEndElement();  // </Config>
            xWriter.WriteEndDocument(); // Tie up loose ends (shouldn't be any)
            xWriter.Flush();    // Flush the file buffer to disk
            xWriter.Close();
        }
    }
}
