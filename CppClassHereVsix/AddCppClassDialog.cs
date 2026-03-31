using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.PlatformUI;

namespace CppClassHereVsix
{
    internal sealed class AddCppClassDialog : Form
    {
        private const int DragRegionHeight = 80;
        private const int WmNclButtonDown = 0xA1;
        private const int HtCaption = 0x2;

        private readonly string targetFolder;
        private readonly Label titleLabel;
        private readonly Label sourceFileCaptionLabel;
        private readonly List<Label> captionLabels = new List<Label>();
        private readonly TextBox classNameTextBox;
        private readonly TextBox headerFileTextBox;
        private readonly TextBox sourceFileTextBox;
        private readonly TextBox baseClassTextBox;
        private readonly ComboBox inheritanceAccessComboBox;
        private readonly CheckBox inlineCheckBox;
        private readonly Button headerBrowseButton;
        private readonly Button sourceBrowseButton;
        private readonly Button okButton;
        private readonly Button cancelButton;
        private readonly ThemeChangedEventHandler themeChangedHandler;
        private ThemePalette palette;
        private bool updatingControls;
        private bool isFileNameSynced;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private AddCppClassDialog(string targetFolder)
        {
            this.targetFolder = targetFolder;
            themeChangedHandler = e => ApplyThemeSafe();

            AutoScaleMode = AutoScaleMode.Font;
            Font = SystemFonts.MessageBoxFont;
            Text = LocalizedStrings.DialogWindowTitle;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(598, 440);
            Padding = new Padding(1);
            DoubleBuffered = true;
            MouseDown += OnChromeMouseDown;
            Paint += OnDialogPaint;
            FormClosed += OnDialogClosed;

            palette = ThemePalette.Create();

            titleLabel = new Label
            {
                AutoSize = true,
                Left = 23,
                Top = 34,
                Font = new Font(Font.FontFamily, 18f, FontStyle.Regular),
                Text = LocalizedStrings.DialogHeaderTitle,
                TabStop = false
            };
            titleLabel.MouseDown += OnChromeMouseDown;
            Controls.Add(titleLabel);

            CreateCaption(LocalizedStrings.ClassNameLabel, 24, 108);
            CreateCaption(LocalizedStrings.HeaderFileLabel, 211, 108);
            sourceFileCaptionLabel = CreateCaption(LocalizedStrings.SourceFileLabel, 399, 108);

            classNameTextBox = CreateTextBox(24, 127, 176, 0);
            classNameTextBox.TextChanged += OnClassNameChanged;
            Controls.Add(classNameTextBox);

            headerFileTextBox = CreateTextBox(211, 127, 148, 1);
            headerFileTextBox.TextChanged += OnFileNameEdited;
            Controls.Add(headerFileTextBox);

            headerBrowseButton = CreateBrowseButton(362, 127, OnBrowseHeaderFile);
            Controls.Add(headerBrowseButton);

            sourceFileTextBox = CreateTextBox(399, 127, 148, 2);
            sourceFileTextBox.TextChanged += OnFileNameEdited;
            Controls.Add(sourceFileTextBox);

            sourceBrowseButton = CreateBrowseButton(550, 127, OnBrowseSourceFile);
            Controls.Add(sourceBrowseButton);

            CreateCaption(LocalizedStrings.BaseClassLabel, 24, 174);
            CreateCaption(LocalizedStrings.AccessLabel, 211, 174);

            baseClassTextBox = CreateTextBox(24, 192, 176, 3);
            Controls.Add(baseClassTextBox);

            inheritanceAccessComboBox = new ComboBox
            {
                Left = 211,
                Top = 192,
                Width = 176,
                Height = 24,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                TabIndex = 4
            };
            inheritanceAccessComboBox.Items.AddRange(new object[]
            {
                new InheritanceAccessOption("public", "public"),
                new InheritanceAccessOption("protected", "protected"),
                new InheritanceAccessOption("private", "private")
            });
            inheritanceAccessComboBox.SelectedIndex = 0;
            Controls.Add(inheritanceAccessComboBox);

            Label otherOptionsLabel = CreateCaption(LocalizedStrings.OtherOptionsLabel, 24, 240);
            otherOptionsLabel.TabStop = false;

            inlineCheckBox = new CheckBox
            {
                Left = 35,
                Top = 260,
                Width = 120,
                Height = 24,
                Text = LocalizedStrings.InlineLabel,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                TabIndex = 5
            };
            inlineCheckBox.CheckedChanged += OnInlineCheckedChanged;
            Controls.Add(inlineCheckBox);

            okButton = CreateActionButton(LocalizedStrings.OkButtonText, 422, 392, 6);
            okButton.DialogResult = DialogResult.OK;
            okButton.Click += OnConfirmClick;
            Controls.Add(okButton);

            cancelButton = CreateActionButton(LocalizedStrings.CancelButtonText, 503, 392, 7);
            cancelButton.DialogResult = DialogResult.Cancel;
            Controls.Add(cancelButton);

            AcceptButton = okButton;
            CancelButton = cancelButton;

            isFileNameSynced = true;
            SyncFileNames();
            ApplyTheme();
            ApplyInlineState();
            ActiveControl = classNameTextBox;
            VSColorTheme.ThemeChanged += themeChangedHandler;
        }

        protected override bool ProcessTabKey(bool forward)
        {
            Control focusedControl = FindFocusedControl(this);
            IReadOnlyList<Control> tabSequence = GetTabSequence();
            int focusedIndex = IndexOfControl(tabSequence, focusedControl);

            if (focusedIndex >= 0)
            {
                int nextIndex = focusedIndex + (forward ? 1 : -1);
                if (nextIndex >= 0 && nextIndex < tabSequence.Count)
                {
                    tabSequence[nextIndex].Focus();
                    return true;
                }
            }

            return base.ProcessTabKey(forward);
        }

        public string ClassName => classNameTextBox.Text.Trim();

        public string HeaderFileName => headerFileTextBox.Text.Trim();

        public string SourceFileName => sourceFileTextBox.Text.Trim();

        public string BaseClassName => baseClassTextBox.Text.Trim();

        public string InheritanceAccess => (inheritanceAccessComboBox.SelectedItem as InheritanceAccessOption)?.Keyword ?? "public";

        public bool IsInline => inlineCheckBox.Checked;

        public static AddCppClassDialogResult ShowDialog(string targetFolder)
        {
            using (AddCppClassDialog dialog = new AddCppClassDialog(targetFolder))
            {
                return dialog.ShowDialog() == DialogResult.OK
                    ? new AddCppClassDialogResult(
                        dialog.ClassName,
                        dialog.HeaderFileName,
                        dialog.SourceFileName,
                        dialog.BaseClassName,
                        dialog.InheritanceAccess,
                        dialog.IsInline)
                    : null;
            }
        }

        private static Control FindFocusedControl(Control parent)
        {
            Control current = parent;
            while (current is ContainerControl container && container.ActiveControl != null)
            {
                current = container.ActiveControl;
            }

            return current;
        }

        private IReadOnlyList<Control> GetTabSequence()
        {
            List<Control> controls = new List<Control>
            {
                classNameTextBox,
                headerFileTextBox
            };

            if (!inlineCheckBox.Checked)
            {
                controls.Add(sourceFileTextBox);
            }

            controls.Add(baseClassTextBox);
            controls.Add(inheritanceAccessComboBox);
            controls.Add(inlineCheckBox);
            controls.Add(okButton);
            controls.Add(cancelButton);
            return controls;
        }

        private static int IndexOfControl(IReadOnlyList<Control> controls, Control target)
        {
            for (int i = 0; i < controls.Count; i++)
            {
                if (controls[i] == target)
                {
                    return i;
                }
            }

            return -1;
        }

        private Label CreateCaption(string text, int x, int y)
        {
            Label label = new Label
            {
                AutoSize = true,
                Left = x,
                Top = y,
                Text = text,
                TabStop = false
            };
            captionLabels.Add(label);
            Controls.Add(label);
            return label;
        }

        private TextBox CreateTextBox(int x, int y, int width, int tabIndex)
        {
            return new TextBox
            {
                Left = x,
                Top = y,
                Width = width,
                Height = 23,
                BorderStyle = BorderStyle.FixedSingle,
                TabIndex = tabIndex
            };
        }

        private Button CreateBrowseButton(int x, int y, EventHandler clickHandler)
        {
            Button button = new Button
            {
                Left = x,
                Top = y,
                Width = 28,
                Height = 23,
                Text = "...",
                FlatStyle = FlatStyle.Flat,
                TabStop = false
            };
            button.Click += clickHandler;
            return button;
        }

        private Button CreateActionButton(string text, int x, int y, int tabIndex)
        {
            Button button = new Button
            {
                Left = x,
                Top = y,
                Width = 76,
                Height = 24,
                Text = text,
                FlatStyle = FlatStyle.Flat,
                TabIndex = tabIndex
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void OnClassNameChanged(object sender, EventArgs e)
        {
            if (updatingControls)
            {
                return;
            }

            isFileNameSynced = true;
            SyncFileNames();
        }

        private void OnFileNameEdited(object sender, EventArgs e)
        {
            if (updatingControls)
            {
                return;
            }

            isFileNameSynced = false;
        }

        private void OnInlineCheckedChanged(object sender, EventArgs e)
        {
            ApplyInlineState();
        }

        private void ApplyInlineState()
        {
            bool sourceEnabled = !inlineCheckBox.Checked;
            sourceFileCaptionLabel.ForeColor = sourceEnabled ? palette.TextColor : palette.MutedTextColor;
            sourceFileTextBox.Enabled = sourceEnabled;
            sourceFileTextBox.BackColor = sourceEnabled ? palette.ControlBackground : palette.DisabledControlBackground;
            sourceFileTextBox.ForeColor = sourceEnabled ? palette.TextColor : palette.MutedTextColor;
            sourceBrowseButton.Enabled = sourceEnabled;
            sourceBrowseButton.BackColor = sourceEnabled ? palette.ControlBackground : palette.DisabledControlBackground;
            sourceBrowseButton.ForeColor = sourceEnabled ? palette.TextColor : palette.MutedTextColor;
            sourceBrowseButton.FlatAppearance.MouseOverBackColor = sourceEnabled ? palette.ControlHoverBackground : palette.DisabledControlBackground;
            sourceBrowseButton.FlatAppearance.MouseDownBackColor = sourceEnabled ? palette.ControlPressedBackground : palette.DisabledControlBackground;
        }

        private void ApplyThemeSafe()
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(ApplyTheme));
                return;
            }

            ApplyTheme();
        }

        private void ApplyTheme()
        {
            palette = ThemePalette.Create();

            BackColor = palette.WindowBackground;
            ForeColor = palette.TextColor;

            titleLabel.ForeColor = palette.TextColor;

            foreach (Label captionLabel in captionLabels)
            {
                captionLabel.ForeColor = palette.TextColor;
                captionLabel.BackColor = Color.Transparent;
            }

            ApplyTextBoxTheme(classNameTextBox, palette.ControlBackground, palette.TextColor);
            ApplyTextBoxTheme(headerFileTextBox, palette.ControlBackground, palette.TextColor);
            ApplyTextBoxTheme(baseClassTextBox, palette.ControlBackground, palette.TextColor);
            ApplyTextBoxTheme(sourceFileTextBox, palette.ControlBackground, palette.TextColor);

            inheritanceAccessComboBox.BackColor = palette.ControlBackground;
            inheritanceAccessComboBox.ForeColor = palette.TextColor;

            inlineCheckBox.BackColor = palette.WindowBackground;
            inlineCheckBox.ForeColor = palette.TextColor;
            inlineCheckBox.FlatAppearance.BorderColor = palette.BorderColor;
            inlineCheckBox.FlatAppearance.CheckedBackColor = palette.WindowBackground;
            inlineCheckBox.FlatAppearance.MouseOverBackColor = palette.WindowBackground;
            inlineCheckBox.FlatAppearance.MouseDownBackColor = palette.WindowBackground;

            ApplyBrowseButtonTheme(headerBrowseButton);
            ApplyBrowseButtonTheme(sourceBrowseButton);
            ApplyActionButtonTheme(okButton, palette.PrimaryButtonBackground, palette.PrimaryButtonTextColor);
            ApplyActionButtonTheme(cancelButton, palette.ButtonBackground, palette.ButtonTextColor);

            ApplyInlineState();
            Invalidate();
        }

        private static void ApplyTextBoxTheme(TextBox textBox, Color backColor, Color foreColor)
        {
            textBox.BackColor = backColor;
            textBox.ForeColor = foreColor;
        }

        private void ApplyBrowseButtonTheme(Button button)
        {
            button.BackColor = palette.ControlBackground;
            button.ForeColor = palette.TextColor;
            button.FlatAppearance.BorderColor = palette.BorderColor;
            button.FlatAppearance.MouseOverBackColor = palette.ControlHoverBackground;
            button.FlatAppearance.MouseDownBackColor = palette.ControlPressedBackground;
        }

        private void ApplyActionButtonTheme(Button button, Color backColor, Color foreColor)
        {
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.FlatAppearance.MouseOverBackColor = AdjustColor(backColor, 0.08f);
            button.FlatAppearance.MouseDownBackColor = AdjustColor(backColor, -0.08f);
        }

        private void OnBrowseHeaderFile(object sender, EventArgs e)
        {
            BrowseForFileName(headerFileTextBox, LocalizedStrings.HeaderBrowseTitle, LocalizedStrings.HeaderFileFilter, ".h");
        }

        private void OnBrowseSourceFile(object sender, EventArgs e)
        {
            if (!sourceFileTextBox.Enabled)
            {
                return;
            }

            BrowseForFileName(sourceFileTextBox, LocalizedStrings.SourceBrowseTitle, LocalizedStrings.SourceFileFilter, ".cpp");
        }

        private void BrowseForFileName(TextBox textBox, string title, string filter, string defaultExtension)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = title;
                dialog.InitialDirectory = Directory.Exists(targetFolder) ? targetFolder : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                dialog.Filter = filter;
                dialog.DefaultExt = defaultExtension.TrimStart('.');
                dialog.AddExtension = true;
                dialog.CheckFileExists = false;
                dialog.OverwritePrompt = false;
                dialog.FileName = textBox.Text;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    updatingControls = true;
                    try
                    {
                        textBox.Text = Path.GetFileName(dialog.FileName);
                    }
                    finally
                    {
                        updatingControls = false;
                    }

                    isFileNameSynced = false;
                }
            }
        }

        private void OnConfirmClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ClassName))
            {
                MessageBox.Show(this, LocalizedStrings.EnterClassNameMessage, LocalizedStrings.DialogWindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                classNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(HeaderFileName))
            {
                MessageBox.Show(this, LocalizedStrings.EnterHeaderFileNameMessage, LocalizedStrings.DialogWindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                headerFileTextBox.Focus();
                return;
            }

            if (!inlineCheckBox.Checked && string.IsNullOrWhiteSpace(SourceFileName))
            {
                MessageBox.Show(this, LocalizedStrings.EnterSourceFileNameMessage, LocalizedStrings.DialogWindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                sourceFileTextBox.Focus();
                return;
            }
        }

        private void SyncFileNames()
        {
            if (!isFileNameSynced)
            {
                return;
            }

            updatingControls = true;
            try
            {
                string className = classNameTextBox.Text.Trim();
                headerFileTextBox.Text = BuildDefaultFileName(className, ".h");
                sourceFileTextBox.Text = BuildDefaultFileName(className, ".cpp");
            }
            finally
            {
                updatingControls = false;
            }
        }

        private static string BuildDefaultFileName(string className, string extension)
        {
            return string.IsNullOrWhiteSpace(className) ? string.Empty : className + extension;
        }

        private void OnDialogPaint(object sender, PaintEventArgs e)
        {
            Rectangle borderBounds = new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            ControlPaint.DrawBorder(e.Graphics, borderBounds, palette.BorderColor, ButtonBorderStyle.Solid);
        }

        private void OnChromeMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (sender == this && e.Y > DragRegionHeight)
            {
                return;
            }

            ReleaseCapture();
            SendMessage(Handle, WmNclButtonDown, (IntPtr)HtCaption, IntPtr.Zero);
        }

        private void OnDialogClosed(object sender, FormClosedEventArgs e)
        {
            VSColorTheme.ThemeChanged -= themeChangedHandler;
        }

        private static Color AdjustColor(Color color, float delta)
        {
            return Color.FromArgb(
                color.A,
                AdjustChannel(color.R, delta),
                AdjustChannel(color.G, delta),
                AdjustChannel(color.B, delta));
        }

        private static int AdjustChannel(int channel, float delta)
        {
            float adjusted = delta >= 0f
                ? channel + ((255 - channel) * delta)
                : channel * (1f + delta);
            return Math.Max(0, Math.Min(255, (int)Math.Round(adjusted)));
        }

        private sealed class ThemePalette
        {
            public Color WindowBackground { get; private set; }
            public Color ControlBackground { get; private set; }
            public Color DisabledControlBackground { get; private set; }
            public Color ControlHoverBackground { get; private set; }
            public Color ControlPressedBackground { get; private set; }
            public Color BorderColor { get; private set; }
            public Color TextColor { get; private set; }
            public Color MutedTextColor { get; private set; }
            public Color ButtonBackground { get; private set; }
            public Color ButtonTextColor { get; private set; }
            public Color PrimaryButtonBackground { get; private set; }
            public Color PrimaryButtonTextColor { get; private set; }

            public static ThemePalette Create()
            {
                try
                {
                    Color windowBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
                    Color controlBackground = VSColorTheme.GetThemedColor(EnvironmentColors.SystemWindowColorKey);
                    Color borderColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBorderColorKey);
                    Color textColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
                    Color mutedTextColor = VSColorTheme.GetThemedColor(EnvironmentColors.SystemGrayTextColorKey);
                    Color buttonBackground = VSColorTheme.GetThemedColor(EnvironmentColors.SystemButtonFaceColorKey);
                    Color buttonTextColor = VSColorTheme.GetThemedColor(EnvironmentColors.SystemButtonTextColorKey);
                    Color accentColor = VSColorTheme.GetThemedColor(EnvironmentColors.AccentMediumColorKey);

                    return new ThemePalette
                    {
                        WindowBackground = windowBackground,
                        ControlBackground = controlBackground,
                        DisabledControlBackground = Blend(windowBackground, controlBackground, 0.35f),
                        ControlHoverBackground = Blend(controlBackground, accentColor, 0.12f),
                        ControlPressedBackground = Blend(controlBackground, accentColor, 0.22f),
                        BorderColor = borderColor,
                        TextColor = textColor,
                        MutedTextColor = mutedTextColor,
                        ButtonBackground = buttonBackground,
                        ButtonTextColor = buttonTextColor,
                        PrimaryButtonBackground = accentColor,
                        PrimaryButtonTextColor = GetReadableTextColor(accentColor, textColor)
                    };
                }
                catch
                {
                    return new ThemePalette
                    {
                        WindowBackground = Color.FromArgb(37, 37, 38),
                        ControlBackground = Color.FromArgb(51, 51, 55),
                        DisabledControlBackground = Color.FromArgb(45, 45, 48),
                        ControlHoverBackground = Color.FromArgb(63, 63, 70),
                        ControlPressedBackground = Color.FromArgb(72, 72, 78),
                        BorderColor = Color.FromArgb(63, 63, 70),
                        TextColor = Color.FromArgb(241, 241, 241),
                        MutedTextColor = Color.FromArgb(160, 160, 160),
                        ButtonBackground = Color.FromArgb(62, 62, 64),
                        ButtonTextColor = Color.FromArgb(241, 241, 241),
                        PrimaryButtonBackground = Color.FromArgb(104, 82, 168),
                        PrimaryButtonTextColor = Color.White
                    };
                }
            }

            private static Color Blend(Color baseColor, Color overlayColor, float amount)
            {
                amount = Math.Max(0f, Math.Min(1f, amount));
                return Color.FromArgb(
                    255,
                    (int)Math.Round((baseColor.R * (1f - amount)) + (overlayColor.R * amount)),
                    (int)Math.Round((baseColor.G * (1f - amount)) + (overlayColor.G * amount)),
                    (int)Math.Round((baseColor.B * (1f - amount)) + (overlayColor.B * amount)));
            }

            private static Color GetReadableTextColor(Color background, Color fallback)
            {
                double luminance = ((0.299 * background.R) + (0.587 * background.G) + (0.114 * background.B)) / 255d;
                if (luminance < 0.45d)
                {
                    return Color.White;
                }

                if (luminance > 0.75d)
                {
                    return Color.Black;
                }

                return fallback;
            }
        }
    }

    internal sealed class AddCppClassDialogResult
    {
        public AddCppClassDialogResult(
            string className,
            string headerFileName,
            string sourceFileName,
            string baseClassName,
            string inheritanceAccess,
            bool isInline)
        {
            ClassName = className;
            HeaderFileName = headerFileName;
            SourceFileName = sourceFileName;
            BaseClassName = baseClassName;
            InheritanceAccess = inheritanceAccess;
            IsInline = isInline;
        }

        public string ClassName { get; }

        public string HeaderFileName { get; }

        public string SourceFileName { get; }

        public string BaseClassName { get; }

        public string InheritanceAccess { get; }

        public bool IsInline { get; }
    }

    internal sealed class InheritanceAccessOption
    {
        public InheritanceAccessOption(string displayText, string keyword)
        {
            DisplayText = displayText;
            Keyword = keyword;
        }

        public string DisplayText { get; }

        public string Keyword { get; }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}

