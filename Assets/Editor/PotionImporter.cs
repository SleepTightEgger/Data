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

        ImportItems("Ingredients", excel, items);
        ImportItems("Potions", excel, items);

        ImportRecipes(excel, items);

        Debug.Log("Import Complete.");
    }

    void ImportRecipes(ExcelImporter excel, Dictionary<string, InventoryItem> items) {
        if (recipes == null) {
           Debug.LogError("No RecipeCollection provided to store imported recipes.");
           return;
        }

        if (!excel.TryGetTable("Recipes", out var table)) {
           Debug.LogError($"Could not find table 'Recipes' in {excelFilePath}");
           return;
        }

        DataHelper.MarkChangesForSaving(recipes);
        recipes.Clear();

        for (int row = 1; row <= table.RowCount; row++) {
            recipes.TryAddRecipe(
                items,
                table.GetValue<string>(row, "Potion"),
                table.GetValue<string>(row, "Item 1"),
                table.GetValue<string>(row, "Item 2"),
                table.GetValue<string>(row, "Item 3")
            );
        }
    }


    void ImportItems(string category, ExcelImporter excel, Dictionary<string, InventoryItem> items){
       if (!excel.TryGetTable(category, out var table))
       {
           Debug.LogError($"Could not find table '{category}' in {excelFilePath}");
           return;
       }

        for (int row = 1; row <= table.RowCount; row++)
        {
            string name = table.GetValue<string>(row, "Name");
            if (string.IsNullOrWhiteSpace(name)) continue;

            var item = DataHelper.GetOrCreateAsset(name, items, category);

            if (string.IsNullOrWhiteSpace(item.displayName))
                item.displayName = name;

            if (table.TryGetEnum<Rarity>(row, "Rarity", out var rarity))
                item.rarity = rarity;


            item.cost = table.GetValue<int>(row, "Cost");


            // My added code.
            if (table.HasColumn("Uses")) {
                item.uses = table.GetValue<int>(row, "Uses");
             }
            if (table.HasColumn("Max Profit")) {
                item.maxProfit = table.GetValue<int>(row, "Max Profit");
            }
            Debug.Log(name);
       }
    }


}