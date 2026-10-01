using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MicroApp
{
    class ShortcutItem
    {
        public bool Enabled { get; set; }
        public string Description { get; set; }
        public string Keys { get; set; }
        public ModernCheckBox CheckBox { get; set; }
        public TextBox KeyTextBox { get; set; }
    }

    class SystemShortcutsForm : PixelPerfectForm
    {
        private List<ShortcutItem> _shortcuts;

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

            _shortcuts = new List<ShortcutItem>
            {
                new ShortcutItem { Enabled = true, Description = "Show desktop", Keys = "Win + D" },
                new ShortcutItem { Enabled = true, Description = "Lock PC", Keys = "Win + L" },
                new ShortcutItem { Enabled = true, Description = "File Explorer", Keys = "Win + E" },
                new ShortcutItem { Enabled = true, Description = "Run dialog", Keys = "Win + R" },
                new ShortcutItem { Enabled = true, Description = "Switch windows", Keys = "Alt + Tab" },
                new ShortcutItem { Enabled = true, Description = "Close window", Keys = "Alt + F4" },
                new ShortcutItem { Enabled = true, Description = "Task Manager", Keys = "Ctrl + Shift + Esc" },
                new ShortcutItem { Enabled = true, Description = "Security screen", Keys = "Ctrl + Alt + Del" },
                new ShortcutItem { Enabled = true, Description = "Maximize window", Keys = "Win + \u2191" },
                new ShortcutItem { Enabled = true, Description = "Minimize / restore", Keys = "Win + \u2193" },
                new ShortcutItem { Enabled = true, Description = "Snap left half", Keys = "Win + \u2190" },
                new ShortcutItem { Enabled = true, Description = "Snap right half", Keys = "Win + \u2192" },
                new ShortcutItem { Enabled = true, Description = "Minimize all except active", Keys = "Win + Home" },
                new ShortcutItem { Enabled = true, Description = "Screenshot (Snip Sketch)", Keys = "Win + Shift + S" },
                new ShortcutItem { Enabled = true, Description = "Display projection", Keys = "Win + P" },
                new ShortcutItem { Enabled = true, Description = "Desktop peek", Keys = "Win + ," },
                new ShortcutItem { Enabled = true, Description = "File menu (in many apps)", Keys = "Alt + F" },
                new ShortcutItem { Enabled = true, Description = "Properties (in Explorer)", Keys = "Alt + Enter" },
                new ShortcutItem { Enabled = true, Description = "Copy", Keys = "Ctrl + C" },
                new ShortcutItem { Enabled = true, Description = "Paste", Keys = "Ctrl + V" },
                new ShortcutItem { Enabled = true, Description = "Cut", Keys = "Ctrl + X" },
                new ShortcutItem { Enabled = true, Description = "Undo", Keys = "Ctrl + Z" },
                new ShortcutItem { Enabled = true, Description = "Redo", Keys = "Ctrl + Y" }
            };
            
            // In a real app, we would load existing settings here.
            // For now, assume fixed but editable.

            var heading = new Label
            {
                Location = new Point(24, 24),
                AutoSize = true,
                Text = "System Shortcuts",
                Font = new Font("Segoe UI Semibold", 12.5F),
                ForeColor = Theme.Text,
                BackColor = Color.Transparent
            };
            Controls.Add(heading);

            var container = new Panel
            {
                Location = new Point(24, 60),
                Size = new Size(500, 400),
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            Controls.Add(container);

            int y = 0;
            foreach (var item in _shortcuts)
            {
                var chk = new ModernCheckBox
                {
                    Location = new Point(0, y),
                    Size = new Size(20, 30),
                    Checked = item.Enabled,
                    Text = ""
                };
                item.CheckBox = chk;
                container.Controls.Add(chk);

                var lbl = new Label
                {
                    Location = new Point(30, y + 5),
                    Size = new Size(250, 20),
                    Text = item.Description,
                    ForeColor = Theme.Text,
                    BackColor = Color.Transparent
                };
                container.Controls.Add(lbl);

                var field = new FieldHost
                {
                    Location = new Point(280, y),
                    Size = new Size(200, 30)
                };
                var txt = new TextBox
                {
                    Text = item.Keys,
                    ForeColor = Theme.Text,
                    BackColor = Theme.FieldBg,
                    BorderStyle = BorderStyle.None
                };
                item.KeyTextBox = txt;
                field.Controls.Add(txt);
                container.Controls.Add(field);

                y += 40;
            }

            var btnCancel = new ModernButton
            {
                Text = "Cancel",
                Size = new Size(96, 36),
                Location = new Point(330, 480)
            };
            btnCancel.Click += (s, e) => Close();
            Controls.Add(btnCancel);

            var btnSave = new ModernButton
            {
                Text = "Save",
                Size = new Size(96, 36),
                Location = new Point(436, 480),
                Accent = true
            };
            btnSave.Click += (s, e) => {
                foreach (var item in _shortcuts)
                {
                    item.Enabled = item.CheckBox.Checked;
                    item.Keys = item.KeyTextBox.Text;
                }
                ModernDialog.Info("Saved", "System shortcut settings updated.");
                Close();
            };
            Controls.Add(btnSave);

            ClientSize = new Size(550, 530);
            Theme.Apply(this);
        }
    }
}
