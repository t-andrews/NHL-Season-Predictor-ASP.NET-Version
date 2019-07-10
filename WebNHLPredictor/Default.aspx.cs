﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SeasonPredict;

namespace WebNHLPredictor
{
    public partial class Default : Page
    {
        public static TeamCollection TeamsCollection;

        public static ObservableCollection<Roster2> PersonCollection;

        public static List<Player> PlayersMemory;

        public static ApiLoader Loader;

        /// <summary>
        /// Loads the page and initializes the following components:
        /// The API loader used to fetch info from the NHL's API
        /// The team collection (all the teams loaded from the NHL's API)
        /// The player memory, used to store already calculated players (prevents extra calls to the API)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if(!IsPostBack)//Prevents from resetting the components at every postback
            {
                Loader = Session["Loader"] == null ? new ApiLoader() : Session["Loader"] as ApiLoader;
                TeamsCollection = Session["TeamCollection"] == null ? new TeamCollection() : Session["TeamCollection"] as TeamCollection;
                PlayersMemory = Session["PlayersMemory"] == null ? new List<Player>() : Session["PlayersMemory"] as List<Player>;

                int teamIndex = (int?) Session["SelectedTeamIndex"] ?? 0;
                int playerIndex = (int?) Session["SelectedPlayerIndex"] ?? 0;

                teamsSelect.SelectedIndex = teamIndex;
                teamsSelect.DataSource = TeamsCollection;
                teamsSelect.DataBind();

                PersonCollection = TeamsCollection[teamIndex].PersonList;

                playersSelect.SelectedIndex = playerIndex;
                playersSelect.DataSource = PersonCollection;
                playersSelect.DataBind();
            }
        }

        /// <summary>
        /// Calls the needed methods to calculate and print the player's expected season
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ComputePlayer(object sender, EventArgs e)
        {
            if (!playersSelect.SelectedItem.Equals(null))
            {
                Session["SelectedPlayerIndex"] = playersSelect.SelectedIndex;
                computeButton.Enabled = false;
                var tempPlayer = PersonCollection[playersSelect.SelectedIndex] as Roster2;
                //Check if the player hasn't already been loaded (avoiding to call the api again)
                if (!PlayersMemory.Any(p => p.Id.Equals(PersonCollection[playersSelect.SelectedIndex].Id)))
                {
                    //result.Text = "Calculating...";

                    //Fetching the player through the player loader
                    var player = new Player(Loader.loadPlayer(tempPlayer.Id), tempPlayer.Name, tempPlayer.Id);

                    //Adding player to the already calculated players
                    PlayersMemory.Add(Player.duplicate(player));
                    Session["PlayersMemory"] = PlayersMemory;
 
                    //Printing the player's expected season to the result textbox
                    result.Text = player.ToString();
                }
                else
                {
                    result.Text = PlayersMemory.First(p => p.Id.Equals(tempPlayer.Id)).ToString();
                }
            }
        }

        /// <summary>
        ///     teamsListbox SelectionChanged event handling method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TeamsSelect_OnServerChange(object sender, EventArgs e)
        {
            Session["SelectedPlayerIndex"] = 0;
            Session["SelectedTeamIndex"] = teamsSelect.SelectedIndex;
            PersonCollection = TeamsCollection[teamsSelect.SelectedIndex].PersonList;
            playersSelect.DataSource = PersonCollection;
            playersSelect.DataBind();
            EnableComputeButton(sender, e);
        }

        /// <summary>
        ///     playersListbox SelectionChanged event handling method
        ///     When selection is changed and the button is disabled, it gets enabled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void EnableComputeButton(object sender, EventArgs e)
        {
            if(sender == playersSelect)
            {
                Session["SelectedTeamIndex"] = teamsSelect.SelectedIndex;
                Session["SelectedPlayerIndex"] = playersSelect.SelectedIndex;
            }

            if (!computeButton.Enabled)
            {
                computeButton.Enabled = true;
            }
        }
    }
}