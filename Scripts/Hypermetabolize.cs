using System;

namespace XRL.World.Parts.Mutation
{

    [Serializable]
    public class Hypermetabolize : BaseMutation
    {

        public const int HEALING_PERCENT_PER_STAGE = 8;

        public static readonly int COOLDOWN = 75;

        public override string GetDescription()
        {
            return "You may forcefully burn through your body's fuel to rapidly heal.";
        }

        public override string GetLevelText(int Level)
        {
            return "PLACEHOLDER";
        }

        public override bool CanLevel()
        {
            return false;
        }

		public override bool WantEvent(int ID, int cascade)
		{
			return base.WantEvent(ID, cascade)
                || ID == CommandEvent.ID;
		}
        public override bool HandleEvent(CommandEvent E)
        {
            if (E.Command == "CommandMetabolize")
            {
                //check for stomach
                Stomach ourStomach = ParentObject.GetPart<Stomach>();
                if (ourStomach != null)
                {
                    //calculate number of thirst levels
                    float waterLevel = ourStomach.Water;
                    int thirstLevels = (int)Math.Ceiling(waterLevel / 10000);
                    
                    //set water to 0
                    ourStomach.Water = 0;
                    
                    //calculate number of hunger levels
                    int foodLevels = 2 - ourStomach.HungerLevel;
                    
                    //set hunger to max
                    ourStomach.CookingCounter = ourStomach.CalculateCookingIncrement()*2;
                    ourStomach.UpdateHunger();
                    
                    //heal max hp * (hunger levels + thirst levels) * 0.05
                    float healPercent = (thirstLevels + foodLevels) * (float)HEALING_PERCENT_PER_STAGE / 100;
                    int healAmount = (int)(ParentObject.baseHitpoints * healPercent);
                    ParentObject.Heal(healAmount);

                    if(ParentObject.IsPlayer())
                    {
                        AddPlayerMessage("PLACEHOLDER");
                    }

                    CooldownMyActivatedAbility(ActivatedAbilityID, COOLDOWN);
                }
                
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
            ActivatedAbilityID = AddMyActivatedAbility("Hypermetabolize", "CommandMetabolize", "Mental Mutations", null, "\r");
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref ActivatedAbilityID);
            return base.Unmutate(GO);
        }

    }
}
