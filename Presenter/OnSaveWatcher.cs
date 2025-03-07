// Adjust namespace if needed

namespace VSOnEventAction.Presenter
{
   using Microsoft.VisualStudio;
   using Microsoft.VisualStudio.Shell;
   using Microsoft.VisualStudio.Shell.Interop;

   using System;
   using System.Diagnostics;
   using System.IO;
   using System.Linq;

   /// <summary>
   ///    Listens for VS document save events and triggers rules with "On Save" trigger.
   /// </summary>
   public class OnSaveWatcher : IVsRunningDocTableEvents
   {
      private readonly IVsRunningDocumentTable _rdt;

      public OnSaveWatcher(IServiceProvider serviceProvider)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         _rdt = (IVsRunningDocumentTable) serviceProvider.GetService(typeof(SVsRunningDocumentTable));
         _rdt.AdviseRunningDocTableEvents(this, out var rdtCookie);
      }

      public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
      {
         return VSConstants.S_OK;
      }

      public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
      {
         return VSConstants.S_OK;
      }

      public int OnAfterFirstDocumentLock(
         uint docCookie,
         uint dwRDTLockType,
         uint dwReadLocksRemaining,
         uint dwEditLocksRemaining)
      {
         return VSConstants.S_OK;
      }

      public int OnAfterSave(uint docCookie)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         // Retrieve info about the single saved document.
         _rdt.GetDocumentInfo(
         docCookie,
         out _,
         out _,
         out _,
         out var moniker,
         out var hierarchy,
         out var itemid,
         out var docData);

         var docExt = Path.GetExtension(moniker)?.TrimStart('.').ToLowerInvariant() ?? "";
         Debug.WriteLine($"[OnAfterSave] Document saved: {moniker} (ext: {docExt})");

         // Process rules for "On Save", using the document's extension.
         RuleProcessor.ProcessRules("On Save", docExt);

         return VSConstants.S_OK;
      }

      public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
      {
         return VSConstants.S_OK;
      }

      public int OnBeforeLastDocumentUnlock(
         uint docCookie,
         uint dwRDTLockType,
         uint dwReadLocksRemaining,
         uint dwEditLocksRemaining)
      {
         return VSConstants.S_OK;
      }
   }
}