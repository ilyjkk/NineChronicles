using System;
using System.Collections.Generic;
using UnityEngine;
using Nekoyume.BlockChain;
using Nekoyume.Data;
using Assets.SimpleLocalization;
using Nekoyume.Data.Table;
using Nekoyume.Game.Controller;
using Nekoyume.UI.Model;
using UniRx;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    public class WorldMap : Widget
    {
        public Module.WorldMapChapter[] chapters;

        public GameObject world;
        public GameObject stage;
        public Transform chapterContainer;
        public Button[] mainButtons;
        public Text[] mainButtonTexts;
        public Button worldButton;
        public Text worldButtonText;
        public Button previousButton;
        public Text pageText;
        public Button nextButton;
        public Button[] closeButtons;

        private World _currentWorld;
        private WorldChapter _currentChapter;
        private int _selectedStage = -1;

        private Module.WorldMapChapter _chapter;
        private readonly List<IDisposable> _disposablesForChapter = new List<IDisposable>();
        
        public int SelectedStage { 
            get
            {
                if (_selectedStage < 0)
                {
                    _selectedStage = States.Instance.currentAvatarState.Value.worldStage;
                }
                
                return _selectedStage;
            }
            set => _selectedStage = value;
        }

        #region Mono

        protected override void Awake()
        {
            base.Awake();

            foreach (var mainButtonText in mainButtonTexts)
            {
                mainButtonText.text = LocalizationManager.Localize("UI_MAIN");    
            }
            worldButtonText.text = LocalizationManager.Localize("UI_WORLD");

            foreach (var mainButton in mainButtons)
            {
                mainButton.OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        AudioController.PlayClick();
                        Close();
                    }).AddTo(gameObject);   
            }
            
            worldButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    AudioController.PlayClick();
                    ShowWorld();
                }).AddTo(gameObject);
            
            previousButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    AudioController.PlayClick();
                    LoadWorld(_currentWorld.id - 1);
                }).AddTo(gameObject);
            
            nextButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    AudioController.PlayClick();
                    LoadWorld(_currentWorld.id + 1);
                }).AddTo(gameObject);

            foreach (var closeButton in closeButtons)
            {
                closeButton.OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        AudioController.PlayClick();
                        Close();
                    }).AddTo(gameObject);   
            }
        }

        #endregion
        
        public override void Show()
        {
            Find<Gold>().Close();
            ShowChapter();
            base.Show();
        }

        public override void Close()
        {
            Find<Gold>().Show();
            Find<QuestPreparation>().OnChangeStage();
            base.Close();
        }

        public void LoadWorld(int worldId)
        {
            if (!Tables.instance.World.TryGetValue(worldId, out var worldRow))
            {
                throw new KeyNotFoundException($"worldId({worldId})");
            }
            
            LoadWorld(worldRow, worldRow.chapterBegin);
        }
        
        private void LoadWorld(World worldRow, int chapterId)
        {
            _currentWorld = worldRow;
            
            if (chapterId < _currentWorld.chapterBegin
                || chapterId > _currentWorld.chapterEnd)
            {
                throw new ArgumentOutOfRangeException($"chapterId({chapterId})");
            }

            ChangeChapter(chapterId);
            SetModelToChapter();

            previousButton.interactable = _currentWorld.id > 1;
            nextButton.interactable = _currentWorld.id < Tables.instance.World.Count;
            pageText.text = $"{_currentWorld.id} / {Tables.instance.World.Count}";
            
            ShowStage();
        }

        private void ChangeChapter(int chapterId)
        {
            if (_chapter)
            {
                _chapter.Model.Dispose();
                _chapter = null;
            }
        
            if (!Tables.instance.WorldChapter.TryGetValue(chapterId, out _currentChapter))
            {
                throw new KeyNotFoundException($"chapterId({chapterId})");
            }
            
            if (!TryGetWorldMapChapter(_currentChapter.prefab, out var worldMapChapter))
            {
                throw new FailedToLoadResourceException<WorldMapChapter>();
            }

            if (chapterContainer.childCount > 0)
            {
                Destroy(chapterContainer.GetChild(0).gameObject);
            }
            
            _chapter = Instantiate(worldMapChapter, chapterContainer);
        }

        private void SetModelToChapter()
        {
            _disposablesForChapter.DisposeAllAndClear();

            var previousStage = 0;
            var stageModels = new List<WorldMapStage>();
            WorldMapStage currentStageModel = null;
            foreach (var stagePair in Tables.instance.Stage)
            {
                if (stagePair.Value.stage < _currentChapter.stageBegin
                    || stagePair.Value.stage > _currentChapter.stageEnd)
                {
                    continue;
                }
                
                var currentStage = stagePair.Value.stage;

                if (previousStage != currentStage)
                {
                    var stageState = WorldMapStage.State.Normal;
                    if (stagePair.Value.stage == SelectedStage)
                    {
                        stageState = WorldMapStage.State.Selected;
                    }
                    else if (stagePair.Value.stage < States.Instance.currentAvatarState.Value.worldStage)
                    {
                        stageState = WorldMapStage.State.Cleared;
                    }
                    else if (stagePair.Value.stage > States.Instance.currentAvatarState.Value.worldStage)
                    {
                        stageState = WorldMapStage.State.Disabled;
                    }
                    
                    currentStageModel = new WorldMapStage(stageState, currentStage, false);
                    currentStageModel.onClick.Subscribe(_ =>
                    {
                        SelectedStage = _.Model.stage.Value;
                        Close();
                    }).AddTo(_disposablesForChapter);
                
                    stageModels.Add(currentStageModel);
                }
                
                if (stagePair.Value.isBoss
                    && currentStageModel != null)
                {
                    currentStageModel.hasBoss.Value = true;
                }

                previousStage = currentStage;
            }
            
            var chapterModel = new WorldMapChapter(stageModels);
            _chapter.SetModel(chapterModel);
        }

        private bool TryGetWorldMapChapter(string chapterPrefab, out Module.WorldMapChapter worldMapChapter)
        {
            foreach (var chapter in chapters)
            {
                if (!chapter.name.Equals(chapterPrefab))
                {
                    continue;
                }

                worldMapChapter = chapter;
                return true;
            }
            
            worldMapChapter = null;
            return false;
        }
        
        private void ShowWorld()
        {
            world.SetActive(true);
            stage.SetActive(false);
        }
        
        private void ShowStage()
        {
            world.SetActive(false);
            stage.SetActive(true);
        }

        private void ShowChapter()
        {
            WorldChapter targetChapter = null;
            foreach (var worldChapterPair in Tables.instance.WorldChapter)
            {
                var chapterRow = worldChapterPair.Value;
                if (SelectedStage < chapterRow.stageBegin ||
                    SelectedStage > chapterRow.stageEnd)
                {
                    continue;
                }
                
                targetChapter = chapterRow;
                    
                break;
            }

            if (targetChapter == null)
            {
                throw new SheetRowNotFoundException();
            }
            
            foreach (var worldPair in Tables.instance.World)
            {
                var worldRow = worldPair.Value;
                if (targetChapter.id < worldRow.chapterBegin
                    || targetChapter.id > worldRow.chapterEnd)
                {
                    continue;
                }
                
                LoadWorld(worldRow, targetChapter.id);
                
                break;
            }
        }
    }
}
