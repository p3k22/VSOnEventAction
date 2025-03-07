﻿using Task = System.Threading.Tasks.Task;

namespace VSOnEventAction
{
   using Microsoft.VisualStudio.Shell;
   using Microsoft.VisualStudio.Shell.Interop;

   using System;
   using System.Linq;
   using System.Runtime.InteropServices;
   using System.Threading;

   using VSOnEventAction.Presenter;

   [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
   [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
   [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
   [Guid(PackageGuidString)]
   [ProvideMenuResource("Menus.ctmenu", 1)]
   public sealed class VSOnEventActionPackage : AsyncPackage
   {
      /// <summary>
      ///    VSOnEventAction GUID string.
      /// </summary>
      public const string PackageGuidString = "06da98f4-5d14-4371-a5b5-15188ea37bf5";

      /// <summary>
      ///    Initialization of the package; this method is called right after the package is sited, so this is the place
      ///    where you can put all the initialization code that rely on services provided by VisualStudio.
      /// </summary>
      /// <param name="cancellationToken">
      ///    A cancellation token to monitor for initialization cancellation, which can occur when
      ///    VS is shutting down.
      /// </param>
      /// <param name="progress">A provider for progress updates.</param>
      /// <returns>
      ///    A task representing the async work of package initialization, or an already completed task if there is none.
      ///    Do not return null from this method.
      /// </returns>
      protected override async Task InitializeAsync(
         CancellationToken cancellationToken,
         IProgress<ServiceProgressData> progress)
      {
         await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
         await OpenEventActionConfigCommand.InitializeAsync(this);
         var onSaveWatcher = new OnSaveWatcher(this);
         var onBuildWatcher = new OnBuildWatcher(this);
      }
   }
}