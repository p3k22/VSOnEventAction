namespace VSOnEventAction.Presenter
{
   using System;
   using System.Diagnostics;
   using System.IO;
   using System.Linq;
   using System.Media;

   using VSOnEventAction.Model;

   using WMPLib;

   public static class RuleProcessor
   {
      /// <summary>
      ///    Processes all rule files in the dynamic folder matching the given trigger.
      ///    For "On Save", a docExtension filter is applied.
      ///    For "On Build" and "Play Sound", the rule fires unconditionally (except for allowed extensions filtering, if
      ///    specified).
      /// </summary>
      /// <param name="trigger">The trigger ("On Save", "On Build", or "Play Sound").</param>
      /// <param name="docExtension">
      ///    For On Save, the extension (lowercase without the dot) of the document that was saved.
      ///    For other triggers, this can be null.
      /// </param>
      public static void ProcessRules(string trigger, string docExtension = null)
      {
         // Get the folder where rule files are stored.
         var rulesFolder = EventActionPresenter.DynamicFolder;
         if (!Directory.Exists(rulesFolder))
         {
            Debug.WriteLine($"[RuleProcessor] Rules folder not found: {rulesFolder}");
            return;
         }

         foreach (var file in Directory.GetFiles(rulesFolder, "*.txt"))
         {
            Debug.WriteLine($"[RuleProcessor] Processing rule file: {file}");
            var rule = EventAction.LoadFromFile(file);
            // Process only rules matching the trigger and that are active.
            if (rule.ETrigger == trigger && rule.IsActive)
            {
               var ruleApplies = false;
               // Parse allowed extensions from the rule.
               var allowedRaw = rule.AllowedExtensions;
               var allowed = !string.IsNullOrWhiteSpace(allowedRaw) ?
                                allowedRaw.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(ext => ext.Trim().TrimStart('.').ToLowerInvariant()).ToArray() :
                                new string[0];

               Debug.WriteLine(
               $"[RuleProcessor] Allowed Extensions: {(allowed.Length > 0 ? string.Join(", ", allowed) : "EMPTY")}");

               // For "On Save", if allowed is non-empty then only fire if docExtension matches.
               // For other triggers (including "On Build" and "Play Sound"), fire unconditionally if allowed is empty,
               // but if allowed is provided, then fire only if a matching extension is found.
               if (allowed.Length == 0)
               {
                  ruleApplies = true;
                  Debug.WriteLine("[RuleProcessor] AllowedExtensions empty: rule applies unconditionally.");
               }
               else
               {
                  // If docExtension is provided (e.g. for On Save) use it to decide.
                  if (!string.IsNullOrWhiteSpace(docExtension))
                  {
                     ruleApplies = allowed.Contains(docExtension);
                     Debug.WriteLine(
                     ruleApplies ?
                        "[RuleProcessor] Matching extension found: rule applies." :
                        "[RuleProcessor] No matching extension: rule does NOT apply.");
                  }
                  else
                  {
                     // For triggers where we do have allowed extensions but no doc extension filter (like On Build or Play Sound)
                     // we decide to fire only if allowed is not empty.
                     ruleApplies = true;
                     Debug.WriteLine("[RuleProcessor] No document extension filter provided: rule applies.");
                  }
               }

               if (ruleApplies)
               {
                  if (rule.EType == "Copy File")
                  {
                     if (File.Exists(rule.SourceFile))
                     {
                        var destFile = Path.Combine(rule.OutputFolder, Path.GetFileName(rule.SourceFile));
                        Debug.WriteLine($"[RuleProcessor] Copying file: {rule.SourceFile} -> {destFile}");
                        File.Copy(rule.SourceFile, destFile, true);
                     }
                     else
                     {
                        Debug.WriteLine($"[RuleProcessor] Source file not found: {rule.SourceFile}");
                     }
                  }
                  else if (rule.EType == "Copy Folder")
                  {
                     if (Directory.Exists(rule.SourceFolder))
                     {
                        // Create a new subfolder in the output directory named after the last segment of the source folder.
                        var folderName = Path.GetFileName(
                        Path.GetFullPath(rule.SourceFolder).TrimEnd(Path.DirectorySeparatorChar));
                        var newOutputFolder = Path.Combine(rule.OutputFolder, folderName);
                        Debug.WriteLine($"[RuleProcessor] Copying folder: {rule.SourceFolder} -> {newOutputFolder}");
                        CopyDirectory(rule.SourceFolder, newOutputFolder, true);
                     }
                     else
                     {
                        Debug.WriteLine($"[RuleProcessor] Source folder not found: {rule.SourceFolder}");
                     }
                  }
                  else if (rule.EType == "Play Sound")
                  {
                     if (File.Exists(rule.SourceFile))
                     {
                        var soundExt = Path.GetExtension(rule.SourceFile)?.ToLowerInvariant();
                        if (soundExt == ".wav")
                        {
                           try
                           {
                              Debug.WriteLine($"[RuleProcessor] Playing WAV sound from: {rule.SourceFile}");
                              var player = new SoundPlayer(rule.SourceFile);
                              player.Play();
                           }
                           catch (Exception ex)
                           {
                              Debug.WriteLine($"[RuleProcessor] Error playing WAV: {ex.Message}");
                           }
                        }
                        else if (soundExt == ".mp3")
                        {
                           try
                           {
                              Debug.WriteLine($"[RuleProcessor] Playing MP3 sound from: {rule.SourceFile}");
                              // Requires adding a COM reference to Windows Media Player.

                              var wplayer = new WindowsMediaPlayer {URL = rule.SourceFile};
                              wplayer.controls.play();
                           }
                           catch (Exception ex)
                           {
                              Debug.WriteLine($"[RuleProcessor] Error playing MP3: {ex.Message}");
                           }
                        }
                        else
                        {
                           Debug.WriteLine($"[RuleProcessor] Unsupported sound format: {rule.SourceFile}");
                        }
                     }
                     else
                     {
                        Debug.WriteLine($"[RuleProcessor] Sound file not found: {rule.SourceFile}");
                     }
                  }
                  else if (rule.EType == "Run Command")
                  {
                     // Launch cmd.exe with the /k option to keep the window open and execute the command.
                     var startInfo = new ProcessStartInfo("cmd.exe", "/k " + rule.SourceFile) {UseShellExecute = false};

                     try
                     {
                        Process.Start(startInfo);
                     }
                     catch (Exception ex)
                     {
                        Debug.WriteLine("Error launching cmd.exe: " + ex.Message);
                     }
                  }
               }
            }
         }
      }

      private static void CopyDirectory(string sourceDir, string destinationDir, bool overwrite)
      {
         Directory.CreateDirectory(destinationDir);
         foreach (var file in Directory.GetFiles(sourceDir))
         {
            var destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite);
         }

         foreach (var dir in Directory.GetDirectories(sourceDir))
         {
            var destSubdir = Path.Combine(destinationDir, Path.GetFileName(dir));
            CopyDirectory(dir, destSubdir, overwrite);
         }
      }
   }
}