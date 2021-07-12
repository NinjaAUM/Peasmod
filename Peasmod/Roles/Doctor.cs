﻿using System.Collections.Generic;
using BepInEx.IL2CPP;
using Hazel;
using PeasAPI;
using PeasAPI.Components;
using PeasAPI.CustomButtons;
using PeasAPI.Roles;
using Peasmod.Utility;
using Reactor.Extensions;
using UnityEngine;

namespace Peasmod.Roles
{
    [RegisterCustomRole]
    public class Doctor: BaseRole
    {
        public Doctor(BasePlugin plugin) : base(plugin) { }

        public override string Name => "Doctor";

        public override string Description => "Revive dead crewmates";

        public override string TaskText => "Revive dead crewmates";

        public override Color Color => Color.yellow;

        public override int Limit => (int) Settings.DoctorAmount.GetValue();

        public override Team Team => Team.Crewmate;

        public override Visibility Visibility => Visibility.NoOne;

        public override bool HasToDoTasks => true;

        public RoleButton Button;

        public override void OnGameStart()
        {
            Button = new RoleButton(() =>
                {
                    List<DeadBody> _bodys = new List<DeadBody>();
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance - 2f, Constants.PlayersOnlyMask))
                    {
                        if (collider2D.tag == "DeadBody")
                        {
                            DeadBody body = (DeadBody)((Component)collider2D).GetComponent<DeadBody>();
                            _bodys.Add(body);
                        }
                    }
                    if (_bodys.Count != 0)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.DoctorAbility, Hazel.SendOption.None, -1);
                        writer.WritePacked(_bodys[0].ParentId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);

                        var player = _bodys[0].ParentId.GetPlayer();
                        player.Revive();
                        player.transform.position = _bodys[0].transform.position;
                        //player.Collider.enabled = true;
                        _bodys[0].gameObject.Destroy();
                    }
                }, Settings.DoctorCooldown.GetValue(),
                Utils.CreateSprite("Buttons.Revive.png"), Vector2.zero, false, this);
        }

        public override void OnUpdate()
        {
            if (Button != null)
            {
                if (Button.KillButtonManager.renderer != null)
                {
                    List<DeadBody> bodys = new List<DeadBody>();
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance - 2f, Constants.PlayersOnlyMask))
                    {
                        if (collider2D.tag == "DeadBody")
                        {
                            DeadBody body = collider2D.GetComponent<DeadBody>();
                            bodys.Add(body);
                        }
                    }
                    if (bodys.Count == 0)
                    {
                        Button.KillButtonManager.renderer.color = Palette.DisabledClear;
                        Button.enabled = false;
                    }
                    else
                    {
                        Button.KillButtonManager.renderer.color = Palette.EnabledColor;
                        Button.enabled = true;
                    }
                }
            }
        }

        public override void OnMeetingUpdate(MeetingHud meeting)
        {
            
        }
    }
}