﻿using NUnit.Framework;
using System;

namespace NHLPredictorASP.Classes
{
    [TestFixture]
    public class ApiLoader_Test
    {

        const int NB_TEAMS = 31, NB_SEASONS = 12;
        string ovechkinId;

        [OneTimeSetUp]
        public void Before()
        {
            ovechkinId = "8471214";
        }

        //LoadTeams method should load the right number of teams
        [Test]
        public void TestTeamNumber()
        {
            Assert.AreEqual(ApiLoader.LoadTeams().Count, NB_TEAMS);
        }

        //LoadPlayer method should load the right player with the passed ID
        [Test]
        public void TestLoadPlayer()
        {
            Assert.AreEqual(NB_SEASONS, ApiLoader.LoadPlayer(2019, ovechkinId).SeasonList.Count);
        }

        //GetSeason should return the right season stats for 2018/2019
        [Test]
        public void TestGetSeason()
        {
            Season testSeason = ApiLoader.GetSeason(2019, ovechkinId);
            Season referenceSeason = new Season(38, 51, 81);
            Assert.AreEqual(referenceSeason, testSeason);
        }

        //GetSeason should return the right season stats for 1989/1990 (invalid season)RA
        [Test]
        public void TestGetSeasonInvalid()
        {
            Season season = ApiLoader.GetSeason(1990, ovechkinId);
            Assert.That(season is null);
        }
    }
}