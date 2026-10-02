using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MicroApp
{
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
    /// Magnifier loupe form that follows the cursor and shows a 9x9 zoomed pixel grid with center reticle.
    /// </summary>
    public class ColorLoupeForm : Form
    {
        private readonly Bitmap _screen;
        private readonly Rectangle _screenBounds;
        private Color _currentColor = Color.Black;
        private string _currentHex = "#000000";
        private Point _cursorPos;

        const int GridCount = 11;      // 11x11 pixel window
        const int PixelSize = 9;       // 9px per screen pixel
        const int LoupeSize = GridCount * PixelSize; // 99px
        const int MarginTotal = 16;
        const int BadgeHeight = 32;

        public ColorLoupeForm(Bitmap screenSnapshot, Rectangle virtualBounds)
        {
            _screen = screenSnapshot;
            _screenBounds = virtualBounds;

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            BackColor = Theme.Surface;
            DoubleBuffered = true;
            Size = new Size(LoupeSize + MarginTotal * 2, LoupeSize + BadgeHeight + MarginTotal * 2);

            try
            {
                using (var path = Theme.Round(ClientRectangle, 12))
                {
                    Region = new Region(path);
                }
            }
            catch { }
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                // WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT | WS_EX_TOPMOST
                cp.ExStyle |= 0x08000000 | 0x80 | 0x20 | 0x8;
                return cp;
            }
        }

        public Color CurrentColor => _currentColor;
        public string CurrentHex => _currentHex;

        public void UpdateCursor(Point p)
        {
            _cursorPos = p;

            // Sample current pixel
            int relX = p.X - _screenBounds.X;
            int relY = p.Y - _screenBounds.Y;

            if (_screen != null && relX >= 0 && relX < _screen.Width && relY >= 0 && relY < _screen.Height)
            {
                _currentColor = _screen.GetPixel(relX, relY);
                _currentHex = ColorUtils.ToHex(_currentColor);
            }

            // Position loupe offset from cursor, flipping near screen boundaries
            var currentScreen = Screen.FromPoint(p).Bounds;
            int x = p.X + 20;
            int y = p.Y + 20;

            if (x + Width > currentScreen.Right - 4) x = p.X - Width - 20;
            if (y + Height > currentScreen.Bottom - 4) y = p.Y - Height - 20;

            Location = new Point(x, y);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Background card
            using (var fill = new SolidBrush(Theme.Dark ? Color.FromArgb(30, 30, 34) : Color.FromArgb(250, 250, 252)))
                g.FillRectangle(fill, ClientRectangle);

            using (var border = new Pen(Theme.Border, 1.4f))
                g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);

            int loupeX = (Width - LoupeSize) / 2;
            int loupeY = MarginTotal;

            // Magnified pixel grid
            int relCursorX = _cursorPos.X - _screenBounds.X;
            int relCursorY = _cursorPos.Y - _screenBounds.Y;
            int half = GridCount / 2;

            for (int py = 0; py < GridCount; py++)
            {
                for (int px = 0; px < GridCount; px++)
                {
                    int srcX = relCursorX - half + px;
                    int srcY = relCursorY - half + py;

                    Color pxColor = Color.Black;
                    if (_screen != null && srcX >= 0 && srcX < _screen.Width && srcY >= 0 && srcY < _screen.Height)
                        pxColor = _screen.GetPixel(srcX, srcY);

                    int destX = loupeX + px * PixelSize;
                    int destY = loupeY + py * PixelSize;

                    using (var b = new SolidBrush(pxColor))
                        g.FillRectangle(b, destX, destY, PixelSize, PixelSize);

                    // Light grid separator
                    using (var gridPen = new Pen(Color.FromArgb(30, 128, 128, 128)))
                        g.DrawRectangle(gridPen, destX, destY, PixelSize, PixelSize);
                }
            }

            // Grid border
            using (var gridBorder = new Pen(Theme.Border, 1.5f))
                g.DrawRectangle(gridBorder, loupeX, loupeY, LoupeSize, LoupeSize);

            // Center target pixel box
            int centerX = loupeX + half * PixelSize;
            int centerY = loupeY + half * PixelSize;
            using (var centerPen = new Pen(Theme.Text, 1.8f))
                g.DrawRectangle(centerPen, centerX - 1, centerY - 1, PixelSize + 1, PixelSize + 1);

            // Live color badge underneath
            int badgeY = loupeY + LoupeSize + 8;
            int chipSize = 16;
            int chipX = loupeX + 2;
            int chipY = badgeY + (20 - chipSize) / 2;

            var chipRect = new Rectangle(chipX, chipY, chipSize, chipSize);
            using (var chipBrush = new SolidBrush(_currentColor))
                g.FillRectangle(chipBrush, chipRect);
            using (var chipBorder = new Pen(Theme.Border, 1f))
                g.DrawRectangle(chipBorder, chipRect);

            var hexRect = new Rectangle(chipX + chipSize + 8, badgeY, LoupeSize - chipSize - 8, 20);
            TextRenderer.DrawText(g, _currentHex, Theme.Strong, hexRect, Theme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _screen?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Detailed color inspector dialog showing HEX, RGB, HSL and recent color palette.
    /// </summary>
    public class ColorInspectorDialog : PixelPerfectForm
    {
        private Color _color;
        private Panel _swatchPanel;
        private TextBox _hexBox, _rgbBox, _hslBox;
        private FlowLayoutPanel _historyFlow;

        public ColorInspectorDialog(Color initialColor)
        {
            _color = initialColor;
            Theme.Init(ThemeHelper.IsDarkMode);

            Text = "MicroApp - Color Details";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.Bg;
            Font = Theme.Base;
            Icon = Properties.Resources.AppIcon;

            SuspendLayout();

            const int CardW = 420;

            var iconBox = new PictureBox
            {
                Location = new Point(24, 20),
                Size = new Size(36, 36),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                TabStop = false
            };
            using (var large = new Icon(Properties.Resources.AppIcon, 36, 36))
                iconBox.Image = large.ToBitmap();

            var heading = new Label
            {
                Location = new Point(72, 18),
                AutoSize = true,
                Text = "Color Details",
                Font = new Font("Segoe UI Semibold", 12F),
                ForeColor = Theme.Text,
                BackColor = Color.Transparent
            };
            var subtitle = new Label
            {
                Location = new Point(74, 42),
                Size = new Size(360, 20),
                Text = "Picked color codes and recent history palette.",
                Font = Theme.Small,
                ForeColor = Theme.TextDim,
                BackColor = Color.Transparent
            };
            Controls.Add(iconBox);
            Controls.Add(heading);
            Controls.Add(subtitle);

            var card = new Card
            {
                Location = new Point(24, 72),
                Size = new Size(CardW, 366),
                Title = "Color code",
                Description = "Click copy to copy formatted code to clipboard."
            };
            Controls.Add(card);

            // Large color preview swatch
            _swatchPanel = new Panel
            {
                Location = new Point(16, 56),
                Size = new Size(CardW - 32, 54)
            };
            _swatchPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
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
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
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
