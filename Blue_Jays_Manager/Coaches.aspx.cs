﻿using Blue_Jays_Manager.Models.DataAccessLayer;
using Blue_Jays_Manager.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Web.UI.WebControls;

namespace Blue_Jays_Manager
{
    public partial class Coaches : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataRetrieval retrieve = new DataRetrieval();
                List<CoachRoster> roster = retrieve.SelectAllCoaches();

                AdminUser user = (AdminUser)Session["AdminUser"];

                if (Session["login"].ToString() == "loggedIn" && user.Role != "coach")
                {
                    SaveCoachChanges.Visible = true;
                    if ((bool)Session["CoachChanges"] == false)
                    {
                        SaveCoachChanges.Enabled = false;
                    }
                    else
                    {
                        SaveCoachChanges.Enabled = true;
                    }
                    AddCoach.Visible = true;
                    CoachRosterGridView.AutoGenerateEditButton = true;
                    CoachRosterGridView.AutoGenerateDeleteButton = true;
                }

                if (Cache["CoachRoster"] == null)
                {
                    Cache.Insert("CoachRoster", roster);
                }

                CoachRosterGridView.DataSource = (List<CoachRoster>)Cache["CoachRoster"];
                CoachRosterGridView.DataBind();
            }



            if (Session["login"].ToString() != "loggedIn")
            {
                _hideColumn(4);
                _hideColumn(5);
            }
        }

        private void _hideColumn(int v)
        {
            CoachRosterGridView.HeaderRow.Cells[v].Visible = false;

            for (int i = 0; i < CoachRosterGridView.Rows.Count; i++)
            {
                CoachRosterGridView.Rows[i].Cells[v].Visible = false;
            }
            CoachRosterGridView.FooterRow.Cells[v].Visible = false;
        }

        protected void EnableUser(object sender, GridViewCommandEventArgs e)
        {
            string name = e.CommandArgument.ToString();
            string[] names = name.Split(' ');
            string firstName = names.First();
            string lastName = names.Last();
            int unlocked = 0;

            List<LockedUser> user = AdminUserDataLayer.GetLockedUsers();

            foreach (LockedUser lockedUser in user)
            {
                if (lockedUser.FirstName == firstName && lockedUser.LastName == lastName)
                {
                    if (lockedUser.Role == "coach")
                    {
                        unlocked = AdminUserDataLayer.EnableUserAccount(firstName, lastName);
                    }
                    else
                    {
                        Label1.Text = "If you are a manager you will need to contact the IT Department to unlock your account";
                    }
                }
            }

            if (unlocked > 0)
            {
                List<CoachRoster> roster = (List<CoachRoster>)Cache["CoachRoster"];
                CoachRoster coach = roster.SingleOrDefault(x => x.Name == firstName + " " + lastName);
                int index = roster.IndexOf(coach);
                roster.RemoveAt(index);
                coach.IsLocked = "Access";
                roster.Insert(index, coach);
                CoachRosterGridView.DataSource = roster;
                CoachRosterGridView.DataBind();
            }



        }

        protected void CoachRosterGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            CoachRosterGridView.EditIndex = e.NewEditIndex;
            CoachRosterGridView.DataSource = (List<CoachRoster>)Cache["CoachRoster"];
            CoachRosterGridView.DataBind();
        }

        protected void CoachRosterGridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            CoachRosterGridView.EditIndex = -1;
            CoachRosterGridView.DataSource = (List<CoachRoster>)Cache["CoachRoster"];
            CoachRosterGridView.DataBind();
        }

        protected void CoachRosterGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            if (Cache["CoachRoster"] != null)
            {
                List<CoachRoster> roster = (List<CoachRoster>)Cache["CoachRoster"];

                string coachNum = CoachRosterGridView.Rows[e.RowIndex].Cells[2].Text;

                Debug.WriteLine(coachNum);

                CoachRoster coach = roster.SingleOrDefault(x => x.CoachNumber == Convert.ToInt32(coachNum));



                if (coach != null)
                {
                    roster.Remove(coach);
                    Cache.Insert("CoachRoster", roster);
                }

                if ((bool)Session["CoachChanges"] == false)
                {
                    Session["CoachChanges"] = true;
                    SaveCoachChanges.Enabled = true;
                }

                CoachRosterGridView.EditIndex = -1;
                CoachRosterGridView.DataSource = (List<CoachRoster>)Cache["CoachRoster"];
                CoachRosterGridView.DataBind();
            }
        }

        protected void CoachRosterGridView_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            if (Cache["CoachRoster"] != null)
            {

                List<CoachRoster> roster = (List<CoachRoster>)Cache["CoachRoster"];

                IOrderedDictionary rowValues = e.NewValues;

                int coachNum = Convert.ToInt32(CoachRosterGridView.Rows[e.RowIndex].Cells[2].Text);

                CoachRoster coach = roster.SingleOrDefault(x => x.CoachNumber == Convert.ToInt32(coachNum));

                int indexOfCoach = roster.IndexOf(coach);

                coach.Name = rowValues["Name"].ToString();
                coach.Position = rowValues["Position"].ToString();


                roster.RemoveAt(indexOfCoach);

                roster.Insert(indexOfCoach, coach);

                if ((bool)Session["CoachChanges"] == false)
                {
                    Session["CoachChanges"] = true;
                    SaveCoachChanges.Enabled = true;
                }

                CoachRosterGridView.EditIndex = -1;

                CoachRosterGridView.DataSource = roster;
                CoachRosterGridView.DataBind();
                Cache["CoachRoster"] = roster;
            }
        }

        protected void SaveCoachChanges_Click(object sender, EventArgs e)
        {
            int rowsInserted = 0;

            if ((bool)Session["CoachChanges"] == true)
            {
                List<CoachRoster> roster = (List<CoachRoster>)Cache["CoachRoster"];

                rowsInserted = DatabaseUpdate.SaveAllCoaches(roster);

                Session["CoachChanges"] = false;
                SaveCoachChanges.Enabled = false;
            }

            if ((bool)Session["PlayerChanges"] == true)
            {
                List<PlayerRoster> roster = (List<PlayerRoster>)Cache["PlayerRoster"];

                rowsInserted = DatabaseUpdate.SaveAllPlayers(roster);

                Session["PlayerChanges"] = false;

            }
        }
    }
}