using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace blacksmithname.src
{
    [HarmonyPatch]
    public class harmPatches
    {
        public static IEnumerable<CodeInstruction> Transpiler_check(IEnumerable<CodeInstruction> instructions)
        {
            bool found = false;
            var codes = new List<CodeInstruction>(instructions);
            var decMethod = AccessTools.GetDeclaredMethods(typeof(IWorldAccessor))
            .Where(m => m.Name == "SpawnItemEntity" && m.GetParameters().Types().Contains(typeof(ItemStack)) && m.GetParameters().Types().Contains(typeof(Vec3d)) && m.GetParameters().Types().Contains(typeof(Vec3d)))
            .Single();
            var proxyMethod = AccessTools.Method(typeof(harmPatches), "addName");
            for (int i = 0; i < codes.Count; i++)
            {
                if (!found &&
                    codes[i].opcode == OpCodes.Ldarg_1 && codes[i + 1].opcode == OpCodes.Brfalse_S && codes[i + 2].opcode == OpCodes.Ldarg_1 && codes[i - 1].opcode == OpCodes.Stfld)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, proxyMethod);
                    found = true;
                }
                yield return codes[i];
            }
        }
        
        public static void addName(IServerPlayer byPlayer, ItemStack itemStack)
        {
            if (byPlayer != null)
            {
                if (itemStack.Item != null && (itemStack.Item.Shape.Base.Path.StartsWith("item/tool") 
                    || itemStack.Item.Shape.Base.Path.StartsWith("item/spytube")
                    || itemStack.Item.Code.Domain.Equals("xmelee")))
                {
                    itemStack.Attributes.SetString("smithname", byPlayer.PlayerName);
                }
            }
        }
        public static void Postfix_GetHeldItemInfo(Vintagestory.API.Common.CollectibleObject __instance, ItemSlot inSlot,
      StringBuilder dsc,
      IWorldAccessor world,
      bool withDebugInfo)
        {
            ItemStack itemstack = inSlot.Itemstack;
            string smithName = itemstack.Attributes.GetString("smithname");
            if (smithName != null && !(smithName.Length == 0))
            {
                dsc.Append(Lang.Get(blacksmithname.getID() + ":smithed_by", "<font color=\"" + Lang.Get(blacksmithname.getID() + ":playername_color") + "\">" + smithName + "</strong>")).Append("\n");
            }
            return;
        }

        public static void Prefix_OnCreatedByCrafting(Vintagestory.API.Common.CollectibleObject __instance, ItemSlot[] allInputslots, ItemSlot outputSlot, GridRecipe byRecipe)
        {

            if (!(outputSlot.Itemstack?.Item?.Shape?.Base?.Path?.StartsWith("item/tool")).HasValue ||
                !(outputSlot.Itemstack?.Item?.Shape?.Base?.Path?.StartsWith("item/tool")).Value
                && !(outputSlot.Itemstack.Item.Code.Domain.Equals("xmelee")))
            {
                return;
            }

            string twoAuthors = "";
            foreach (var it in allInputslots)
            {
                if (it.Itemstack != null && it.Itemstack.Attributes.GetString("smithname") != null)
                {
                    if (it.Itemstack.Item.Code.Path.StartsWith("xweapongrip"))
                    {
                        it.Itemstack.Attributes.SetString("smithname", "notme");
                        if (twoAuthors != "" && !it.Itemstack.Attributes.GetString("smithname").Equals(twoAuthors))
                        {
                            twoAuthors += " & " + it.Itemstack.Attributes.GetString("smithname");
                        }               
                        outputSlot.Itemstack.Attributes.SetString("smithname", twoAuthors);
                        return;
                    }
                    if(it.Itemstack.Item.Code.Path.StartsWith("xspearhead") || it.Itemstack.Item.Code.Path.StartsWith("xhalberdhead"))
                    {
                        outputSlot.Itemstack.Attributes.SetString("smithname", it.Itemstack.Attributes.GetString("smithname"));
                        return;
                    }
                    if(it.Itemstack.Item.Code.Domain.StartsWith("xmelee"))
                    {
                        twoAuthors = it.Itemstack.Attributes.GetString("smithname");
                        continue;
                    }
                    outputSlot.Itemstack.Attributes.SetString("smithname", it.Itemstack.Attributes.GetString("smithname"));
                    return;
                }
            }
        }
    }

}