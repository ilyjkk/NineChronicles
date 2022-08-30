using Nekoyume.Helper;
using Nekoyume.UI.Module;
using Nekoyume.UI.Module.WorldBoss;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    public class WorldBossResultPopup : PopupWidget
    {
        [SerializeField]
        private Transform gradeParent;

        [SerializeField]
        private GameObject seasonBestObject;

        [SerializeField]
        private TextMeshProUGUI scoreText;

        [SerializeField]
        private Button closeButton;

        private GameObject _gradeObject;

        protected override void Awake()
        {
            base.Awake();
            closeButton.onClick.AddListener(() => Close());
        }

        protected override void OnDisable()
        {
            Destroy(_gradeObject);
            base.OnDisable();
        }

        public void Show(int bossId, int score, bool isBest)
        {
            base.Show();

            scoreText.text = score.ToString("N0");
            seasonBestObject.SetActive(isBest);

            if (Game.Game.instance.TableSheets.WorldBossCharacterSheet.TryGetValue(bossId, out var row))
            {
                var grade = (WorldBossGrade)WorldBossHelper.CalculateRank(row, score);

                if (WorldBossFrontHelper.TryGetGrade(grade, false, out var prefab))
                {
                    _gradeObject = Instantiate(prefab, gradeParent);
                    _gradeObject.transform.localScale = Vector3.one;
                }
            }
        }

        public override void Close(bool ignoreCloseAnimation = false)
        {
            Find<WorldBoss>().ShowAsync().Forget();
            base.Close(ignoreCloseAnimation);
        }
    }
}
