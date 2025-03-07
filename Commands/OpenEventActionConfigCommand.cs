using Task = System.Threading.Tasks.Task;

namespace VSOnEventAction
{
   using Microsoft.VisualStudio.Shell;

   using System;
   using System.ComponentModel.Design;
   using System.Linq;

   using VSOnEventAction.View;

   /// <summary>
   ///    Command handler
   /// </summary>
   internal sealed class OpenEventActionConfigCommand
   {
      /// <summary>
      ///    Command ID.
      /// </summary>
      public const int CommandId = 0x0100;

      /// <summary>
      ///    Command menu group (command set GUID).
      /// </summary>
      public static readonly Guid CommandSet = new Guid("eea8d766-33e6-46ba-82d9-fdaf271f0873");

      /// <summary>
      ///    VS Package that provides this command, not null.
      /// </summary>
      private readonly AsyncPackage _package;

      /// <summary>
      ///    Initializes a new instance of the <see cref="OpenEventActionConfigCommand" /> class.
      ///    Adds our command handlers for menu (commands must exist in the command table file)
      /// </summary>
      /// <param name="package">Owner package, not null.</param>
      /// <param name="commandService">Command service to add command to, not null.</param>
      private OpenEventActionConfigCommand(AsyncPackage package, OleMenuCommandService commandService)
      {
         _package = package ?? throw new ArgumentNullException(nameof(package));
         commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

         var menuCommandID = new CommandID(CommandSet, CommandId);
         var menuItem = new MenuCommand(Execute, menuCommandID);
         commandService.AddCommand(menuItem);
      }

      /// <summary>
      ///    Gets the instance of the command.
      /// </summary>
      public static OpenEventActionConfigCommand Instance { get; private set; }

      /// <summary>
      ///    Gets the service provider from the owner package.
      /// </summary>
      private IAsyncServiceProvider ServiceProvider => _package;

      /// <summary>
      ///    Initializes the singleton instance of the command.
      /// </summary>
      /// <param name="package">Owner package, not null.</param>
      public static async Task InitializeAsync(AsyncPackage package)
      {
         // Switch to the main thread - the call to AddCommand in Command1's constructor requires
         // the UI thread.
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

         var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
         Instance = new OpenEventActionConfigCommand(package, commandService);
      }

      /// <summary>
      ///    This function is the callback used to execute the command when the menu item is clicked.
      ///    See the constructor to see how the menu item is associated with this function using
      ///    OleMenuCommandService service and MenuCommand class.
      /// </summary>
      /// <param name="sender">Event sender.</param>
      /// <param name="e">Event args.</param>
      private static void Execute(object sender, EventArgs e)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         var configWin = new EventActionView();
         configWin.ShowDialog();
      }
   }
}