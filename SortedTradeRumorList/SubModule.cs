using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SortedTradeRumorList
{
	public class SubModule : MBSubModuleBase
	{
		protected override void OnBeforeInitialModuleScreenSetAsRoot()
		{
			Harmony harmony = new Harmony("SortedTradeRumorList");
			harmony.PatchAll();			
			base.OnBeforeInitialModuleScreenSetAsRoot();
		}
	}
}
