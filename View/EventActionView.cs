namespace VSOnEventAction.View
{
   using Microsoft.Win32;

   using System;
   using System.Drawing;
   using System.Linq;
   using System.Runtime.InteropServices;
   using System.Windows.Forms;

   using VSOnEventAction.Presenter;

   public sealed class EventActionView : Form, IEventActionView
   {
      private const int DwmwaUseImmersiveDarkMode = 20;

      private Button _btnAddEvent;

      private Button _btnDelete;

      private Button _btnParam1;

      private Button _btnParam2;

      private Button _btnRename;

      private Button _btnSaveConfig;

      // Controls for event configuration
      private ComboBox _cbxEventTrigger;

      private ComboBox _cbxEventType;

      private EventActionPresenter _presenter;

      private Label _lblAllowedExtensions; // New label

      private Label _lblEventTrigger;

      private Label _lblEventType;

      private Label _lblParam1;

      private Label _lblParam2;

      // List and buttons for managing saved events
      private ListView _lvEvents;

      private TextBox _txtAllowedExtensions; // New control

      private TextBox _txtParam1;

      private TextBox _txtParam2;

      public EventActionView()
      {
         SetupControls();
         ApplyDynamicTheme();
         // Lock window size.
         FormBorderStyle = FormBorderStyle.FixedSingle;
         MaximizeBox = false;
         Size = new Size(600, 500);
         Text = "Event Configuration";
         _presenter = new EventActionPresenter(this);
         _presenter.LoadSavedEvents();
         UpdateActionButtons(false);
      }

      public event EventHandler AddEventClicked;

      public event EventHandler DeleteEventClicked;

      public event ItemCheckedEventHandler EventItemChecked;

      public event EventHandler EventTypeChanged;

      public event EventHandler RenameEventClicked;

      public event EventHandler SaveConfigClicked;

      public event EventHandler SelectedEventChanged;

      public string AllowedExtensions
      {
         get => _txtAllowedExtensions.Text;
         set => _txtAllowedExtensions.Text = value;
      }

      public ListView.ListViewItemCollection EventItems => _lvEvents.Items;

      public string EventTrigger
      {
         get => _cbxEventTrigger.SelectedItem != null ? _cbxEventTrigger.SelectedItem.ToString() : "";
         set
         {
            if (_cbxEventTrigger.Items.Contains(value))
            {
               _cbxEventTrigger.SelectedItem = value;
            }
            else
            {
               _cbxEventTrigger.SelectedIndex = -1;
            }
         }
      }

      public string EventType
      {
         get => _cbxEventType.SelectedItem != null ? _cbxEventType.SelectedItem.ToString() : "";
         set
         {
            if (_cbxEventType.Items.Contains(value))
            {
               _cbxEventType.SelectedItem = value;
            }
            else
            {
               _cbxEventType.SelectedIndex = -1;
            }
         }
      }

      public string Param1
      {
         get => _txtParam1.Text;
         set => _txtParam1.Text = value;
      }

      public string Param2
      {
         get => _txtParam2.Text;
         set => _txtParam2.Text = value;
      }

      public string SelectedEventTitle => _lvEvents.SelectedItems.Count > 0 ? _lvEvents.SelectedItems[0].Text : "";

      public void AddEventItem(string title, bool isActive)
      {
         var item = new ListViewItem(title) {Checked = isActive};
         _lvEvents.Items.Add(item);
      }

      public void ClearEventSelection()
      {
         _lvEvents.SelectedItems.Clear();
      }

      public void RemoveSelectedEventItem()
      {
         if (_lvEvents.SelectedItems.Count > 0)
         {
            _lvEvents.Items.Remove(_lvEvents.SelectedItems[0]);
         }
      }

      public void SelectEventItem(string title)
      {
         foreach (ListViewItem item in _lvEvents.Items)
         {
            if (item.Text == title)
            {
               item.Selected = true;
               item.Focused = true;
               item.EnsureVisible();
               break;
            }
         }
      }

      public string ShowInputDialog(string prompt, string defaultValue = "")
      {
         string input = null;
         using (var inputForm = new Form())
         {
            inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputForm.StartPosition = FormStartPosition.CenterParent;
            inputForm.Width = 300;
            inputForm.Height = 150;
            inputForm.Text = string.IsNullOrWhiteSpace(defaultValue) ? "New Event" : "Rename Event";

            var lblPrompt = new Label {Left = 10, Top = 10, Text = prompt, AutoSize = true};
            var txtInput = new TextBox {Left = 10, Top = 40, Width = 260, Text = defaultValue};
            var btnOk = new Button
                           {
                              Text = "OK",
                              Left = 110,
                              Width = 80,
                              Top = 70,
                              DialogResult = DialogResult.OK
                           };
            var btnCancel = new Button
                               {
                                  Text = "Cancel",
                                  Left = 200,
                                  Width = 80,
                                  Top = 70,
                                  DialogResult = DialogResult.Cancel
                               };

            inputForm.Controls.Add(lblPrompt);
            inputForm.Controls.Add(txtInput);
            inputForm.Controls.Add(btnOk);
            inputForm.Controls.Add(btnCancel);
            inputForm.AcceptButton = btnOk;
            inputForm.CancelButton = btnCancel;

            if (inputForm.ShowDialog() == DialogResult.OK)
            {
               input = txtInput.Text;
            }
         }

         return input;
      }

      // Single definition of UpdateActionButtons.
      public void UpdateActionButtons(bool hasSelection)
      {
         _btnSaveConfig.Enabled = hasSelection;
         _btnDelete.Enabled = hasSelection;
         _btnRename.Enabled = hasSelection;
      }

      public void UpdateSelectedEventTitle(string newTitle)
      {
         if (_lvEvents.SelectedItems.Count > 0)
         {
            _lvEvents.SelectedItems[0].Text = newTitle;
         }
      }

      [DllImport("dwmapi.dll")]
      private static extern int DwmGetWindowAttribute(
         IntPtr hwnd,
         int dwAttribute,
         out bool pvAttribute,
         int cbAttribute);

      [DllImport("dwmapi.dll")]
      private static extern int DwmSetWindowAttribute(
         IntPtr hwnd,
         int dwAttribute,
         ref int pvAttribute,
         int cbAttribute);

      private static void EnableDarkTitleBar(IntPtr hwnd, bool enable)
      {
         if (Environment.OSVersion.Version.Major >= 10)
         {
            var darkMode = enable ? 1 : 0;
            DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref darkMode, sizeof(int));
         }
      }

      private bool IsDarkModeEnabled()
      {
         try
         {
            using (var key = Registry.CurrentUser.OpenSubKey(
                   @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
               if (key?.GetValue("AppsUseLightTheme") != null)
               {
                  var value = (int) key.GetValue("AppsUseLightTheme");
                  return value == 0;
               }
            }

            if (Environment.OSVersion.Version.Major >= 10)
            {
               var dark = false;
               if (DwmGetWindowAttribute(
                   Handle,
                   DwmwaUseImmersiveDarkMode,
                   out dark,
                   Marshal.SizeOf(typeof(bool))) == 0)
               {
                  return dark;
               }
            }

            return false;
         }
         catch
         {
            return false;
         }
      }

      private void ApplyDynamicTheme()
      {
         var isDarkMode = IsDarkModeEnabled();
         Console.WriteLine($"Dark Mode Detected: {isDarkMode}");

         if (isDarkMode)
         {
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;
         }
         else
         {
            BackColor = Color.FromArgb(40, 40, 40);
            ForeColor = Color.White;
         }

         if (Environment.OSVersion.Version.Major >= 10)
         {
            EnableDarkTitleBar(Handle, isDarkMode);
         }
      }

      private void BtnAddEvent_Click(object sender, EventArgs e)
      {
         AddEventClicked?.Invoke(sender, e);
      }

      private void BtnDelete_Click(object sender, EventArgs e)
      {
         DeleteEventClicked?.Invoke(sender, e);
      }

      private void BtnParam1_Click(object sender, EventArgs e)
      {
         if (_cbxEventType.SelectedItem != null && (_cbxEventType.SelectedItem.ToString() == "Copy File"
                                                    || _cbxEventType.SelectedItem.ToString() == "Play Sound"))
         {
            using (var ofd = new OpenFileDialog())
            {
               if (ofd.ShowDialog() == DialogResult.OK)
               {
                  _txtParam1.Text = ofd.FileName;
               }
            }
         }
         else
         {
            using (var fbd = new FolderBrowserDialog())
            {
               if (fbd.ShowDialog() == DialogResult.OK)
               {
                  _txtParam1.Text = fbd.SelectedPath;
               }
            }
         }
      }

      private void BtnParam2_Click(object sender, EventArgs e)
      {
         using (var fbd = new FolderBrowserDialog())
         {
            if (fbd.ShowDialog() == DialogResult.OK)
            {
               _txtParam2.Text = fbd.SelectedPath;
            }
         }
      }

      private void BtnRename_Click(object sender, EventArgs e)
      {
         RenameEventClicked?.Invoke(sender, e);
      }

      private void BtnSaveConfig_Click(object sender, EventArgs e)
      {
         SaveConfigClicked?.Invoke(sender, e);
      }

      private void CbxEventTrigger_SelectedIndexChanged(object sender, EventArgs e)
      {
         // Show the Allowed Extensions controls only when EventTrigger == "On Save"
         if (_cbxEventTrigger.SelectedItem != null && _cbxEventTrigger.SelectedItem.ToString() == "On Save")
         {
            _lblAllowedExtensions.Visible = true;
            _txtAllowedExtensions.Visible = true;
         }
         else
         {
            _lblAllowedExtensions.Visible = false;
            _txtAllowedExtensions.Visible = false;
         }
      }

      private void CbxEventType_SelectedIndexChanged(object sender, EventArgs e)
      {
         _txtParam1.Clear();
         _txtParam2.Clear();
         _lblParam1.Visible = false;
         _txtParam1.Visible = false;
         _btnParam1.Visible = false;
         _lblParam2.Visible = false;
         _txtParam2.Visible = false;
         _btnParam2.Visible = false;

         if (_cbxEventType.SelectedIndex != -1)
         {
            var type = _cbxEventType.SelectedItem.ToString();
            _lblParam1.Text = type == "Copy Folder" ? "Source Directory:" : "Source File:";
            _lblParam1.Text = type == "Run Command" ? "Command:" : _lblParam1.Text;
            _lblParam2.Text = "Output Directory:";
            _lblParam1.Visible = true;
            _txtParam1.Visible = true;
            _btnParam1.Visible = type == "Copy Folder" || type == "Copy File" || type == "Play Sound";
            _lblParam2.Visible = type == "Copy Folder" || type == "Copy File";
            _txtParam2.Visible = type == "Copy Folder" || type == "Copy File";
            _btnParam2.Visible = type == "Copy Folder" || type == "Copy File";
         }

         EventTypeChanged?.Invoke(sender, e);
      }

      private void LvEvents_ItemChecked(object sender, ItemCheckedEventArgs e)
      {
         EventItemChecked?.Invoke(sender, e);
      }

      private void LvEvents_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (_lvEvents.SelectedItems.Count > 0)
         {
            var title = _lvEvents.SelectedItems[0].Text;
            _presenter?.LoadSelectedEvent();
         }
         else
         {
            _cbxEventTrigger.SelectedIndex = -1;
            _cbxEventType.SelectedIndex = -1;
            _txtParam1.Clear();
            _txtParam2.Clear();
            _txtAllowedExtensions.Clear();
         }

         SelectedEventChanged?.Invoke(sender, e);
         UpdateActionButtons(_lvEvents.SelectedItems.Count > 0);
      }

      private void SetupControls()
      {
         Controls.Clear();

         // Event Trigger controls
         _lblEventTrigger = new Label {Text = "Event Trigger:", Location = new Point(20, 20), AutoSize = true};
         _cbxEventTrigger = new ComboBox
                               {
                                  Location = new Point(140, 20), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList
                               };
         _cbxEventTrigger.Items.AddRange(new object[] {"On Save", "On Build"});
         _cbxEventTrigger.SelectedIndex = -1;
         _cbxEventTrigger.SelectedIndexChanged += CbxEventTrigger_SelectedIndexChanged;

         // Event Type controls
         _lblEventType = new Label {Text = "Event Type:", Location = new Point(20, 60), AutoSize = true};
         _cbxEventType = new ComboBox
                            {
                               Location = new Point(140, 60), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList
                            };
         _cbxEventType.Items.AddRange(new object[] {"Copy Folder", "Copy File", "Play Sound", "Run Command"});
         _cbxEventType.SelectedIndex = -1;
         _cbxEventType.SelectedIndexChanged += CbxEventType_SelectedIndexChanged;

         // New Allowed Extensions controls (to the right of the EventType combobox)
         _lblAllowedExtensions = new Label
                                    {
                                       Text = "Allowed Extensions:",
                                       Location = new Point(300, 60),
                                       AutoSize = true,
                                       Visible = false
                                    };
         _txtAllowedExtensions = new TextBox {Location = new Point(430, 60), Width = 120, Visible = false};

         // Dynamic parameter controls
         _lblParam1 = new Label {Text = "", Location = new Point(20, 100), AutoSize = true, Visible = false};
         _txtParam1 = new TextBox {Location = new Point(140, 100), Width = 250, Visible = false};
         _btnParam1 = new Button
                         {
                            Text = "...",
                            Location = new Point(400, 100),
                            Size = new Size(30, _txtParam1.Height),
                            Visible = false,
                            FlatStyle = FlatStyle.Flat,
                            BackColor = Color.FromArgb(60, 60, 60),
                            ForeColor = Color.White
                         };
         _btnParam1.FlatAppearance.BorderSize = 0;
         _btnParam1.Click += BtnParam1_Click;

         _lblParam2 = new Label {Text = "", Location = new Point(20, 140), AutoSize = true, Visible = false};
         _txtParam2 = new TextBox {Location = new Point(140, 140), Width = 250, Visible = false};
         _btnParam2 = new Button
                         {
                            Text = "...",
                            Location = new Point(400, 140),
                            Size = new Size(30, _txtParam2.Height),
                            Visible = false,
                            FlatStyle = FlatStyle.Flat,
                            BackColor = Color.FromArgb(60, 60, 60),
                            ForeColor = Color.White
                         };
         _btnParam2.FlatAppearance.BorderSize = 0;
         _btnParam2.Click += BtnParam2_Click;

         // ListView for saved events with dark-mode appearance.
         _lvEvents = new ListView
                        {
                           Location = new Point(20, 200),
                           Width = 400,
                           Height = 150,
                           View = View.List,
                           CheckBoxes = true,
                           BackColor = Color.FromArgb(45, 45, 45),
                           ForeColor = Color.White
                        };
         _lvEvents.ItemChecked += LvEvents_ItemChecked;
         _lvEvents.SelectedIndexChanged += LvEvents_SelectedIndexChanged;

         // "Add Config" button.
         _btnAddEvent = new Button
                           {
                              Text = "Add Config",
                              Location = new Point(430, 200),
                              Size = new Size(100, 30),
                              FlatStyle = FlatStyle.Flat,
                              BackColor = Color.FromArgb(60, 60, 60),
                              ForeColor = Color.White
                           };
         _btnAddEvent.FlatAppearance.BorderSize = 0;
         _btnAddEvent.Click += BtnAddEvent_Click;

         // "Save Config" button.
         _btnSaveConfig = new Button
                             {
                                Text = "Save Config",
                                Location = new Point(430, 240),
                                Size = new Size(100, 30),
                                FlatStyle = FlatStyle.Flat,
                                BackColor = Color.FromArgb(60, 60, 60),
                                ForeColor = Color.White,
                                Enabled = false
                             };
         _btnSaveConfig.FlatAppearance.BorderSize = 0;
         _btnSaveConfig.Click += BtnSaveConfig_Click;

         // "Delete Config" button.
         _btnDelete = new Button
                         {
                            Text = "Delete Config",
                            Location = new Point(430, 280),
                            Size = new Size(100, 30),
                            FlatStyle = FlatStyle.Flat,
                            BackColor = Color.FromArgb(60, 60, 60),
                            ForeColor = Color.White,
                            Enabled = false
                         };
         _btnDelete.FlatAppearance.BorderSize = 0;
         _btnDelete.Click += BtnDelete_Click;

         // "Rename" button.
         _btnRename = new Button
                         {
                            Text = "Rename",
                            Location = new Point(430, 320),
                            Size = new Size(100, 30),
                            FlatStyle = FlatStyle.Flat,
                            BackColor = Color.FromArgb(60, 60, 60),
                            ForeColor = Color.White,
                            Enabled = false
                         };
         _btnRename.FlatAppearance.BorderSize = 0;
         _btnRename.Click += BtnRename_Click;

         Controls.AddRange(
         new Control[]
            {
               _lblEventTrigger, _cbxEventTrigger, _lblEventType, _cbxEventType, _lblAllowedExtensions,
               _txtAllowedExtensions, _lblParam1, _txtParam1, _btnParam1, _lblParam2, _txtParam2, _btnParam2, _lvEvents,
               _btnAddEvent, _btnSaveConfig, _btnDelete, _btnRename
            });
      }
   }
}