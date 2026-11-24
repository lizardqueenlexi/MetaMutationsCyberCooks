using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using XRL.Language;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.UI;
using DaylightMurder.Effects;
using DaylightMurder.Parts;

namespace XRL.World.Parts
{
    public class DaylightMurder_CyberneticsMorselMembrane : IPart
    {
        public Guid ActivatedAbilityId = Guid.Empty;
        public string CommandId;
        public override bool WantEvent(int ID, int cascade)
        {
            return
                base.WantEvent(ID, cascade)
                || ID == ImplantedEvent.ID
                || ID == PartSupportEvent.ID
                || ID == UnimplantedEvent.ID
            ;
        }

        public override bool HandleEvent(ImplantedEvent @event)
        {
            @event.Actor.RequirePart<MorselMembraneAbility>();
            return base.HandleEvent(@event);
        }

        public override bool HandleEvent(PartSupportEvent @event)
        {
            if (@event.Type == MorselMembraneAbility.SUPPORT_TYPE && @event.Skip != this)
            {
                return false; // positive case
            }
            else
            {
                return base.HandleEvent(@event);
            }
        }

        public override bool HandleEvent(UnimplantedEvent @event)
        {
            NeedPartSupportEvent.Send(@event.Actor, Type: MorselMembraneAbility.SUPPORT_TYPE, Skip: this);
            return base.HandleEvent(@event);
        }
    }
}

namespace DaylightMurder.Parts
{
    public class MorselMembraneAbility : IPart
    {
        public const string SUPPORT_TYPE = "DaylightMurder_MorselMembrane";
        public static readonly string COMMAND_NAME = "DaylightMurder_CommandSubsume";
        public Guid ActivatedAbilityId = Guid.Empty;

        public override void Initialize()
        {
            ActivatedAbilityId = AddMyActivatedAbility(
                Name: "Subsume",
                Command: COMMAND_NAME,
                Class: "Cybernetics"
            );
            base.Initialize();
        }

        public override void Remove()
        {
            RemoveMyActivatedAbility(ref ActivatedAbilityId);
            base.Remove();
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return
                base.WantEvent(ID, cascade)
                || ID == CommandEvent.ID
                || ID == NeedPartSupportEvent.ID
            ;
        }

        public override bool HandleEvent(CommandEvent @event)
        {
            if (@event.Command == COMMAND_NAME)
            {
                var maxAmount = @event.Actor.GetInstalledCybernetics().FindAll(
                    item => item.HasPart<DaylightMurder_CyberneticsMorselMembrane>()
                ).Count;
                var inventory = @event.Actor.Inventory;
                var options = new List<string>();
                var items = new List<GameObject>();
                var stacks = new List<int>();
                var icons = new List<IRenderable>();

                foreach (var item in @event.Actor.GetInventory(
                    item => item.HasPart<PreparedCookingIngredient>()
                ))
                {
                    options.Add(item.DisplayName);
                    items.Add(item);
                    stacks.Add(item.Count);
                    icons.Add(item.Render);
                }

                List<(int Selected, int Amount)> choices;
                while (true)
                {
                    choices = Popup.PickSeveral(
                        Title: $"Choose up to {maxAmount} food item(s) to subsume.",
                        Options: options,
                        Stacks: stacks,
                        Icons: icons,
                        Amount: maxAmount,
                        AllowEscape: true
                    );

                    if (choices.IsNullOrEmpty())
                    {
                        break;
                    }

                    if (choices.Sum(choice => choice.Amount) > maxAmount)
                    {
                        Popup.Show($"You cannot select more than {Grammar.Cardinal(maxAmount)} options!");
                        continue;
                    }

                    var types = new List<string>();
                    foreach (var choice in choices)
                    {
                        var item = items[choice.Selected];
                        var amount = choice.Amount;
                        item.SplitStack(amount);

                        for (var n = 0; n < amount; n++)
                        {
                            var ingredient = item.GetPart<PreparedCookingIngredient>();
                            types.Add(ingredient.GetTypeInstance());
                        }
                    }

                    var effect = new Micrometabolizing(ProceduralCookingEffect.CreateJustUnits(types));
                    effect.Init(@event.Actor);
                    @event.Actor.ApplyEffect(effect);
                    break;
                }
            }

            return base.HandleEvent(@event);
        }

        public override bool HandleEvent(NeedPartSupportEvent @event)
        {
            if (@event.Type == SUPPORT_TYPE && !PartSupportEvent.Check(@event, this))
            {
                ParentObject.RemovePart(this);
            }
            return base.HandleEvent(@event);
        }
    }
}

namespace DaylightMurder.Effects
{
    class Micrometabolizing : ProceduralCookingEffect
    {
        public Micrometabolizing(ProceduralCookingEffect Original)
        {
            DisplayName = "{{c|micrometabolizing}}";

            foreach (var unit in Original.units)
            {
                units.Add(unit);
            }
        }

        public override string GetDescription()
        {
            return DisplayName;
        }

        public override bool FireEvent(Event @event)
        {
            if (@event.ID is "ClearFoodEffects" or "RemoveProceduralCookingEffects")
            {
                return true;
            }
            return base.FireEvent(@event);
        }
    }
}
