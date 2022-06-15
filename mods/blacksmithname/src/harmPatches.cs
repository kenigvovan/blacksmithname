using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public static bool Prefix_CheckIfFinished(Vintagestory.GameContent.BlockEntityAnvil __instance, IPlayer byPlayer)
        {
            MethodInfo dynMethod = __instance.GetType().GetMethod("MatchesRecipe",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(__instance, new object[] { });
            if (__instance.SelectedRecipe == null || /*!__instance.MatchesRecipe()*/!((bool)dynMethod.Invoke(__instance, new object[] { })) || !(__instance.Api.World is IServerWorldAccessor))
                return false;
            __instance.Voxels = new byte[16, 6, 16];
            ItemStack itemstack = __instance.SelectedRecipe.Output.ResolvedItemstack.Clone();
            __instance.SelectedRecipeId = -1;
            //typeof(Vintagestory.GameContent.BlockEntityAnvil).GetField("SelectedRecipe", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, null);
            var itSt = (ItemStack)typeof(Vintagestory.GameContent.BlockEntityAnvil).GetField("workItemStack", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            itemstack.Collectible.SetTemperature(__instance.Api.World, itemstack, /*__instance.workItemStack*/ itSt.Collectible.GetTemperature(__instance.Api.World, /*__instance.workItemStack*/itSt));
            /*__instance.workItemStack*/
           // var t = typeof(Vintagestory.GameContent.BlockEntityAnvil).GetField("workItemStack", BindingFlags.NonPublic | BindingFlags.Instance);
            //var t = typeof(Vintagestory.GameContent.BlockEntityAnvil).GetField("workItemStack", BindingFlags.NonPublic | BindingFlags.Instance).GetValue();
            typeof(Vintagestory.GameContent.BlockEntityAnvil).GetField("workItemStack", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, null);

            if(byPlayer != null)
            {
                if(itemstack.Item != null && (itemstack.Item.Shape.Base.Path.StartsWith("item/tool") || itemstack.Item.Shape.Base.Path.StartsWith("item/spytube")))
                {
                    itemstack.Attributes.SetString("smithname", byPlayer.PlayerName);
                }
            }
               

            if (byPlayer != null && byPlayer.InventoryManager.TryGiveItemstack(itemstack)) 
                __instance.Api.World.PlaySoundFor(new AssetLocation("sounds/player/collect"), byPlayer, false, 24f);
            else
                __instance.Api.World.SpawnItemEntity(itemstack, __instance.Pos.ToVec3d().Add(0.5, 0.5, 0.5));

            dynMethod = __instance.GetType().GetMethod("RegenMeshAndSelectionBoxes",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(__instance, new object[] { });
            //__instance.RegenMeshAndSelectionBoxes();
            __instance.MarkDirty();
            __instance.Api.World.BlockAccessor.MarkBlockDirty(__instance.Pos);
            __instance.rotation = 0;
            return false;
        }
        
        public static void Postfix_GetHeldItemInfo(Vintagestory.API.Common.CollectibleObject __instance, ItemSlot inSlot,
      StringBuilder dsc,
      IWorldAccessor world,
      bool withDebugInfo)
        {
            ItemStack itemstack = inSlot.Itemstack;
            string smithName = itemstack.Attributes.GetString("smithname");
            if(smithName != null && !(smithName.Length == 0))
            {                
                dsc.Append(Lang.Get(blacksmithname.getID() + ":smithed_by", "<font color=\"" + Lang.Get(blacksmithname.getID() + ":playername_color") + "\">" + smithName + "</strong>")).Append("\n");
            }
            return;
        }

        public static void Prefix_OnCreatedByCrafting(Vintagestory.API.Common.CollectibleObject __instance, ItemSlot[] allInputslots, ItemSlot outputSlot, GridRecipe byRecipe)
        {

            if (!(outputSlot.Itemstack?.Item?.Shape?.Base?.Path?.StartsWith("item/tool")).HasValue ||
                !(outputSlot.Itemstack?.Item?.Shape?.Base?.Path?.StartsWith("item/tool")).Value)
            {
                return;
            }              

            foreach(var it in allInputslots)
            {
                if(it.Itemstack != null && it.Itemstack.Attributes.GetString("smithname")!=null)
                {
                    outputSlot.Itemstack.Attributes.SetString("smithname", it.Itemstack.Attributes.GetString("smithname"));
                    return;
                }
            }         
        }         
    }
}
