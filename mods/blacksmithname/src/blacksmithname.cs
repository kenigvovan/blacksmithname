using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;


namespace blacksmithname.src
{
    public class blacksmithname: ModSystem
    {
        public static ICoreServerAPI api;
        public static Harmony harmonyInstance;
        public const string harmonyID = "blacksmithname.Patches";

        public static string getID()
        {
            return "blacksmithname";
        }
        public static void doPathches()
        {
            //DoPlaceBlock
            harmonyInstance = new Harmony(harmonyID);
            harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockEntityAnvil).GetMethod("CheckIfFinished"), transpiler: new HarmonyMethod(typeof(harmPatches).GetMethod("Transpiler_check")));
            harmonyInstance.Patch(typeof(Vintagestory.API.Common.CollectibleObject).GetMethod("GetHeldItemInfo"), postfix: new HarmonyMethod(typeof(harmPatches).GetMethod("Postfix_GetHeldItemInfo")));
            harmonyInstance.Patch(typeof(Vintagestory.API.Common.CollectibleObject).GetMethod("OnCreatedByCrafting"), postfix: new HarmonyMethod(typeof(harmPatches).GetMethod("Prefix_OnCreatedByCrafting")));
        }
        public override void Start(ICoreAPI api)
        {           
            doPathches();
        }
        public override void Dispose()
        {
            harmonyInstance.UnpatchAll();
        }
    }
}
