﻿using SeasonPredict;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebNHLPredictor
{
    public partial class Ranking : Page
    {
        protected DataTable dt;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["DataTable"] == null)
            {
                dt = new DataTable();
                //Initiating data table's column model
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("A", typeof(int));
                dt.Columns.Add("G", typeof(int));
                dt.Columns.Add("P", typeof(int));
                dt.Columns.Add("GP", typeof(int));
            }
            else
            {
                dt = Session["DataTable"] as DataTable;
            }

            exportButton.Visible = false;

            if (Default.PlayersMemory != null && Default.PlayersMemory.Count > 0)
            {
                //Calling the populating method
                PopulateGrid();
            }
        }

        /// <summary>
        /// Populates and binds the grid to the player memory
        /// </summary>
        private void PopulateGrid()
        {
            dt.Rows.Clear();

            //Adds players to the data table
            foreach (var player in Default.PlayersMemory)
            {
                //Adding new row containing the player's expected season's info if it has sufficient information
                if (player.HasSufficientInfo)
                {
                    dt.Rows.Add(player.FullName, player.ExpectedSeason.Assists, player.ExpectedSeason.Goals, player.ExpectedSeason.Points, player.ExpectedSeason.GamesPlayed);
                }
            }

            BindGrid();
            exportButton.Visible = true;
        }

        /// <summary>
        /// Sorts the ranking gridview according to a specific column
        /// Can be descending or ascending
        /// </summary>
        protected void SortColumn_Event(object sender, System.Web.UI.WebControls.GridViewSortEventArgs e)
        {
            SortDirection direction = SortDirection.Ascending;

            if (Session["SortExpression"] == null || (SortDirection)Session["SortDirection"] == SortDirection.Ascending || !Session["SortExpression"].Equals(e.SortExpression))
            {
                direction = SortDirection.Descending;
            }

            DataRow[] rows;

            //Sorting according to the sorting direction
            if (direction == SortDirection.Descending)
            {
                rows = dt.Select().OrderByDescending(r => r[e.SortExpression]).ToArray();
            }
            else
            {
                rows = dt.Select().OrderBy(r => r[e.SortExpression]).ToArray();
            }
            
            dt = rows.CopyToDataTable();

            //Setting session sorting states to present states so that sorting is reversed each time
            Session["SortDirection"] = direction;
            if (Session["SortExpression"] == null || !Session["SortExpression"].Equals(e.SortExpression))
            {
                Session["SortExpression"] = e.SortExpression;
            }

            BindGrid();
        }

        /// <summary>
        /// Computes all players in the NHL and populates the ranking grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ComputeAll_Click(object sender, EventArgs e)
        {
            dt.Rows.Clear();
            Default.ResetPlayersMemory();

            if (Default.TeamsCollection != null)
            {
                foreach (Team team in Default.TeamsCollection)
                {
                    foreach(Roster2 person in team.PersonList)
                    {
                        var player = ApiLoader.loadPlayer(DateTime.Now.Year,person.Id);
                        player.FullName = person.Name;

                        if (player.HasSufficientInfo)
                        {
                            dt.Rows.Add(player.FullName, player.ExpectedSeason.Assists, player.ExpectedSeason.Goals, player.ExpectedSeason.Points, player.ExpectedSeason.GamesPlayed);
                            Default.AddToPlayersMemory(player);
                        }
                    }
                }

                BindGrid();
            }
            else
            {
                //Initializing Default.TeamsCollection
                Default.LoadTeamsCollection();
                //Callback to the method with a now initialized TeamsCollection
                ComputeAll_Click(sender, e);
            }

            //Making the computeAll button invisible
            computeAllButton.Visible = false;

            //Making the export button visible
            exportButton.Visible = true;
        }

        protected void BindGrid()
        {
            rankingGrid.DataSource = dt;
            rankingGrid.DataBind();
            Session["DataTable"] = dt;
        }

        /// <summary>
        /// Exports the ranking grid to a Word file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Export_Click(object sender, EventArgs e)
        {
            if (rankingGrid.Rows.Count > 0)
            {
                Response.ClearContent();
                Response.AppendHeader("content-disposition", "attachment; filename=ranking.doc");
                Response.ContentType = "application/word";
                StringWriter stringWriter = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(stringWriter);
                rankingGrid.HeaderRow.Style.Clear();
                rankingGrid.HeaderRow.Style.Add("background-color", "#999999");
                rankingGrid.RenderControl(htw);
                Response.Write(stringWriter.ToString());
                Response.End();
            }
        }

        //Prevents the RenderControl method from verifying the rendering for the Gridview
        public override void VerifyRenderingInServerForm(Control control) {}
        
    }
}