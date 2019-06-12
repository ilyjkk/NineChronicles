using System.Linq;
using UnityEngine;

namespace Nekoyume.Game.VFX.Skill
{
    public class SkillAreaVFX : SkillVFX
    {
        protected override float EmmitDuration => 0f;
        public ParticleSystem finisher;
        public ParticleSystem last;
        private ParticleSystem[] _loops;

        public override void Play()
        {
            if (finisher)
                finisher.gameObject.SetActive(false);
            base.Play();
        }

        public void StopLoop()
        {
            foreach (var ps in _loops)
            {
                ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }
        public void Finisher()
        {
            if (finisher)
            {
                finisher.gameObject.SetActive(true);
                finisher.Play();
            }
        }

        public override void Awake()
        {
            base.Awake();

            _loops = GetComponentsInChildren<ParticleSystem>().Where(ps => ps.main.loop).ToArray();
        }
    }
}
