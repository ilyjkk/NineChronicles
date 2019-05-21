using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Libplanet;
using Libplanet.Action;
using Nekoyume.Game;
using Nekoyume.Game.Item;
using Nekoyume.State;

namespace Nekoyume.Action
{
    [ActionType("hack_and_slash")]
    public class HackAndSlash : GameAction
    {
        public List<Equipment> Equipments;
        public List<Food> Foods;
        public int Stage;

        protected override IImmutableDictionary<string, object> PlainValueInternal =>
            new Dictionary<string, object>
            {
                ["equipments"] = ByteSerializer.Serialize(Equipments),
                ["foods"] = ByteSerializer.Serialize(Foods),
                ["stage"] = ByteSerializer.Serialize(Stage),
            }.ToImmutableDictionary();


        protected override void LoadPlainValueInternal(IImmutableDictionary<string, object> plainValue)
        {
            Equipments = ByteSerializer.Deserialize<List<Equipment>>((byte[]) plainValue["equipments"]);
            Foods = ByteSerializer.Deserialize<List<Food>>((byte[]) plainValue["foods"]);
            Stage = ByteSerializer.Deserialize<int>((byte[]) plainValue["stage"]);
        }

        protected override IAccountStateDelta ExecuteInternal(IActionContext actionCtx)
        {
            var states = actionCtx.PreviousStates;
            var avatar = (AvatarState) states.GetState(actionCtx.Signer);
            if (actionCtx.Rehearsal)
            {
                if (avatar == null)
                {
                    avatar = CreateNovice.CreateState("dummy", default(Address));
                }
                states = states.SetState(AddressBook.Ranking, new RankingBoard());

                return states.SetState(actionCtx.Signer, avatar);
            }
            var items = avatar.avatar.Items.Select(i => i.Item).ToImmutableHashSet();
            var currentEquipments = items.OfType<Equipment>().ToImmutableHashSet();
            foreach (var equipment in currentEquipments)
            {
                equipment.Unequip();
            }

            if (Equipments.Count > 0)
            {
                foreach (var equipment in Equipments)
                {
                    if (!currentEquipments.Contains(equipment))
                    {
                        throw new InvalidActionException();
                    }

                    var equip = currentEquipments.First(e => e.Data.id == equipment.Data.id);
                    equip.Equip();
                }
            }

            if (Foods.Count > 0)
            {
                var currentFoods = items.OfType<Food>().ToImmutableHashSet();
                foreach (var food in Foods)
                {
                    if (!currentFoods.Contains(food))
                    {
                        Foods.Remove(food);
                    }
                }
            }

            var simulator = new Simulator(actionCtx.Random, avatar.avatar, Foods, Stage);
            var player = simulator.Simulate();
            avatar.avatar.Update(player);
            avatar.battleLog = simulator.Log;
            avatar.updatedAt = DateTimeOffset.UtcNow;
            if (avatar.avatar.WorldStage > Stage)
            {
                var ranking = (RankingBoard) states.GetState(AddressBook.Ranking);
                if (ranking is null)
                {
                    ranking = new RankingBoard();
                }
                avatar.clearedAt = DateTimeOffset.UtcNow;
                ranking.Update(avatar);
                states = states.SetState(AddressBook.Ranking, ranking);
            }
            return states.SetState(actionCtx.Signer, avatar);
        }
    }
}
