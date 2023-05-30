using System;
using System.Collections.Generic;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.UI
{
    class ValidationTypeDropdown
    {
        public const string StructureLabelText = "Structure";
        public const string AssetStoreLabelText = "Against Asset Store standards";

        public static List<string> ToList()
        {
            var listOfChoices = new List<string>();

            foreach (var type in (ValidationType[])Enum.GetValues(typeof(ValidationType)))
            {
                string choiceText;
                switch (type)
                {
                    case ValidationType.Structure:
                        choiceText = StructureLabelText;
                        break;
                    case ValidationType.AssetStore:
                        choiceText = AssetStoreLabelText;
                        break;
                    default:
                        choiceText = null;
                        break;
                }

                if (choiceText != null) listOfChoices.Add(choiceText);
            }

            return listOfChoices;
        }

        public static ValidationType ValidationTypeFromDropdown(string popupFieldValue, PackageSource packageSource)
        {
            switch (popupFieldValue)
            {
                case StructureLabelText:
                    return ValidationType.Structure;
                case AssetStoreLabelText:
                    return ValidationType.AssetStore;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
