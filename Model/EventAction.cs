namespace VSOnEventAction.Model
{
   using System;
   using System.IO;
   using System.Linq;

   /// <summary>
   ///    Represents an event configuration.
   /// </summary>
   public class EventAction
   {
      public string AllowedExtensions { get; set; }

      public string ETrigger { get; set; }

      public string EType { get; set; }

      public bool IsActive { get; set; }

      public string OutputFile { get; set; }

      public string OutputFolder { get; set; }

      public string SourceFile { get; set; }

      public string SourceFolder { get; set; }

      public static EventAction LoadFromFile(string filePath)
      {
         var config = new EventAction();
         foreach (var line in File.ReadAllLines(filePath))
         {
            var parts = line.Split(new[] {'='}, 2);
            if (parts.Length != 2)
            {
               continue;
            }

            var key = parts[0].Trim();
            var value = parts[1].Trim();
            switch (key)
            {
               case "eTrigger":
                  config.ETrigger = value;
                  break;
               case "eType":
                  config.EType = value;
                  break;
               case "sourceFolder":
                  config.SourceFolder = value;
                  break;
               case "sourceFile":
                  config.SourceFile = value;
                  break;
               case "outputFolder":
                  config.OutputFolder = value;
                  break;
               case "outputFile":
                  config.OutputFile = value;
                  break;
               case "isActive":
                  bool.TryParse(value, out var active);
                  config.IsActive = active;
                  break;
               case "allowedExtensions":
                  config.AllowedExtensions = value;
                  break;
            }
         }

         return config;
      }

      public void SaveToFile(string filePath)
      {
         var content = $"eTrigger={ETrigger}{Environment.NewLine}" + $"eType={EType}{Environment.NewLine}"
                       + $"sourceFolder={SourceFolder}{Environment.NewLine}"
                       + $"sourceFile={SourceFile}{Environment.NewLine}"
                       + $"outputFolder={OutputFolder}{Environment.NewLine}"
                       + $"outputFile={OutputFile}{Environment.NewLine}" + $"isActive={IsActive}{Environment.NewLine}"
                       + $"allowedExtensions={AllowedExtensions}";
         File.WriteAllText(filePath, content);
      }
   }
}