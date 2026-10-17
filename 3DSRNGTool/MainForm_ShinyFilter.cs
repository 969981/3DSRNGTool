using System;
using System.Drawing;
using System.Windows.Forms;
using Pk3DSRNGTool.Core;

namespace Pk3DSRNGTool
{
    public partial class MainForm
    {
        private ComboBox ShinyModeSelector;
        private bool _updatingShinyMode;
        private bool _confirmedTRV;
        private int _previousShinyMode;

        private static readonly string[][] ShinyModeLabels =
        {
            new[] { "Any", "Non-shiny", "Star shiny", "Square shiny", "Star / Square" },
            new[] { "指定なし", "色違い以外", "星型色違い", "四角型色違い", "色違いすべて" },
            new[] { "Tous", "Non chromatique", "Étoile", "Carré", "Étoile / Carré" },
            new[] { "Beliebig", "Nicht schillernd", "Stern", "Quadrat", "Stern / Quadrat" },
            new[] { "Cualquiera", "No variocolor", "Estrella", "Cuadrado", "Estrella / Cuadrado" },
            new[] { "任意", "不闪光", "仅星闪", "仅方闪", "星闪 / 方闪" },
        };

        private void InitializeShinyModeFilter()
        {
            // Keep the original checkboxes as compatibility bridges for existing presets.
            ShinyOnly.Visible = false;
            SquareShinyOnly.Visible = false;
            ShinyModeSelector = new ComboBox
            {
                Name = "ShinyModeSelector",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(257, 218),
                Size = new Size(127, 21),
                TabIndex = ShinyOnly.TabIndex,
                IntegralHeight = false,
                MaxDropDownItems = 5
            };
            Filters.Controls.Add(ShinyModeSelector);
            ShinyModeSelector.BringToFront();
            RefreshShinyModeLabels();
            ShinyModeSelector.SelectedIndexChanged += ShinyModeSelector_SelectedIndexChanged;

            const string hint = "仅有 TSV 无法区分星闪/方闪。需要确认 TRV = (TID XOR SID) & 0xF；" +
                                "其他 TSV 列表没有 TRV 时仅能判定是否闪光。";
            DGVToolTip.SetToolTip(ShinyModeSelector, hint);
            DGVToolTip.SetToolTip(TRV, hint);
        }

        private void RefreshShinyModeLabels()
        {
            if (ShinyModeSelector == null) return;
            int selected = Math.Max(0, ShinyModeSelector.SelectedIndex);
            int languageIndex = Math.Max(0, Math.Min(lindex, ShinyModeLabels.Length - 1));
            _updatingShinyMode = true;
            try
            {
                ShinyModeSelector.Items.Clear();
                ShinyModeSelector.Items.AddRange(ShinyModeLabels[languageIndex]);
                ShinyModeSelector.SelectedIndex = selected;
            }
            finally { _updatingShinyMode = false; }
        }

        private void InvalidateConfirmedTRV()
        {
            _confirmedTRV = false;
            if (ShinyModeSelector != null && (ShinyModeSelector.SelectedIndex == 2 ||
                                             ShinyModeSelector.SelectedIndex == 3))
                ShinyModeSelector.SelectedIndex = 0;
        }

        private void ShinyModeSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingShinyMode || ShinyModeSelector.SelectedIndex < 0) return;
            int selected = ShinyModeSelector.SelectedIndex;
            if ((selected == 2 || selected == 3) && !_confirmedTRV)
            {
                DialogResult answer = MessageBox.Show(
                    "要准确区分星闪和方闪，必须知道当前训练家的 TRV（TID XOR SID 的低 4 位）。\n" +
                    "仅有 TSV 无法推算 TRV。请先在顶部 TRV 输入框填入真实值。\n\n" +
                    "确认已填写正确的 TRV？",
                    "星闪 / 方闪筛选", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (answer != DialogResult.Yes)
                {
                    ShinyModeSelector.SelectedIndex = _previousShinyMode;
                    return;
                }
                _confirmedTRV = true;
            }

            _updatingShinyMode = true;
            try
            {
                // Keep legacy call sites and filter-file import functioning.
                ShinyOnly.Checked = selected >= 2;
                SquareShinyOnly.Checked = selected == 3;
                ShinyOnly.Visible = false;
                SquareShinyOnly.Visible = false;
                _previousShinyMode = selected;
            }
            finally { _updatingShinyMode = false; }
        }
    }
}
