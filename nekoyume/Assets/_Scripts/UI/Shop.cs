using System;
using System.Collections.Generic;
using System.Linq;
using Assets.SimpleLocalization;
using DG.Tweening;
using Nekoyume.BlockChain;
using Nekoyume.EnumType;
using Nekoyume.Game.Controller;
using Nekoyume.Game.Item;
using Nekoyume.Model;
using Nekoyume.State;
using Nekoyume.TableData;
using Nekoyume.UI.Model;
using Nekoyume.UI.Module;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using ShopItem = Nekoyume.UI.Model.ShopItem;
using ShopItems = Nekoyume.UI.Module.ShopItems;

namespace Nekoyume.UI
{
    public class Shop : Widget
    {
        public enum StateType
        {
            Show,
            Buy,
            Sell
        }

        private float _defaultAnchoredPositionXOfBg1;
        private float _defaultAnchoredPositionXOfRight;
        private float _goOutTweenX = 800f;

        private Sequence _sequenceOfShopItems;

        public CanvasGroup canvasGroup;
        public RectTransform bg1;
        public CategoryButton buyButton;
        public CategoryButton sellButton;
        public RectTransform right;
        public Text catQuoteText;
        public Module.Inventory inventory;
        public ShopItems shopItems;
        public GameObject shopNotice;

        public Model.Shop SharedModel { get; private set; }

        public ItemCountAndPricePopup ItemCountAndPricePopup { get; private set; }

        #region Mono

        protected override void Awake()
        {
            _defaultAnchoredPositionXOfBg1 = bg1.anchoredPosition.x;
            _defaultAnchoredPositionXOfRight = right.anchoredPosition.x;
            base.Awake();
            
            SharedModel = new Model.Shop();
        }

        #endregion

        #region Override

        public override void Initialize()
        {
            base.Initialize();

            ItemCountAndPricePopup = Find<ItemCountAndPricePopup>();

            inventory.SharedModel.SelectedItemView.Subscribe(SubscribeInventorySelectedItemView)
                .AddTo(gameObject);
            shopItems.SharedModel.SelectedItemView.Subscribe(SubscribeShopItemsSelectedItemView)
                .AddTo(gameObject);

            SharedModel.State.Value = StateType.Show;
            SharedModel.State.Subscribe(SubscribeState).AddTo(gameObject);
            SharedModel.ItemCountAndPricePopup.Value.Item.Subscribe(SubscribeItemPopup).AddTo(gameObject);
            SharedModel.ItemCountAndPricePopup.Value.OnClickSubmit.Subscribe(SubscribeItemPopupSubmit)
                .AddTo(gameObject);
            SharedModel.ItemCountAndPricePopup.Value.OnClickCancel.Subscribe(SubscribeItemPopupCancel)
                .AddTo(gameObject);

            catQuoteText.text = LocalizationManager.Localize("SPEECH_SHOP_0");

            buyButton.button.OnClickAsObservable()
                .Subscribe(_ => SharedModel.State.Value = StateType.Buy)
                .AddTo(gameObject);
            sellButton.button.OnClickAsObservable()
                .Subscribe(_ => SharedModel.State.Value = StateType.Sell)
                .AddTo(gameObject);
        }

        public override void Show()
        {
            Game.Game.instance.stage.GetPlayer().gameObject.SetActive(false);

            base.Show();

            inventory.SharedModel.State.Value = ItemType.Equipment;
            shopItems.SharedModel.State.Value = StateType.Buy;
            SharedModel.State.Value = StateType.Show;

            Find<BottomMenu>().Show(UINavigator.NavigationType.Back, SubscribeBackButtonClick);

            AudioController.instance.PlayMusic(AudioController.MusicCode.Shop);
        }

        protected override void OnCompleteOfShowAnimation()
        {
            base.OnCompleteOfShowAnimation();
            canvasGroup.interactable = true;
        }

        public override void Close(bool ignoreCloseAnimation = false)
        {
            Find<BottomMenu>().Close(ignoreCloseAnimation);

            _sequenceOfShopItems?.Kill();
            bg1.anchoredPosition = new Vector2(_defaultAnchoredPositionXOfBg1, bg1.anchoredPosition.y);
            right.anchoredPosition = new Vector2(_defaultAnchoredPositionXOfRight, right.anchoredPosition.y);

            base.Close(ignoreCloseAnimation);

            AudioController.instance.PlayMusic(AudioController.MusicCode.Main);
        }

        #endregion

        #region Subscribe

        private void SubscribeState(StateType stateType)
        {
            switch (stateType)
            {
                case StateType.Show:
                    shopItems.SharedModel.State.Value = stateType;
                    SharedModel.State.Value = StateType.Buy;
                    buyButton.SetToggledOn();
                    sellButton.SetToggledOff();
                    return;
                case StateType.Buy:
                    inventory.SharedModel.DimmedFunc.Value = null;
                    buyButton.button.interactable = false;
                    sellButton.button.interactable = true;
                    shopNotice.SetActive(false);
                    buyButton.SetToggledOn();
                    sellButton.SetToggledOff();
                    break;
                case StateType.Sell:
                    inventory.SharedModel.DimmedFunc.Value = DimmedFuncForSell;
                    inventory.SharedModel.EquippedFunc.Value = EquippedFuncForSell;
                    buyButton.button.interactable = true;
                    sellButton.button.interactable = false;
                    shopNotice.SetActive(true);
                    buyButton.SetToggledOff();
                    sellButton.SetToggledOn();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stateType), stateType, null);
            }

            inventory.Tooltip.Close();
            inventory.SharedModel.DeselectItemView();
            shopItems.SharedModel.DeselectItemView();

            canvasGroup.interactable = false;
            _sequenceOfShopItems?.Kill();
            _sequenceOfShopItems = DOTween.Sequence();
            SetSequenceOfShopItems(true, ref _sequenceOfShopItems);
            _sequenceOfShopItems.AppendCallback(() => shopItems.SharedModel.State.Value = stateType);
            SetSequenceOfShopItems(false, ref _sequenceOfShopItems);
            _sequenceOfShopItems.OnComplete(() => canvasGroup.interactable = true);
        }

        private void SubscribeInventorySelectedItemView(InventoryItemView view)
        {
            shopItems.SharedModel.DeselectItemView();
            
            if (view is null ||
                view.RectTransform == inventory.Tooltip.Target)
            {
                inventory.Tooltip.Close();

                return;
            }
            
            if (inventory.Tooltip.Model.target.Value == view.RectTransform)
            {
                inventory.Tooltip.Close();
                return;
            }

            if (SharedModel.State.Value == StateType.Buy)
            {
                inventory.Tooltip.Show(view.RectTransform, view.Model);
            }
            else
            {
                inventory.Tooltip.Show(view.RectTransform, view.Model,
                    value => !DimmedFuncForSell(view.Model),
                    LocalizationManager.Localize("UI_SELL"),
                    tooltip =>
                    {
                        SharedModel.ShowItemPopup(tooltip.itemInformation.Model.item.Value);
                        inventory.Tooltip.Close();
                    },
                    tooltip => inventory.SharedModel.DeselectItemView());
            }
        }
        
        private void SubscribeShopItemsSelectedItemView(ShopItemView view)
        {
            inventory.SharedModel.DeselectItemView();

            if (view is null ||
                view.RectTransform == inventory.Tooltip.Target)
            {
                inventory.Tooltip.Close();

                return;
            }

            if (inventory.Tooltip.Model.target.Value == view.RectTransform)
            {
                inventory.Tooltip.Close();
                return;
            }

            if (SharedModel.State.Value == StateType.Buy)
            {
                inventory.Tooltip.Show(view.RectTransform, view.Model,
                    value => ButtonEnabledFuncForBuy(view.Model),
                    LocalizationManager.Localize("UI_BUY"),
                    tooltip =>
                    {
                        SharedModel.ShowItemPopup(tooltip.itemInformation.Model.item.Value);
                        inventory.Tooltip.Close();
                    },
                    tooltip => { shopItems.SharedModel.DeselectItemView(); });
            }
            else
            {
                inventory.Tooltip.Show(view.RectTransform, view.Model,
                    value => ButtonEnabledFuncForSell(view.Model),
                    LocalizationManager.Localize("UI_RETRIEVE"),
                    tooltip =>
                    {
                        SharedModel.ShowItemPopup(tooltip.itemInformation.Model.item.Value);
                        inventory.Tooltip.Close();
                    },
                    tooltip => { shopItems.SharedModel.DeselectItemView(); });
            }
        }
        
        private void SubscribeItemPopup(CountableItem data)
        {
            if (data is null)
            {
                ItemCountAndPricePopup.Close();
                return;
            }

            ItemCountAndPricePopup.Pop(SharedModel.ItemCountAndPricePopup.Value);
        }

        private void SubscribeItemPopupSubmit(Model.ItemCountAndPricePopup data)
        {
            if (SharedModel.State.Value == StateType.Buy)
            {
                var shopItem = shopItems.SharedModel.OtherProducts
                    .FirstOrDefault(i => i.ItemBase.Value.Equals(data.Item.Value.ItemBase.Value));
                if (shopItem is null)
                    return;
                ActionManager.instance
                    .Buy(shopItem.SellerAgentAddress.Value, shopItem.SellerAvatarAddress.Value,
                        shopItem.ProductId.Value);
                ResponseBuy(shopItem);
            }
            else
            {
                var shopItem = shopItems.SharedModel.CurrentAgentsProducts
                    .FirstOrDefault(i => i.ItemBase.Value.Equals(data.Item.Value.ItemBase.Value));
                if (shopItem is null)
                {
                    ActionManager.instance.Sell((ItemUsable) data.Item.Value.ItemBase.Value, data.Price.Value);
                    ResponseSell();
                    return;
                }

                ActionManager.instance.SellCancellation(shopItem.SellerAvatarAddress.Value, shopItem.ProductId.Value);
                ResponseSellCancellation(shopItem);
            }
        }

        private void SubscribeItemPopupCancel(Model.ItemCountAndPricePopup data)
        {
            SharedModel.ItemCountAndPricePopup.Value.Item.Value = null;
            ItemCountAndPricePopup.Close();
        }

        #endregion

        #region Private Static Methods

        private static bool DimmedFuncForSell(InventoryItem inventoryItem)
        {
            return inventoryItem.ItemBase.Value.Data.ItemType == ItemType.Material;
        }

        private static bool EquippedFuncForSell(InventoryItem inventoryItem)
        {
            if (!(inventoryItem.ItemBase.Value is Equipment equipment))
            {
                return false;
            }

            return equipment.equipped;
        }

        private static bool ButtonEnabledFuncForBuy(InventoryItem inventoryItem)
        {
            return inventoryItem is ShopItem shopItem &&
                   ReactiveAgentState.Gold.Value >= shopItem.Price.Value;
        }

        private static bool ButtonEnabledFuncForSell(InventoryItem inventoryItem)
        {
            switch (inventoryItem)
            {
                case null:
                    return false;
                case ShopItem _:
                    return true;
                default:
                    return !inventoryItem.Dimmed.Value;
            }
        }

        #endregion

        #region Response

        private void ResponseSell()
        {
            var item = SharedModel.ItemCountAndPricePopup.Value.Item.Value;
            var price = SharedModel.ItemCountAndPricePopup.Value.Price.Value;
            SharedModel.ItemCountAndPricePopup.Value.Item.Value = null;
            
            States.Instance.CurrentAvatarState.Value.inventory.RemoveNonFungibleItem((ItemUsable) item.ItemBase.Value);
            inventory.SharedModel.RemoveItem(item.ItemBase.Value);
            
            AudioController.instance.PlaySfx(AudioController.SfxCode.InputItem);
            var format = LocalizationManager.Localize("NOTIFICATION_SELL_START");
            Notification.Push(string.Format(format, item.ItemBase.Value.Data.GetLocalizedName()));
        }

        private void ResponseSellCancellation(ShopItem shopItem)
        {
            SharedModel.ItemCountAndPricePopup.Value.Item.Value = null;

            var sellerAgentAddress = shopItem.SellerAgentAddress.Value;
            var productId = shopItem.ProductId.Value;

            States.Instance.ShopState.Value.Unregister(sellerAgentAddress, productId);
            shopItems.SharedModel.RemoveCurrentAgentsProduct(productId);
            
            AudioController.instance.PlaySfx(AudioController.SfxCode.InputItem);
            var format = LocalizationManager.Localize("NOTIFICATION_SELL_CANCEL_START");
            Notification.Push(string.Format(format, shopItem.ItemBase.Value.Data.GetLocalizedName()));
        }

        private void ResponseBuy(ShopItem shopItem)
        {
            SharedModel.ItemCountAndPricePopup.Value.Item.Value = null;
            
            var sellerAgentAddress = shopItem.SellerAgentAddress.Value;
            var productId = shopItem.ProductId.Value;
            
            States.Instance.ShopState.Value.Unregister(sellerAgentAddress, productId);
            shopItems.SharedModel.RemoveOtherProduct(productId);
            
            AudioController.instance.PlaySfx(AudioController.SfxCode.BuyItem);
            var format = LocalizationManager.Localize("NOTIFICATION_BUY_START");
            Notification.Push(string.Format(format, shopItem.ItemBase.Value.Data.GetLocalizedName()));
        }

        #endregion

        private void SubscribeBackButtonClick(BottomMenu bottomMenu)
        {
            Close();
            Find<Menu>().ShowRoom();
        }

        private void SetSequenceOfShopItems(bool isGoOut, ref Sequence sequence)
        {
            var goOutTweenXAbs = Math.Abs(_goOutTweenX);
            sequence.Append(DOTween
                .To(
                    () => bg1.anchoredPosition.x,
                    value =>
                    {
                        var p = bg1.anchoredPosition;
                        p.x = value;
                        bg1.anchoredPosition = p;
                    },
                    isGoOut
                        ? _defaultAnchoredPositionXOfBg1 + _goOutTweenX
                        : _defaultAnchoredPositionXOfBg1,
                    isGoOut
                        ? Math.Abs(goOutTweenXAbs - Math.Abs(bg1.anchoredPosition.x - _defaultAnchoredPositionXOfBg1)) /
                          goOutTweenXAbs
                        : Math.Abs(goOutTweenXAbs - Math.Abs(_defaultAnchoredPositionXOfBg1 - bg1.anchoredPosition.x)) /
                          goOutTweenXAbs)
                .SetEase(isGoOut ? Ease.InQuint : Ease.OutQuint));
            sequence.Join(DOTween
                .To(
                    () => right.anchoredPosition.x,
                    value =>
                    {
                        var p = right.anchoredPosition;
                        p.x = value;
                        right.anchoredPosition = p;
                    },
                    isGoOut
                        ? _defaultAnchoredPositionXOfRight + _goOutTweenX
                        : _defaultAnchoredPositionXOfRight,
                    isGoOut
                        ? Math.Abs(goOutTweenXAbs -
                                   Math.Abs(right.anchoredPosition.x - _defaultAnchoredPositionXOfRight)) /
                          goOutTweenXAbs
                        : Math.Abs(goOutTweenXAbs -
                                   Math.Abs(_defaultAnchoredPositionXOfRight - right.anchoredPosition.x)) /
                          goOutTweenXAbs)
                .SetEase(isGoOut ? Ease.InQuint : Ease.OutQuint));
        }
    }
}
