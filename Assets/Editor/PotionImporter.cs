using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PotionImporter", menuName = "Data Pipeline/Potion Importer")]
public class PotionImporter : ScriptableObject
{
    [Tooltip("Path to the spreadsheet to read, relative to Assets folder. Use forward slashes.")]
    public string excelFilePath = "Editor/PotionCrafting.xlsx";

    [Tooltip("RecipeCollection asset to store the loaded recipes.")]
    public RecipeCollection recipes;

    [ContextMenu("Import")]
    public void Import()
    {
        var excel = new ExcelImporter(excelFilePath);
        var items = DataHelper.GetAllAssetsOfType<InventoryItem>();

        ImportIngredients(excel, items);

        Debug.Log("Import Complete.");
    }
     
    void ImportIngredients(ExcelImporter excel, Dictionary<string, InventoryItem> items){
       if (!excel.TryGetTable("Ingredients", out var table))
       {
           Debug.LogError($"Could not find table 'Ingredients' in {excelFilePath}");
           return;
       }

       for (int row = 1; row <= table.RowCount; row++) {
            string name = table.GetValue<string>(row, "Name");
            if (string.IsNullOrWhiteSpace(name)) continue;

            var item = DataHelper.GetOrCreateAsset(name, items, "Ingredients");

            if (string.IsNullOrWhiteSpace(item.displayName))
                item.displayName = name;

            if (table.TryGetEnum<Rarity>(row, "Rarity", out var rarity))
               item.rarity = rarity;
            

            item.cost = table.GetValue<int>(row, "Cost");
            Debug.Log(name);
       }
    }
}