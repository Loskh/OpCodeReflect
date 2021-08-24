using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using Machina.FFXIV.Headers;
using System.IO;

namespace OpCodeReflect
{
    public class OpCodeReflect : UserControl, IActPluginV1
    {
        private static OpCodeReflectUI PluginUI;
        private Label _lblStatus; // The status label that appears in ACT's Plugin tab
        public Dictionary<string, ushort> CurrentOpcodes { get; set; }
        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText) {
            pluginScreenSpace.Text = "OpCodeReflect";
            PluginUI = new OpCodeReflectUI();
            PluginUI.InitializeComponent(pluginScreenSpace);
            if (PluginUI.OpCodePath == "") {
                var selfPluginData = ActGlobals.oFormActMain.PluginGetSelfData(this);
                var path = selfPluginData.pluginFile.DirectoryName;
                var txtPath = path + "\\OpCodes.txt";
                PluginUI.OpCodePath = txtPath;
            }
            Dock = DockStyle.Fill; // Expand the UserControl to fill the tab's client space
            _lblStatus = pluginStatusText; // Hand the status label's reference to our local var
            PluginUI.ButtonStart.Click += ButtonStart_Click;
            PluginUI.ButtonLoad.Click += ButtonLoad_Click;
            if (PluginUI.AutoStart) {
                GetOpcodesFromTxt(PluginUI.OpCodePath);
                SetOpcodes();
                GetOpcodes();
            }
        }

        private void ButtonLoad_Click(object sender, EventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "请选择文件";
            dialog.Filter = "所有文件(*.txt)|*.txt";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string file = dialog.FileName;
                PluginUI.SetPath(file);
                GetOpcodesFromTxt(file);
                SetOpcodes();
                GetOpcodes();
            }
        }

        private void ButtonStart_Click(object sender, EventArgs e) {
            GetOpcodesFromTxt(PluginUI.OpCodePath);
            SetOpcodes();
            GetOpcodes();
        }

        public void GetOpcodesFromTxt(string txtPath) {

            //https://github.com/ravahn/machina/blob/master/Machina.FFXIV/Headers/Opcodes/OpcodeManager.cs#L55
            //PluginUI.Log(txtPath);
            if (!File.Exists(txtPath)) {
                PluginUI.Log($"{txtPath}不存在！");
                return;
            }
            using (StreamReader sr = new StreamReader(txtPath)) {
                string[][] data = sr.ReadToEnd()
                    .Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries)).ToArray();
                var dict = data.ToDictionary(
                    x => x[0].Trim(),
                    x => Convert.ToUInt16(x[1].Trim(), 16));
                CurrentOpcodes = dict;
            }
            PluginUI.Log($"已载入{CurrentOpcodes.Count}个OpCode");
            //foreach (KeyValuePair<string, ushort> kvp in CurrentOpcodes) {
            //    PluginUI.Log($"{kvp.Key}={kvp.Value:X4}");
            //}
        }

        public void Test() {
            private static IAbilityResource Abilities;
        }

        public void GetOpcodes() {
            PluginUI.Log("获取OpCode...");
            FieldInfo[] opcodes = typeof(Server_MessageType).GetFields();
            Type type = typeof(Server_MessageType);
            object obj1 = type.Assembly.CreateInstance(type.FullName);
            foreach (var opcode in opcodes) {
                PluginUI.Log($"{opcode.Name}={(ushort)(Server_MessageType)opcode.GetValue(obj1):X4}");
            }
        }

        public void SetOpcodes() {
            PluginUI.Log("设置OpCode...");
            FieldInfo[] opcodes = typeof(Server_MessageType).GetFields();
            Type type = typeof(Server_MessageType);
            object obj1 = type.Assembly.CreateInstance(type.FullName);
            foreach (var opcode in opcodes) {
                ushort newOpCode = 0;
                if (CurrentOpcodes.TryGetValue(opcode.Name, out newOpCode))
                    opcode.SetValue(obj1, (Server_MessageType)newOpCode);
                else
                    PluginUI.Log($"没找到{opcode.Name}");
            }
            PluginUI.Log("设置OpCode成功");
        }
        public void DeInitPlugin() {
            PluginUI.SaveSettings();
        }
    }
}
