﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Hazel;
using UnityEngine;
using UnhollowerBaseLib;
using Peasmod.Gamemodes;
using Peasmod.Utility;

namespace Peasmod.Patches
{
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString),
        new[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    public class PatchColours
    {
        public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name) {
            if (ExileController.Instance != null && ExileController.Instance.exiled != null)
            {
                if (name == StringNames.ExileTextPN || name == StringNames.ExileTextSN)
                {
                    #region JesterMode
                    if (ExileController.Instance.exiled.Object.IsRole(Role.Jester))
                    {
                        __result = ExileController.Instance.exiled.PlayerName + " was The Jester.";
                    }
                    #endregion JesterMode
                    #region DoctorMode
                    if (ExileController.Instance.exiled.Object.IsRole(Role.Doctor))
                    {
                        __result = ExileController.Instance.exiled.PlayerName + " was The Doctor.";
                    }
                    #endregion DoctorMode
                    #region MayorMode
                    if (ExileController.Instance.exiled.Object.IsRole(Role.Mayor))
                    {
                        __result = ExileController.Instance.exiled.PlayerName + " was The Mayor.";
                    }
                    #endregion MayorMode
                    #region InspectorMode
                    if (ExileController.Instance.exiled.Object.IsRole(Role.Inspector))
                    {
                        __result = ExileController.Instance.exiled.PlayerName + " was The Inspector.";
                    }
                    #endregion InspectorMode
                    #region SheriffMode
                    if (ExileController.Instance.exiled.Object.IsRole(Role.Sheriff))
                    {
                        __result = ExileController.Instance.exiled.PlayerName + " was The Sheriff.";
                    }
                    #endregion SheriffMode
                    if (__result == null)
                    {
                        if (Peasmod.impostors.Count == 1)
                        {
                            __result = ExileController.Instance.exiled.PlayerName + " was not The Impostor.";
                        }
                        else
                        {
                            __result = ExileController.Instance.exiled.PlayerName + " was not An Impostor.";
                        }
                    }
                    return false;
                }
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class PlayerControlWinPatch
    {
        public static void Prefix(PlayerControl __instance)
        {
            #region MorphingMode
            if(PlayerControl.LocalPlayer.IsMorphed())
                MorphingMode.OnLabelClick(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer, false);
            #endregion MorphingMode
            #region JesterMode
            if (__instance.IsRole(Role.Jester))
            {
                ShipStatus.Instance.enabled = false;
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.JesterWin, Hazel.SendOption.None, -1);
                writer.Write(__instance.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                JesterMode.Winner = __instance;
                JesterMode.JesterWon = true;
                JesterMode.Winner.Data.IsImpostor = true;
                JesterMode.Winner.infectedSet = true;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.PlayerId != JesterMode.Winner.PlayerId)
                    {
                        player.RemoveInfected();
                        player.Data.IsImpostor = false;
                    }
                }
                HandleWinRpc();
                Utils.Log("1");
                #if ITCH
                    ShipStatus.Method_34(GameOverReason.ImpostorByVote, false);
                #elif STEAM
                    ShipStatus.RpcEndGame(GameOverReason.ImpostorByVote, false);
                #endif
            }
            #endregion JesterMode
        }

        public static void HandleWinRpc()
        {
            #region JesterMode
            if(JesterMode.JesterWon)
            {
                if (PlayerControl.LocalPlayer.PlayerId == JesterMode.Winner.PlayerId)
                {
                    EndGameScreenPatch.Text = "Victory";
                    EndGameScreenPatch.TextColor = Palette.CrewmateBlue;
                    EndGameScreenPatch.Winner = "crew";
                }
                else
                {
                    EndGameScreenPatch.Text = "Defeat";
                    EndGameScreenPatch.TextColor = Palette.ImpostorRed;
                    EndGameScreenPatch.Winner = "impostor";
                }
                EndGameScreenPatch.BGColor = JesterMode.JesterColor;
            }
            #endregion JesterMode
            #region BattleRoyaleMode
            /*if (Peasmod.Settings.IsGameMode(Peasmod.Settings.GameMode.BattleRoyale) && BattleRoyaleMode.HasWon)
            {
                if (PlayerControl.LocalPlayer.PlayerId == BattleRoyaleMode.Winner.PlayerId)
                {
                    EndGameScreenPatch.Text = "Victory Royale";
                    EndGameScreenPatch.TextColor = Palette.CrewmateBlue;
                    EndGameScreenPatch.Winner = "crew";
                }
                else
                {
                    EndGameScreenPatch.Text = "Defeat";
                    EndGameScreenPatch.TextColor = Palette.ImpostorRed;
                    EndGameScreenPatch.Winner = "impostor";
                }
                EndGameScreenPatch.BGColor = Palette.ImpostorRed;
            }*/
            #endregion BattleRoyaleMode
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public static class GameEndPatch
    {
        public static void Prefix()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                player.Visible = true;
                player.moveable = true;
            }
            PlayerControlExtensions.ResetRoles();
            #region JesterMode
            if (JesterMode.JesterWon)
            {
                Il2CppSystem.Collections.Generic.List<WinningPlayerData> _winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                JesterMode.Winner.Data.IsDead = false;
                _winners.Add(new WinningPlayerData(JesterMode.Winner.Data));
                TempData.winners = _winners;
            }
            #endregion JesterMode
            #region BattleRoyaleMode
            if(Peasmod.Settings.IsGameMode(Peasmod.Settings.GameMode.BattleRoyale) && BattleRoyaleMode.HasWon)
            {
                Il2CppSystem.Collections.Generic.List<WinningPlayerData> _winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                //_winners.Add(new WinningPlayerData(BattleRoyaleMode.Winner.Data));
                TempData.winners = _winners;
            }
            #endregion BattleRoyaleMode
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
    public static class EndGameScreenPatch
    {
        public static string Text;
        public static Color? TextColor;

        public static Color? BGColor;
        public static string Winner;

        static void Prefix(EndGameManager __instance)
        {
            Patch(__instance, false);
        }

        public static void Postfix(EndGameManager __instance)
        {
            Patch(__instance, true);
        }

        private static void Patch(EndGameManager __instance, bool removeState)
        {
            __instance.DisconnectStinger = Winner switch
            {
                "crew" => __instance.CrewStinger,
                "impostor" => __instance.ImpostorStinger,
                _ => __instance.DisconnectStinger
            };
            if (Text != null)
            {
                __instance.WinText.text = Text;
            }

            if (TextColor != null)
            {
                __instance.WinText.color = (Color)TextColor;
            }

            if (BGColor != null)
            {
                __instance.BackgroundBar.material.color = (Color)BGColor;
            }

            if (removeState)
            {
                Text = null;
                TextColor = null;
                BGColor = null;
            }
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
    [HarmonyPriority(Priority.First)]
    public static class CheckEndCriteriaPatch
    {
        public static bool Prefix()
        {
            if(Peasmod.Settings.IsGameMode(Peasmod.Settings.GameMode.HotPotato))
            {
                var impostors = 0;
                var crewmates = 0;
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if(!player.Data.IsDead)
                    {
                        if (player.Data.IsImpostor)
                            impostors++;
                        else
                            crewmates++;
                    }
                }
                if (impostors > crewmates)
                    return true;
                else
                    return false;
            }
            else if(Peasmod.Settings.IsGameMode(Peasmod.Settings.GameMode.BattleRoyale))
            {   
                int num = (from x in GameData.Instance.AllPlayers.ToArray() where !x.IsDead && !x.Disconnected select x).ToArray<GameData.PlayerInfo>().Length;
                bool flag2 = num == 1;
                if (flag2)
                {
                    PlayerControl winner =
                        (from x in PlayerControl.AllPlayerControls.ToArray() where !x.Data.IsDead && !x.Data.Disconnected select x)
                        .ToArray<PlayerControl>().First();
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.VictoryRoyale, Hazel.SendOption.None, -1);
                    writer.Write(winner.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    foreach (var player in PlayerControl.AllPlayerControls)
                    {
                        if (player.PlayerId != winner.PlayerId)
                            player.Data.IsImpostor = false;
                    }
                    ShipStatus.RpcEndGame(GameOverReason.ImpostorByVote, false);
                }
                return false;
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcEndGame))]
    [HarmonyPriority(Priority.First)]
    public static class RPCEndGamePatch
    {
        public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] GameOverReason reason)
        {
            if (Peasmod.Settings.IsGameMode(Peasmod.Settings.GameMode.BattleRoyale) && Peasmod.GameStarted &&
                reason == GameOverReason.HumansByTask)
                return false;
            return true;
        }
    }
}
