﻿using System;
using System.Linq;
using System.Security.Cryptography;
using Libplanet;
using Nekoyume.Helper;
using Nekoyume.Model.Item;
using Nekoyume.TableData;
using UnityEngine;

namespace Nekoyume
{
    public static class ItemExtensions
    {
        public static Sprite GetIconSprite(this ItemBase item) => SpriteHelper.GetItemIcon(item.Id);

        public static Sprite GetBackgroundSprite(this ItemBase item) => SpriteHelper.GetItemBackground(item.Grade);

        public static bool TryParseAsTradableId(this int rowId, ItemSheet itemSheet, out Guid tradableId)
        {
            var itemRow = itemSheet.OrderedList.FirstOrDefault(e => e.Id == rowId);
            if (itemRow is null ||
                !(itemRow is MaterialItemSheet.Row materialRow))
            {
                return false;
            }

            tradableId = TradableMaterial.DeriveTradableId(materialRow.ItemId);
            return true;
        }

        public static bool TryGetFungibleId(this int rowId, ItemSheet itemSheet, out HashDigest<SHA256> fungibleId)
        {
            var itemRow = itemSheet.OrderedList.FirstOrDefault(e => e.Id == rowId);
            if (itemRow is null ||
                !(itemRow is MaterialItemSheet.Row materialRow))
            {
                return false;
            }

            fungibleId = materialRow.ItemId;
            return true;
        }
    }
}