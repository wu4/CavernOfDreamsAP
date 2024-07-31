using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Packets;

namespace CoDArchipelago.APClient
{

    class Client : InstantiateOnGameSceneLoad
    {
        public static Client Instance;
        ConcurrentQueue<Action> mainThreadQueue;
        MainThreadExecutor executor;
        // Timer jingleCooldown;

        class MainThreadExecutor : MonoBehaviour
        {
            void Update()
            {
                // Instance.jingleCooldown.Update();

                while (Instance.mainThreadQueue.TryDequeue(out var action)) {
                    action();
                }
            }
        }

        [HarmonyPatch(typeof(MO_QUIT_GAME), nameof(MO_QUIT_GAME.OnSelect))]
        static class OnQuit
        {
            static void Postfix(MO_QUIT_GAME __instance)
            {
                Instance.session = null;
                Instance = null;
            }
        }

        ArchipelagoSession session;
        string playerName = "wuffie";
        int slot;

        [LoadOrder(Int32.MaxValue)]
        public Client()
        {
            Instance = this;
            // jingleCooldown = new(60);
            // jingleCooldown.Reset();
            mainThreadQueue = new();
            session = ArchipelagoSessionFactory.CreateSession("localhost");
            GameObject manager = new GameObject("AP Connection Manager");
            executor = manager.AddComponent<MainThreadExecutor>();
            session.MessageLog.OnMessageReceived += OnMessageReceived;

            Initialize();
        }

        static string MessageColorAsHex(Archipelago.MultiClient.Net.Models.Color messageColor)
        {
            return "#" + BitConverter.ToString(new[] {messageColor.R, messageColor.G, messageColor.B}).Replace("-", "");
        }

        void OnMessageReceived(LogMessage message)
        {
            string messageString = "";

            foreach (var part in message.Parts) {
                messageString += $"<color=\"{MessageColorAsHex(part.Color)}\">";
                messageString += part.Text.Replace("<", "&lt;").Replace(">", "&gt;");
                messageString += "</color>";
            }

            mainThreadQueue.Enqueue(() => {
                Messaging.TextLogManager.AddLine(messageString);
            });
        }

        public void SendLocationCollected(long locationId)
        {
            session.Locations.CompleteLocationChecksAsync(new[]{locationId});
        }

        public void SendMessage(string message)
        {
            session.Say(message);
        }

        async void Initialize()
        {
            var connectResult = await session.ConnectAsync();

            InitializeItems();

            var loginResult = await session.LoginAsync(
                game: "Cavern of Dreams",
                name: playerName,
                itemsHandlingFlags: ItemsHandlingFlags.AllItems,
                version: new("0.5.0"),
                tags: new string[]{"DeathLink"},
                requestSlotData: true
            );

            if (!loginResult.Successful) {
                LoginFailure failure = (LoginFailure)loginResult;
                string errorMessage = $"Failed to connect:";
                foreach (string error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }
                foreach (ConnectionRefusedError error in failure.ErrorCodes)
                {
                    errorMessage += $"\n    {error}";
                }
                Debug.LogError(errorMessage);
                return;
            }

            var loginSuccess = (LoginSuccessful)loginResult;
            slot = loginSuccess.Slot;

            bool splitGratitudeAndTeleports = (bool)loginSuccess.SlotData["splitGratitude"];

            Debug.Log($"splitGratitudeAndTeleports = {splitGratitudeAndTeleports}");

            if (!splitGratitudeAndTeleports) {
                LocationSplitPatches.GratitudeTeleports.LinkGratitudeWithTeleports.RegisterLinks();
            }

            // mainThreadQueue.Append(jingleCooldown.Reset);

            // foreach (ItemInfo item in session.Items.AllItemsReceived)
            // {
            //     new Collecting.MyItem(Data.allItemsByName[item.ItemName]).Collect();
            // }

            await InitializeLocations();
        }

        void InitializeItems()
        {
            session.Items.ItemReceived += (receivedItemsHelper) => {
                var peekedItem = receivedItemsHelper.PeekItem();
                var itemReceivedName = peekedItem.ItemName;
                var itemFlag = Data.allItemsByName[itemReceivedName];

                mainThreadQueue.Enqueue(() => {
                    Save save = GlobalHub.Instance.save;

                    bool locallyCollected = save.GetFlag(itemFlag).on;
                    if (locallyCollected) return;

                    save.SetFlag(itemFlag, true);

                    var collectingItem = new Collecting.MyItem(itemFlag);
                    collectingItem.Collect();

                    bool isCheatedOrStartingInventory = peekedItem.Player.Slot == 0;
                    if (isCheatedOrStartingInventory) return;

                    VisualPatches.VisualPatches.CollectJingle(collectingItem);
                });

                receivedItemsHelper.DequeueItem();
            };
        }

        public void SendVictory()
        {
            session.SetGoalAchieved();
        }

        async Task InitializeLocations()
        {
            var ids = session.Locations.AllMissingLocations.ToArray();

            var info = await session.Locations.ScoutLocationsAsync(
                createAsHint: false,
                ids
            );

            Collecting.Location.checks.Clear();

            foreach (var entry in info) {
                long location = entry.Key;
                var itemInfo = entry.Value;

                if (location == -1) continue;

                var locationName = session.Locations.GetLocationNameFromId(location);
                var locationFlag = Data.allLocationsByName[locationName];

                if (itemInfo.Player.Slot == slot) {
                    var itemName = itemInfo.ItemName;
                    var itemFlag = Data.allItemsByName[itemName];
                    Collecting.Location.checks.Add(
                        locationFlag,
                        new Collecting.MyItem(
                            flag: itemFlag,
                            locationId: location
                        )
                    );
                } else {
                    Collecting.Location.checks.Add(
                        locationFlag,
                        new Collecting.TheirItem(
                            playerId: itemInfo.Player.Slot,
                            itemName: itemInfo.ItemName,
                            itemPrettyName: itemInfo.ItemDisplayName,
                            isMajor: itemInfo.Flags.HasFlag(ItemFlags.Advancement),

                            locationId: location
                        )
                    );
                }
                //if (itemInfo.Player.Slot != slot) continue;
            }

            List<string> checkedLocationFlags = session.Locations.AllLocationsChecked.Select(location => Data.allLocationsByName[session.Locations.GetLocationNameFromId(location)]).ToList();

            mainThreadQueue.Enqueue(() => {
                VisualPatches.VisualPatches.PatchAllVisuals();

                Collecting.Location.skipCollect = true;
                foreach (string locationFlag in checkedLocationFlags) {
                    GlobalHub.Instance.save.SetFlag(locationFlag, true);
                }
                Collecting.Location.skipCollect = false;

                GlobalHub.Instance.GetArea().Activate();
            });
        }
    }
}
