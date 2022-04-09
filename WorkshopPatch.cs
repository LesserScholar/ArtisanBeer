using HarmonyLib;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;

namespace ArtisanBeer
{
    [HarmonyPatch(typeof(WorkshopsCampaignBehavior), "RunTownWorkshop")]
    public class WorkshopPatch
    {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var getResultNumber = AccessTools.Method(typeof(ExplainedNumber), "get_ResultNumber");
			var found = false;
			foreach (var instruction in instructions)
			{
				yield return instruction;
				if (instruction.opcode == OpCodes.Call && instruction.operand == (object)getResultNumber)
				{
					if (found)
						throw new ArgumentException("Found multiple ExplainedNumber::get_ResultNumber in WorkshopsCampaignBehavior.RunTownWorkshop");

					// ... * ArtisanBeerBehavior.WorkshopProductionEfficiency(workshop)
					yield return new CodeInstruction(OpCodes.Ldarg_2, null);
					yield return new CodeInstruction(OpCodes.Call,
						AccessTools.Method(typeof(ArtisanBeerBehavior), nameof(ArtisanBeerBehavior.WorkshopProductionEfficiency)));
					yield return new CodeInstruction(OpCodes.Mul, null);
					found = true;
				}
			}
			if (found is false)
				throw new ArgumentException("Cannot find ExplainedNumber::get_ResultNumber in WorkshopsCampaignBehavior.RunTownWorkshop");
		}
	}
}
