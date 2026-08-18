using System;
using XRL.World;
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
            } else
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
                AddPlayerMessage("TODO: morsel membrane");
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
