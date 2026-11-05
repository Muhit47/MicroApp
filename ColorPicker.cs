using System.Collections.Generic;
using Gma.System.MouseKeyHook;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace MicroApp
{
    /// <summary>
    /// Screen colour picker: "+" crosshair everywhere, a magnified loupe of the pixels
    /// under it with the live #RRGGBB, and one click copies that hex code to the clipboard.
    /// Right-click or Esc cancels.
    /// </summary>
    static class ColorPicker
    {
        static IKeyboardMouseEvents _hook;
        static ColorPickerHud _hud;
        static Timer _timer;
        static Action<string> _picked;

        public static bool Active { get { return _hook != null; } }

        /// <summary>Starts picking; <paramref name="picked"/> gets "#RRGGBB" on a click.</summary>
        public static void Start(Action<string> picked)
        {
            if (_hook != null) return;
            _picked = picked;

            uint[] cursors = { Native.NORMAL, Native.IBEAM, Native.HAND };
            for (int i = 0; i < cursors.Length; i++)
                Native.SetSystemCursor(Native.CopyIcon(Native.LoadCursor(IntPtr.Zero, (int)Native.CROSS)), cursors[i]);

            _hud = new ColorPickerHud();
            _hud.Sample(Cursor.Position);
            _hud.Show();

            _hook = Hook.GlobalEvents();
            _hook.MouseDownExt += Hook_MouseDownExt;
            _hook.KeyDown += Hook_KeyDown;

            _timer = new Timer { Interval = 30 };
            _timer.Tick += (s, e) => { if (_hud != null) _hud.Sample(Cursor.Position); };
            _timer.Start();
        }

        static void Hook_MouseDownExt(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                e.Handled = true;   // the click belongs to the picker, not the app underneath
                var p = new Point(e.X, e.Y);
                _hud.BeginInvoke(new Action(() => Finish(p)));
            }
            else if (e.Button == MouseButtons.Right)
            {
                e.Handled = true;
                _hud.BeginInvoke(new Action(Cancel));
            }
        }

        static void Hook_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                _hud.BeginInvoke(new Action(Cancel));
            }
        }

        static void Finish(Point p)
        {
            if (_hook == null) return;
            Color c = ReadPixel(p);
            var callback = _picked;
            End();
            string hex = Hex(c);
            ColorUtils.AddToHistory(hex);
            callback?.Invoke(hex);
        }

        public static void Cancel()
        {
            if (_hook == null) return;
            End();
        }

        static void End()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
            if (_hook != null)
            {
                _hook.MouseDownExt -= Hook_MouseDownExt;
                _hook.KeyDown -= Hook_KeyDown;
                _hook.Dispose();
                _hook = null;
            }
            if (_hud != null)
            {
                _hud.Close();
                _hud.Dispose();
                _hud = null;
            }
            _picked = null;
            Native.SystemParametersInfo(0x0057, 0, null, 0);   // restore the real cursors
        }

        public static string Hex(Color c)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
        }

        public static Color ReadPixel(Point p)
        {
            try
            {
                using (var bmp = new Bitmap(1, 1, PixelFormat.Format32bppArgb))
                {
                    using (var g = Graphics.FromImage(bmp))
                        g.CopyFromScreen(p.X, p.Y, 0, 0, new Size(1, 1));
                    return bmp.GetPixel(0, 0);
                }
            }
            catch (Exception)
            {
                return Color.Black;
            }
        }
    }

    /// <summary>The loupe that follows the crosshair: zoomed pixels, swatch, hex and RGB.</summary>
    class ColorPickerHud : Form
    {
        const int Cells = 11;          // odd, so there is a centre pixel
        const int Zoom = 10;           // screen pixels → loupe pixels
        const int Pad = 8;
        const int Loupe = Cells * Zoom;
        const string Hint = "Click to copy.  Esc cancels.";

        readonly Bitmap _grab = new Bitmap(Cells, Cells, PixelFormat.Format32bppArgb);
        Color _center = Color.Black;

        public ColorPickerHud()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            BackColor = Theme.Surface;
            Font = Theme.Base;
            DoubleBuffered = true;
            int textH = Theme.Strong.Height + Theme.Small.Height * 2 + 10;
            int hintW = TextRenderer.MeasureText(Hint, Theme.Small).Width;
            Size = new Size(Math.Max(Loupe, hintW) + Pad * 2, Pad + Loupe + 6 + textH + Pad);
        }

        protected override bool ShowWithoutActivation { get { return true; } }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x08000000 | 0x80 | 0x20 | 0x8;   // no-activate, toolwindow, hit-transparent, topmost
                return cp;
            }
        }

        public void Sample(Point cursor)
        {
            try
            {
                using (var g = Graphics.FromImage(_grab))
                {
                    g.Clear(Color.Black);
                    g.CopyFromScreen(cursor.X - Cells / 2, cursor.Y - Cells / 2, 0, 0, new Size(Cells, Cells));
                }
                _center = _grab.GetPixel(Cells / 2, Cells / 2);
            }
            catch (Exception) { }

            // sit below-right of the crosshair, flipping when near a screen edge
            var screen = Screen.FromPoint(cursor).Bounds;
            int x = cursor.X + 20, y = cursor.Y + 24;
            if (x + Width > screen.Right - 4) x = cursor.X - Width - 20;
            if (y + Height > screen.Bottom - 4) y = cursor.Y - Height - 24;
            if (Location.X != x || Location.Y != y) Location = new Point(x, y);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            using (var fill = new SolidBrush(Theme.Surface))
                g.FillRectangle(fill, ClientRectangle);
            using (var border = new Pen(Theme.Accent))
                g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);

            // magnified pixels, crisp
            var loupe = new Rectangle((Width - Loupe) / 2, Pad, Loupe, Loupe);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(_grab, loupe);
            g.PixelOffsetMode = PixelOffsetMode.Default;

            // the centre cell, outlined in a colour that shows up on it
            var cell = new Rectangle(loupe.X + (Cells / 2) * Zoom, loupe.Y + (Cells / 2) * Zoom, Zoom, Zoom);
            bool light = (_center.R * 299 + _center.G * 587 + _center.B * 114) / 1000 > 140;
            using (var pen = new Pen(light ? Color.Black : Color.White))
                g.DrawRectangle(pen, cell.X - 1, cell.Y - 1, cell.Width + 1, cell.Height + 1);
            using (var pen = new Pen(Theme.Border))
                g.DrawRectangle(pen, loupe.X - 1, loupe.Y - 1, loupe.Width + 1, loupe.Height + 1);

            // swatch + hex
            int y = loupe.Bottom + 6;
            int sw = Theme.Strong.Height;
            using (var b = new SolidBrush(_center))
                g.FillRectangle(b, Pad, y, sw, sw);
            using (var pen = new Pen(Theme.Border))
                g.DrawRectangle(pen, Pad, y, sw - 1, sw - 1);
            TextRenderer.DrawText(g, ColorPicker.Hex(_center), Theme.Strong,
                new Point(Pad + sw + 6, y), Theme.Text, TextFormatFlags.NoPrefix);
            y += sw + 2;
            TextRenderer.DrawText(g, string.Format("RGB {0}, {1}, {2}", _center.R, _center.G, _center.B), Theme.Small,
                new Point(Pad, y), Theme.TextDim, TextFormatFlags.NoPrefix);
            y += Theme.Small.Height + 2;
            TextRenderer.DrawText(g, Hint, Theme.Small, new Rectangle(Pad, y, Width - Pad * 2, Theme.Small.Height + 4),
                Theme.TextDim, TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _grab.Dispose();
            base.Dispose(disposing);
        }
    }
    /// <summary>Tray → Color Setting: the colour picker's hot key.</summary>
    class ColorPickSettingsForm : PixelPerfectForm
    {
        static readonly string[] ModNames = { "Alt", "Ctrl", "Shift", "Win" };
        static readonly int[] ModBits = { 1, 2, 4, 8 };

        readonly ModernCheckBox[] _mods = new ModernCheckBox[4];
        readonly TextBox _key;

        public ColorPickSettingsForm()
        {
            Theme.Init(ThemeHelper.IsDarkMode);

            Text = "MicroApp - Color Picker";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.Bg;
            Font = Theme.Base;

            SuspendLayout();

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
            Controls.Add(iconBox);
            Controls.Add(new Label
            {
                Location = new Point(84, 24),
                AutoSize = true,
                Text = "Color Picker",
                Font = new Font("Segoe UI Semibold", 12.5F),
                ForeColor = Theme.Text,
                BackColor = Color.Transparent
            });
            Controls.Add(new Label
            {
                Location = new Point(86, 52),
                Size = new Size(430, 32),
                Text = "Press the hot key (or tray \u2192 Pick Color), then click any pixel on screen: " +
                       "its #RRGGBB code is copied to the clipboard.",
                Font = Theme.Small,
                ForeColor = Theme.TextDim,
                BackColor = Color.Transparent
            });

            // ----- hot key --------------------------------------------------------------
            var keyCard = new Card
            {
                Location = new Point(24, 96),
                Size = new Size(500, 110),
                Title = "Hot key",
                Description = "Tick the modifiers and give it a key. Clear the key box to turn it off."
            };
            Controls.Add(keyCard);
            int mods = Properties.Settings.Default.ColorPickHotKeyModifier;
            for (int m = 0; m < 4; m++)
            {
                _mods[m] = new ModernCheckBox
                {
                    Location = new Point(20 + m * 78, 66),
                    AutoSize = false,
                    Size = new Size(74, 24),
                    Text = ModNames[m],
                    Checked = (mods & ModBits[m]) != 0
                };
                keyCard.Controls.Add(_mods[m]);
            }
            keyCard.Controls.Add(new Label { Location = new Point(338, 68), AutoSize = true, Text = "Key", Font = Theme.Base, ForeColor = Theme.TextDim, BackColor = Color.Transparent });
            // press a key to set it, Delete clears it
            var keyHost = new FieldHost { Location = new Point(374, 60), Size = new Size(96, 32) };
            _key = new TextBox
            {
                Location = new Point(10, 8),
                Size = new Size(76, 16),
                MaxLength = 12,
                Text = Properties.Settings.Default.ColorPickHotKey,
                TextAlign = HorizontalAlignment.Center,
                BackColor = Theme.FieldBg,
                ForeColor = Theme.Text
            };
            _key.KeyDown += Key_KeyDown;
            keyHost.Controls.Add(_key);
            keyCard.Controls.Add(keyHost);

            // ----- buttons ------------------------------------------------------------------
            var save = new ModernButton
            {
                Text = "Save",
                Accent = true,
                Size = new Size(112, 36),
                Location = new Point(500 + 24 - 112, keyCard.Bottom + 14)
            };
            save.Click += Save_Click;
            var cancel = new ModernButton
            {
                Text = "Cancel",
                Size = new Size(96, 36),
                Location = new Point(save.Left - 96 - 8, keyCard.Bottom + 14),
                DialogResult = DialogResult.Cancel
            };
            Controls.Add(save);
            Controls.Add(cancel);
            AcceptButton = save;
            CancelButton = cancel;

            ClientSize = new Size(500 + 48, save.Bottom + 20);
            ResumeLayout();

            Theme.Apply(this);
            Native.SetDarkModeForWindow(Handle, ThemeHelper.IsDarkMode);
            Theme.RoundWindowCorners(Handle);
            Icon = Properties.Resources.AppIcon;
        }

        /// <summary>Shows the pressed key in the box, ignoring bare modifiers.</summary>
        void Key_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Menu: case Keys.LMenu: case Keys.RMenu:
                case Keys.ShiftKey: case Keys.LShiftKey: case Keys.RShiftKey:
                case Keys.ControlKey: case Keys.LControlKey: case Keys.RControlKey:
                case Keys.LWin: case Keys.RWin: case Keys.Return: case Keys.Tab: case Keys.Escape:
                    return;
                case Keys.Delete:
                case Keys.Back:
                    _key.Text = string.Empty;
                    break;
                default:
                    _key.Text = e.KeyCode.ToString();
                    break;
            }
            e.SuppressKeyPress = true;
        }

        void Save_Click(object sender, EventArgs e)
        {
            string letter = _key.Text.Trim();
            if (letter.Length == 1) letter = letter.ToUpperInvariant();
            if (letter.Length > 0)
            {
                Keys parsed;
                if (!Enum.TryParse(letter, true, out parsed))
                {
                    ModernDialog.Info("That is not a key", "Use a letter, a digit or a key name such as F8.");
                    return;
                }
            }
            int mods = 0;
            for (int m = 0; m < 4; m++) if (_mods[m].Checked) mods |= ModBits[m];

            Properties.Settings.Default.ColorPickHotKey = letter;
            Properties.Settings.Default.ColorPickHotKeyModifier = mods;
            Properties.Settings.Default.Save();
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    /// <summary>
    /// Helper for colour space conversions and history management.
    /// </summary>
    public static class ColorUtils
    {
        public static string ToHex(Color c)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
        }

        public static string ToRgb(Color c)
        {
            return string.Format("rgb({0}, {1}, {2})", c.R, c.G, c.B);
        }

        public static string ToHsl(Color c)
        {
            float r = c.R / 255f;
            float g = c.G / 255f;
            float b = c.B / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            float h = 0f;
            float s = 0f;
            float l = (max + min) / 2f;

            if (delta > 0.00001f)
            {
                s = l > 0.5f ? delta / (2f - max - min) : delta / (max + min);

                if (Math.Abs(max - r) < 0.00001f)
                {
                    h = (g - b) / delta + (g < b ? 6f : 0f);
                }
                else if (Math.Abs(max - g) < 0.00001f)
                {
                    h = (b - r) / delta + 2f;
                }
                else
                {
                    h = (r - g) / delta + 4f;
                }
                h *= 60f;
            }

            return string.Format("hsl({0}, {1}%, {2}%)", (int)Math.Round(h), (int)Math.Round(s * 100f), (int)Math.Round(l * 100f));
        }

        public static List<string> GetHistory()
        {
            var raw = Properties.Settings.Default.ColorHistory ?? "";
            var list = new List<string>();
            foreach (var part in raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var hex = part.Trim();
                if (hex.Length == 7 && hex.StartsWith("#") && !list.Contains(hex))
                    list.Add(hex);
            }
            return list;
        }

        public static void AddToHistory(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return;
            hex = hex.Trim().ToUpperInvariant();
            var list = GetHistory();
            list.Remove(hex);
            list.Insert(0, hex);
            while (list.Count > 10) list.RemoveAt(list.Count - 1);
            Properties.Settings.Default.ColorHistory = string.Join(";", list);
            Properties.Settings.Default.Save();
        }

        public static Color FromHex(string hex)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hex)) return Color.Black;
                hex = hex.Trim().TrimStart('#');
                if (hex.Length == 6)
                {
                    int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                    return Color.FromArgb(r, g, b);
                }
            }
            catch { }
            return Color.Black;
        }
    }

    /// <summary>
    /// Color inspector dialog displaying swatch, HEX, RGB, HSL values, and recent color history.
    /// </summary>
    public class ColorInspectorDialog : PixelPerfectForm
    {
        private Color _color;
        private readonly Panel _swatchPanel;
        private readonly TextBox _hexBox;
        private readonly TextBox _rgbBox;
        private readonly TextBox _hslBox;
        private readonly FlowLayoutPanel _historyFlow;

        public ColorInspectorDialog(Color initialColor)
        {
            _color = initialColor;
            Text = "Color Inspector";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Font = Theme.Base;
            BackColor = Theme.Surface;
            ForeColor = Theme.Text;

            SuspendLayout();

            const int CardW = 420;

            // Header
            var iconBox = new PictureBox
            {
                Location = new Point(24, 20),
                Size = new Size(32, 32),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Image = Properties.Resources.AppIcon.ToBitmap()
            };
            Controls.Add(iconBox);

            var titleLbl = new Label
            {
                Location = new Point(68, 16),
                AutoSize = true,
                Text = "Color Inspector",
                Font = Theme.Heading,
                ForeColor = Theme.Text,
                BackColor = Color.Transparent
            };
            Controls.Add(titleLbl);

            var subLbl = new Label
            {
                Location = new Point(68, 44),
                AutoSize = true,
                Text = "Inspect picked color formats and recent history.",
                Font = Theme.Small,
                ForeColor = Theme.TextDim,
                BackColor = Color.Transparent
            };
            Controls.Add(subLbl);

            var card = new Card
            {
                Location = new Point(24, 80),
                Size = new Size(CardW, 360),
                Title = "Color Details",
                Description = "Click copy to copy formatted string to clipboard."
            };
            Controls.Add(card);

            _swatchPanel = new Panel
            {
                Location = new Point(16, 60),
                Size = new Size(CardW - 32, 50),
                Cursor = Cursors.Hand
            };
            _swatchPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, _swatchPanel.Width - 1, _swatchPanel.Height - 1);
                using (var path = Theme.Round(r, 8))
                {
                    using (var b = new SolidBrush(_color))
                        g.FillPath(b, path);
                    using (var pen = new Pen(Theme.Border, 1.5f))
                        g.DrawPath(pen, path);
                }
            };
            card.Controls.Add(_swatchPanel);

            // Format rows
            int rowY = 122;
            _hexBox = AddFormatRow(card, rowY, "HEX", ColorUtils.ToHex(_color));
            _rgbBox = AddFormatRow(card, rowY + 38, "RGB", ColorUtils.ToRgb(_color));
            _hslBox = AddFormatRow(card, rowY + 76, "HSL", ColorUtils.ToHsl(_color));

            // Recent history palette
            var histLabel = new Label
            {
                Location = new Point(16, 246),
                AutoSize = true,
                Text = "Recent colors",
                Font = Theme.Small,
                ForeColor = Theme.TextDim,
                BackColor = Color.Transparent
            };
            card.Controls.Add(histLabel);

            _historyFlow = new FlowLayoutPanel
            {
                Location = new Point(16, 268),
                Size = new Size(CardW - 32, 78),
                BackColor = Color.Transparent,
                AutoScroll = true
            };
            card.Controls.Add(_historyFlow);
            PopulateHistory();

            var close = new ModernButton
            {
                Text = "Close",
                Accent = true,
                Size = new Size(96, 34),
                Location = new Point(24 + CardW - 96, card.Bottom + 12),
                DialogResult = DialogResult.OK
            };
            close.Click += (s, e) => Close();
            Controls.Add(close);
            AcceptButton = close;
            CancelButton = close;

            ClientSize = new Size(CardW + 48, close.Bottom + 16);
            ActiveControl = close;
            ResumeLayout();

            Theme.Apply(this);
            Native.SetDarkModeForWindow(Handle, ThemeHelper.IsDarkMode);
            Theme.RoundWindowCorners(Handle);
        }

        private TextBox AddFormatRow(Card card, int y, string label, string value)
        {
            var lbl = new Label
            {
                Location = new Point(16, y + 5),
                Size = new Size(42, 20),
                Text = label,
                Font = Theme.Strong,
                ForeColor = Theme.Text,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lbl);

            var field = new FieldHost
            {
                Location = new Point(64, y),
                Size = new Size(244, 30)
            };
            var txt = new TextBox
            {
                Text = value,
                ForeColor = Theme.Text,
                BackColor = Theme.FieldBg,
                BorderStyle = BorderStyle.None,
                ReadOnly = true
            };
            field.Controls.Add(txt);
            card.Controls.Add(field);

            var copyBtn = new ModernButton
            {
                Text = "Copy",
                Size = new Size(64, 30),
                Location = new Point(316, y)
            };
            copyBtn.Click += (s, e) =>
            {
                try
                {
                    Clipboard.SetText(txt.Text);
                    Toast.Show("Copied " + txt.Text + " to clipboard");
                }
                catch { }
            };
            card.Controls.Add(copyBtn);

            return txt;
        }

        private void SetColor(Color c)
        {
            _color = c;
            _swatchPanel.Invalidate();
            _hexBox.Text = ColorUtils.ToHex(c);
            _rgbBox.Text = ColorUtils.ToRgb(c);
            _hslBox.Text = ColorUtils.ToHsl(c);
        }

        private void PopulateHistory()
        {
            _historyFlow.Controls.Clear();
            var history = ColorUtils.GetHistory();
            foreach (var hex in history)
            {
                var col = ColorUtils.FromHex(hex);
                var chip = new Panel
                {
                    Size = new Size(28, 28),
                    Margin = new Padding(3),
                    Cursor = Cursors.Hand
                };
                var tip = new ToolTip();
                tip.SetToolTip(chip, hex + " (click to inspect)");
                var chipColor = col;
                chip.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    var r = new Rectangle(0, 0, chip.Width - 1, chip.Height - 1);
                    using (var path = Theme.Round(r, 6))
                    {
                        using (var b = new SolidBrush(chipColor))
                            g.FillPath(b, path);
                        using (var p = new Pen(Theme.Border, 1.2f))
                            g.DrawPath(p, path);
                    }
                };
                chip.Click += (s, e) => SetColor(col);
                _historyFlow.Controls.Add(chip);
            }
        }
    }
}
