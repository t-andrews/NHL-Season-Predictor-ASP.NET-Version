﻿using System;

namespace NHLPredictorASP.Classes
{
    #region Season Class representing the offensive production of a Player for a given year

    public class Season
    {
        public int Assists { get; set; }
        public int Goals { get; set; }
        public int Points { get; set; }
        public int GamesPlayed { get; set; }

        public Season()
        {
            Assists = 0;
            Goals = 0;
            Points = 0;
            GamesPlayed = 0;
        }

        public Season(int assists, int goals, int gamesPlayed)
        {
            Assists = assists;
            Goals = goals;
            GamesPlayed = gamesPlayed;
            CalculatePoints();
        }

        public Season Duplicate()
        {
            return new Season(Assists, Goals, GamesPlayed);
        }

        public void CalculatePoints()
        {
            Points = Assists + Goals;
        }

        public override bool Equals(Object o)
        {
            if (!(o is Season))
            {
                return false;
            }

            Season s = o as Season;

            return s.Points == Points && s.Goals == Goals && s.GamesPlayed == GamesPlayed && GetHashCode() == s.GetHashCode();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;

                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, Assists) ? Assists.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, Goals) ? Goals.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, GamesPlayed) ? GamesPlayed.GetHashCode() : 0);
                return hash;
            }
        }
    }
}

#endregion
