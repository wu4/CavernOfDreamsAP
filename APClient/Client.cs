using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Newtonsoft.Json.Linq;

namespace CoDArchipelago.APClient
{
    static class Client
    {
        static ConcurrentQueue<(string, Action)> mainThreadQueue;
        static MainThreadExecutor executor;

        static string playerName;
        static string address;
        static int port;

        static ArchipelagoSession session;
        static DeathLinkService deathLinkService;
        static int slot;

        // =====
        // initialization steps
        // =====
        public static void Initialize(string _playerName, string _address, int _port)
        {
            playerName = _playerName;
            address = _address;
            port = _port;
            beforeConnect = true;
        }

        static bool tryConnectSuccess = false;

        public static IEnumerator TryLoadGame(DragonMainMenu dragonMainMenu, MenuScreen returnMenu)
        {
            var connectTask = TryConnect();
            while (!connectTask.IsCompleted) {
                yield return null;
            }
            if (tryConnectSuccess) {
                dragonMainMenu.StartCoroutine(GlobalHub.LoadGame(SaveHandler.SAVE_FILE_DEBUG));
            } else {
                dragonMainMenu.GetComponentInChildren<Animator>().Play("Sleep");
                dragonMainMenu.meshRenderer.materials[dragonMainMenu.indexMatEye].mainTexture = dragonMainMenu.texEyesClosed;
                MenuHandler.Instance.SetMenu(returnMenu, escape: false);
            }
        }

        static async Task TryConnect()
        {
            mainThreadQueue = new();
            session = ArchipelagoSessionFactory.CreateSession(address, port);

            try {
                var connectResult = await session.ConnectAsync();
            } catch (Exception e) {
                session = null;
                Debug.LogError($"Failed to connect to {address}:{port} - {e.Message}");
                tryConnectSuccess = false;
                return;
            }

            var loginResult = await session.LoginAsync(
                game: "Cavern of Dreams",
                name: playerName,
                itemsHandlingFlags: ItemsHandlingFlags.AllItems,
                version: new("0.5.0"),
                tags: new string[]{"DeathLink"},
                requestSlotData: true
            );

            if (!loginResult.Successful) {
                session = null;
                LoginFailure failure = (LoginFailure)loginResult;
                string errorMessage = $"Failed to connect to {address}:{port}";
                foreach (string error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }
                foreach (ConnectionRefusedError error in failure.ErrorCodes)
                {
                    errorMessage += $"\n    {error}";
                }
                Debug.LogError(errorMessage);
                tryConnectSuccess = false;
                return;
            }

            var loginSuccess = (LoginSuccessful)loginResult;
            slot = loginSuccess.Slot;

            ProcessSlotData(loginSuccess.SlotData);

            session.MessageLog.OnMessageReceived += OnMessageReceived;

            deathLinkService = session.CreateDeathLinkService();
            deathLinkService.EnableDeathLink();

            deathLinkService.OnDeathLinkReceived += OnDeathLinkReceived;

            tryConnectSuccess = true;
        }

        static void ProcessSlotData(Dictionary<string, object> slotData)
        {
            string startLocation = (string)slotData["startLocation"];
            MiscPatches.ChangeStartLocation.SetStartLocation(startLocation);

            var entranceMap = (List<List<string>>)((JArray)slotData["entranceMap"]).ToObject(typeof(List<List<string>>));
            if (entranceMap.Count != 0) {
                MiscPatches.EntranceRando.SetEntranceMap(entranceMap);
            }

            bool splitGratitudeAndTeleports = (bool)slotData["splitGratitude"];
            if (!splitGratitudeAndTeleports) {
                LocationSplitPatches.GratitudeTeleports.LinkGratitudeWithTeleports.RegisterLinks();
            }

            bool dropCarryables = (bool)slotData["dropCarryables"];
            MiscPatches.DropCarryablesOnWarp.shouldDropCarryables = dropCarryables;

            var pityItems = (List<string>)((JArray)slotData["pityItems"]).ToObject(typeof(List<string>));
            if (pityItems.Count > 0) {
                string pityItemsStr = string.Join(", ", pityItems.Select(item => $"<color=#ee9999>{item}</color>"));
                mainThreadQueue.Enqueue(("Pity Items", () => {
                    Messaging.TextLogManager.AddLine($"<color=#aaaaaa>The following items have been given to Fynn in order to minimize the odds of royal fast food consumption (or otherwise make the seed playable):</color>");
                    Messaging.TextLogManager.AddLine(pityItemsStr);
                }));
            }

            MiscPatches.NoJesterBootsCarry.allowFun = (bool)slotData["allowFun"];
        }

        class StartGame : InstantiateOnGameSceneLoad
        {
            public StartGame()
            {
                GameObject manager = new GameObject("AP Connection Manager");
                executor = manager.AddComponent<MainThreadExecutor>();
                var task = InitializeLocations();
                beforeConnect = false;
            }
        }

        //=====
        //public methods
        //=====

        public static void SendLocationCollected(long locationId)
        {
            session.Locations.CompleteLocationChecksAsync(new[]{locationId});
        }

        public static void SendMessage(string message)
        {
            session.Say(message);
        }

        public static void SendDeathLink(Kill.KillType killType)
        {
            DeathLink deathLink = new(playerName);
            deathLinkService.SendDeathLink(deathLink);
        }

        public static void SendVictory()
        {
            session.SetGoalAchieved();
        }

        //=====
        //misc
        //=====

        class MainThreadExecutor : MonoBehaviour
        {
            bool inFirstLoop = true;

            void Update()
            {
                while (mainThreadQueue.TryDequeue(out var nameAndAction)) {
                    (var name, var action) = nameAndAction;
                    try {
                        action();
                    } catch (Exception e) {
                        Debug.LogError($"Error while executing {name}:");
                        Debug.LogError(e);
                    }
                }

                ItemInfo item;
                while ((item = session.Items.DequeueItem()) != null) {
                    var itemReceivedName = item.ItemName;
                    if (Data.carryableItems.ContainsValue(itemReceivedName)){
                        continue;
                    }

                    var isReceivedBeforeConnect = inFirstLoop;
                    Save save = GlobalHub.Instance.save;

                    if (itemReceivedName == "Shroom") {
                        if (slot != item.Player.Slot || isReceivedBeforeConnect) {
                            var shroom = new Collecting.MyItem(
                                "Shroom",
                                silent: isReceivedBeforeConnect
                            );
                            shroom.Collect();
                            if (!isReceivedBeforeConnect)
                                VisualPatches.VisualPatches.CollectJingle(shroom);
                        }
                        return;
                    }

                    var itemFlag = Data.allItemsByName[itemReceivedName];
                    bool locallyCollected = save.GetFlag(itemFlag).on;
                    if (locallyCollected) return;

                    save.SetFlag(itemFlag, true);

                    var collectingItem = new Collecting.MyItem(itemFlag, silent: isReceivedBeforeConnect);
                    collectingItem.Collect();

                    bool isCheatedOrStartingInventory = isReceivedBeforeConnect || item.Player.Slot == 0;
                    if (isCheatedOrStartingInventory) return;

                    VisualPatches.VisualPatches.CollectJingle(collectingItem);
                }

                inFirstLoop = false;
            }
        }

        static string MessageColorAsHex(Archipelago.MultiClient.Net.Models.Color messageColor)
        {
            return "#" + BitConverter.ToString(new[] {messageColor.R, messageColor.G, messageColor.B}).Replace("-", "");
        }

        //=====
        //listeners & callbacks
        //=====

        [HarmonyPatch(typeof(MO_QUIT_GAME), nameof(MO_QUIT_GAME.OnSelect))]
        static class OnQuit
        {
            static void Postfix(MO_QUIT_GAME __instance)
            {
                session.MessageLog.OnMessageReceived -= OnMessageReceived;
                deathLinkService.OnDeathLinkReceived -= OnDeathLinkReceived;
                session.Socket.DisconnectAsync();
                mainThreadQueue = null;
                deathLinkService = null;
                GameObject.DestroyImmediate(executor.gameObject);
                executor = null;
            }
        }

        static void OnMessageReceived(LogMessage message)
        {
            string messageString = "";

            foreach (var part in message.Parts) {
                messageString += $"<color=\"{MessageColorAsHex(part.Color)}\">";
                messageString += part.Text.Replace("<", "&lt;").Replace(">", "&gt;");
                messageString += "</color>";
            }

            mainThreadQueue.Enqueue(("OnMessageReceived", () => {
                Messaging.TextLogManager.AddLine(messageString);
            }));
        }

        static void OnDeathLinkReceived(DeathLink deathLink)
        {
            mainThreadQueue.Enqueue(("OnDeathLinkReceived", () => {
                MiscPatches.DeathPatches.shouldSendDeathLink = false;
                GlobalHub.Instance.player.Die(Kill.KillType.WOUND);
                MiscPatches.DeathPatches.shouldSendDeathLink = true;
            }));
        }

        static bool beforeConnect;

        static async Task InitializeLocations()
        {
            var ids = session.Locations.AllLocations.ToArray();

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

                    if (!(itemName == "Shroom" || Data.carryableItems.ContainsValue(itemName))) {
                        itemName = Data.allItemsByName[itemName];
                    }

                    Collecting.Location.checks.Add(
                        locationFlag,
                        new Collecting.MyItem(
                            flag: itemName,
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
            }

            List<string> checkedLocationFlags =
                session.Locations.AllLocationsChecked
                .Select(
                    location =>
                        Data.allLocationsByName[session.Locations.GetLocationNameFromId(location)]
                )
                .ToList();

            mainThreadQueue.Enqueue(("PatchAllVisuals", () => {
                VisualPatches.VisualPatches.PatchAllVisuals();

                Collecting.Location.skipCollect = true;
                foreach (string locationFlag in checkedLocationFlags) {
                    GlobalHub.Instance.save.SetFlag(locationFlag, true);
                }
                Collecting.Location.skipCollect = false;

                GlobalHub.Instance.GetArea().Activate();
            }));
        }
    }
}
