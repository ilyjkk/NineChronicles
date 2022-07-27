﻿using System.Collections.Generic;

namespace Nekoyume.UI.Model
{
    public class WorldBossRankingRecord
    {
        public int Ranking;
        public int Level;
        public int Cp;
        public int IconId;
        public string AvatarName;
        public string Address;
        public int HighScore;
        public int TotalScore;
    }

    public class WorldBossRankingResponse
    {
        public List<WorldBossRankingRecord> WorldBossRanking;
    }

    public class WorldBossTotalUsersResponse
    {
        public int WorldBossTotalUsers;
    }

}
