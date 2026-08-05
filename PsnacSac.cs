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
        public ExtraMeal ActiveEffect;

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
            if (IsImprinted())
            {
                ApplyEffect(@event.Actor);
            } else
            {
                ActivatedAbilityId = @event.Implantee.AddDynamicCommand(
                    Name: "Imprint a metabolizing effect",
                    Command: out CommandId,
                    CommandForDescription: "CommandActivatePsnacSacImprinting",
                    Class: "Cybernetics"
                );
            }
            return base.HandleEvent(@event);
        }

        public override bool HandleEvent(CommandEvent @event)
        {
            if (@event.Command != null && @event.Command == CommandId && @event.Actor == ParentObject.Implantee)
            {
                Imprint(@event.Actor);
            }
            return base.HandleEvent(@event);
        }

        public override bool HandleEvent(UnimplantedEvent @event)
        {
            RemoveAbility(@event.Actor);
            RemoveEffect(@event.Actor);
            return base.HandleEvent(@event);
        }

        public bool Imprint(GameObject target)
        {
            var effect = ParentObject.Implantee.GetEffect<ProceduralCookingEffect>();
            if (effect == null)
            {
                Popup.Show("Withering hunger briefly tugs at your gut from within.");
                target.ApplyEffect(new Famished());
                return false;
            }
            Popup.Show("You feel a dissatisfying suction as your satiety is syphoned away.");
            ImprintedEffect = effect.DeepCopy(ParentObject.Implantee) as ProceduralCookingEffect;
            ParentObject.Implantee.RemoveEffect(effect);
            Popup.Show("Distilled satisfaction permeates your metabolism.");
            ApplyEffect(target);
            RemoveAbility(ParentObject.Implantee);
            return true;
        }

        public bool IsImprinted() => ImprintedEffect != null;

        // All of the following can be safely called even if the cybernetic is
        // in the wrong mode for it.

        public bool RemoveAbility(GameObject target)
        {
            return target.RemoveActivatedAbility(ref ActivatedAbilityId);
        }

        public bool ApplyEffect(GameObject target)
        {
            if (ImprintedEffect == null)
            {
                return false;
            }
            var effect = new ExtraMeal(ImprintedEffect);
            effect.Init(target);
            var success = target.ApplyEffect(effect);
            if (success)
            {
                ActiveEffect = effect;
            }
            return success;
        }

        public bool RemoveEffect(GameObject target)
        {
            if (ActiveEffect != null)
            {
                return target.RemoveEffect(ActiveEffect);
            }
            return true;
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
