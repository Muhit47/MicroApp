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
                "Win + D : Show desktop",
                "Win + L : Lock PC",
                "Win + E : File Explorer",
                "Win + R : Run dialog",
                "Alt + Tab : Switch windows",
                "Alt + F4 : Close window",
                "Ctrl + Shift + Esc : Task Manager",
                "Ctrl + Alt + Del : Security screen",
                "Win + \u2191 : Maximize window",
                "Win + \u2193 : Minimize / restore",
                "Win + \u2190 : Snap left half",
                "Win + \u2192 : Snap right half",
                "Win + Home : Minimize all except active",
                "Win + Shift + S : Screenshot (Snip Sketch)",
                "Win + P : Display projection",
                "Win + , : Desktop peek",
                "Alt + F : File menu (in many apps)",
                "Alt + Enter : Properties (in Explorer)",
                "Ctrl + C : Copy",
                "Ctrl + V : Paste",
                "Ctrl + X : Cut",
                "Ctrl + Z : Undo",
                "Ctrl + Y : Redo"
            };

            var list = new ListBox
            {
                Location = new Point(24, 24),
                Size = new Size(400, 400),
                BackColor = Theme.FieldBg,
                ForeColor = Theme.Text,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9F)
            };
            list.Items.AddRange(shortcuts);
            Controls.Add(list);

            var close = new ModernButton
            {
                Text = "Close",
                Size = new Size(96, 36),
                Location = new Point(176, 440),
                DialogResult = DialogResult.OK
            };
            close.Click += (s, e) => Close();
            Controls.Add(close);

            ClientSize = new Size(448, 500);
            Theme.Apply(this);
        }
    }
}
