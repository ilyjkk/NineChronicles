using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Libplanet.Action;
using Nekoyume.Data;
using Nekoyume.Data.Table;
using Nekoyume.Game.Item;
using Nekoyume.Game.Skill;
using Nekoyume.State;

namespace Nekoyume.Action
{
    [ActionType("combination")]
    public class Combination : GameAction
    {
        [Serializable]
        public struct Material
        {
            public int id;
            public int count;

            public Material(UI.Model.CountableItem item)
            {
                id = item.item.Value.Data.id;
                count = item.count.Value;
            }
        }

        public List<Material> Materials { get; private set; }

        protected override IImmutableDictionary<string, object> PlainValueInternal =>
            new Dictionary<string, object>
            {
                ["Materials"] = ByteSerializer.Serialize(Materials),
            }.ToImmutableDictionary();

        public Combination()
        {
            Materials = new List<Material>();
        }

        protected override void LoadPlainValueInternal(IImmutableDictionary<string, object> plainValue)
        {
            Materials = ByteSerializer.Deserialize<List<Material>>((byte[]) plainValue["Materials"]);
        }

        public override IAccountStateDelta Execute(IActionContext ctx)
        {
            var states = ctx.PreviousStates;
            if (ctx.Rehearsal)
            {
                return states.SetState(ctx.Signer, MarkChanged);
            }

            var avatarState = (AvatarState) states.GetState(ctx.Signer);
            if (avatarState == null)
            {
                return states;
            }

            // 사용한 재료를 인벤토리에서 제거.
            foreach (var material in Materials)
            {
                if (!avatarState.inventory.RemoveFungibleItem(material.id, material.count))
                {
                    return states;
                }
            }

            // 조합식 테이블 로드.
            var recipeTable = Tables.instance.Recipe;

            // 조합 재료 정렬.
            var orderedMaterials = Materials.OrderBy(order => order.id).ToList();

            // 장비인지 소모품인지 확인.
            var equipmentMaterials = orderedMaterials.Where(item => GameConfig.EquipmentMaterials.Contains(item.id))
                .ToList();
            if (equipmentMaterials.Any())
            {
                // 장비
                orderedMaterials.RemoveAll(item => equipmentMaterials.Contains(item));
                if (orderedMaterials.Count == 0)
                {
                    return states;
                }

                var equipmentMaterial = equipmentMaterials[0];
                var monsterPartsMaterial = orderedMaterials[0];

                if (!Tables.instance.Item.TryGetValue(equipmentMaterial.id, out var outEquipmentMaterialRow) ||
                    !Tables.instance.Item.TryGetValue(monsterPartsMaterial.id, out var outMonsterPartsMaterialRow))
                {
                    return states;
                }

                if (!TryGetItemType(outEquipmentMaterialRow.name, out var outItemType))
                {
                    return states;
                }

                if (!TryGetItemEquipmentRow(outItemType, outMonsterPartsMaterialRow.elemental,
                    outEquipmentMaterialRow.grade,
                    out var itemEquipmentRow))
                {
                    return states;
                }

                // 조합 결과 획득.
                var itemUsable = GetItemUsableWithRandomSkill(itemEquipmentRow, ctx.Random.Next());

                // 추가 스탯 적용.
                var normalizedRandomValue = ctx.Random.Next(0, 100000) * 0.00001f;
                var roll = GetRoll(monsterPartsMaterial.count, 0, normalizedRandomValue);
                var stat = GetStat(outMonsterPartsMaterialRow, roll);
                itemUsable.Stats.SetStatAdditionalValue(stat.Key, (float) stat.Value);

                avatarState.inventory.AddNonFungibleItem(itemUsable);
            }
            else
            {
                // 소모품
                foreach (var recipe in recipeTable)
                {
                    if (!recipe.Value.IsMatchForConsumable(orderedMaterials))
                    {
                        continue;
                    }

                    if (!Tables.instance.ItemEquipment.TryGetValue(recipe.Value.Id, out var itemEquipmentRow))
                    {
                        return states;
                    }

                    if (recipe.Value.GetCombinationResultCountForConsumable(orderedMaterials) == 0)
                    {
                        return states.SetState(ctx.Signer, avatarState);
                    }

                    // 조합 결과 획득.
                    var itemUsable = GetItemUsableWithRandomSkill(itemEquipmentRow, ctx.Random.Next());
                    avatarState.inventory.AddNonFungibleItem(itemUsable);

                    break;
                }
            }

            avatarState.updatedAt = DateTimeOffset.UtcNow;
            return states.SetState(ctx.Signer, avatarState);
        }

        private bool TryGetItemType(string itemName, out ItemBase.ItemType outItemType)
        {
            if (itemName.Contains("검"))
            {
                outItemType = ItemBase.ItemType.Weapon;
            }
            else if (itemName.Contains("옷"))
            {
                outItemType = ItemBase.ItemType.Armor;
            }
            else if (itemName.Contains("끈"))
            {
                outItemType = ItemBase.ItemType.Belt;
            }
            else
            {
                outItemType = ItemBase.ItemType.Material;
                return false;
            }

            return true;
        }

        private bool TryGetItemEquipmentRow(ItemBase.ItemType itemType, Elemental.ElementalType elementalType,
            int grade, out ItemEquipment outItemEquipmentRow)
        {
            foreach (var pair in Tables.instance.ItemEquipment)
            {
                if ((ItemBase.ItemType) Enum.Parse(typeof(ItemBase.ItemType), pair.Value.cls) != itemType ||
                    pair.Value.elemental != elementalType ||
                    pair.Value.grade != grade)
                {
                    continue;
                }

                outItemEquipmentRow = pair.Value;
                return true;
            }

            outItemEquipmentRow = null;
            return false;
        }

        private double GetRoll(int monsterPartsCount, int deltaLevel, float normalizedRandomValue)
        {
            var rollMax = Math.Pow(1f / (1f + GameConfig.CombinationValueP1 / monsterPartsCount),
                              GameConfig.CombinationValueP2) *
                          (deltaLevel <= 0
                              ? 1f
                              : Math.Pow(1f / (1f + GameConfig.CombinationValueL1 / deltaLevel),
                                  GameConfig.CombinationValueL2));
            var rollMin = rollMax * 0.5f;
            return rollMin + (rollMax - rollMin) * Math.Pow(normalizedRandomValue, GameConfig.CombinationValueR1);
        }

        private KeyValuePair<string, double> GetStat(Item itemRow, double roll)
        {
            var key = itemRow.stat;
            var value = Math.Floor(itemRow.minStat + (itemRow.maxStat - itemRow.minStat) * roll);
            return new KeyValuePair<string, double>(key, value);
        }

        // ToDo. 순수 랜덤이 아닌 조합식이 적용되어야 함.
        private ItemUsable GetItemUsableWithRandomSkill(ItemEquipment itemEquipment, int randomValue)
        {
            // FixMe. 소모품에 랜덤 스킬을 할당했을 때, `HackAndSlash` 액션에서 예외 발생. 그래서 소모품은 랜덤 스킬을 할당하지 않음.
            /*
             * InvalidTxSignatureException: 8383de6800f00416bfec1be66745895134083b431bd48766f1f6c50b699f6708: The signature (3045022100c2fffb0e28150fd6ddb53116cc790f15ca595b19ba82af8c6842344bd9f6aae10220705c37401ff35c3eb471f01f384ea6a110dd7e192d436ca99b91c9bed9b6db17) is failed to verify.
             * Libplanet.Tx.Transaction`1[T].Validate () (at <7284bf7c1f1547329a0963c7fa3ab23e>:0)
             * Libplanet.Blocks.Block`1[T].Validate (System.DateTimeOffset currentTime) (at <7284bf7c1f1547329a0963c7fa3ab23e>:0)
             * Libplanet.Store.BlockSet`1[T].set_Item (Libplanet.HashDigest`1[T] key, Libplanet.Blocks.Block`1[T] value) (at <7284bf7c1f1547329a0963c7fa3ab23e>:0)
             * Libplanet.Blockchain.BlockChain`1[T].Append (Libplanet.Blocks.Block`1[T] block, System.DateTimeOffset currentTime, System.Boolean render) (at <7284bf7c1f1547329a0963c7fa3ab23e>:0)
             * Libplanet.Blockchain.BlockChain`1[T].Append (Libplanet.Blocks.Block`1[T] block, System.DateTimeOffset currentTime) (at <7284bf7c1f1547329a0963c7fa3ab23e>:0)
             * Libplanet.Blockchain.BlockChain`1[T].MineBlock (Libplanet.Address miner, System.DateTimeOffset currentTime) (at <7284bf7c1f1547329a0963c7fa3ab23e>:0)
             * Libplanet.Blockchain.BlockChain`1[T].MineBlock (Libplanet.Address miner) (at <7284bf7c1f1547329a0963c7fa3ab23e>:0)
             * Nekoyume.BlockChain.Agent+<>c__DisplayClass31_0.<CoMiner>b__0 () (at Assets/_Scripts/BlockChain/Agent.cs:168)
             * System.Threading.Tasks.Task`1[TResult].InnerInvoke () (at <1f0c1ef1ad524c38bbc5536809c46b48>:0)
             * System.Threading.Tasks.Task.Execute () (at <1f0c1ef1ad524c38bbc5536809c46b48>:0)
             * UnityEngine.Debug:LogException(Exception)
             * Nekoyume.BlockChain.<CoMiner>d__31:MoveNext() (at Assets/_Scripts/BlockChain/Agent.cs:208)
             * UnityEngine.SetupCoroutine:InvokeMoveNext(IEnumerator, IntPtr)
             */
            if (itemEquipment.cls.ToEnumItemType() == ItemBase.ItemType.Food)
            {
                return (ItemUsable) ItemBase.ItemFactory(itemEquipment);
            }

            var table = Tables.instance.SkillEffect;
            var skillEffect = table.ElementAt(randomValue % table.Count);
            var elementalValues = Enum.GetValues(typeof(Elemental.ElementalType));
            var elementalType =
                (Elemental.ElementalType) elementalValues.GetValue(randomValue % elementalValues.Length);
            var skill = SkillFactory.Get(0.05f, skillEffect.Value, elementalType); // FixMe. 테스트를 위해서 5% 확률로 발동되도록 함.
            return (ItemUsable) ItemBase.ItemFactory(itemEquipment, skill);
        }
    }
}
