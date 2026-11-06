using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace EpsonMassReboot
{
    public class MainWindow : Window
    {
        private bool _isDarkMode = true; // DEFAULT: DARK MODE

        // Theme Palette Colors
        private Color _winBg;
        private Color _contentBg;
        private Color _boxBg;
        private Color _borderColor;
        private Color _textColor;
        private Color _secondaryTextColor;
        private Color _statusBarBg;
        private Color _statusBarBorder;
        private Color _statusTextColor;
        private Color _btnBg;
        private Color _btnBorder;
        private Color _btnHoverBg;
        private Color _btnPressedBg;
        private Color _rebootBg;
        private Color _rebootText;
        private Color _rebootBorder;

        // Controls
        private Grid _mainGrid;
        private Button _btnSettings;
        private TextBlock _lblLang;
        private RadioButton _rbLangEn;
        private RadioButton _rbLangDe;

        // Tabs
        private TabControl _tabControl;
        private TabItem _tabSpecific;
        private TabItem _tabRange;

        // Specific Tab
        private GroupBox _grpSpecific;
        private TextBox[] _txtSpecificIps = new TextBox[5];
        private TextBlock[] _lblSpecificIps = new TextBlock[5];

        // Range / Subnet Tab
        private GroupBox _grpScanMode;
        private RadioButton _rbRange;
        private RadioButton _rbSubnet;

        private GroupBox _grpRangeParams;
        private Grid _gridRangeInputs;
        private Grid _gridSubnetInputs;
        private TextBlock _lblStartIp;
        private TextBlock _lblEndIp;
        private TextBox _txtStartIp;
        private TextBox _txtEndIp;

        private TextBlock _lblSubnetStartIp;
        private TextBlock _lblSubnetEndIp;
        private TextBox _txtSubnetStartIp;
        private TextBox _txtSubnetEndIp;
        private TextBlock _lblSubnetMask;
        private TextBox _txtSubnetMask;
        private TextBlock _lblSubnetHint;
        private TextBlock _lblRangeNote;

        // Bottom Controls
        private Button _btnThemeToggle;
        private Button _btnReboot;
        private StatusBar _statusBar;
        private TextBlock _tbStatus;
        private ProgressBar _progressBar;

        private bool _isRunning = false;

        public MainWindow()
        {
            InitializePalette();
            InitializeWindow();
            BuildUi();
            ApplyLanguage();
            ApplyTheme();
        }

        private void InitializePalette()
        {
            if (_isDarkMode)
            {
                _winBg = Color.FromRgb(32, 32, 32);             // #202020
                _contentBg = Color.FromRgb(37, 37, 38);         // #252526
                _boxBg = Color.FromRgb(28, 28, 28);             // #1C1C1C
                _borderColor = Color.FromRgb(65, 65, 65);       // #414141
                _textColor = Color.FromRgb(230, 230, 230);      // #E6E6E6
                _secondaryTextColor = Color.FromRgb(160, 160, 160);
                _statusBarBg = Color.FromRgb(24, 24, 24);       // #181818
                _statusBarBorder = Color.FromRgb(45, 45, 45);
                _statusTextColor = Color.FromRgb(210, 210, 210);
                _btnBg = Color.FromRgb(48, 48, 48);
                _btnBorder = Color.FromRgb(70, 70, 70);
                _btnHoverBg = Color.FromRgb(62, 62, 62);
                _btnPressedBg = Color.FromRgb(0, 120, 215);
                _rebootBg = Color.FromRgb(0, 120, 215);         // Subtle blue accent for dark mode
                _rebootText = Colors.White;
                _rebootBorder = Color.FromRgb(0, 90, 165);
            }
            else
            {
                _winBg = Color.FromRgb(240, 240, 240);          // #F0F0F0
                _contentBg = Color.FromRgb(245, 245, 245);
                _boxBg = Color.FromRgb(255, 255, 255);          // #FFFFFF
                _borderColor = Color.FromRgb(170, 170, 170);    // #AAAAAA
                _textColor = Color.FromRgb(0, 0, 0);            // #000000
                _secondaryTextColor = Color.FromRgb(90, 90, 90);
                _statusBarBg = Color.FromRgb(230, 230, 230);
                _statusBarBorder = Color.FromRgb(200, 200, 200);
                _statusTextColor = Color.FromRgb(30, 30, 30);
                _btnBg = Color.FromRgb(225, 225, 225);
                _btnBorder = Color.FromRgb(173, 173, 173);
                _btnHoverBg = Color.FromRgb(229, 241, 251);
                _btnPressedBg = Color.FromRgb(204, 228, 247);
                _rebootBg = Color.FromRgb(225, 225, 225);
                _rebootText = Colors.Black;
                _rebootBorder = Color.FromRgb(0, 120, 215);
            }
        }

        private void InitializeWindow()
        {
            Title = "Printer reboot tool";
            Width = 540;
            Height = 475;
            MinWidth = 510;
            MinHeight = 450;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(_winBg);
            Foreground = new SolidColorBrush(_textColor);
            FontFamily = new FontFamily("Segoe UI, Tahoma, Arial");
            FontSize = 12;

            try
            {
                if (System.IO.File.Exists("app.ico"))
                {
                    Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri(System.IO.Path.GetFullPath("app.ico")));
                }
                else
                {
                    using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location))
                    {
                        if (icon != null)
                        {
                            using (var bmp = icon.ToBitmap())
                            {
                                var hBmp = bmp.GetHbitmap();
                                Icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                    hBmp, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void BuildUi()
        {
            _mainGrid = new Grid();
            _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Toolbar / Top strip (0)
            _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Tabs (1)
            _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Action Row (2)
            _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Status Bar (3)

            // ================= 0: TOP TOOLBAR =================
            var topPanel = new Grid { Margin = new Thickness(10, 8, 10, 6) };
            topPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            topPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Left: Settings Button
            _btnSettings = CreateModernButton(Localization.Get("BtnSettings"), 155, 26);
            _btnSettings.Click += (s, e) => OpenSettings();
            Grid.SetColumn(_btnSettings, 0);
            topPanel.Children.Add(_btnSettings);

            // Right: Language Selector
            var langStack = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            _lblLang = new TextBlock
            {
                Text = Localization.Get("LangLabel"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0),
                Foreground = new SolidColorBrush(_secondaryTextColor)
            };
            _rbLangEn = new RadioButton
            {
                Content = "English",
                IsChecked = true,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(_textColor)
            };
            _rbLangDe = new RadioButton
            {
                Content = "Deutsch",
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(_textColor)
            };
            _rbLangEn.Checked += (s, e) => SwitchLanguage(AppLanguage.English);
            _rbLangDe.Checked += (s, e) => SwitchLanguage(AppLanguage.German);

            langStack.Children.Add(_lblLang);
            langStack.Children.Add(_rbLangEn);
            langStack.Children.Add(_rbLangDe);
            Grid.SetColumn(langStack, 2);
            topPanel.Children.Add(langStack);

            Grid.SetRow(topPanel, 0);
            _mainGrid.Children.Add(topPanel);

            // ================= 1: TAB CONTROL =================
            _tabControl = new TabControl { Margin = new Thickness(10, 0, 10, 8) };

            // Tab 1: Specific IPs
            _tabSpecific = new TabItem { Header = Localization.Get("TabSpecific") };
            _tabSpecific.Content = BuildSpecificIpsContent();
            _tabControl.Items.Add(_tabSpecific);

            // Tab 2: Range / Subnet
            _tabRange = new TabItem { Header = Localization.Get("TabRange") };
            _tabRange.Content = BuildRangeSubnetContent();
            _tabControl.Items.Add(_tabRange);

            Grid.SetRow(_tabControl, 1);
            _mainGrid.Children.Add(_tabControl);

            // ================= 2: ACTION ROW (Theme Toggle on Bottom-Left, Reboot on Bottom-Right) =================
            var actionGrid = new Grid { Margin = new Thickness(10, 0, 10, 8) };
            actionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Left: Theme Toggle
            actionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Spacer
            actionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Right: Reboot

            // Bottom-Left: Dark/Light Mode Toggle
            _btnThemeToggle = CreateModernButton(_isDarkMode ? "☼ " + Localization.Get("ThemeLight") : "🌙 " + Localization.Get("ThemeDark"), 120, 26);
            _btnThemeToggle.FontSize = 11;
            _btnThemeToggle.Click += (s, e) => ToggleTheme();
            Grid.SetColumn(_btnThemeToggle, 0);
            actionGrid.Children.Add(_btnThemeToggle);

            // Bottom-Right: Reboot Button
            _btnReboot = CreateModernButton(Localization.Get("BtnReboot"), 140, 28, true);
            _btnReboot.FontWeight = FontWeights.Bold;
            _btnReboot.Click += async (s, e) => await OnRebootClicked();
            Grid.SetColumn(_btnReboot, 2);
            actionGrid.Children.Add(_btnReboot);

            Grid.SetRow(actionGrid, 2);
            _mainGrid.Children.Add(actionGrid);

            // ================= 3: STATUS BAR =================
            _statusBar = new StatusBar
            {
                Background = new SolidColorBrush(_statusBarBg),
                BorderBrush = new SolidColorBrush(_statusBarBorder),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Height = 24
            };

            var statusItemText = new StatusBarItem { HorizontalContentAlignment = HorizontalAlignment.Left };
            _tbStatus = new TextBlock
            {
                Text = Localization.Get("StatusReady"),
                FontSize = 11,
                Foreground = new SolidColorBrush(_statusTextColor),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 0, 0)
            };
            statusItemText.Content = _tbStatus;
            _statusBar.Items.Add(statusItemText);

            var statusItemProgress = new StatusBarItem { HorizontalAlignment = HorizontalAlignment.Right };
            _progressBar = new ProgressBar
            {
                Width = 140,
                Height = 14,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 0, 6, 0)
            };
            statusItemProgress.Content = _progressBar;
            _statusBar.Items.Add(statusItemProgress);

            Grid.SetRow(_statusBar, 3);
            _mainGrid.Children.Add(_statusBar);

            Content = _mainGrid;
        }

        private FrameworkElement BuildSpecificIpsContent()
        {
            var panel = new StackPanel { Margin = new Thickness(8) };

            _grpSpecific = new GroupBox
            {
                Header = Localization.Get("GrpTargets"),
                Padding = new Thickness(10, 8, 10, 8),
                BorderThickness = new Thickness(1)
            };

            var stackRows = new StackPanel();
            for (int i = 0; i < 5; i++)
            {
                var rowGrid = new Grid { Margin = new Thickness(0, i == 0 ? 0 : 5, 0, 0) };
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                _lblSpecificIps[i] = new TextBlock
                {
                    Text = Localization.Get("IpLabel", i + 1),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(_lblSpecificIps[i], 0);
                rowGrid.Children.Add(_lblSpecificIps[i]);

                _txtSpecificIps[i] = CreateModernTextBox();
                Grid.SetColumn(_txtSpecificIps[i], 1);
                rowGrid.Children.Add(_txtSpecificIps[i]);

                stackRows.Children.Add(rowGrid);
            }

            _grpSpecific.Content = stackRows;
            panel.Children.Add(_grpSpecific);
            return panel;
        }

        private FrameworkElement BuildRangeSubnetContent()
        {
            var panel = new StackPanel { Margin = new Thickness(8) };

            // Group 1: Scan Mode Selection
            _grpScanMode = new GroupBox
            {
                Header = Localization.Get("GrpScanMode"),
                Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(0, 0, 0, 8),
                BorderThickness = new Thickness(1)
            };
            var modeStack = new StackPanel { Orientation = Orientation.Horizontal };
            _rbRange = new RadioButton
            {
                Content = Localization.Get("ModeRange"),
                IsChecked = true,
                Margin = new Thickness(0, 0, 20, 0)
            };
            _rbRange.Checked += (s, e) => UpdateRangeMode();

            _rbSubnet = new RadioButton
            {
                Content = Localization.Get("ModeSubnet"),
                IsChecked = false
            };
            _rbSubnet.Checked += (s, e) => UpdateRangeMode();

            modeStack.Children.Add(_rbRange);
            modeStack.Children.Add(_rbSubnet);
            _grpScanMode.Content = modeStack;
            panel.Children.Add(_grpScanMode);

            // Group 2: Parameters
            _grpRangeParams = new GroupBox
            {
                Header = Localization.Get("GrpRangeParams"),
                Padding = new Thickness(10, 8, 10, 8),
                BorderThickness = new Thickness(1)
            };
            var paramStack = new StackPanel();

            // Mode A: Standard Range Inputs
            _gridRangeInputs = new Grid { Margin = new Thickness(0, 0, 0, 6) };
            _gridRangeInputs.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _gridRangeInputs.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            _gridRangeInputs.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var startPanel = new StackPanel();
            _lblStartIp = new TextBlock { Text = Localization.Get("StartIp"), Margin = new Thickness(0, 0, 0, 3) };
            _txtStartIp = CreateModernTextBox();
            startPanel.Children.Add(_lblStartIp);
            startPanel.Children.Add(_txtStartIp);
            Grid.SetColumn(startPanel, 0);
            _gridRangeInputs.Children.Add(startPanel);

            var endPanel = new StackPanel();
            _lblEndIp = new TextBlock { Text = Localization.Get("EndIp"), Margin = new Thickness(0, 0, 0, 3) };
            _txtEndIp = CreateModernTextBox();
            endPanel.Children.Add(_lblEndIp);
            endPanel.Children.Add(_txtEndIp);
            Grid.SetColumn(endPanel, 2);
            _gridRangeInputs.Children.Add(endPanel);

            paramStack.Children.Add(_gridRangeInputs);

            // Mode B: Subnet / VLAN Mode Inputs
            _gridSubnetInputs = new Grid { Margin = new Thickness(0, 0, 0, 6), Visibility = Visibility.Collapsed };
            var subStack = new StackPanel();

            var subIpRow = new Grid { Margin = new Thickness(0, 0, 0, 6) };
            subIpRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            subIpRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            subIpRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var subStart = new StackPanel();
            _lblSubnetStartIp = new TextBlock { Text = Localization.Get("StartIp"), Margin = new Thickness(0, 0, 0, 3) };
            _txtSubnetStartIp = CreateModernTextBox();
            _txtSubnetStartIp.TextChanged += (s, e) => TryAutoSplitSubnetInput();
            subStart.Children.Add(_lblSubnetStartIp);
            subStart.Children.Add(_txtSubnetStartIp);
            Grid.SetColumn(subStart, 0);
            subIpRow.Children.Add(subStart);

            var subEnd = new StackPanel();
            _lblSubnetEndIp = new TextBlock { Text = Localization.Get("EndIp"), Margin = new Thickness(0, 0, 0, 3) };
            _txtSubnetEndIp = CreateModernTextBox();
            subEnd.Children.Add(_lblSubnetEndIp);
            subEnd.Children.Add(_txtSubnetEndIp);
            Grid.SetColumn(subEnd, 2);
            subIpRow.Children.Add(subEnd);
            subStack.Children.Add(subIpRow);

            var maskPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 4) };
            _lblSubnetMask = new TextBlock { Text = Localization.Get("SubnetMask"), Margin = new Thickness(0, 0, 0, 3) };
            _txtSubnetMask = CreateModernTextBox();
            _txtSubnetMask.Text = "255.255.255.0";
            maskPanel.Children.Add(_lblSubnetMask);
            maskPanel.Children.Add(_txtSubnetMask);
            subStack.Children.Add(maskPanel);

            _lblSubnetHint = new TextBlock
            {
                Text = Localization.Get("SubnetHint"),
                FontSize = 11,
                Foreground = new SolidColorBrush(_secondaryTextColor),
                Margin = new Thickness(0, 2, 0, 4),
                TextWrapping = TextWrapping.Wrap
            };
            subStack.Children.Add(_lblSubnetHint);

            _gridSubnetInputs.Children.Add(subStack);
            paramStack.Children.Add(_gridSubnetInputs);

            // Note
            _lblRangeNote = new TextBlock
            {
                Text = Localization.Get("RangeNote"),
                FontSize = 11,
                Foreground = new SolidColorBrush(_secondaryTextColor),
                Margin = new Thickness(0, 4, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            paramStack.Children.Add(_lblRangeNote);

            _grpRangeParams.Content = paramStack;
            panel.Children.Add(_grpRangeParams);

            return panel;
        }

        private void TryAutoSplitSubnetInput()
        {
            if (_txtSubnetStartIp == null || _txtSubnetEndIp == null || _txtSubnetMask == null) return;
            string t = _txtSubnetStartIp.Text.Trim();
            if (string.IsNullOrEmpty(t)) return;

            int dash = t.IndexOf('-');
            if (dash > 0 && string.IsNullOrEmpty(_txtSubnetEndIp.Text.Trim()))
            {
                string pStart = t.Substring(0, dash).Trim();
                string rest = t.Substring(dash + 1).Trim();
                string pEnd = rest;
                string pMask = "";

                string[] keys = new string[] { "subnet", "Subnet", "SUBNET", "mask", "Mask", "MASK", "/" };
                foreach (var k in keys)
                {
                    int ki = rest.IndexOf(k, StringComparison.OrdinalIgnoreCase);
                    if (ki >= 0)
                    {
                        pEnd = rest.Substring(0, ki).Trim();
                        pMask = rest.Substring(ki + k.Length).Trim();
                        break;
                    }
                }
                if (string.IsNullOrEmpty(pMask))
                {
                    string[] tokens = rest.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length >= 2)
                    {
                        pEnd = tokens[0].Trim();
                        pMask = tokens[1].Trim();
                    }
                }

                IPAddress d1, d2;
                if (IPAddress.TryParse(pStart, out d1) && IPAddress.TryParse(pEnd, out d2))
                {
                    _txtSubnetStartIp.Text = pStart;
                    _txtSubnetEndIp.Text = pEnd;
                    if (!string.IsNullOrEmpty(pMask))
                    {
                        _txtSubnetMask.Text = pMask;
                    }
                }
            }
        }

        private void UpdateRangeMode()
        {
            if (_rbRange.IsChecked == true)
            {
                _gridRangeInputs.Visibility = Visibility.Visible;
                _gridSubnetInputs.Visibility = Visibility.Collapsed;
            }
            else
            {
                _gridRangeInputs.Visibility = Visibility.Collapsed;
                _gridSubnetInputs.Visibility = Visibility.Visible;
            }
        }

        private void ToggleTheme()
        {
            _isDarkMode = !_isDarkMode;
            InitializePalette();
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            var winBrush = new SolidColorBrush(_winBg);
            var contentBrush = new SolidColorBrush(_contentBg);
            var textBrush = new SolidColorBrush(_textColor);
            var secTextBrush = new SolidColorBrush(_secondaryTextColor);
            var borderBrush = new SolidColorBrush(_borderColor);

            Background = winBrush;
            Foreground = textBrush;
            if (_mainGrid != null) _mainGrid.Background = winBrush;

            // TabControl & TabItems Style
            _tabControl.Style = CreateTabControlStyle(_isDarkMode);
            _tabSpecific.Style = CreateTabItemStyle(_isDarkMode);
            _tabRange.Style = CreateTabItemStyle(_isDarkMode);

            // Header & Language
            _lblLang.Foreground = secTextBrush;
            _rbLangEn.Foreground = textBrush;
            _rbLangDe.Foreground = textBrush;

            // GroupBoxes
            if (_grpSpecific != null) _grpSpecific.Style = CreateModernGroupBoxStyle(_isDarkMode);
            if (_grpScanMode != null) _grpScanMode.Style = CreateModernGroupBoxStyle(_isDarkMode);
            if (_grpRangeParams != null) _grpRangeParams.Style = CreateModernGroupBoxStyle(_isDarkMode);

            // Radio Buttons
            if (_rbRange != null) _rbRange.Foreground = textBrush;
            if (_rbSubnet != null) _rbSubnet.Foreground = textBrush;

            // Labels
            for (int i = 0; i < 5; i++)
            {
                if (_lblSpecificIps[i] != null) _lblSpecificIps[i].Foreground = textBrush;
            }
            if (_lblStartIp != null) _lblStartIp.Foreground = textBrush;
            if (_lblEndIp != null) _lblEndIp.Foreground = textBrush;
            if (_lblSubnetStartIp != null) _lblSubnetStartIp.Foreground = textBrush;
            if (_lblSubnetEndIp != null) _lblSubnetEndIp.Foreground = textBrush;
            if (_lblSubnetMask != null) _lblSubnetMask.Foreground = textBrush;
            if (_lblSubnetHint != null) _lblSubnetHint.Foreground = secTextBrush;
            if (_lblRangeNote != null) _lblRangeNote.Foreground = secTextBrush;

            // TextBoxes
            for (int i = 0; i < 5; i++)
            {
                if (_txtSpecificIps[i] != null) UpdateTextBoxTheme(_txtSpecificIps[i]);
            }
            if (_txtStartIp != null) UpdateTextBoxTheme(_txtStartIp);
            if (_txtEndIp != null) UpdateTextBoxTheme(_txtEndIp);
            if (_txtSubnetStartIp != null) UpdateTextBoxTheme(_txtSubnetStartIp);
            if (_txtSubnetEndIp != null) UpdateTextBoxTheme(_txtSubnetEndIp);
            if (_txtSubnetMask != null) UpdateTextBoxTheme(_txtSubnetMask);

            // Buttons
            if (_btnSettings != null) _btnSettings.Style = CreateModernButtonStyle(_isDarkMode, false);
            if (_btnThemeToggle != null)
            {
                _btnThemeToggle.Style = CreateModernButtonStyle(_isDarkMode, false);
                _btnThemeToggle.Content = _isDarkMode ? "☼ " + Localization.Get("ThemeLight") : "🌙 " + Localization.Get("ThemeDark");
            }
            if (_btnReboot != null) _btnReboot.Style = CreateModernButtonStyle(_isDarkMode, true);

            // Status Bar
            if (_statusBar != null)
            {
                _statusBar.Background = new SolidColorBrush(_statusBarBg);
                _statusBar.BorderBrush = new SolidColorBrush(_statusBarBorder);
            }
            if (_tbStatus != null)
            {
                _tbStatus.Foreground = new SolidColorBrush(_statusTextColor);
            }
        }

        private void UpdateTextBoxTheme(TextBox tb)
        {
            tb.Background = new SolidColorBrush(_boxBg);
            tb.Foreground = new SolidColorBrush(_textColor);
            tb.CaretBrush = new SolidColorBrush(_textColor);
            tb.BorderBrush = new SolidColorBrush(_borderColor);
        }

        private void OpenSettings()
        {
            var dlg = new SettingsWindow(this, _isDarkMode);
            dlg.ShowDialog();
        }

        private void SwitchLanguage(AppLanguage lang)
        {
            Localization.CurrentLanguage = lang;
            _rbLangEn.IsChecked = (lang == AppLanguage.English);
            _rbLangDe.IsChecked = (lang == AppLanguage.German);
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            Title = Localization.Get("AppTitle");
            _btnSettings.Content = Localization.Get("BtnSettings");
            _lblLang.Text = Localization.Get("LangLabel");

            _tabSpecific.Header = Localization.Get("TabSpecific");
            _tabRange.Header = Localization.Get("TabRange");

            _grpSpecific.Header = Localization.Get("GrpTargets");
            for (int i = 0; i < 5; i++)
            {
                _lblSpecificIps[i].Text = Localization.Get("IpLabel", i + 1);
            }

            _grpScanMode.Header = Localization.Get("GrpScanMode");
            _rbRange.Content = Localization.Get("ModeRange");
            _rbSubnet.Content = Localization.Get("ModeSubnet");

            _grpRangeParams.Header = Localization.Get("GrpRangeParams");
            _lblStartIp.Text = Localization.Get("StartIp");
            _lblEndIp.Text = Localization.Get("EndIp");

            _lblSubnetStartIp.Text = Localization.Get("StartIp");
            _lblSubnetEndIp.Text = Localization.Get("EndIp");
            _lblSubnetMask.Text = Localization.Get("SubnetMask");
            _lblSubnetHint.Text = Localization.Get("SubnetHint");
            _lblRangeNote.Text = Localization.Get("RangeNote");

            _btnThemeToggle.Content = _isDarkMode ? "☼ " + Localization.Get("ThemeLight") : "🌙 " + Localization.Get("ThemeDark");

            if (!_isRunning)
            {
                _btnReboot.Content = Localization.Get("BtnReboot");
                _tbStatus.Text = Localization.Get("StatusReady");
            }
            else
            {
                _btnReboot.Content = Localization.Get("BtnRunning");
            }
        }

        private async Task OnRebootClicked()
        {
            if (_isRunning) return;

            _tbStatus.Text = "";

            var targets = new List<IPAddress>();
            bool isRangeTab = (_tabControl.SelectedItem == _tabRange);

            if (!isRangeTab)
            {
                // Tab 1: Specific IPs
                for (int i = 0; i < 5; i++)
                {
                    string text = _txtSpecificIps[i].Text.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        IPAddress ip;
                        if (IPAddress.TryParse(text, out ip))
                        {
                            targets.Add(ip);
                        }
                    }
                }

                if (targets.Count == 0)
                {
                    _tbStatus.Text = Localization.Get("StatusNoIps");
                    return;
                }
            }
            else
            {
                // Tab 2: Range or Subnet
                if (_rbRange.IsChecked == true)
                {
                    string start = _txtStartIp.Text.Trim();
                    string end = _txtEndIp.Text.Trim();
                    targets = SnmpEngine.ParseIpRange(start, end);
                }
                else
                {
                    string subStart = _txtSubnetStartIp.Text.Trim();
                    string subEnd = _txtSubnetEndIp.Text.Trim();
                    string subMask = _txtSubnetMask.Text.Trim();
                    targets = SnmpEngine.ParseRangeWithSubnet(subStart, subEnd, subMask);
                }

                if (targets.Count == 0)
                {
                    _tbStatus.Text = Localization.Get("StatusInvalidRange");
                    return;
                }
            }

            var cfg = SnmpConfig.Current;
            SetUiRunningState(true);

            int totalCount = targets.Count;
            int dispatchedCount = 0;
            int rebootCount = 0;
            var detectedList = new List<string>();

            // Setup real-time progress bar in status bar
            _progressBar.IsIndeterminate = false;
            _progressBar.Minimum = 0;
            _progressBar.Maximum = totalCount;
            _progressBar.Value = 0;
            _progressBar.Visibility = Visibility.Visible;

            _tbStatus.Text = Localization.Get("StatusScanning", targets[0].ToString(), 1, totalCount);

            try
            {
                bool verifyEpson = isRangeTab; // Range/subnet mode verifies Epson before reboot

                await Task.Run(async () =>
                {
                    var tasks = new List<Task<SnmpTargetResult>>();
                    using (var semaphore = new System.Threading.SemaphoreSlim(20))
                    {
                        foreach (var ip in targets)
                        {
                            var targetIp = ip;
                            await semaphore.WaitAsync();

                            tasks.Add(Task.Run(async () =>
                            {
                                int currentIdx = Interlocked.Increment(ref dispatchedCount);

                                // Show current scanning IP in real time
                                var op1 = Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    _tbStatus.Text = Localization.Get("StatusScanning", targetIp.ToString(), currentIdx, totalCount);
                                    _progressBar.Value = currentIdx;
                                }));

                                try
                                {
                                    var res = await SnmpEngine.ProbeAndRebootAsync(targetIp, cfg, verifyEpson);
                                    if (res.RebootSent)
                                    {
                                        var op2 = Dispatcher.BeginInvoke(new Action(() =>
                                        {
                                            _tbStatus.Text = Localization.Get("StatusRebootSentTo", targetIp.ToString());
                                        }));
                                    }
                                    return res;
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }));
                        }

                        var results = await Task.WhenAll(tasks);
                        foreach (var res in results)
                        {
                            if (res.RebootSent)
                            {
                                rebootCount++;
                                if (!string.IsNullOrEmpty(res.Model))
                                {
                                    detectedList.Add(res.IP + " (" + res.Model + ")");
                                }
                            }
                        }
                    }
                });

                _progressBar.Value = totalCount;
                if (rebootCount > 0)
                {
                    if (detectedList.Count > 0 && detectedList.Count <= 2)
                    {
                        _tbStatus.Text = Localization.Get("StatusCompletedDetails", rebootCount, string.Join(", ", detectedList.ToArray()));
                    }
                    else
                    {
                        _tbStatus.Text = Localization.Get("StatusCompleted", rebootCount);
                    }
                }
                else
                {
                    _tbStatus.Text = Localization.Get("StatusNoneFound");
                }
            }
            catch (Exception ex)
            {
                _tbStatus.Text = "Error: " + ex.Message;
            }
            finally
            {
                SetUiRunningState(false);
            }
        }

        private void SetUiRunningState(bool running)
        {
            _isRunning = running;

            _tabControl.IsEnabled = !running;
            _btnSettings.IsEnabled = !running;
            _rbLangEn.IsEnabled = !running;
            _rbLangDe.IsEnabled = !running;
            _btnThemeToggle.IsEnabled = !running;
            _btnReboot.IsEnabled = !running;

            if (running)
            {
                _btnReboot.Content = Localization.Get("BtnRunning");
            }
            else
            {
                _btnReboot.Content = Localization.Get("BtnReboot");
                _progressBar.Visibility = Visibility.Collapsed;
            }
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

        private Button CreateModernButton(string text, double width, double height, bool isReboot = false)
        {
            var btn = new Button
            {
                Content = text,
                Width = width,
                Height = height,
                FontSize = 12,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            btn.Style = CreateModernButtonStyle(_isDarkMode, isReboot);
            return btn;
        }

        private Style CreateModernButtonStyle(bool isDark, bool isReboot)
        {
            var style = new Style(typeof(Button));
            var template = new ControlTemplate(typeof(Button));
            var border = new FrameworkElementFactory(typeof(Border));
            border.Name = "btnBorder";
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4)); // Subtle modern rounding

            Color bg = isReboot ? _rebootBg : _btnBg;
            Color fg = isReboot ? _rebootText : _textColor;
            Color bc = isReboot ? _rebootBorder : _btnBorder;

            border.SetValue(Border.BackgroundProperty, new SolidColorBrush(bg));
            border.SetValue(Border.BorderBrushProperty, new SolidColorBrush(bc));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));

            var cp = new FrameworkElementFactory(typeof(ContentPresenter));
            cp.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(Button.ContentProperty));
            cp.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            cp.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(cp);

            template.VisualTree = border;

            var hover = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            hover.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(isReboot ? Color.FromRgb(20, 135, 230) : _btnHoverBg), "btnBorder"));
            hover.Setters.Add(new Setter(Border.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(0, 120, 215)), "btnBorder"));
            template.Triggers.Add(hover);

            var pressed = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressed.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(isReboot ? Color.FromRgb(0, 95, 175) : _btnPressedBg), "btnBorder"));
            template.Triggers.Add(pressed);

            var disabled = new Trigger { Property = Button.IsEnabledProperty, Value = false };
            disabled.Setters.Add(new Setter(Button.OpacityProperty, 0.5));
            template.Triggers.Add(disabled);

            style.Setters.Add(new Setter(Button.TemplateProperty, template));
            style.Setters.Add(new Setter(Button.ForegroundProperty, new SolidColorBrush(fg)));
            return style;
        }

        private Style CreateTabControlStyle(bool isDark)
        {
            var style = new Style(typeof(TabControl));
            var template = new ControlTemplate(typeof(TabControl));
            var grid = new FrameworkElementFactory(typeof(Grid));

            var row0 = new FrameworkElementFactory(typeof(RowDefinition));
            row0.SetValue(RowDefinition.HeightProperty, GridLength.Auto);
            var row1 = new FrameworkElementFactory(typeof(RowDefinition));
            row1.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
            grid.AppendChild(row0);
            grid.AppendChild(row1);

            var tabHeaderPanel = new FrameworkElementFactory(typeof(TabPanel));
            tabHeaderPanel.SetValue(Grid.RowProperty, 0);
            tabHeaderPanel.SetValue(TabPanel.IsItemsHostProperty, true);
            tabHeaderPanel.SetValue(TabPanel.MarginProperty, new Thickness(2, 2, 2, 0));
            grid.AppendChild(tabHeaderPanel);

            var contentBorder = new FrameworkElementFactory(typeof(Border));
            contentBorder.SetValue(Grid.RowProperty, 1);
            contentBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(0, 4, 4, 4));
            contentBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            contentBorder.SetValue(Border.BorderBrushProperty, new SolidColorBrush(isDark ? Color.FromRgb(65, 65, 65) : Color.FromRgb(180, 180, 180)));
            contentBorder.SetValue(Border.BackgroundProperty, new SolidColorBrush(isDark ? Color.FromRgb(37, 37, 38) : Color.FromRgb(245, 245, 245)));

            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.ContentSourceProperty, "SelectedContent");
            contentBorder.AppendChild(contentPresenter);

            grid.AppendChild(contentBorder);
            template.VisualTree = grid;
            style.Setters.Add(new Setter(TabControl.TemplateProperty, template));
            return style;
        }

        private Style CreateTabItemStyle(bool isDark)
        {
            var style = new Style(typeof(TabItem));
            var template = new ControlTemplate(typeof(TabItem));
            var border = new FrameworkElementFactory(typeof(Border));
            border.Name = "tabBorder";
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4, 4, 0, 0));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1, 1, 1, 0));
            border.SetValue(Border.MarginProperty, new Thickness(0, 0, 4, 0));
            border.SetValue(Border.PaddingProperty, new Thickness(12, 5, 12, 5));

            var cp = new FrameworkElementFactory(typeof(ContentPresenter));
            cp.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(TabItem.HeaderProperty));
            cp.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            cp.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(cp);

            template.VisualTree = border;

            // Default state
            border.SetValue(Border.BackgroundProperty, new SolidColorBrush(isDark ? Color.FromRgb(45, 45, 48) : Color.FromRgb(230, 230, 230)));
            border.SetValue(Border.BorderBrushProperty, new SolidColorBrush(isDark ? Color.FromRgb(65, 65, 65) : Color.FromRgb(180, 180, 180)));

            // Selected trigger
            var trigSelected = new Trigger { Property = TabItem.IsSelectedProperty, Value = true };
            trigSelected.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(isDark ? Color.FromRgb(37, 37, 38) : Color.FromRgb(245, 245, 245)), "tabBorder"));
            trigSelected.Setters.Add(new Setter(Border.BorderBrushProperty, new SolidColorBrush(isDark ? Color.FromRgb(85, 85, 85) : Color.FromRgb(160, 160, 160)), "tabBorder"));
            template.Triggers.Add(trigSelected);

            // Hover trigger
            var trigHover = new Trigger { Property = TabItem.IsMouseOverProperty, Value = true };
            trigHover.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(isDark ? Color.FromRgb(55, 55, 58) : Color.FromRgb(238, 238, 238)), "tabBorder"));
            template.Triggers.Add(trigHover);

            style.Setters.Add(new Setter(TabItem.TemplateProperty, template));
            style.Setters.Add(new Setter(TabItem.ForegroundProperty, new SolidColorBrush(isDark ? Color.FromRgb(230, 230, 230) : Color.FromRgb(0, 0, 0))));
            return style;
        }
    }
}
