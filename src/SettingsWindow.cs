using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EpsonMassReboot
{
    public class SettingsWindow : Window
    {
        private bool _isDark = true;

        // Theme Colors
        private Color _winBg;
        private Color _boxBg;
        private Color _borderColor;
        private Color _textColor;
        private Color _secondaryTextColor;
        private Color _warningColor;
        private Color _btnBg;
        private Color _btnBorder;
        private Color _btnHoverBg;
        private Color _btnPressedBg;

        // GroupBoxes & Controls
        private ScrollViewer _scroll;
        private GroupBox _grpVersion;
        private RadioButton _rbVer1;
        private RadioButton _rbVer2c;
        private RadioButton _rbVer3;
        private SnmpVersion _tempVersion;

        // V1/V2 Panel
        private GroupBox _grpCommunity;
        private TextBlock _lblCommunity;
        private TextBlock _lblCommunityNotice;
        private TextBox _txtCommunity;

        // V3 Panel
        private GroupBox _grpV3;
        private TextBlock _lblV3User;
        private TextBox _txtV3User;
        private GroupBox _grpV3Auth;
        private RadioButton _rbAuthMd5;
        private RadioButton _rbAuthSha1;
        private TextBlock _lblV3AuthPass;
        private PasswordBox _txtV3AuthPass;

        private GroupBox _grpV3Priv;
        private RadioButton _rbPrivDes;
        private RadioButton _rbPrivAes;
        private TextBlock _lblV3PrivPass;
        private PasswordBox _txtV3PrivPass;

        private TextBlock _lblV3Context;
        private TextBox _txtV3Context;

        private Button _btnOk;
        private Button _btnCancel;

        public SettingsWindow(Window owner, bool isDark = true)
        {
            Owner = owner;
            _isDark = isDark;
            InitializeColors();
            InitializeWindow();
            BuildUi();
            LoadValues();
            ApplyLanguage();
        }

        private void InitializeColors()
        {
            if (_isDark)
            {
                _winBg = Color.FromRgb(32, 32, 32);             // #202020
                _boxBg = Color.FromRgb(30, 30, 30);             // #1E1E1E
                _borderColor = Color.FromRgb(65, 65, 65);       // #414141
                _textColor = Color.FromRgb(230, 230, 230);      // #E6E6E6
                _secondaryTextColor = Color.FromRgb(160, 160, 160);
                _warningColor = Color.FromRgb(255, 110, 110);   // Soft red
                _btnBg = Color.FromRgb(48, 48, 48);
                _btnBorder = Color.FromRgb(70, 70, 70);
                _btnHoverBg = Color.FromRgb(62, 62, 62);
                _btnPressedBg = Color.FromRgb(0, 120, 215);
            }
            else
            {
                _winBg = Color.FromRgb(240, 240, 240);          // #F0F0F0
                _boxBg = Color.FromRgb(255, 255, 255);          // #FFFFFF
                _borderColor = Color.FromRgb(170, 170, 170);    // #AAAAAA
                _textColor = Color.FromRgb(0, 0, 0);            // #000000
                _secondaryTextColor = Color.FromRgb(90, 90, 90);
                _warningColor = Color.FromRgb(180, 0, 0);        // Deep red
                _btnBg = Color.FromRgb(225, 225, 225);
                _btnBorder = Color.FromRgb(173, 173, 173);
                _btnHoverBg = Color.FromRgb(229, 241, 251);
                _btnPressedBg = Color.FromRgb(204, 228, 247);
            }
        }

        private void InitializeWindow()
        {
            Title = Localization.Get("SettingsTitle");
            Width = 490;
            Height = 465;
            MinWidth = 450;
            MinHeight = 420;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(_winBg);
            Foreground = new SolidColorBrush(_textColor);
            FontFamily = new FontFamily("Segoe UI, Tahoma, Arial");
            FontSize = 12;
            ShowInTaskbar = false;
            ResizeMode = ResizeMode.CanResizeWithGrip;

            try
            {
                if (System.IO.File.Exists("app.ico"))
                {
                    Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri(System.IO.Path.GetFullPath("app.ico")));
                }
                else if (Owner != null && Owner.Icon != null)
                {
                    Icon = Owner.Icon;
                }
            }
            catch { }
        }

        private void BuildUi()
        {
            var mainGrid = new Grid { Margin = new Thickness(12) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Bottom Buttons

            _scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Padding = new Thickness(0, 0, 4, 0)
            };
            var body = new StackPanel();

            // 1. Version Selection GroupBox
            _grpVersion = new GroupBox
            {
                Header = Localization.Get("GrpSnmpVersion"),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(10, 8, 10, 8),
                Style = CreateModernGroupBoxStyle(_isDark)
            };
            var verStack = new StackPanel { Orientation = Orientation.Horizontal };
            _rbVer1 = new RadioButton { Content = "SNMPv1", IsChecked = true, Margin = new Thickness(0, 0, 20, 0), Foreground = new SolidColorBrush(_textColor) };
            _rbVer2c = new RadioButton { Content = "SNMPv2c", Margin = new Thickness(0, 0, 20, 0), Foreground = new SolidColorBrush(_textColor) };
            _rbVer3 = new RadioButton { Content = "SNMPv3", Foreground = new SolidColorBrush(_textColor) };

            _rbVer1.Checked += (s, e) => SetVersion(SnmpVersion.V1);
            _rbVer2c.Checked += (s, e) => SetVersion(SnmpVersion.V2c);
            _rbVer3.Checked += (s, e) => SetVersion(SnmpVersion.V3);

            verStack.Children.Add(_rbVer1);
            verStack.Children.Add(_rbVer2c);
            verStack.Children.Add(_rbVer3);
            _grpVersion.Content = verStack;
            body.Children.Add(_grpVersion);

            // 2. V1/V2 Community GroupBox
            _grpCommunity = new GroupBox
            {
                Header = Localization.Get("GrpCommunity"),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(10, 8, 10, 8),
                Style = CreateModernGroupBoxStyle(_isDark)
            };
            var commStack = new StackPanel();
            _lblCommunity = new TextBlock
            {
                Text = Localization.Get("CommunityString"),
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(_warningColor),
                Margin = new Thickness(0, 0, 0, 4),
                TextWrapping = TextWrapping.Wrap
            };
            commStack.Children.Add(_lblCommunity);

            _txtCommunity = CreateModernTextBox();
            commStack.Children.Add(_txtCommunity);

            _lblCommunityNotice = new TextBlock
            {
                Text = Localization.Get("CommunityDeviceNotice"),
                FontSize = 11,
                Foreground = new SolidColorBrush(_secondaryTextColor),
                Margin = new Thickness(0, 4, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            commStack.Children.Add(_lblCommunityNotice);

            _grpCommunity.Content = commStack;
            body.Children.Add(_grpCommunity);

            // 3. V3 GroupBox (Matching Epson Web Config)
            _grpV3 = new GroupBox
            {
                Header = Localization.Get("GrpV3"),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(10, 8, 10, 8),
                Style = CreateModernGroupBoxStyle(_isDark),
                Visibility = Visibility.Collapsed
            };
            var v3Stack = new StackPanel();

            // User Name
            _lblV3User = new TextBlock { Text = Localization.Get("V3User"), Margin = new Thickness(0, 0, 0, 3), Foreground = new SolidColorBrush(_textColor) };
            _txtV3User = CreateModernTextBox();
            _txtV3User.Text = "admin";
            v3Stack.Children.Add(_lblV3User);
            v3Stack.Children.Add(_txtV3User);

            // Authentication Settings GroupBox
            _grpV3Auth = new GroupBox
            {
                Header = Localization.Get("GrpV3Auth"),
                Margin = new Thickness(0, 8, 0, 8),
                Padding = new Thickness(8, 6, 8, 6),
                Style = CreateModernGroupBoxStyle(_isDark)
            };
            var authStack = new StackPanel();
            var authAlgoRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
            _rbAuthMd5 = new RadioButton { Content = "MD5", IsChecked = true, Margin = new Thickness(0, 0, 16, 0), Foreground = new SolidColorBrush(_textColor) };
            _rbAuthSha1 = new RadioButton { Content = "SHA-1", Foreground = new SolidColorBrush(_textColor) };
            authAlgoRow.Children.Add(_rbAuthMd5);
            authAlgoRow.Children.Add(_rbAuthSha1);
            authStack.Children.Add(authAlgoRow);

            _lblV3AuthPass = new TextBlock { Text = Localization.Get("V3Password"), Margin = new Thickness(0, 0, 0, 3), Foreground = new SolidColorBrush(_textColor) };
            _txtV3AuthPass = CreateModernPasswordBox();
            authStack.Children.Add(_lblV3AuthPass);
            authStack.Children.Add(_txtV3AuthPass);
            _grpV3Auth.Content = authStack;
            v3Stack.Children.Add(_grpV3Auth);

            // Encryption Settings GroupBox
            _grpV3Priv = new GroupBox
            {
                Header = Localization.Get("GrpV3Priv"),
                Margin = new Thickness(0, 0, 0, 8),
                Padding = new Thickness(8, 6, 8, 6),
                Style = CreateModernGroupBoxStyle(_isDark)
            };
            var privStack = new StackPanel();
            var privAlgoRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
            _rbPrivDes = new RadioButton { Content = "DES", IsChecked = true, Margin = new Thickness(0, 0, 16, 0), Foreground = new SolidColorBrush(_textColor) };
            _rbPrivAes = new RadioButton { Content = "AES-128", Foreground = new SolidColorBrush(_textColor) };
            privAlgoRow.Children.Add(_rbPrivDes);
            privAlgoRow.Children.Add(_rbPrivAes);
            privStack.Children.Add(privAlgoRow);

            _lblV3PrivPass = new TextBlock { Text = Localization.Get("V3Password"), Margin = new Thickness(0, 0, 0, 3), Foreground = new SolidColorBrush(_textColor) };
            _txtV3PrivPass = CreateModernPasswordBox();
            privStack.Children.Add(_lblV3PrivPass);
            privStack.Children.Add(_txtV3PrivPass);
            _grpV3Priv.Content = privStack;
            v3Stack.Children.Add(_grpV3Priv);

            // Context Name
            _lblV3Context = new TextBlock { Text = Localization.Get("V3ContextName"), Margin = new Thickness(0, 0, 0, 3), Foreground = new SolidColorBrush(_textColor) };
            _txtV3Context = CreateModernTextBox();
            _txtV3Context.Text = "EPSON";
            v3Stack.Children.Add(_lblV3Context);
            v3Stack.Children.Add(_txtV3Context);

            _grpV3.Content = v3Stack;
            body.Children.Add(_grpV3);

            _scroll.Content = body;
            Grid.SetRow(_scroll, 0);
            mainGrid.Children.Add(_scroll);

            // Bottom Buttons: OK & Cancel
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            _btnOk = CreateModernButton(Localization.Get("BtnOk"), 80, 25);
            _btnOk.IsDefault = true;
            _btnOk.Margin = new Thickness(0, 0, 8, 0);
            _btnOk.Click += (s, e) => OnSaveClicked();

            _btnCancel = CreateModernButton(Localization.Get("BtnCancel"), 80, 25);
            _btnCancel.IsCancel = true;
            _btnCancel.Click += (s, e) => { DialogResult = false; Close(); };

            buttonPanel.Children.Add(_btnOk);
            buttonPanel.Children.Add(_btnCancel);
            Grid.SetRow(buttonPanel, 1);
            mainGrid.Children.Add(buttonPanel);

            Content = mainGrid;
        }

        private void LoadValues()
        {
            var cfg = SnmpConfig.Current;
            SetVersion(cfg.Version);
            _txtCommunity.Text = cfg.Community ?? "";
            _txtV3User.Text = cfg.V3UserName ?? "admin";

            if (string.Equals(cfg.V3AuthAlgorithm, "SHA1", StringComparison.OrdinalIgnoreCase))
                _rbAuthSha1.IsChecked = true;
            else
                _rbAuthMd5.IsChecked = true;

            _txtV3AuthPass.Password = cfg.V3AuthPassword ?? "";

            if (string.Equals(cfg.V3PrivAlgorithm, "AES128", StringComparison.OrdinalIgnoreCase))
                _rbPrivAes.IsChecked = true;
            else
                _rbPrivDes.IsChecked = true;

            _txtV3PrivPass.Password = cfg.V3PrivPassword ?? "";
            _txtV3Context.Text = cfg.V3ContextName ?? "EPSON";
        }

        private void SetVersion(SnmpVersion ver)
        {
            _tempVersion = ver;

            _rbVer1.IsChecked = (ver == SnmpVersion.V1);
            _rbVer2c.IsChecked = (ver == SnmpVersion.V2c);
            _rbVer3.IsChecked = (ver == SnmpVersion.V3);

            if (ver == SnmpVersion.V3)
            {
                _grpCommunity.Visibility = Visibility.Collapsed;
                _grpV3.Visibility = Visibility.Visible;
            }
            else
            {
                _grpCommunity.Visibility = Visibility.Visible;
                _grpV3.Visibility = Visibility.Collapsed;
            }
        }

        private void ApplyLanguage()
        {
            Title = Localization.Get("SettingsTitle");
            _grpVersion.Header = Localization.Get("GrpSnmpVersion");
            _grpCommunity.Header = Localization.Get("GrpCommunity");
            _lblCommunity.Text = Localization.Get("CommunityString");
            _lblCommunityNotice.Text = Localization.Get("CommunityDeviceNotice");
            _grpV3.Header = Localization.Get("GrpV3");
            _lblV3User.Text = Localization.Get("V3User");
            _grpV3Auth.Header = Localization.Get("GrpV3Auth");
            _lblV3AuthPass.Text = Localization.Get("V3Password");
            _grpV3Priv.Header = Localization.Get("GrpV3Priv");
            _lblV3PrivPass.Text = Localization.Get("V3Password");
            _lblV3Context.Text = Localization.Get("V3ContextName");
            _btnOk.Content = Localization.Get("BtnOk");
            _btnCancel.Content = Localization.Get("BtnCancel");
        }

        private void OnSaveClicked()
        {
            var cfg = SnmpConfig.Current;
            cfg.Version = _tempVersion;
            cfg.Community = _txtCommunity.Text.Trim();

            cfg.V3UserName = string.IsNullOrEmpty(_txtV3User.Text.Trim()) ? "admin" : _txtV3User.Text.Trim();
            cfg.V3AuthAlgorithm = (_rbAuthSha1.IsChecked == true) ? "SHA1" : "MD5";
            cfg.V3AuthPassword = _txtV3AuthPass.Password;
            cfg.V3PrivAlgorithm = (_rbPrivAes.IsChecked == true) ? "AES128" : "DES";
            cfg.V3PrivPassword = _txtV3PrivPass.Password;
            cfg.V3ContextName = string.IsNullOrEmpty(_txtV3Context.Text.Trim()) ? "EPSON" : _txtV3Context.Text.Trim();

            DialogResult = true;
            Close();
        }

        private Style CreateModernGroupBoxStyle(bool isDark)
        {
            var style = new Style(typeof(GroupBox));
            var template = new ControlTemplate(typeof(GroupBox));
            var grid = new FrameworkElementFactory(typeof(Grid));

            var row0 = new FrameworkElementFactory(typeof(RowDefinition));
            row0.SetValue(RowDefinition.HeightProperty, GridLength.Auto);
            var row1 = new FrameworkElementFactory(typeof(RowDefinition));
            row1.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
            grid.AppendChild(row0);
            grid.AppendChild(row1);

            // Header Container
            var headerBorder = new FrameworkElementFactory(typeof(Border));
            headerBorder.SetValue(Grid.RowProperty, 0);
            headerBorder.SetValue(Border.MarginProperty, new Thickness(2, 0, 0, 4));

            var headerCp = new FrameworkElementFactory(typeof(ContentPresenter));
            headerCp.SetValue(ContentPresenter.ContentSourceProperty, "Header");
            headerCp.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);
            headerCp.SetValue(System.Windows.Documents.TextElement.FontWeightProperty, FontWeights.SemiBold);
            headerCp.SetValue(System.Windows.Documents.TextElement.ForegroundProperty, new TemplateBindingExtension(GroupBox.ForegroundProperty));
            headerBorder.AppendChild(headerCp);
            grid.AppendChild(headerBorder);

            // Content Border
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Grid.RowProperty, 1);
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            border.SetValue(Border.BorderBrushProperty, new SolidColorBrush(isDark ? Color.FromRgb(65, 65, 65) : Color.FromRgb(200, 200, 200)));
            border.SetValue(Border.BackgroundProperty, new SolidColorBrush(isDark ? Color.FromRgb(34, 34, 36) : Color.FromRgb(250, 250, 250)));
            border.SetValue(Border.PaddingProperty, new TemplateBindingExtension(GroupBox.PaddingProperty));

            var contentCp = new FrameworkElementFactory(typeof(ContentPresenter));
            contentCp.SetValue(ContentPresenter.ContentSourceProperty, "Content");
            border.AppendChild(contentCp);
            grid.AppendChild(border);

            template.VisualTree = grid;
            style.Setters.Add(new Setter(GroupBox.TemplateProperty, template));
            style.Setters.Add(new Setter(GroupBox.ForegroundProperty, new SolidColorBrush(isDark ? Color.FromRgb(220, 220, 220) : Color.FromRgb(30, 30, 30))));
            return style;
        }

        private TextBox CreateModernTextBox()
        {
            var tb = new TextBox
            {
                Height = 24,
                Background = new SolidColorBrush(_boxBg),
                Foreground = new SolidColorBrush(_textColor),
                CaretBrush = new SolidColorBrush(_textColor),
                BorderBrush = new SolidColorBrush(_borderColor),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5, 2, 5, 2),
                VerticalContentAlignment = VerticalAlignment.Center,
                FontSize = 12
            };

            var style = new Style(typeof(TextBox));
            var template = new ControlTemplate(typeof(TextBox));
            var border = new FrameworkElementFactory(typeof(Border));
            border.Name = "border";
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4)); // Subtle modern rounding
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(TextBox.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(TextBox.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(TextBox.BorderThicknessProperty));

            var sv = new FrameworkElementFactory(typeof(ScrollViewer));
            sv.Name = "PART_ContentHost";
            border.AppendChild(sv);

            template.VisualTree = border;
            style.Setters.Add(new Setter(TextBox.TemplateProperty, template));
            tb.Style = style;

            return tb;
        }

        private PasswordBox CreateModernPasswordBox()
        {
            var pb = new PasswordBox
            {
                Height = 24,
                Background = new SolidColorBrush(_boxBg),
                Foreground = new SolidColorBrush(_textColor),
                CaretBrush = new SolidColorBrush(_textColor),
                BorderBrush = new SolidColorBrush(_borderColor),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5, 2, 5, 2),
                VerticalContentAlignment = VerticalAlignment.Center,
                FontSize = 12
            };

            var style = new Style(typeof(PasswordBox));
            var template = new ControlTemplate(typeof(PasswordBox));
            var border = new FrameworkElementFactory(typeof(Border));
            border.Name = "border";
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4)); // Subtle modern rounding
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(PasswordBox.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(PasswordBox.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(PasswordBox.BorderThicknessProperty));

            var sv = new FrameworkElementFactory(typeof(ScrollViewer));
            sv.Name = "PART_ContentHost";
            border.AppendChild(sv);

            template.VisualTree = border;
            style.Setters.Add(new Setter(PasswordBox.TemplateProperty, template));
            pb.Style = style;

            return pb;
        }

        private Button CreateModernButton(string text, double width, double height)
        {
            var btn = new Button
            {
                Content = text,
                Width = width,
                Height = height,
                FontSize = 12,
                Foreground = new SolidColorBrush(_textColor),
                Background = new SolidColorBrush(_btnBg),
                BorderBrush = new SolidColorBrush(_btnBorder),
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var style = new Style(typeof(Button));
            var template = new ControlTemplate(typeof(Button));
            var border = new FrameworkElementFactory(typeof(Border));
            border.Name = "btnBorder";
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4)); // Subtle modern rounding
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));

            var cp = new FrameworkElementFactory(typeof(ContentPresenter));
            cp.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(Button.ContentProperty));
            cp.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            cp.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(cp);

            template.VisualTree = border;

            var hover = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            hover.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(_btnHoverBg), "btnBorder"));
            hover.Setters.Add(new Setter(Border.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(0, 120, 215)), "btnBorder"));
            template.Triggers.Add(hover);

            var pressed = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressed.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(_btnPressedBg), "btnBorder"));
            template.Triggers.Add(pressed);

            style.Setters.Add(new Setter(Button.TemplateProperty, template));
            btn.Style = style;

            return btn;
        }
    }
}
