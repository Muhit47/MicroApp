using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MicroApp
{
    class SystemShortcutsForm : PixelPerfectForm
    {
        public SystemShortcutsForm()
        {
            Theme.Init(ThemeHelper.IsDarkMode);
            Text = "MicroApp - System Shortcuts";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.Bg;
            Font = Theme.Base;

            var shortcuts = new string[]
            {
                "Win + D : Show/Hide Desktop",
                "Win + E : Open File Explorer",
                "Win + L : Lock Screen",
                "Win + I : Settings",
                "Win + V : Clipboard History",
                "Alt + Tab : Switch Apps",
                "Ctrl + Shift + Esc : Task Manager"
            };

            var list = new ListBox
            {
                Location = new Point(24, 24),
                Size = new Size(300, 200),
                BackColor = Theme.FieldBg,
                ForeColor = Theme.Text,
                BorderStyle = BorderStyle.None
            };
            list.Items.AddRange(shortcuts);
            Controls.Add(list);

            var close = new ModernButton
            {
                Text = "Close",
                Size = new Size(96, 36),
                Location = new Point(126, 240),
                DialogResult = DialogResult.OK
            };
            close.Click += (s, e) => Close();
            Controls.Add(close);

            ClientSize = new Size(348, 296);
            Theme.Apply(this);
        }
    }
}
