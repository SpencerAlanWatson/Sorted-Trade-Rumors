using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SortedTradeRumorList
{
	public class SubModule : MBSubModuleBase
	{
		protected override void OnBeforeInitialModuleScreenSetAsRoot()
		{
			Harmony testHarmony = new Harmony("SortedTradeRumorList");
			testHarmony.PatchAll();			
			base.OnBeforeInitialModuleScreenSetAsRoot();
		}
	}
}
