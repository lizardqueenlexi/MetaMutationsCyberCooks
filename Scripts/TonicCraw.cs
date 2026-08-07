using System;
using System.Security.Policy;
using HistoryKit;
using XRL.Collections;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation
{

    [Serializable]
    public class TonicCraw : BaseDefaultEquipmentMutation
    {

        public const int TONIC_CRAW_BASE_CAPACITY = 2;
        public const int ALLERGY_NEGATE_CHANCE_PER_LEVEL = 8;
        public Guid StoreTonicActivatedAbilityID = Guid.Empty;
        public Guid GulpActivatedAbilityID = Guid.Empty;
        public GameObject Crawbject;
        public string BodyPartType = "Body";

        [NonSerialized]
        protected GameObjectBlueprint _Blueprint;

        public override string GetDescription()
        {
            return "You may consume and store tonics in your craw in bulk.";
        }

        private static int GetCrawCapacity(int Level)
        {
            return TONIC_CRAW_BASE_CAPACITY + (Level/2);
        }

        private static int GetNegateChance(int Level)
        {
            return ALLERGY_NEGATE_CHANCE_PER_LEVEL * Level;
        }

        public override string GetLevelText(int Level)
        {
            return "You may fill your craw with up to {{rules|" + GetCrawCapacity(Level) + "}} tonics.\n"
                + "You may milk your craw to apply all stored tonics simultaneously.\n"
                + "{{rules|" + GetNegateChance(Level) + "}}% chance to negate adverse reactions to tonics.\n"
                + "+50 reputation with {{w|birds}}, {{w|insects}}, {{w|worms}}, and {{w|mollusks}}.";
        }

		public override bool WantEvent(int ID, int cascade)
		{
			return base.WantEvent(ID, cascade)
                || ID == CommandEvent.ID;
		}
        public override bool HandleEvent(CommandEvent E)
        {
            // Stow a tonic in the craw. Largely copied from medassist module.
            if (E.Command == "CommandStoreTonic")
            {
                Inventory inventory = Crawbject.Inventory;
                if (inventory == null)
                {
                    throw new Exception("inventory missing from " + Crawbject.DebugName);
                }
                if (GetLoadedCount() >= GetCrawCapacity(Level))
                {
                    E.Actor.Fail("Your craw is full.");
                }
                else
                {
                    using ScopeDisposedList<GameObject> scopeDisposedList = ScopeDisposedList<GameObject>.GetFromPool();
                    using ScopeDisposedList<GameObject> scopeDisposedList2 = ScopeDisposedList<GameObject>.GetFromPool();
                    E.Actor.GetContents(scopeDisposedList2);
                    foreach (GameObject item in scopeDisposedList2)
                    {
                        if (CanBeLoaded(item))
                        {
                            BodyPart BodyPartContext;
                            int Relation;
                            IContextRelationManager RelationManager;
                            GameObject objectContext = item.GetObjectContext(out BodyPartContext, out Relation, out RelationManager);
                            if (objectContext != null && objectContext != Crawbject && Relation != 4 && Relation != 6)
                            {
                                scopeDisposedList.Add(item);
                            }
                        }
                    }
                    if (scopeDisposedList.Count <= 0)
                    {
                        E.Actor.Fail("You have no tonics to load.");
                    }
                    else
                    {
                        GameObject gameObject2 = PickItem.ShowPicker(scopeDisposedList, null, PickItem.PickItemDialogStyle.SelectItemDialog, E.Actor, null, null, null, PreserveOrder: false, null, ShowContext: true);
                        if (gameObject2 != null)
                        {
                            E.Actor.PlayWorldSound("Sounds/Interact/sfx_interact_medassistModule_tonic_load");
                            gameObject2.SplitFromStack();
                            gameObject2.RemoveFromContext();
                            inventory.AddObject(gameObject2);
                            if (E.Actor.IsPlayer())
                            {
                                Popup.Show("You squirt " + gameObject2.an() + " into your craw.");
                            }
                            E.Actor.UseEnergy(1000);
                            E.RequestInterfaceExit();
                        }
                    }
                }
            }
            else if (E.Command == "CommandGulpTonics")
            {
                Inventory inv= Crawbject.Inventory;
                if (inv == null || inv.Objects.Count <= 0)
                {
                    E.Actor.Fail("Your craw is empty!");
                }
                else
                {
                    GameObject who = ParentObject;
                    if (who.IsPlayer())
                    {
                        AddPlayerMessage("You gulp the tonic milk from your craw.");
                    }
                    while(inv.Objects.Count > 0)
                    {
                        GameObject nextTonic = inv.Objects[0];
                        for (int k = 0, l = nextTonic.Count; k < l; k++)
                        {
                            int dosage = GetTonicDosageEvent.GetFor(
                                Object: nextTonic,
                                Subject: who,
                                Actor: ParentObject
                            );
                            if (dosage <= 0)
                            {
                                nextTonic.Destroy();
                                continue;
                            }
                            Event eApplyingTonic = Event.New("ApplyingTonic");
                            eApplyingTonic.SetParameter("Subject", who);
                            eApplyingTonic.SetParameter("Actor", Crawbject);
                            eApplyingTonic.SetParameter("Tonic", nextTonic);
                            eApplyingTonic.SetParameter("Dosage", dosage);
                            eApplyingTonic.SetFlag("External", false);
                            eApplyingTonic.SetFlag("Involuntary", false);
                            if (!who.FireEvent(eApplyingTonic))
                            {
                                if (who.IsPlayer())
                                {
                                    AddPlayerMessage("Nothing happens.");
                                }
                                nextTonic.Destroy();
                                continue;
                            }
                            Event eApplyTonic = Event.New("ApplyTonic");
                            eApplyTonic.SetParameter("Owner", Crawbject);
                            eApplyTonic.SetParameter("Target", who);
                            eApplyTonic.SetParameter("Actor", Crawbject);
                            eApplyTonic.SetParameter("Subject", who);
                            eApplyTonic.SetParameter("Attacker", (GameObject) null);
                            eApplyTonic.SetParameter("Overdose", "No");
                            eApplyTonic.SetParameter("Dosage", dosage);
                            eApplyTonic.SetFlag("External", false);
                            eApplyTonic.SetFlag("Involuntary", false);
                            bool setOverdosing = GetOverdose();
                            if (setOverdosing)
                            {
                                who.SetLongProperty("Overdosing", 1);
                            }
                            if (!nextTonic.FireEvent(eApplyTonic))
                            {
                                if (who.IsPlayer())
                                {
                                    AddPlayerMessage("Nothing happens.");
                                }
                                nextTonic.Destroy();
                                continue;
                            }
                            if (setOverdosing)
                            {
                                who.SetLongProperty("Overdosing", 0);
                            }
                            nextTonic.Destroy();
                        }
                    }
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

        public override void OnRegenerateDefaultEquipment(Body body)
        {
            if (!TryGetRegisteredSlot(body, BodyPartType, out var Part))
            {
                Part = body.GetFirstPart(BodyPartType);
                if (Part != null)
                {
                    RegisterSlot(BodyPartType, Part);
                }
            }
            if (Part != null)
            {
                Crawbject = GameObjectFactory.Factory.CreateObject(Variant ?? "TonicCraw");
                Armor part = Crawbject.GetPart<Armor>();
                part.WornOn = Part.Type;
                part.AV = 0;
                Part.DefaultBehavior = Crawbject;
                Part.DefaultBehavior.SetStringProperty("TemporaryDefaultBehavior", "TonicCraw");
                ResetDisplayName();
            }
            base.OnRegenerateDefaultEquipment(body);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            StoreTonicActivatedAbilityID = AddMyActivatedAbility("Fill Craw", "CommandStoreTonic", "Physical Mutations", null, "\r");
            GulpActivatedAbilityID = AddMyActivatedAbility("Milk Craw", "CommandGulpTonics", "Physical Mutations", null, "\r");
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref StoreTonicActivatedAbilityID);
            RemoveMyActivatedAbility(ref GulpActivatedAbilityID);
            return base.Unmutate(GO);
        }

        public static bool CanBeLoaded(GameObject obj)
        {
            if (obj.IsRusted() || obj.IsBroken())
            {
                return false;
            }
            Tonic part = obj.GetPart<Tonic>();
            if (part == null)
            {
                return false;
            }
            if (part.Eat)
            {
                return false;
            }
            return true;
        }

        public int GetLoadedCount()
        {
            int num = 0;
            Inventory inventory = Crawbject.Inventory;
            if (inventory != null)
            {
                foreach (GameObject @object in inventory.Objects)
                {
                    num += @object.Count;
                }
            }
            return num;
        }

        public bool GetOverdose()
        {
            GameObject subject = ParentObject;
            if (subject.IsMutant())
            {
                int chance = 5;
                int negateChance = GetNegateChance(Level);
                if (subject.HasPart<TonicAllergy>())
                {
                    chance = 33;
                }
                if (subject.IsMutant() && chance.in100() && !negateChance.in100())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
