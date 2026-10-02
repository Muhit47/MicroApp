using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MicroApp
{
    /// <summary>
    /// Keycap-styled chip for rendering keyboard shortcuts in reference lists.
    /// </summary>
    class KeyBadge : Label
    {
        public KeyBadge()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            AutoSize = false;
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            Theme.PaintBackdrop(this, g);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = Theme.Round(r, 6))
            using (var fill = new SolidBrush(Theme.FieldBg))
            using (var pen = new Pen(Theme.FieldBorder, 1.2f))
            {
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
            }
            TextRenderer.DrawText(g, Text, Theme.Strong, r, Theme.Text,
                                  TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
    }

    public enum AppTarget
    {
        Windows,
        Word,
        Excel,
        PowerPoint
    }

    public class ShortcutEntry
    {
        public AppTarget App { get; }
        public string Section { get; }
        public string Keys { get; }
        public string Description { get; }

        public ShortcutEntry(AppTarget app, string section, string keys, string description)
        {
            App = app;
            Section = section;
            Keys = keys;
            Description = description;
        }
    }

    /// <summary>
    /// App switcher tab button for selecting between Windows, Word, Excel, PowerPoint.
    /// </summary>
    class AppTabButton : Control
    {
        private bool _active;
        public bool Active
        {
            get => _active;
            set { if (_active != value) { _active = value; Invalidate(); } }
        }

        public AppTarget Target { get; }

        public AppTabButton(string text, AppTarget target)
        {
            Text = text;
            Target = target;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            Cursor = Cursors.Hand;
            Font = Theme.Base;
            Height = 32;
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = Theme.Round(r, 8))
            {
                if (_active)
                {
                    using (var b = new SolidBrush(Theme.Accent))
                        g.FillPath(b, path);
                    TextRenderer.DrawText(g, Text, Theme.Strong, r, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }
                else
                {
                    using (var b = new SolidBrush(Theme.FieldBg))
                        g.FillPath(b, path);
                    using (var p = new Pen(Theme.Border, 1f))
                        g.DrawPath(p, path);
                    TextRenderer.DrawText(g, Text, Theme.Small, r, Theme.TextDim,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }
            }
        }
    }

    /// <summary>
    /// Section divider banner inside the shortcuts list.
    /// </summary>
    class SectionBanner : Control
    {
        public SectionBanner(string title)
        {
            Text = title;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            Height = 28;
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 3, Width - 1, Height - 6);
            using (var path = Theme.Round(r, 6))
            using (var b = new SolidBrush(Color.FromArgb(32, Theme.Accent)))
            {
                g.FillPath(b, path);
            }
            var textRect = new Rectangle(12, 0, Width - 24, Height);
            TextRenderer.DrawText(g, Text, Theme.Strong, textRect, Theme.Accent,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
    }

    class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }
    }

    /// <summary>
    /// Comprehensive Productivity Shortcuts Cheat Sheet Hub covering Windows, Word, Excel, and PowerPoint.
    /// </summary>
    class SystemShortcutsForm : PixelPerfectForm
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        private static readonly List<ShortcutEntry> Entries = new List<ShortcutEntry>
        {
            // =========================================================================
            // WINDOWS SHORTCUTS
            // =========================================================================
            // --- 1. Navigation ---
            new ShortcutEntry(AppTarget.Windows, "1. Navigation & Windows", "Win", "Toggle Start menu"),
            new ShortcutEntry(AppTarget.Windows, "1. Navigation & Windows", "Win + D", "Toggle Desktop (minimize/restore all)"),
            new ShortcutEntry(AppTarget.Windows, "1. Navigation & Windows", "Win + Alt + D", "Toggle date and time calendar menu"),
            new ShortcutEntry(AppTarget.Windows, "1. Navigation & Windows", "Win + E", "Open File Explorer"),
            new ShortcutEntry(AppTarget.Windows, "1. Navigation & Windows", "Win + A", "Toggle Action Center / Quick Settings"),
            new ShortcutEntry(AppTarget.Windows, "1. Navigation & Windows", "Win + I", "Open Windows Settings"),
            new ShortcutEntry(AppTarget.Windows, "1. Navigation & Windows", "Win + X", "Open Quick Link / Power User menu"),
            new ShortcutEntry(AppTarget.Windows, "1. Navigation & Windows", "Win + Tab", "Toggle Task View (virtual desktops)"),
            new ShortcutEntry(AppTarget.Windows, "1. Navigation & Windows", "Win + ,", "Peek at desktop while keys are held"),
            new ShortcutEntry(AppTarget.Windows, "1. Navigation & Windows", "Win + Pause", "Open System Properties"),
            new ShortcutEntry(AppTarget.Windows, "1. Navigation & Windows", "Win + S", "Open Windows Search"),

            // --- 2. Window Snapping & Management ---
            new ShortcutEntry(AppTarget.Windows, "2. Window Snapping & Management", "Win + \u2191", "Maximize active window"),
            new ShortcutEntry(AppTarget.Windows, "2. Window Snapping & Management", "Win + \u2193", "Restore or minimize active window"),
            new ShortcutEntry(AppTarget.Windows, "2. Window Snapping & Management", "Win + \u2190", "Snap active window to left half of screen"),
            new ShortcutEntry(AppTarget.Windows, "2. Window Snapping & Management", "Win + \u2192", "Snap active window to right half of screen"),
            new ShortcutEntry(AppTarget.Windows, "2. Window Snapping & Management", "Win + Home", "Minimize all windows except active one"),
            new ShortcutEntry(AppTarget.Windows, "2. Window Snapping & Management", "Win + M", "Minimize all windows"),
            new ShortcutEntry(AppTarget.Windows, "2. Window Snapping & Management", "Win + Shift + M", "Restore minimized windows on desktop"),
            new ShortcutEntry(AppTarget.Windows, "2. Window Snapping & Management", "Win + Shift + \u2191", "Stretch window to top and bottom of screen"),
            new ShortcutEntry(AppTarget.Windows, "2. Window Snapping & Management", "Win + Shift + \u2193", "Restore / minimize window vertically"),
            new ShortcutEntry(AppTarget.Windows, "2. Window Snapping & Management", "Win + Shift + \u2190 or \u2192", "Move active window to adjacent monitor"),
            new ShortcutEntry(AppTarget.Windows, "2. Window Snapping & Management", "Win + Z", "Show commands / snap layout menu"),

            // --- 3. System & Tools ---
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + Shift + S", "Toggle Snip & Sketch (Snipping Tool)"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + V", "Open clipboard history"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + Shift + V", "Cycle through notifications"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + . or ;", "Open emoji, symbols & Kaomoji panel"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + R", "Open Run dialog box"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + L", "Lock PC immediately"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + G", "Toggle Xbox Game Bar"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + H", "Toggle dictation / voice typing"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + K", "Toggle Connect / Cast menu"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + P", "Toggle Project / Multi-display mode menu"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + U", "Open Accessibility / Ease of Access center"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + Ctrl + Q", "Open Quick Assist for remote help"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + Ctrl + Shift + B", "Restart graphics driver (wake blank screen)"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + + / -", "Open & zoom in / zoom out with Magnifier"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + Esc", "Exit Magnifier"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + Ctrl + Enter", "Toggle Narrator"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + Shift + C", "Open Color Picker (PowerToys) / Charms"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + C", "Open Cortana / Copilot in listening mode"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + F", "Open Feedback Hub & take screenshot"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + Space", "Switch input language & keyboard layout"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + Ctrl + Space", "Change to previously selected input"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + /", "Begin IME reconversion (foreign alphabets)"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + Ctrl + F", "Search for PCs on a network"),
            new ShortcutEntry(AppTarget.Windows, "3. System & Tools", "Win + Y", "Toggle Windows Mixed Reality and desktop"),

            // --- 4. Taskbar & Apps ---
            new ShortcutEntry(AppTarget.Windows, "4. Taskbar & Apps", "Win + [0-9]", "Open or switch to app pinned at taskbar slot"),
            new ShortcutEntry(AppTarget.Windows, "4. Taskbar & Apps", "Win + Shift + [0-9]", "Open new instance of pinned taskbar app"),
            new ShortcutEntry(AppTarget.Windows, "4. Taskbar & Apps", "Win + Ctrl + [0-9]", "Switch to last active window of pinned app"),
            new ShortcutEntry(AppTarget.Windows, "4. Taskbar & Apps", "Win + Alt + [0-9]", "Open Jump List for pinned taskbar app"),
            new ShortcutEntry(AppTarget.Windows, "4. Taskbar & Apps", "Win + Ctrl + Shift + [0-9]", "Open new instance of pinned app as Admin"),
            new ShortcutEntry(AppTarget.Windows, "4. Taskbar & Apps", "Win + T", "Cycle through apps on the taskbar"),
            new ShortcutEntry(AppTarget.Windows, "4. Taskbar & Apps", "Win + B", "Set focus to system tray notification area"),
            new ShortcutEntry(AppTarget.Windows, "4. Taskbar & Apps", "Win + O", "Toggle device orientation lock"),
            new ShortcutEntry(AppTarget.Windows, "4. Taskbar & Apps", "Win + J", "Set focus to Windows tip (if available)"),

            // =========================================================================
            // MICROSOFT WORD SHORTCUTS
            // =========================================================================
            // --- 1. Basic & File Operations ---
            new ShortcutEntry(AppTarget.Word, "1. Basic & File Operations", "Ctrl + N", "New Document"),
            new ShortcutEntry(AppTarget.Word, "1. Basic & File Operations", "Ctrl + O", "Open File"),
            new ShortcutEntry(AppTarget.Word, "1. Basic & File Operations", "Ctrl + S", "Save File"),
            new ShortcutEntry(AppTarget.Word, "1. Basic & File Operations", "Ctrl + P", "Print Window"),
            new ShortcutEntry(AppTarget.Word, "1. Basic & File Operations", "Ctrl + Z", "Undo Action"),
            new ShortcutEntry(AppTarget.Word, "1. Basic & File Operations", "Ctrl + Y", "Redo Action"),
            new ShortcutEntry(AppTarget.Word, "1. Basic & File Operations", "F12", "Save As"),
            new ShortcutEntry(AppTarget.Word, "1. Basic & File Operations", "Ctrl + W", "Close Document"),

            // --- 2. Text Formatting ---
            new ShortcutEntry(AppTarget.Word, "2. Text Formatting", "Ctrl + B", "Bold Text"),
            new ShortcutEntry(AppTarget.Word, "2. Text Formatting", "Ctrl + I", "Italic Text"),
            new ShortcutEntry(AppTarget.Word, "2. Text Formatting", "Ctrl + U", "Underline Text"),
            new ShortcutEntry(AppTarget.Word, "2. Text Formatting", "Ctrl + [", "Decrease Font Size"),
            new ShortcutEntry(AppTarget.Word, "2. Text Formatting", "Ctrl + ]", "Increase Font Size"),
            new ShortcutEntry(AppTarget.Word, "2. Text Formatting", "Ctrl + Shift + D", "Double Underline"),
            new ShortcutEntry(AppTarget.Word, "2. Text Formatting", "Ctrl + Shift + K", "Small Caps"),
            new ShortcutEntry(AppTarget.Word, "2. Text Formatting", "Ctrl + D", "Font Options"),

            // --- 3. Alignment & Paragraphs ---
            new ShortcutEntry(AppTarget.Word, "3. Alignment & Paragraphs", "Ctrl + L", "Left Align"),
            new ShortcutEntry(AppTarget.Word, "3. Alignment & Paragraphs", "Ctrl + E", "Center Align"),
            new ShortcutEntry(AppTarget.Word, "3. Alignment & Paragraphs", "Ctrl + R", "Right Align"),
            new ShortcutEntry(AppTarget.Word, "3. Alignment & Paragraphs", "Ctrl + J", "Justify Align"),
            new ShortcutEntry(AppTarget.Word, "3. Alignment & Paragraphs", "Ctrl + 1", "Single Line Space"),
            new ShortcutEntry(AppTarget.Word, "3. Alignment & Paragraphs", "Ctrl + 2", "Double Line Space"),
            new ShortcutEntry(AppTarget.Word, "3. Alignment & Paragraphs", "Ctrl + 5", "1.5 Line Space"),
            new ShortcutEntry(AppTarget.Word, "3. Alignment & Paragraphs", "Ctrl + M", "Indent Paragraph"),

            // --- 4. Navigation & Edits ---
            new ShortcutEntry(AppTarget.Word, "4. Navigation & Edits", "Ctrl + F", "Find (Words)"),
            new ShortcutEntry(AppTarget.Word, "4. Navigation & Edits", "Ctrl + H", "Replace (Words)"),
            new ShortcutEntry(AppTarget.Word, "4. Navigation & Edits", "Ctrl + A", "Select All"),
            new ShortcutEntry(AppTarget.Word, "4. Navigation & Edits", "Ctrl + C / X / V", "Copy / Cut / Paste"),
            new ShortcutEntry(AppTarget.Word, "4. Navigation & Edits", "Ctrl + K", "Insert Hyperlink"),
            new ShortcutEntry(AppTarget.Word, "4. Navigation & Edits", "F7", "Spell & Grammar Check"),
            new ShortcutEntry(AppTarget.Word, "4. Navigation & Edits", "Ctrl + Enter", "Page Break"),
            new ShortcutEntry(AppTarget.Word, "4. Navigation & Edits", "Alt + Shift + D", "Insert Date"),

            // =========================================================================
            // MICROSOFT EXCEL SHORTCUTS
            // =========================================================================
            // --- 1. Basic & File Operations ---
            new ShortcutEntry(AppTarget.Excel, "1. Basic & File Operations", "Ctrl + N", "New Workbook"),
            new ShortcutEntry(AppTarget.Excel, "1. Basic & File Operations", "Ctrl + O", "Open File"),
            new ShortcutEntry(AppTarget.Excel, "1. Basic & File Operations", "Ctrl + S", "Save File"),
            new ShortcutEntry(AppTarget.Excel, "1. Basic & File Operations", "Ctrl + P", "Print Window"),
            new ShortcutEntry(AppTarget.Excel, "1. Basic & File Operations", "Ctrl + Z", "Undo Action"),
            new ShortcutEntry(AppTarget.Excel, "1. Basic & File Operations", "Ctrl + Y", "Redo Action"),
            new ShortcutEntry(AppTarget.Excel, "1. Basic & File Operations", "F12", "Save As"),
            new ShortcutEntry(AppTarget.Excel, "1. Basic & File Operations", "Ctrl + W", "Close Workbook"),

            // --- 2. Navigation & Selection ---
            new ShortcutEntry(AppTarget.Excel, "2. Navigation & Selection", "Ctrl + Arrow", "Jump to Edge of Data"),
            new ShortcutEntry(AppTarget.Excel, "2. Navigation & Selection", "Ctrl + Shift + Arrow", "Select to Edge of Data"),
            new ShortcutEntry(AppTarget.Excel, "2. Navigation & Selection", "Ctrl + A", "Select All / Table"),
            new ShortcutEntry(AppTarget.Excel, "2. Navigation & Selection", "Ctrl + Home", "Go to First Cell (A1)"),
            new ShortcutEntry(AppTarget.Excel, "2. Navigation & Selection", "Ctrl + End", "Go to Last Cell"),
            new ShortcutEntry(AppTarget.Excel, "2. Navigation & Selection", "Shift + Space", "Select Entire Row"),
            new ShortcutEntry(AppTarget.Excel, "2. Navigation & Selection", "Ctrl + Space", "Select Entire Column"),

            // --- 3. Formatting & Data Entry ---
            new ShortcutEntry(AppTarget.Excel, "3. Formatting & Data Entry", "Ctrl + 1", "Format Cells Dialog"),
            new ShortcutEntry(AppTarget.Excel, "3. Formatting & Data Entry", "Ctrl + B / I / U", "Bold / Italic / Underline"),
            new ShortcutEntry(AppTarget.Excel, "3. Formatting & Data Entry", "Ctrl + Shift + $", "Apply Currency Format"),
            new ShortcutEntry(AppTarget.Excel, "3. Formatting & Data Entry", "Ctrl + Shift + %", "Apply Percentage Format"),
            new ShortcutEntry(AppTarget.Excel, "3. Formatting & Data Entry", "Ctrl + Shift + #", "Apply Date Format"),
            new ShortcutEntry(AppTarget.Excel, "3. Formatting & Data Entry", "Ctrl + Alt + V", "Paste Special"),
            new ShortcutEntry(AppTarget.Excel, "3. Formatting & Data Entry", "F2", "Edit Active Cell"),

            // --- 4. Rows, Columns & Formulas ---
            new ShortcutEntry(AppTarget.Excel, "4. Rows, Columns & Formulas", "Alt + =", "AutoSum Formula (=SUM)"),
            new ShortcutEntry(AppTarget.Excel, "4. Rows, Columns & Formulas", "Ctrl + Shift + (+)", "Insert Row / Column"),
            new ShortcutEntry(AppTarget.Excel, "4. Rows, Columns & Formulas", "Ctrl + (-)", "Delete Row / Column"),
            new ShortcutEntry(AppTarget.Excel, "4. Rows, Columns & Formulas", "Ctrl + 9", "Hide Selected Row"),
            new ShortcutEntry(AppTarget.Excel, "4. Rows, Columns & Formulas", "Ctrl + 0", "Hide Selected Column"),
            new ShortcutEntry(AppTarget.Excel, "4. Rows, Columns & Formulas", "Ctrl + Shift + L", "Toggle Filter"),
            new ShortcutEntry(AppTarget.Excel, "4. Rows, Columns & Formulas", "Ctrl + T", "Create Table"),
            new ShortcutEntry(AppTarget.Excel, "4. Rows, Columns & Formulas", "F4", "Toggle Absolute Cell Reference"),

            // =========================================================================
            // MICROSOFT POWERPOINT SHORTCUTS
            // =========================================================================
            // --- 1. Slide Creation & Selection ---
            new ShortcutEntry(AppTarget.PowerPoint, "1. Slide Creation & Selection", "Ctrl + N", "New Presentation"),
            new ShortcutEntry(AppTarget.PowerPoint, "1. Slide Creation & Selection", "Ctrl + M", "Insert New Slide"),
            new ShortcutEntry(AppTarget.PowerPoint, "1. Slide Creation & Selection", "Ctrl + D", "Duplicate Selected Slide"),
            new ShortcutEntry(AppTarget.PowerPoint, "1. Slide Creation & Selection", "Ctrl + Shift + D", "Duplicate Slide / Object"),
            new ShortcutEntry(AppTarget.PowerPoint, "1. Slide Creation & Selection", "Delete", "Delete Selected Slide"),
            new ShortcutEntry(AppTarget.PowerPoint, "1. Slide Creation & Selection", "Ctrl + Up / Down", "Move Slide Up / Down"),
            new ShortcutEntry(AppTarget.PowerPoint, "1. Slide Creation & Selection", "Ctrl + Click", "Select Multiple Objects"),

            // --- 2. Slide Show Controls ---
            new ShortcutEntry(AppTarget.PowerPoint, "2. Slide Show Controls", "F5", "Start Presentation from Beginning"),
            new ShortcutEntry(AppTarget.PowerPoint, "2. Slide Show Controls", "Shift + F5", "Start Presentation from Current Slide"),
            new ShortcutEntry(AppTarget.PowerPoint, "2. Slide Show Controls", "N / Spacebar", "Next Slide or Animation"),
            new ShortcutEntry(AppTarget.PowerPoint, "2. Slide Show Controls", "P / Backspace", "Previous Slide or Animation"),
            new ShortcutEntry(AppTarget.PowerPoint, "2. Slide Show Controls", "B", "Display Black Screen / Resume"),
            new ShortcutEntry(AppTarget.PowerPoint, "2. Slide Show Controls", "W", "Display White Screen / Resume"),
            new ShortcutEntry(AppTarget.PowerPoint, "2. Slide Show Controls", "Esc", "End Slide Show"),

            // --- 3. Editing & Formatting ---
            new ShortcutEntry(AppTarget.PowerPoint, "3. Editing & Formatting", "Ctrl + B / I / U", "Bold / Italic / Underline"),
            new ShortcutEntry(AppTarget.PowerPoint, "3. Editing & Formatting", "Ctrl + E / L / R", "Center / Left / Right Align"),
            new ShortcutEntry(AppTarget.PowerPoint, "3. Editing & Formatting", "Ctrl + Shift + >", "Increase Font Size"),
            new ShortcutEntry(AppTarget.PowerPoint, "3. Editing & Formatting", "Ctrl + Shift + <", "Decrease Font Size"),
            new ShortcutEntry(AppTarget.PowerPoint, "3. Editing & Formatting", "Ctrl + G", "Group Selected Objects"),
            new ShortcutEntry(AppTarget.PowerPoint, "3. Editing & Formatting", "Ctrl + Shift + G", "Ungroup Selected Objects"),
            new ShortcutEntry(AppTarget.PowerPoint, "3. Editing & Formatting", "Ctrl + K", "Insert Hyperlink"),

            // --- 4. Annotation & Navigation ---
            new ShortcutEntry(AppTarget.PowerPoint, "4. Annotation & Navigation", "Ctrl + P", "Change Cursor to Pen Tool"),
            new ShortcutEntry(AppTarget.PowerPoint, "4. Annotation & Navigation", "Ctrl + A", "Change Cursor to Arrow"),
            new ShortcutEntry(AppTarget.PowerPoint, "4. Annotation & Navigation", "Ctrl + E", "Change Cursor to Eraser"),
            new ShortcutEntry(AppTarget.PowerPoint, "4. Annotation & Navigation", "Ctrl + L", "Change Cursor to Laser Pointer"),
            new ShortcutEntry(AppTarget.PowerPoint, "4. Annotation & Navigation", "Ctrl + H", "Hide Cursor & Navigation"),
            new ShortcutEntry(AppTarget.PowerPoint, "4. Annotation & Navigation", "Ctrl + Plus (+)", "Zoom In Slide"),
            new ShortcutEntry(AppTarget.PowerPoint, "4. Annotation & Navigation", "Ctrl + Minus (-)", "Zoom Out Slide")
        };

        private TextBox _searchBox;
        private DoubleBufferedPanel _listContainer;
        private Label _countLabel;
        private Label _noMatchLabel;
        private AppTarget _selectedApp = AppTarget.Windows;
        private readonly List<AppTabButton> _appTabs = new List<AppTabButton>();

        public SystemShortcutsForm()
        {
            Theme.Init(ThemeHelper.IsDarkMode);
            Text = "MicroApp - Shortcuts Cheat Sheet";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.Bg;
            Font = Theme.Base;
            Icon = Properties.Resources.AppIcon;

            SuspendLayout();

            const int CardW = 592;

            // ----- Header ------------------------------------------------------------
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
                Text = "Shortcuts Cheat Sheet",
                Font = new Font("Segoe UI Semibold", 12.5F),
                ForeColor = Theme.Text,
                BackColor = Color.Transparent
            };
            var subtitle = new Label
            {
                Location = new Point(74, 44),
                Size = new Size(530, 20),
                Text = "Essential productivity cheat sheet for Windows, Word, Excel, and PowerPoint.",
                Font = Theme.Small,
                ForeColor = Theme.TextDim,
                BackColor = Color.Transparent
            };
            Controls.Add(iconBox);
            Controls.Add(heading);
            Controls.Add(subtitle);

            // ----- App Switcher Tabs --------------------------------------------------
            int tabY = 76;
            int tabGap = 6;
            int tabW = (CardW - tabGap * 3) / 4; // ~143px each

            AddAppTab("Windows", AppTarget.Windows, 24 + (tabW + tabGap) * 0, tabY, tabW, true);
            AddAppTab("Word", AppTarget.Word, 24 + (tabW + tabGap) * 1, tabY, tabW, false);
            AddAppTab("Excel", AppTarget.Excel, 24 + (tabW + tabGap) * 2, tabY, tabW, false);
            AddAppTab("PowerPoint", AppTarget.PowerPoint, 24 + (tabW + tabGap) * 3, tabY, tabW, false);

            // ----- Search box --------------------------------------------------------
            var searchHost = new FieldHost
            {
                Location = new Point(24, 118),
                Size = new Size(CardW, 34)
            };
            _searchBox = new TextBox
            {
                Location = new Point(12, 7),
                Size = new Size(CardW - 24, 20),
                BorderStyle = BorderStyle.None,
                BackColor = Theme.FieldBg,
                ForeColor = Theme.Text,
                Font = Theme.Base
            };
            _searchBox.TextChanged += (s, e) => FilterList();
            searchHost.Controls.Add(_searchBox);
            Controls.Add(searchHost);

            // ----- Card Container -----------------------------------------------------
            var card = new Card
            {
                Location = new Point(24, 162),
                Size = new Size(CardW, 436),
                Title = "Cheat Sheet Reference",
                Description = "Browse sections or search actions and shortcut keys."
            };
            Controls.Add(card);

            int headY = 56;
            card.Controls.Add(new Label
            {
                Location = new Point(20, headY),
                AutoSize = true,
                Text = "Action / Function",
                Font = Theme.Small,
                ForeColor = Theme.TextDim,
                BackColor = Color.Transparent
            });
            card.Controls.Add(new Label
            {
                Location = new Point(366, headY),
                AutoSize = true,
                Text = "Shortcut Key",
                Font = Theme.Small,
                ForeColor = Theme.TextDim,
                BackColor = Color.Transparent
            });

            _listContainer = new DoubleBufferedPanel
            {
                Location = new Point(12, 78),
                Size = new Size(CardW - 24, 344),
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(_listContainer);

            _noMatchLabel = new Label
            {
                Location = new Point(0, 120),
                Size = new Size(_listContainer.Width, 32),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "No shortcuts found.",
                Font = Theme.Base,
                ForeColor = Theme.TextDim,
                BackColor = Color.Transparent,
                Visible = false
            };
            _listContainer.Controls.Add(_noMatchLabel);

            // ----- Footer -------------------------------------------------------------
            _countLabel = new Label
            {
                Location = new Point(26, card.Bottom + 18),
                AutoSize = true,
                Font = Theme.Small,
                ForeColor = Theme.TextDim,
                BackColor = Color.Transparent,
                Text = "Loading shortcuts..."
            };
            Controls.Add(_countLabel);

            var close = new ModernButton
            {
                Text = "Close",
                Accent = true,
                Size = new Size(100, 34),
                Location = new Point(24 + CardW - 100, card.Bottom + 12),
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

            if (_searchBox.IsHandleCreated)
                SendMessage(_searchBox.Handle, EM_SETCUEBANNER, (IntPtr)1, "Search shortcuts... (e.g. bold, slide, format, snip, formula)");
            else
                _searchBox.HandleCreated += (s, e) => SendMessage(_searchBox.Handle, EM_SETCUEBANNER, (IntPtr)1, "Search shortcuts... (e.g. bold, slide, format, snip, formula)");

            if (_listContainer.IsHandleCreated)
                Native.SetWindowTheme(_listContainer.Handle, Theme.Dark ? "DarkMode_Explorer" : null, null);
            else
                _listContainer.HandleCreated += (s, e) => Native.SetWindowTheme(_listContainer.Handle, Theme.Dark ? "DarkMode_Explorer" : null, null);

            FilterList();
        }

        private void AddAppTab(string title, AppTarget target, int x, int y, int width, bool active)
        {
            var tab = new AppTabButton(title, target)
            {
                Location = new Point(x, y),
                Size = new Size(width, 32),
                Active = active
            };
            tab.Click += (s, e) =>
            {
                if (_selectedApp == target) return;
                _selectedApp = target;
                foreach (var t in _appTabs) t.Active = (t.Target == _selectedApp);
                FilterList();
            };
            _appTabs.Add(tab);
            Controls.Add(tab);
        }

        private void FilterList()
        {
            string query = _searchBox.Text?.Trim().ToLowerInvariant() ?? "";
            bool isSearching = !string.IsNullOrEmpty(query);

            var matches = new List<ShortcutEntry>();
            foreach (var item in Entries)
            {
                if (item.App != _selectedApp) continue;

                if (isSearching)
                {
                    bool matchKey = item.Keys.ToLowerInvariant().Contains(query);
                    bool matchDesc = item.Description.ToLowerInvariant().Contains(query);
                    bool matchSec = item.Section.ToLowerInvariant().Contains(query);
                    if (!matchKey && !matchDesc && !matchSec) continue;
                }

                matches.Add(item);
            }

            int totalForApp = 0;
            foreach (var item in Entries)
            {
                if (item.App == _selectedApp) totalForApp++;
            }

            _listContainer.SuspendLayout();

            var toRemove = new List<Control>();
            foreach (Control c in _listContainer.Controls)
            {
                if (c != _noMatchLabel) toRemove.Add(c);
            }
            foreach (var c in toRemove)
            {
                _listContainer.Controls.Remove(c);
                c.Dispose();
            }

            if (matches.Count == 0)
            {
                _noMatchLabel.Text = isSearching
                    ? $"No {_selectedApp} shortcuts match \"{_searchBox.Text.Trim()}\""
                    : "No shortcuts available.";
                _noMatchLabel.Visible = true;
                _countLabel.Text = "0 shortcuts found";
            }
            else
            {
                _noMatchLabel.Visible = false;
                _countLabel.Text = isSearching
                    ? $"Showing {matches.Count} of {totalForApp} {_selectedApp} shortcuts"
                    : $"Showing all {totalForApp} {_selectedApp} shortcuts";

                int y = 4;
                string currentSection = null;

                for (int i = 0; i < matches.Count; i++)
                {
                    var item = matches[i];

                    // Insert section banner when section changes
                    if (item.Section != currentSection)
                    {
                        currentSection = item.Section;
                        if (y > 4) y += 6; // Spacing before section banner

                        var banner = new SectionBanner(currentSection)
                        {
                            Location = new Point(4, y),
                            Size = new Size(_listContainer.Width - 28, 28)
                        };
                        _listContainer.Controls.Add(banner);
                        y += 32;
                    }

                    var lbl = new Label
                    {
                        Location = new Point(8, y + 4),
                        Size = new Size(340, 20),
                        Text = item.Description,
                        Font = Theme.Base,
                        ForeColor = Theme.Text,
                        BackColor = Color.Transparent,
                        AutoEllipsis = true,
                        UseMnemonic = false
                    };
                    _listContainer.Controls.Add(lbl);

                    var badge = new KeyBadge
                    {
                        Location = new Point(354, y),
                        Size = new Size(184, 26),
                        Text = item.Keys
                    };
                    _listContainer.Controls.Add(badge);

                    y += 34;
                }
            }

            _listContainer.ResumeLayout();
        }
    }
}
