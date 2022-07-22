using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using mixpanel;
using Nekoyume.BlockChain;
using Nekoyume.EnumType;
using Nekoyume.Game;
using Nekoyume.Game.Controller;
using Nekoyume.Game.VFX;
using Nekoyume.L10n;
using Nekoyume.Model.BattleStatus;
using Nekoyume.Model.Item;
using Nekoyume.State;
using Nekoyume.UI.Model;
using Nekoyume.UI.Module;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    using UniRx;

    public class BattleResultPopup : PopupWidget
    {
        public enum NextState
        {
            GoToMain,
            RepeatStage,
            NextStage,
        }

        public class Model
        {
            private readonly List<CountableItem> _rewards = new();

            public StageType StageType;
            public NextState NextState;
            public BattleLog.Result State;
            public string WorldName;
            public long Exp;
            public int WorldID;
            public int StageID;
            public int ClearedWaveNumber;
            public int ActionPoint;
            public int LastClearedStageId;
            public bool ActionPointNotEnough;
            public bool IsClear;
            public bool IsEndStage;

            public IReadOnlyList<CountableItem> Rewards => _rewards;

            public void AddReward(CountableItem reward)
            {
                var sameReward = _rewards.FirstOrDefault(e =>
                    e.ItemBase.Value.Equals(reward.ItemBase.Value));
                if (sameReward is null)
                {
                    _rewards.Add(reward);
                    return;
                }

                sameReward.Count.Value += reward.Count.Value;
            }
        }

        [Serializable]
        public struct RewardsArea
        {
            public GameObject root;
            public BattleReward[] rewards;
        }

        [Serializable]
        public struct DefeatTextArea
        {
            public GameObject root;
            public TextMeshProUGUI defeatText;
            public TextMeshProUGUI expText;
        }

        private const int Timer = 10;
        private static readonly Vector3 VfxBattleWinOffset = new(-0.05f, 1.2f, 10f);

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private GameObject victoryImageContainer;

        [SerializeField]
        private GameObject defeatImageContainer;

        [SerializeField]
        private TextMeshProUGUI worldStageId;

        [SerializeField]
        private GameObject topArea;

        [SerializeField]
        private DefeatTextArea defeatTextArea;

        [SerializeField]
        private RewardsArea rewardsArea;

        [SerializeField]
        private TextMeshProUGUI bottomText;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private Button stagePreparationButton;

        [SerializeField]
        private Button nextButton;

        [SerializeField]
        private Button repeatButton;

        [SerializeField]
        private StageProgressBar stageProgressBar;

        [SerializeField]
        private GameObject[] victoryResultTexts;

        [SerializeField]
        private ActionPoint actionPoint;

        private BattleWin01VFX _battleWin01VFX;

        private BattleWin02VFX _battleWin02VFX;

        private BattleWin03VFX _battleWin03VFX;

        private BattleWin04VFX _battleWin04VFX;

        private Coroutine _coUpdateBottomText;

        private readonly WaitForSeconds _battleWinVFXYield = new(0.2f);
        private static readonly int ClearedWave = Animator.StringToHash("ClearedWave");

        private Animator _victoryImageAnimator;

        private bool _IsAlreadyOut;

        private Model SharedModel { get; set; }

        public StageProgressBar StageProgressBar => stageProgressBar;

        protected override void Awake()
        {
            base.Awake();

            closeButton.OnClickAsObservable().Subscribe(_ =>
            {
                var wi = States.Instance.CurrentAvatarState.worldInformation;
                if (!wi.TryGetUnlockedWorldByStageClearedBlockIndex(out var world))
                {
                    return;
                }

                // NOTE: This `BattleResultPopup` cannot be closed when
                //       the player is not cleared `Battle.RequiredStageForExitButton` stage yet.
                var canExit = world.StageClearedId >= Battle.RequiredStageForExitButton;
                if (canExit)
                {
                    StartCoroutine(OnClickClose());
                }
            }).AddTo(gameObject);

            stagePreparationButton.OnClickAsObservable().Subscribe(_ =>
            {
                OnClickStage();
            }).AddTo(gameObject);

            nextButton.OnClickAsObservable()
                .Subscribe(_ => StartCoroutine(OnClickNext()))
                .AddTo(gameObject);

            repeatButton.OnClickAsObservable()
                .Subscribe(_ => StartCoroutine(OnClickRepeat()))
                .AddTo(gameObject);

            CloseWidget = closeButton.onClick.Invoke;
            SubmitWidget = nextButton.onClick.Invoke;
            defeatTextArea.root.SetActive(false);

            _victoryImageAnimator = victoryImageContainer.GetComponent<Animator>();
        }

        private IEnumerator OnClickClose()
        {
            _IsAlreadyOut = true;
            AudioController.PlayClick();
            if (SharedModel.IsClear)
            {
                yield return CoDialog(SharedModel.StageID);
            }

            GoToMain();
        }

        private void OnClickStage()
        {
            _IsAlreadyOut = true;
            AudioController.PlayClick();
            GoToPreparation();
        }

        private IEnumerator OnClickNext()
        {
            if (_IsAlreadyOut)
            {
                yield break;
            }

            AudioController.PlayClick();
            yield return CoProceedNextStage();
        }

        private IEnumerator OnClickRepeat()
        {
            if (_IsAlreadyOut)
            {
                yield break;
            }

            AudioController.PlayClick();
            yield return CoRepeatStage();
        }

        private IEnumerator CoDialog(int stageId)
        {
            if (SharedModel.StageType == StageType.EventDungeon)
            {
                yield break;
            }

            var stageDialogs = TableSheets.Instance.StageDialogSheet
                .OrderedList
                .Where(i => i.StageId == stageId)
                .OrderBy(i => i.DialogId)
                .ToArray();
            if (!stageDialogs.Any())
            {
                yield break;
            }

            var dialog = Find<DialogPopup>();
            foreach (var stageDialog in stageDialogs)
            {
                dialog.Show(stageDialog.DialogId);
                yield return new WaitWhile(() => dialog.gameObject.activeSelf);
            }
        }

        public void Show(Model model, bool isBoosted)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            SharedModel = model;
            _IsAlreadyOut = false;

            var stageText = StageInformation.GetStageIdString(
                SharedModel.StageType,
                SharedModel.StageID,
                 true);
            worldStageId.text = $"{SharedModel.WorldName}" +
                                $" {stageText}";
            actionPoint.SetActionPoint(model.ActionPoint);
            actionPoint.SetEventTriggerEnabled(true);

            foreach (var reward in rewardsArea.rewards)
            {
                reward.gameObject.SetActive(false);
            }

            base.Show();
            closeButton.gameObject.SetActive(
                model.StageID >= Battle.RequiredStageForExitButton ||
                model.LastClearedStageId >= Battle.RequiredStageForExitButton);
            stagePreparationButton.gameObject.SetActive(false);
            repeatButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);

            UpdateView(isBoosted);
            HelpTooltip.HelpMe(100006, true);
        }

        public override void Close(bool ignoreCloseAnimation = false)
        {
            StopVFX();

            foreach (var obj in victoryResultTexts)
            {
                obj.SetActive(false);
            }

            stageProgressBar.Close();
            base.Close(ignoreCloseAnimation);
        }

        private void UpdateView(bool isBoosted)
        {
            switch (SharedModel.State)
            {
                case BattleLog.Result.Win:
                    StartCoroutine(CoUpdateViewAsVictory(isBoosted));
                    break;
                case BattleLog.Result.Lose:
                    UpdateViewAsDefeat(SharedModel.State);
                    break;
                case BattleLog.Result.TimeOver:
                    if (SharedModel.ClearedWaveNumber > 0)
                    {
                        StartCoroutine(CoUpdateViewAsVictory(isBoosted));
                    }
                    else
                    {
                        UpdateViewAsDefeat(SharedModel.State);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEnumerator CoUpdateViewAsVictory(bool isBoosted)
        {
            AudioController.instance.PlayMusic(AudioController.MusicCode.Win, 0.3f);
            StartCoroutine(EmitBattleWinVFX());

            victoryImageContainer.SetActive(true);
            // 4 is index of animation about boost.
            // if not use boost, set animation index to SharedModel.ClearedWaveNumber (1/2/3).
            _victoryImageAnimator.SetInteger(
                ClearedWave,
                isBoosted
                    ? 4
                    : SharedModel.ClearedWaveNumber);

            defeatImageContainer.SetActive(false);
            topArea.SetActive(true);
            defeatTextArea.root.SetActive(false);
            stageProgressBar.Show();
            stageProgressBar.SetStarProgress(SharedModel.ClearedWaveNumber);

            _coUpdateBottomText = StartCoroutine(CoUpdateBottom(Timer));
            yield return StartCoroutine(CoUpdateRewards());
        }

        private IEnumerator EmitBattleWinVFX()
        {
            yield return _battleWinVFXYield;
            AudioController.instance.PlaySfx(AudioController.SfxCode.Win);

            switch (SharedModel.ClearedWaveNumber)
            {
                case 1:
                    _battleWin01VFX =
                        VFXController.instance.CreateAndChase<BattleWin01VFX>(
                            ActionCamera.instance.transform,
                            VfxBattleWinOffset);
                    break;
                case 2:
                    _battleWin02VFX =
                        VFXController.instance.CreateAndChase<BattleWin02VFX>(
                            ActionCamera.instance.transform,
                            VfxBattleWinOffset);
                    break;
                case 3:
                    _battleWin03VFX =
                        VFXController.instance.CreateAndChase<BattleWin03VFX>(
                            ActionCamera.instance.transform,
                            VfxBattleWinOffset);
                    break;
            }
        }

        private void UpdateViewAsDefeat(BattleLog.Result result)
        {
            AudioController.instance.PlayMusic(AudioController.MusicCode.Lose);

            victoryImageContainer.SetActive(false);
            defeatImageContainer.SetActive(true);
            topArea.SetActive(false);
            defeatTextArea.root.SetActive(true);
            var key = "UI_BATTLE_RESULT_DEFEAT_MESSAGE";
            if (result == BattleLog.Result.TimeOver)
            {
                key = "UI_BATTLE_RESULT_TIMEOUT_MESSAGE";
            }

            defeatTextArea.defeatText.text = L10nManager.Localize(key);
            defeatTextArea.expText.text = $"EXP + {SharedModel.Exp}";
            bottomText.enabled = false;

            _coUpdateBottomText = StartCoroutine(CoUpdateBottom(Timer));
            StartCoroutine(CoUpdateRewards());
        }

        private IEnumerator CoUpdateRewards()
        {
            rewardsArea.root.SetActive(true);
            for (var i = 0; i < rewardsArea.rewards.Length; i++)
            {
                var view = rewardsArea.rewards[i];
                view.StartShowAnimation();
                var cleared = SharedModel.ClearedWaveNumber > i;
                switch (i)
                {
                    case 0:
                        view.Set(SharedModel.Exp, cleared);
                        break;
                    case 1:
                        view.Set(SharedModel.Rewards, Game.Game.instance.Stage.stageId, cleared);
                        break;
                    case 2:
                        view.Set(SharedModel.State == BattleLog.Result.Win && cleared);
                        break;
                }

                yield return new WaitForSeconds(0.5f);

                view.gameObject.SetActive(true);
                view.EnableStar(cleared);
                yield return null;
                AudioController.instance.PlaySfx(AudioController.SfxCode.RewardItem);
            }

            yield return new WaitForSeconds(0.5f);

            foreach (var reward in rewardsArea.rewards)
            {
                reward.StopShowAnimation();
                reward.StartScaleTween();
            }
        }

        private IEnumerator CoUpdateBottom(int limitSeconds)
        {
            var secondsFormat = L10nManager.Localize("UI_AFTER_N_SECONDS");
            string fullFormat = string.Empty;
            closeButton.interactable = true;

            if (!SharedModel.IsClear)
            {
                stagePreparationButton.gameObject.SetActive(true);
                stagePreparationButton.interactable = true;
            }

            if (!SharedModel.ActionPointNotEnough)
            {
                var value =
                    SharedModel.StageID >= Battle.RequiredStageForExitButton ||
                    SharedModel.LastClearedStageId >= Battle.RequiredStageForExitButton;
                repeatButton.gameObject.SetActive(value);
                repeatButton.interactable = value;
            }

            if (!SharedModel.IsEndStage &&
                !SharedModel.ActionPointNotEnough &&
                SharedModel.IsClear)
            {
                nextButton.gameObject.SetActive(true);
                nextButton.interactable = true;
            }

            switch (SharedModel.NextState)
            {
                case NextState.GoToMain:
                    SubmitWidget = closeButton.onClick.Invoke;
                    fullFormat = SharedModel.ActionPointNotEnough ?
                        L10nManager.Localize("UI_BATTLE_RESULT_NOT_ENOUGH_ACTION_POINT_FORMAT") :
                        L10nManager.Localize("UI_BATTLE_EXIT_FORMAT");
                    break;
                case NextState.RepeatStage:
                    SubmitWidget = repeatButton.onClick.Invoke;
                    fullFormat = L10nManager.Localize("UI_BATTLE_RESULT_REPEAT_STAGE_FORMAT");
                    break;
                case NextState.NextStage:
                    SubmitWidget = nextButton.onClick.Invoke;
                    fullFormat = L10nManager.Localize("UI_BATTLE_RESULT_NEXT_STAGE_FORMAT");
                    break;
                default:
                    bottomText.text = string.Empty;
                    yield break;
            }

            // for tutorial
            if (SharedModel.StageID == Battle.RequiredStageForExitButton &&
                SharedModel.LastClearedStageId == Battle.RequiredStageForExitButton &&
                SharedModel.State == BattleLog.Result.Win)
            {
                stagePreparationButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(false);
                repeatButton.gameObject.SetActive(false);
                bottomText.text = string.Empty;
                yield break;
            }

            bottomText.text = string.Format(fullFormat, string.Format(secondsFormat, limitSeconds));

            yield return new WaitUntil(() => CanClose);

            var floatTime = (float) limitSeconds;
            var floatTimeMinusOne = limitSeconds - 1f;
            while (limitSeconds > 0)
            {
                yield return null;

                floatTime -= Time.deltaTime;
                if (floatTimeMinusOne < floatTime)
                {
                    continue;
                }

                limitSeconds--;
                bottomText.text = string.Format(fullFormat, string.Format(secondsFormat, limitSeconds));
                floatTimeMinusOne = limitSeconds - 1f;
            }

            StopVFX();
            switch (SharedModel.NextState)
            {
                case NextState.GoToMain:
                    StartCoroutine(OnClickClose());
                    break;
                case NextState.RepeatStage:
                    StartCoroutine(OnClickRepeat());
                    break;
                case NextState.NextStage:
                    StartCoroutine(OnClickNext());
                    break;
            }
        }

        private IEnumerator CoProceedNextStage()
        {
            if (!nextButton.interactable)
            {
                yield break;
            }

            if (Find<Menu>().IsActive())
            {
                yield break;
            }

            closeButton.interactable = false;
            stagePreparationButton.interactable = false;
            repeatButton.interactable = false;
            nextButton.interactable = false;
            actionPoint.SetEventTriggerEnabled(false);

            StopCoUpdateBottomText();
            StartCoroutine(CoFadeOut());
            var stage = Game.Game.instance.Stage;
            stage.IsRepeatStage = false;
            stage.IsExitReserved = false;
            var stageLoadingScreen = Find<StageLoadingEffect>();
            stageLoadingScreen.Show(
                SharedModel.StageType,
                stage.zone,
                SharedModel.WorldName,
                SharedModel.StageID + 1,
                true, SharedModel.StageID);
            Find<Status>().Close();

            StopVFX();
            var player = stage.RunPlayerForNextStage();
            player.DisableHUD();
            ActionRenderHandler.Instance.Pending = true;

            yield return StartCoroutine(SendBattleActionAsync(
                player.Equipments,
                player.Costumes,
                1));
        }

        private IEnumerator CoRepeatStage()
        {
            if (!repeatButton.interactable)
            {
                yield break;
            }

            if (Find<Menu>().IsActive())
            {
                yield break;
            }

            closeButton.interactable = false;
            stagePreparationButton.interactable = false;
            repeatButton.interactable = false;
            nextButton.interactable = false;
            actionPoint.SetEventTriggerEnabled(false);

            StopCoUpdateBottomText();
            StartCoroutine(CoFadeOut());
            var stage = Game.Game.instance.Stage;
            stage.IsExitReserved = false;
            var stageLoadingScreen = Find<StageLoadingEffect>();
            stageLoadingScreen.Show(
                SharedModel.StageType,
                stage.zone,
                SharedModel.WorldName,
                SharedModel.StageID,
                false,
                SharedModel.StageID);
            Find<Status>().Close();

            StopVFX();
            var player = stage.RunPlayerForNextStage();
            player.DisableHUD();
            ActionRenderHandler.Instance.Pending = true;

            var props = new Value
            {
                ["StageId"] = SharedModel.StageID,
            };
            var eventKey = SharedModel.ClearedWaveNumber == 3
                ? "Repeat"
                : "Retry";
            var eventName = $"Unity/Stage Exit {eventKey}";
            Analyzer.Instance.Track(eventName, props);

            yield return StartCoroutine(SendBattleActionAsync(
                player.Equipments,
                player.Costumes,
                0));
        }

        private IEnumerator SendBattleActionAsync(
            List<Equipment> equipments,
            List<Costume> costumes,
            int stageIdOffset)
        {
            yield return SharedModel.StageType switch
            {
                StageType.HackAndSlash => Game.Game.instance.ActionManager
                    .HackAndSlash(
                        costumes,
                        equipments,
                        new List<Consumable>(),
                        SharedModel.WorldID,
                        SharedModel.StageID + stageIdOffset)
                    .StartAsCoroutine(),
                StageType.Mimisbrunnr => Game.Game.instance.ActionManager
                    .MimisbrunnrBattle(
                        costumes,
                        equipments,
                        new List<Consumable>(),
                        SharedModel.WorldID,
                        SharedModel.StageID + stageIdOffset,
                        1)
                    .StartAsCoroutine(),
                StageType.EventDungeon => Game.Game.instance.ActionManager
                    .EventDungeonBattle(
                        RxProps.EventScheduleRowForDungeon.Value.Id,
                        SharedModel.WorldID,
                        SharedModel.StageID + stageIdOffset,
                        equipments,
                        costumes,
                        new List<Consumable>())
                    .StartAsCoroutine(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void NextStage(BattleLog log)
        {
            StartCoroutine(CoGoToNextStageClose(log));
        }

        private IEnumerator CoGoToNextStageClose(BattleLog log)
        {
            if (Find<Menu>().IsActive())
            {
                yield break;
            }

            yield return StartCoroutine(Find<StageLoadingEffect>().CoClose());
            yield return StartCoroutine(CoFadeOut());
            Game.Event.OnStageStart.Invoke(log);
            Close();
        }

        public void NextMimisbrunnrStage(BattleLog log)
        {
            StartCoroutine(CoGoToNextMimisbrunnrStageClose(log));
        }
        private IEnumerator CoGoToNextMimisbrunnrStageClose(BattleLog log)
        {
            if (Find<Menu>().IsActive())
            {
                yield break;
            }

            yield return StartCoroutine(Find<StageLoadingEffect>().CoClose());
            yield return StartCoroutine(CoFadeOut());
            Game.Event.OnStageStart.Invoke(log);
            Close();
        }

        private void GoToMain()
        {
            var props = new Value
            {
                ["StageId"] = Game.Game.instance.Stage.stageId,
            };
            var eventKey = Game.Game.instance.Stage.IsExitReserved ? "Quit" : "Main";
            var eventName = $"Unity/Stage Exit {eventKey}";
            Analyzer.Instance.Track(eventName, props);

            Find<Battle>().Close(true);
            Game.Game.instance.Stage.DestroyBackground();
            Game.Event.OnRoomEnter.Invoke(true);
            Close();

            if (States.Instance.CurrentAvatarState.worldInformation.TryGetLastClearedStageId(
                    out var lastClearedStageId))
            {
                if (SharedModel.IsClear
                    && SharedModel.IsEndStage
                    && lastClearedStageId == SharedModel.StageID
                    && !Find<WorldMap>().SharedViewModel.UnlockedWorldIds.Contains(SharedModel.WorldID + 1))
                {
                    var worldMapLoading = Find<WorldMapLoadingScreen>();
                    worldMapLoading.Show();
                    Game.Game.instance.Stage.OnRoomEnterEnd.First().Subscribe(_ =>
                    {
                        Find<HeaderMenuStatic>().Show();
                        Find<Menu>().Close();
                        Find<WorldMap>().Show(States.Instance.CurrentAvatarState.worldInformation);
                        worldMapLoading.Close(true);
                    });
                }
            }
        }

        private void GoToPreparation()
        {
            Find<Battle>().Close(true);
            Game.Game.instance.Stage.DestroyBackground();
            Game.Event.OnRoomEnter.Invoke(true);
            Close();

            var worldMapLoading = Find<WorldMapLoadingScreen>();
            worldMapLoading.Show();
            Game.Game.instance.Stage.OnRoomEnterEnd.First().Subscribe(_ =>
            {
                CloseWithOtherWidgets();
                Find<HeaderMenuStatic>().UpdateAssets(HeaderMenuStatic.AssetVisibleState.Battle);

                if (SharedModel.WorldID > 10000)
                {
                    var viewModel = new WorldMap.ViewModel
                    {
                        WorldInformation = States.Instance.CurrentAvatarState.worldInformation,
                    };
                    viewModel.SelectedStageId.SetValueAndForceNotify(SharedModel.WorldID);
                    viewModel.SelectedStageId.SetValueAndForceNotify(SharedModel.StageID);
                    Game.Game.instance.TableSheets.WorldSheet.TryGetValue(SharedModel.WorldID,
                        out var worldRow);

                    Find<StageInformation>().Show(viewModel, worldRow, StageType.Mimisbrunnr);

                    Find<BattlePreparation>().Show(
                        StageType.Mimisbrunnr,
                        GameConfig.MimisbrunnrWorldId,
                        SharedModel.StageID,
                        $"{SharedModel.WorldName.ToUpper()} {SharedModel.StageID % 10000000}",
                        true);
                }
                else
                {
                    Find<WorldMap>().Show(SharedModel.WorldID, SharedModel.StageID, false);

                    Find<BattlePreparation>().Show(
                        StageType.HackAndSlash,
                        SharedModel.WorldID,
                        SharedModel.StageID,
                        $"{SharedModel.WorldName.ToUpper()} {SharedModel.StageID}",
                        true);
                }

                worldMapLoading.Close(true);
            });
        }

        private void StopCoUpdateBottomText()
        {
            if (_coUpdateBottomText != null)
            {
                StopCoroutine(_coUpdateBottomText);
            }
        }

        private IEnumerator CoFadeOut()
        {
            while (canvasGroup.alpha > 0f)
            {
                canvasGroup.alpha -= Time.deltaTime;

                yield return null;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        private void StopVFX()
        {
            if (_battleWin01VFX)
            {
                _battleWin01VFX.Stop();
                _battleWin01VFX = null;
            }

            if (_battleWin02VFX)
            {
                _battleWin02VFX.Stop();
                _battleWin02VFX = null;
            }

            if (_battleWin03VFX)
            {
                _battleWin03VFX.Stop();
                _battleWin03VFX = null;
            }

            if (_battleWin04VFX)
            {
                _battleWin04VFX.Stop();
                _battleWin04VFX = null;
            }

            foreach (var reward in rewardsArea.rewards)
            {
                reward.StopVFX();
            }
        }
    }
}
