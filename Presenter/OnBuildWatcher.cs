// Adjust namespace as needed

namespace VSOnEventAction.Presenter
{
   using Microsoft.VisualStudio;
   using Microsoft.VisualStudio.Shell;
   using Microsoft.VisualStudio.Shell.Interop;

   using System;
   using System.Diagnostics;
   using System.Linq;

   /// <summary>
   ///    Listens for solution build events and triggers rules with "On Build" trigger.
   /// </summary>
   public class OnBuildWatcher : IVsUpdateSolutionEvents
   {
      private readonly IServiceProvider _serviceProvider;

      private readonly IVsSolutionBuildManager _buildManager;

      private uint _buildEventsCookie;

      public OnBuildWatcher(IServiceProvider serviceProvider)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         _serviceProvider = serviceProvider;
         _buildManager = (IVsSolutionBuildManager) _serviceProvider.GetService(typeof(SVsSolutionBuildManager));
         if (_buildManager != null)
         {
            _buildManager.AdviseUpdateSolutionEvents(this, out _buildEventsCookie);
            Debug.WriteLine("[OnBuildWatcher] Subscribed to solution build events.");
         }
         else
         {
            Debug.WriteLine("[OnBuildWatcher] Failed to obtain IVsSolutionBuildManager.");
         }
      }

      // Called when the active project configuration changes.
      public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         Debug.WriteLine("[OnBuildWatcher] OnActiveProjectCfgChange fired.");
         return VSConstants.S_OK;
      }

      // Called at the beginning of the solution build.
      public int UpdateSolution_Begin(ref int pfCancelUpdate)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         Debug.WriteLine("[OnBuildWatcher] UpdateSolution_Begin fired.");
         return VSConstants.S_OK;
      }

      // Called if the solution build is canceled.
      public int UpdateSolution_Cancel()
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         Debug.WriteLine("[OnBuildWatcher] UpdateSolution_Cancel fired.");
         return VSConstants.S_OK;
      }

      // Called when the solution build is finished.
      public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         Debug.WriteLine("[OnBuildWatcher] UpdateSolution_Done fired. fSucceeded: " + fSucceeded);
         ProcessBuildRules();
         return VSConstants.S_OK;
      }

      // Called when the solution build is about to start updating.
      public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         Debug.WriteLine("[OnBuildWatcher] UpdateSolution_StartUpdate fired.");
         return VSConstants.S_OK;
      }

      /// <summary>
      ///    Processes all build rules. For On Build, we fire all matching rules unconditionally.
      /// </summary>
      private static void ProcessBuildRules()
      {
         Debug.WriteLine("[OnBuildWatcher] Processing build rules.");
         // In this case, we pass null for the file extension so that our rule processor
         // will fire the rule unconditionally for "On Build".
         RuleProcessor.ProcessRules("On Build");
      }
   }
}