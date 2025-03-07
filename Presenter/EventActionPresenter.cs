namespace VSOnEventAction.Presenter
{
   using System;
   using System.IO;
   using System.Linq;
   using System.Reflection;
   using System.Windows.Forms;

   using VSOnEventAction.Model;
   using VSOnEventAction.View;

   /// <summary>
   ///    The presenter that handles business logic for the event configuration.
   /// </summary>
   public class EventActionPresenter
   {
      private readonly IEventActionView _view;

      public EventActionPresenter(IEventActionView view)
      {
         _view = view;
         // Subscribe to view events.
         _view.EventTypeChanged += (s, e) =>
            {
               /* Handled by view */
            };
         _view.SaveConfigClicked += (s, e) => UpdateCurrentEvent();
         _view.AddEventClicked += (s, e) => AddNewEvent();
         _view.DeleteEventClicked += (s, e) => DeleteEvent();
         _view.RenameEventClicked += (s, e) => RenameEvent();
         _view.SelectedEventChanged += (s, e) => LoadSelectedEvent();
         _view.EventItemChecked += (s, e) => SaveActiveOnly(e.Item.Text);
      }

       public static string DynamicFolder
       {
          get
          {
             ThreadHelper.ThrowIfNotOnUIThread();
             try
             {
                // Get the DTE service
                var dte = (DTE) Package.GetGlobalService(typeof(DTE));
      
                // Check if a solution is loaded
                if (dte?.Solution != null && dte.Solution.IsOpen)
                {
                   var solutionPath = dte.Solution.FullName;
                   if (!string.IsNullOrEmpty(solutionPath))
                   {
                      // Get the directory containing the solution file
                      var solutionDir = Path.GetDirectoryName(solutionPath);
                      return Path.Combine(solutionDir, "SavedEventActions");
                   }
                }
             }
             catch (Exception)
             {
                // Handle any potential errors (optional: add logging)
             }
      
             // Fallback to original behavior if no solution is loaded
             var extensionFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
             return Path.Combine(extensionFolder, "SavedEventActions");
          }
       }

      public void LoadSavedEvents()
      {
         _view.EventItems.Clear();
         if (Directory.Exists(DynamicFolder))
         {
            foreach (var file in Directory.GetFiles(DynamicFolder, "*.txt"))
            {
               var title = Path.GetFileNameWithoutExtension(file);
               var config = EventAction.LoadFromFile(file);
               _view.AddEventItem(title, config.IsActive);
            }
         }
      }

      public void LoadSelectedEvent()
      {
         var title = _view.SelectedEventTitle;
         if (string.IsNullOrWhiteSpace(title))
         {
            return;
         }

         var filePath = Path.Combine(DynamicFolder, title + ".txt");
         if (!File.Exists(filePath))
         {
            return;
         }

         var config = EventAction.LoadFromFile(filePath);
         _view.EventTrigger = config.ETrigger;
         _view.EventType = config.EType;
         _view.Param1 = config.EType == "Copy Folder" ? config.SourceFolder : config.SourceFile;
         _view.Param2 = config.OutputFolder;

         // Load the allowed extensions value.
         _view.AllowedExtensions = config.AllowedExtensions;
      }

      private void AddNewEvent()
      {
         var title = _view.ShowInputDialog("Enter event title:");
         if (string.IsNullOrWhiteSpace(title))
         {
            return;
         }

         if (!Directory.Exists(DynamicFolder))
         {
            Directory.CreateDirectory(DynamicFolder);
         }

         var filePath = Path.Combine(DynamicFolder, title + ".txt");
         if (File.Exists(filePath))
         {
            MessageBox.Show(
            "An event with that title already exists.",
            "Duplicate Event",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
            return;
         }

         SaveConfigToFile(filePath);
         _view.AddEventItem(title, true);
         _view.SelectEventItem(title);
         UpdateActionButtons();
      }

      private void DeleteEvent()
      {
         var title = _view.SelectedEventTitle;
         if (string.IsNullOrWhiteSpace(title))
         {
            return;
         }

         var filePath = Path.Combine(DynamicFolder, title + ".txt");
         if (File.Exists(filePath))
         {
            File.Delete(filePath);
         }

         _view.RemoveSelectedEventItem();
         _view.ClearEventSelection();
         UpdateActionButtons();
      }

      private void RenameEvent()
      {
         var oldTitle = _view.SelectedEventTitle;
         if (string.IsNullOrWhiteSpace(oldTitle))
         {
            return;
         }

         var newTitle = _view.ShowInputDialog("Enter new title:", oldTitle);
         if (string.IsNullOrWhiteSpace(newTitle) || newTitle == oldTitle)
         {
            return;
         }

         var oldFilePath = Path.Combine(DynamicFolder, oldTitle + ".txt");
         var newFilePath = Path.Combine(DynamicFolder, newTitle + ".txt");
         if (File.Exists(newFilePath))
         {
            MessageBox.Show(
            "An event with that title already exists.",
            "Duplicate Event",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
            return;
         }

         try
         {
            File.Move(oldFilePath, newFilePath);
            _view.UpdateSelectedEventTitle(newTitle);
         }
         catch (Exception ex)
         {
            MessageBox.Show("Error renaming event: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }

         UpdateActionButtons();
      }

      private void SaveActiveOnly(string title)
      {
         var filePath = Path.Combine(DynamicFolder, title + ".txt");
         if (!File.Exists(filePath))
         {
            return;
         }

         var config = EventAction.LoadFromFile(filePath);
         foreach (ListViewItem item in _view.EventItems)
         {
            if (item.Text == title)
            {
               config.IsActive = item.Checked;
               break;
            }
         }

         config.SaveToFile(filePath);
      }

      private void SaveConfigToFile(string filePath)
      {
         var config = new EventAction
                         {
                            ETrigger = _view.EventTrigger,
                            EType = _view.EventType,
                            SourceFolder = _view.EventType == "Copy Folder" ? _view.Param1 : "",
                            SourceFile = _view.EventType != "Copy Folder" ? _view.Param1 : "",
                            OutputFolder = _view.Param2,
                            OutputFile =
                               _view.EventType == "Copy File" && !string.IsNullOrWhiteSpace(_view.Param1) ?
                                  Path.GetFileName(_view.Param1) :
                                  "",
                            IsActive = true,
                            AllowedExtensions = _view.AllowedExtensions // Save allowed extensions.
                         };
         config.SaveToFile(filePath);
      }

      private void UpdateActionButtons()
      {
         var hasSelection = !string.IsNullOrWhiteSpace(_view.SelectedEventTitle);
         _view.UpdateActionButtons(hasSelection);
      }

      private void UpdateCurrentEvent()
      {
         var title = _view.SelectedEventTitle;
         if (string.IsNullOrWhiteSpace(title))
         {
            return;
         }

         var filePath = Path.Combine(DynamicFolder, title + ".txt");
         SaveConfigToFile(filePath);
      }
   }
}
