namespace VSOnEventAction.View
{
   using System;
   using System.Linq;
   using System.Windows.Forms;

   public interface IEventActionView
   {
      event EventHandler AddEventClicked;

      event EventHandler DeleteEventClicked;

      event ItemCheckedEventHandler EventItemChecked;

      // Events raised by the view.
      event EventHandler EventTypeChanged;

      event EventHandler RenameEventClicked;

      event EventHandler SaveConfigClicked;

      event EventHandler SelectedEventChanged;

      ListView.ListViewItemCollection EventItems { get; }

      // Properties corresponding to controls.
      string EventTrigger { get; set; }

      string EventType { get; set; }

      string AllowedExtensions { get; set; }

      string Param1 { get; set; }

      string Param2 { get; set; }

      string SelectedEventTitle { get; }

      void AddEventItem(string title, bool isActive);

      // Methods for updating the list.
      void ClearEventSelection();

      void RemoveSelectedEventItem();

      void SelectEventItem(string title);

      // Helper method for input dialogs.
      string ShowInputDialog(string prompt, string defaultValue = "");

      // Method to update action buttons' enabled state.
      void UpdateActionButtons(bool hasSelection);

      void UpdateSelectedEventTitle(string newTitle);
   }
}