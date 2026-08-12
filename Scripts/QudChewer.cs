using System;
using System.Security.Policy;
using HistoryKit;
using XRL.Collections;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation
{

    [Serializable]
    public class QudChewer : BaseMutation
    {

        public override string GetDescription()
        {
            return "Your labyrinthine guts store and slow-churn multiple meals.";
        }

        public override string GetLevelText(int Level)
        {
            return "PLACEHOLDER";
        }

		public override bool WantEvent(int ID, int cascade)
		{
			return base.WantEvent(ID, cascade)
                || ID == CommandEvent.ID;
		}
        public override bool HandleEvent(CommandEvent E)
        {
            if (E.Command == "CommandStoreMeal")
            {
                
            }
            return base.HandleEvent(E);
        }

        public override bool FireEvent(Event E)
        {
            return base.FireEvent(E);
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            ActivatedAbilityID = AddMyActivatedAbility("Ruminate", "CommandStoreMeal", "Physical Mutations", null, "\r");
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref ActivatedAbilityID);
            return base.Unmutate(GO);
        }
    }
}
