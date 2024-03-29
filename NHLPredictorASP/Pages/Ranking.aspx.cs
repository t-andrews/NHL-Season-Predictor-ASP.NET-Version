﻿#region Header

// Author: Tommy Andrews
// File: Ranking.aspx.cs
// Project: NHLPredictorASP
// Created: 06/07/2019

#endregion

using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using NHLPredictorASP.Classes;
using NHLPredictorASP.Classes.Entities;
using NHLPredictorASP.Classes.Utility;

namespace NHLPredictorASP.Pages
{
    public partial class Ranking : Page
    {
        //Data table used to populate the page's grid
        private DataTable _dt;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["DataTable"] == null)
            {
                InitDataTable();
                exportButton.Visible = false;
            }
            else
            {
                _dt = Session["DataTable"] as DataTable;
            }

            if (SelectionResources.PlayersMemory != null && SelectionResources.PlayersMemory.Count > 0)
            {
                //Calling the populating method
                PopulateGrid();
            }
        }

        /// <summary>
        ///     Initializes the DataTable _dt with the right columns
        /// </summary>
        private void InitDataTable()
        {
            _dt = new DataTable("Ranking");
            //Initiating data table's column model
            _dt.Columns.Add("Name", typeof(string));
            _dt.Columns.Add("Team", typeof(string));
            _dt.Columns.Add("Pos", typeof(string));
            _dt.Columns.Add("A", typeof(int));
            _dt.Columns.Add("G", typeof(int));
            _dt.Columns.Add("P", typeof(int));
            _dt.Columns.Add("GP", typeof(int));
        }

        /// <summary>
        ///     Adds a player to the data table's' row table
        /// </summary>
        /// <param name="player">Player to add to the data table</param>
        private void AddPlayerToDt(Player player)
        {
            _dt.Rows.Add(player.FullName, player.TeamAbv, player.Code, player.ExpectedSeason.Assists, player.ExpectedSeason.Goals,
                player.ExpectedSeason.Points, player.ExpectedSeason.GamesPlayed);
        }

        /// <summary>
        ///     Populates and binds the grid to the player memory
        /// </summary>
        private void PopulateGrid()
        {
            exportButton.Visible = true;

            AddMissingPlayers();

            BindGrid();
        }

        /// <summary>
        ///     Adds players that are in PlayersMemory and not in the data table
        /// </summary>
        private void AddMissingPlayers()
        {
            if (SelectionResources.PlayersMemory.Count < _dt.Rows.Count)
            {
                return;
            }

            //Adds new players to the data table
            for (var i = _dt.Rows.Count; i < SelectionResources.PlayersMemory.Count; i++)
            {
                AddPlayerToDt(SelectionResources.PlayersMemory[i]);
            }
        }

        /// <summary>
        ///     Sorts the ranking GridView according to a specific column
        ///     Can be descending or ascending
        /// </summary>
        protected void SortColumn_Event(object sender, GridViewSortEventArgs e)
        {
            var direction = SortDirection.Ascending;

            if (Session["SortExpression"] == null ||
                (SortDirection) Session["SortDirection"] == SortDirection.Ascending ||
                !Session["SortExpression"].Equals(e.SortExpression))
            {
                direction = SortDirection.Descending;
            }

            //Sorting according to the sorting direction
            var rows = direction == SortDirection.Descending
                ? _dt.Select().OrderByDescending(r => r[e.SortExpression]).ToArray()
                : _dt.Select().OrderBy(r => r[e.SortExpression]).ToArray();

            _dt = rows.CopyToDataTable();

            //Setting session sorting states to present states so that sorting is reversed each time
            Session["SortDirection"] = direction;
            if (Session["SortExpression"] == null || !Session["SortExpression"].Equals(e.SortExpression))
            {
                Session["SortExpression"] = e.SortExpression;
            }

            BindGrid();
        }

        /// <summary>
        ///     Computes all players in the NHL and populates the ranking grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ComputeAll_Click(object sender, EventArgs e)
        {
            _dt.Rows.Clear();

            foreach (var team in SelectionResources.TeamList)
            {
                foreach (var person in team.PersonList)
                {
                    var player = new Player(ApiLoader.LoadPlayer(DateTime.Now.Year, person.Id), person);
                    SeasonCalculator.CalculateExpectedSeason(player);

                    if (player.HasSufficientInfo)
                    {
                        AddPlayerToDt(player);
                    }
                }
            }

            BindGrid();

            //Making the computeAll button invisible
            computeAllButton.Visible = false;

            //Making the export button visible
            exportButton.Visible = true;
        }

        /// <summary>
        ///     Binds the DataTable to the ranking GridView
        /// </summary>
        protected void BindGrid()
        {
            _dt.AcceptChanges();
            Session["DataTable"] = _dt;
            rankingGrid.DataSource = _dt;
            rankingGrid.DataBind();
        }

        /// <summary>
        ///     Exports the present ranking grid to a Word file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Export_Click(object sender, EventArgs e)
        {
            if (rankingGrid.Rows.Count <= 0)
            {
                return;
            }

            Response.ClearContent();
            Response.AppendHeader("content-disposition", "attachment; filename=ranking.doc");
            Response.ContentType = "application/word";

            var stringWriter = new StringWriter();
            var htw = new HtmlTextWriter(stringWriter);

            rankingGrid.HeaderRow.Style.Clear();
            rankingGrid.HeaderRow.Style.Add("background-color", "#999999");
            rankingGrid.RenderControl(htw);

            Response.Write(stringWriter.ToString());
            Response.End();
            htw.Dispose();
        }

        //Prevents the RenderControl method from verifying the rendering for the ranking GridView
        public override void VerifyRenderingInServerForm(Control control)
        {
        }
    }
}