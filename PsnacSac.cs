using System;
using XRL.UI;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.Parts
{
    class DaylightMurder_CyberneticsPsnacSac : IPart
    {
        public Guid ActivatedAbilityId = Guid.Empty;
        public string CommandId;
        public ProceduralCookingEffect MealEffect;

        public override bool WantEvent(int ID, int cascade)
        {
            return
                base.WantEvent(ID, cascade)
                || ID == ImplantedEvent.ID
                || ID == CommandEvent.ID
                || ID == UnimplantedEvent.ID
            ;
        }

        public override bool HandleEvent(ImplantedEvent @event)
        {
            AddPlayerMessage("implanted!");
            ActivatedAbilityId = @event.Implantee.AddDynamicCommand(
                Name: "Imprint a metabolizing effect",
                Command: out CommandId,
                CommandForDescription: "CommandActivatePsnacSacImprinting",
                Class: "Cybernetics"
            );
            return base.HandleEvent(@event);
        }

        public override bool HandleEvent(CommandEvent @event)
        {
            if (@event.Command != null && @event.Command == CommandId && @event.Actor == ParentObject.Implantee)
            {
                Imprint();
            }
            return base.HandleEvent(@event);
        }

        public override bool HandleEvent(UnimplantedEvent @event)
        {
            AddPlayerMessage("unimplanted!");
            RemoveAbility(@event.Actor);
            return base.HandleEvent(@event);
        }

        public bool Imprint()
        {
            var effect = ParentObject.Implantee.GetEffect<ProceduralCookingEffect>();
            if (effect == null)
            {
                Popup.Show("Withering hunger briefly tugs at your gut from within.");
                // TODO: Make you famished
                return false;
            }
            Popup.Show("You feel a dissatisfying suction as your satiety is syphoned away.");
            ParentObject.Implantee.RemoveEffect(effect);
            MealEffect = effect.DeepCopy(ParentObject.Implantee) as ProceduralCookingEffect;
            Popup.Show("Distilled satisfaction permeates your metabolism.");
            ApplyMealEffect();
            RemoveAbility(ParentObject.Implantee);
            return true;
        }

        public bool RemoveAbility(GameObject target)
        {
            return target.RemoveActivatedAbility(ref ActivatedAbilityId);
        }

        public bool ApplyMealEffect()
        {
            return ParentObject.Implantee.ApplyEffect(new DaylightMurder_ExtraMeal(MealEffect));
        }
    }
}

class DaylightMurder_ExtraMeal : ProceduralCookingEffect
{
    public DaylightMurder_ExtraMeal(ProceduralCookingEffect Original)
    {
        DisplayName = "pseudometabolizing";
        units = Original.units;
    }
}
