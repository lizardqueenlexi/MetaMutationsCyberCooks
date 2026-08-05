using System;
using XRL.UI;
using XRL.World;
using XRL.World.Effects;
using DaylightMurder.Effects;

namespace XRL.World.Parts
{
    class DaylightMurder_CyberneticsPsnacSac : IPart
    {
        public Guid ActivatedAbilityId = Guid.Empty;
        public string CommandId;
        public ProceduralCookingEffect ImprintedEffect;

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
            ImprintedEffect = effect.DeepCopy(ParentObject.Implantee) as ProceduralCookingEffect;
            ParentObject.Implantee.RemoveEffect(effect);
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
            var effect = new ExtraMeal(ImprintedEffect);
            effect.Init(ParentObject.Implantee);
            return ParentObject.Implantee.ApplyEffect(effect);
        }
    }
}

namespace DaylightMurder.Effects
{
    class ExtraMeal : ProceduralCookingEffect
    {
        public ExtraMeal(ProceduralCookingEffect Original)
        {
            DisplayName = "{{c|pseudometabolizing}}";
            units = Original.units;
        }

        public override string GetDescription()
        {
            return DisplayName;
        }

        // There are various circumstances under which ProceduralCookingEffect
        // is checked for expiry; we will instead not be checking any of them.
        public override bool HandleEvent(AfterGameLoadedEvent E)
        {
            return true;
        }
        public override bool HandleEvent(AIBoredEvent E)
        {
            return true;
        }

        public override bool HandleEvent(ZoneThawedEvent E)
        {
            return true;
        }

        public override bool FireEvent(Event @event)
        {
            if (@event.ID is "ApplyWellFed" or "BecameHungry" or "BecameFamished" or "ClearFoodEffects" or "RemoveProceduralCookingEffects")
            {
                return true;
            }
            return base.FireEvent(@event);
        }
    }
}
