using System.Collections.Generic;
using HarmonyLib;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection;

namespace SortedTradeRumorList.Patches
{
	// Through testing I found that this entry point gets called at the right time and only once per time needed.
	[HarmonyPatch(typeof(ItemMenuVM), "SetMerchandiseComponentTooltip")]
	public static class ItemMenuVMPatch
	{
		public static Color SuccessTextColor = Color.FromUint((uint)0x4BB543);
		public static int getPrice(string valueLabelPrice)
		{
			if (valueLabelPrice == null) return 0;
			int price = 0;
			bool parseSuccess = false;
			bool hasBuy = valueLabelPrice.Contains("buy");
			bool hasSell = valueLabelPrice.Contains("sell");
			if (hasBuy || hasSell)
			{
				int lastSpaceIndex = valueLabelPrice.LastIndexOf(" ");
				string priceString = valueLabelPrice.Substring(lastSpaceIndex);
				parseSuccess = int.TryParse(priceString, out price);
			}
			price = parseSuccess && hasSell ? 0 - price : price;
			return price;
		}

		public static int Compare(ItemMenuTooltipPropertyVM x, ItemMenuTooltipPropertyVM y)
		{
			string firstValueLabel = x.ValueLabel;
			string secondValueLabel = y.ValueLabel;

			int firstPrice = getPrice(firstValueLabel);
			int secondPrice = getPrice(secondValueLabel);
			int result = firstPrice - secondPrice;
			return result;
		}

		public static void Postfix(ItemMenuVM __instance)
		{
			List<ItemMenuTooltipPropertyVM> allTooltips = new List<ItemMenuTooltipPropertyVM>();
			List<ItemMenuTooltipPropertyVM> nonPriceTooltips = new List<ItemMenuTooltipPropertyVM>();
			int lowestBuyPrice = 0;
			string lowestBuyString = "";
			int highestSellPrice = 0;
			string highestSellString = "";

			// In testing I found that attempting to modify the original list
			// Resulted in very spoty updates and a lot of issues
			// Going around the setter function entirely makde those issues disapper
			__instance.TargetItemProperties.Do<ItemMenuTooltipPropertyVM>((ItemMenuTooltipPropertyVM item) => {
			string text = item.ValueLabel;
				if (text.Contains("buy"))
				{
					allTooltips.Add(item);
					int itemPrice = getPrice(text);
					// The buy price for an item will never be zero in actual play.
					if (itemPrice < lowestBuyPrice || lowestBuyPrice == 0)
					{
						lowestBuyPrice = itemPrice;
						lowestBuyString = text;
					}
				}
				else if (text.Contains("sell"))
				{
					allTooltips.Add(item);

					int itemPrice = getPrice(text);
					// For the sorting fuction to sort things in the desired order
					// I make sell price negative
					if (MathF.Abs(itemPrice) > highestSellPrice)
					{
						highestSellPrice = MathF.Abs(itemPrice);
						highestSellString = text;
					}
				}
				//Doing it this way fixes the labels like "Trade Rumors" stay at the top where they belong
				else {
					nonPriceTooltips.Add(item);
				}

			});
			
			
			System.Comparison<ItemMenuTooltipPropertyVM> sortDelegate = Compare;
			allTooltips.Sort(sortDelegate);

			// After sorting I put the non trade rumor labels back at the top
			int counter = 0;
			foreach (ItemMenuTooltipPropertyVM item in nonPriceTooltips)
			{
				allTooltips.Insert(counter, item);
				counter++;
			}

			// Seperating the loops like this may not be super performant
			// but it makes the code a lot easier to understand which is a worthy trade off.
			List<(string, Color)> origianlTooltipInfo = new List<(string, Color)>();
			counter = 0;
			foreach (ItemMenuTooltipPropertyVM tooltip in allTooltips)
			{
				origianlTooltipInfo.Insert(counter, (tooltip.ValueLabel, tooltip.TextColor));
				counter++;
			}

			counter = 0;
			foreach ((string valueLabel, Color orignalColor) info in origianlTooltipInfo)
			{
				Color itemColor = info.orignalColor;
				// Very straight forward way of highlighting important information
				// While keeping the original information about how old the trade rumor is.
				if (info.valueLabel == lowestBuyString || info.valueLabel == highestSellString)
				{
					itemColor = new Color(SuccessTextColor.Red, SuccessTextColor.Green, SuccessTextColor.Blue, itemColor.Alpha);
				}
				__instance.TargetItemProperties[counter].ValueLabel = info.valueLabel;
				__instance.TargetItemProperties[counter].TextColor = itemColor;
				counter++;
			}
		}
	}

}
