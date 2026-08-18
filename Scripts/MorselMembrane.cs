using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL;
using XRL.World;
using DaylightMurder.Parts;
using XRL.World.Parts;
using XRL.UI;

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
                var amount = @event.Actor.GetInstalledCybernetics().FindAll(
                    item => item.HasPart<DaylightMurder_CyberneticsMorselMembrane>()
                ).Count;
                var inventory = @event.Actor.Inventory;
                var options = new List<string>();
                var items = new List<GameObject>();
                var icons = new List<IRenderable>();

                foreach (var item in @event.Actor.GetInventory(
                    item => item.HasPart<PreparedCookingIngredient>()
                ))
                {
                    options.Add(item.DisplayName);
                    items.Add(item);
                    icons.Add(item.Render);
                }

                var chosenItems = Popup.PickSeveral(
                    Title: $"Choose up to {amount} food item(s) to subsume.",
                    Options: options,
                    Icons: icons,
                    Amount: amount,
                    AllowEscape: true
                );

                AddPlayerMessage("TODO: pseudometabolize");
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
