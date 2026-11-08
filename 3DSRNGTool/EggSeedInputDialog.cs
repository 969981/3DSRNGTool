using System.Drawing;
using System.Windows.Forms;

namespace Pk3DSRNGTool
{
    internal sealed class EggSeedInputDialog : Form
    {
        private readonly ComboBox mode = new ComboBox();
        private readonly TextBox seed = new TextBox();
        public bool UseFullSeed => mode.SelectedIndex == 0;
        public string SeedText => seed.Text;

        public EggSeedInputDialog(bool gen7, bool useFullSeed, string currentSeed)
        {
            Text = "孵蛋 Seed 输入";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = MinimizeBox = false;
            ClientSize = new Size(390, 142);
            Controls.Add(new Label { Text = "输入模式", AutoSize = true, Location = new Point(14, 17) });
            mode.DropDownStyle = ComboBoxStyle.DropDownList;
            mode.Items.AddRange(new object[] { "完整长 Seed", "分段 Seed（原有输入框）" });
            mode.SelectedIndex = useFullSeed ? 0 : 1;
            mode.Location = new Point(100, 12);
            mode.Size = new Size(270, 24);
            Controls.Add(mode);
            Controls.Add(new Label { Text = "完整 Seed", AutoSize = true, Location = new Point(14, 55) });
            seed.Location = new Point(100, 50);
            seed.Size = new Size(270, 24);
            seed.Font = new Font("Consolas", 10F);
            seed.CharacterCasing = CharacterCasing.Upper;
            seed.MaxLength = gen7 ? 32 : 16;
            seed.Text = currentSeed;
            seed.Enabled = useFullSeed;
            mode.SelectedIndexChanged += (s, e) => seed.Enabled = UseFullSeed;
            Controls.Add(seed);
            Controls.Add(new Label { Text = gen7 ? "SM / USUM：32 位十六进制（4 × 8 位）" : "XY / ORAS：16 位十六进制（2 × 8 位）", AutoSize = true, Location = new Point(100, 80), ForeColor = SystemColors.GrayText });
            var ok = new Button { Text = "应用", DialogResult = DialogResult.OK, Location = new Point(214, 108), Size = new Size(75, 26) };
            var cancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, Location = new Point(295, 108), Size = new Size(75, 26) };
            Controls.AddRange(new Control[] { ok, cancel });
            AcceptButton = ok;
            CancelButton = cancel;
        }
    }
}
