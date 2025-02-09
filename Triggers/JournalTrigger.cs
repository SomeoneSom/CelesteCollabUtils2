﻿using Celeste.Mod.CollabUtils2.Entities;
using Celeste.Mod.CollabUtils2.UI;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.CollabUtils2.Triggers {
    [CustomEntity("CollabUtils2/JournalTrigger")]
    public class JournalTrigger : Trigger {
        private static bool showOnlyDiscovered;
        private static bool vanillaJournal;

        internal static void Load() {
            Everest.Events.Journal.OnEnter += onJournalEnter;
        }

        internal static void Unload() {
            Everest.Events.Journal.OnEnter -= onJournalEnter;
        }

        private static void onJournalEnter(OuiJournal journal, Oui from) {
            // if using the vanilla journal, we just don't have anything to do, since vanilla already did everything for us!
            if (vanillaJournal)
                return;

            AreaData forceArea = journal.Overworld == null ? null : new DynData<Overworld>(journal.Overworld).Get<AreaData>("collabInGameForcedArea");
            if (forceArea != null) {
                // custom journal: throw away all pages, except the cover.
                for (int i = 0; i < journal.Pages.Count; i++) {
                    if (journal.Pages[i].GetType() != typeof(OuiJournalCover)) {
                        journal.Pages.RemoveAt(i);
                        i--;
                    }
                }

                // then, fill in the journal with our custom pages.
                journal.Pages.AddRange(OuiJournalCollabProgressInLobby.GeneratePages(journal, forceArea.GetLevelSet(), showOnlyDiscovered));

                // and add the map if we have it as well.
                if (MTN.Journal.Has("collabLobbyMaps/" + forceArea.GetLevelSet())) {
                    journal.Pages.Add(new OuiJournalLobbyMap(journal, MTN.Journal["collabLobbyMaps/" + forceArea.GetLevelSet()]));
                }
            }
        }

        public string levelset;

        private readonly TalkComponent talkComponent;

        public JournalTrigger(EntityData data, Vector2 offset)
            : base(data, offset) {
            levelset = data.Attr("levelset");

            Add(talkComponent = new TalkComponent(
                new Rectangle(0, 0, data.Width, data.Height),
                data.Nodes.Length != 0 ? (data.Nodes[0] - data.Position) : new Vector2(data.Width / 2f, data.Height / 2f),
                player => {
                    showOnlyDiscovered = data.Bool("showOnlyDiscovered", defaultValue: false);
                    vanillaJournal = data.Bool("vanillaJournal", defaultValue: false);
                    InGameOverworldHelper.OpenJournal(player, levelset);
                }
            ) { PlayerMustBeFacing = false });
        }

        public override void Update() {
            base.Update();
            talkComponent.Enabled = !InGameOverworldHelper.IsOpen;
        }

    }
}
