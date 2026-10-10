using System;

namespace XRL.World.Parts
{
	[Serializable]
	public class DaylightMurder_CyberneticsInternalApex : IPoweredPart
	{
		public DaylightMurder_CyberneticsInternalApex()
		{
			WorksOnImplantee = true;
			ChargeUse = 0;
			NameForStatus = "InternalApex";
		}

		public override bool SameAs(IPart P) => false;

		public override bool WantEvent(int ID, int Cascade)
			=> base.WantEvent(ID, Cascade)
			|| ID == RespiresEvent.ID
			|| ID == InduceVomitingEvent.ID
			|| ID == GetSpecialEffectChanceEvent.ID
			|| ID == WaterRitualStartEvent.ID
			|| ID == GetWaterRitualReputationAmountEvent.ID;

		public override bool HandleEvent(RespiresEvent E)
		{
			if (IsObjectActivePartSubject(E.Object) && IsReady(UseCharge: true))
			{
				return false;
			}

			return base.HandleEvent(E);
		}

		public override bool HandleEvent(InduceVomitingEvent E)
		{
			// TODO: check if this happens early enough
			if (IsObjectActivePartSubject(E.Object) && IsReady(UseCharge: true))
			{
				E.Vomited = false;
				E.InterfaceExit = false;
				E.MessageHolder?.Clear();
				return false;
			}

			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetSpecialEffectChanceEvent E)
		{
			if (IsObjectActivePartSubject(E.Subject) && IsAffectedDrinkType(E.Type) && IsReady(UseCharge: true))
			{
				E.Chance = 0;
				return false;
			}

			return base.HandleEvent(E);
		}

		public static bool IsAffectedDrinkType(string Type)
			=> Type.HasDelimitedSubstring(' ', "QuenchOnHit")
			|| Type.HasDelimitedSubstring(' ', "ThirstOnHit")
			|| Type.HasDelimitedSubstring(' ', "DrunkOnHit")
			|| Type.HasDelimitedSubstring(' ', "VomitOnHit");

		public override bool HandleEvent(WaterRitualStartEvent E)
		{
			if (IsObjectActivePartSubject(E.Actor) && !E.Record.Has("DMUsedInternalApex") && E.Actor.IsPlayer() && IsReady(UseCharge: true))
			{
				E.Record.attributes.Add("DMUsedInternalApex");
				The.Game.PlayerReputation.Modify(
					Faction: E.Record.faction,
					Amount: Skill.Customs_Tactful.GetBonusFor(E.SpeakingWith),
					Type: "WaterRitualInternalApexMalus"
				);
			}

			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetWaterRitualReputationAmountEvent E)
		{
			if (IsObjectActivePartSubject(E.Actor) && !E.Record.Has("DMUsedInternalApex") && E.Actor.IsPlayer() && IsReady(UseCharge: true) && E.Faction == E.Record.faction)
			{
				E.Record.attributes.Add("DMUsedInternalApex");
				E.Amount += Skill.Customs_Tactful.GetBonusFor(E.SpeakingWith);
			}

			return base.HandleEvent(E);
		}
	}
}
