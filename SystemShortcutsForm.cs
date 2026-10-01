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

            // Add an icon and title area similar to ShortcutSettingsForm
            var iconBox = new PictureBox
            {
                Location = new Point(28, 24),
                Size = new Size(40, 40),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                TabStop = false
            };
            using (var large = new Icon(Properties.Resources.AppIcon, 40, 40))
                iconBox.Image = large.ToBitmap();

            var heading = new Label
            {
                Location = new Point(84, 24),
                AutoSize = true,
                Text = "System Shortcuts",
                Font = new Font("Segoe UI Semibold", 12.5F),
                ForeColor = Theme.Text,
                BackColor = Color.Transparent
            };
            Controls.Add(iconBox);
            Controls.Add(heading);

            // Container for the list, styled as a card
            var card = new Card
            {
                Location = new Point(24, 80),
                Size = new Size(400, 350),
                Title = "", // No title in card, we used a heading label
                Description = ""
            };
            Controls.Add(card);

            var container = new Panel
            {
                Location = new Point(15, 15),
                Size = new Size(370, 320),
                BackColor = Theme.Surface, // Matches card background
                AutoScroll = true
            };
            card.Controls.Add(container);

            int y = 10;
            foreach (var shortcut in shortcuts)
            {
                var lbl = new Label
                {
                    Location = new Point(10, y),
                    Size = new Size(350, 20),
                    Text = shortcut,
                    Font = Theme.Base,
                    ForeColor = Theme.Text,
                    BackColor = Color.Transparent
                };
                container.Controls.Add(lbl);
                y += 25;
            }

            var close = new ModernButton
            {
                Text = "Close",
                Size = new Size(96, 36),
                Location = new Point(328 - 96, card.Bottom + 14),
                DialogResult = DialogResult.OK
            };
            close.Click += (s, e) => Close();
            Controls.Add(close);

            ClientSize = new Size(448, close.Bottom + 20);
            Theme.Apply(this);
        }
    }
}
