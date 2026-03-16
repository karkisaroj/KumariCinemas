using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class Showtimes : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connStr = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadShowtimes();
                LoadDropdowns();
            }
        }

        private void LoadShowtimes()
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                string sql =
                    "SELECT s.SHOW_ID, s.SHOW_DATE, " +
                    "TO_CHAR(s.SHOW_TIME,'HH24:MI') AS SHOW_TIME, " +
                    "TO_CHAR(s.SHOW_END_TIME,'HH24:MI') AS SHOW_END_TIME, " +
                    "NVL((SELECT m.MOVIE_TITLE FROM \"ShowHallMovCust\" shmc " +
                         "JOIN MOVIE m ON shmc.MOVIE_ID = m.MOVIE_ID " +
                         "WHERE shmc.SHOW_ID = s.SHOW_ID AND ROWNUM = 1), 'N/A') AS MOVIE_TITLE, " +
                    "NVL((SELECT th.THEATER_NAME FROM \"ShowHallMovCust\" shmc " +
                         "JOIN THEATER th ON shmc.THEATER_ID = th.THEATER_ID " +
                         "WHERE shmc.SHOW_ID = s.SHOW_ID AND ROWNUM = 1), 'N/A') AS THEATER_NAME, " +
                    "NVL((SELECT h.HALL_NAME FROM \"ShowHallMovCust\" shmc " +
                         "JOIN HALL h ON shmc.HALL_ID = h.HALL_ID " +
                         "WHERE shmc.SHOW_ID = s.SHOW_ID AND ROWNUM = 1), 'N/A') AS HALL_NAME " +
                    "FROM SHOWTIME s " +
                    "ORDER BY s.SHOW_DATE, s.SHOW_TIME";

                var da = new OracleDataAdapter(sql, conn);
                var dt = new DataTable();
                da.Fill(dt);
                gvShowtimes.DataSource = dt;
                gvShowtimes.DataBind();
            }
        }

        private void LoadDropdowns()
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                FillDDL(ddlMovie, conn, "SELECT MOVIE_ID, MOVIE_TITLE FROM MOVIE ORDER BY MOVIE_TITLE", "MOVIE_TITLE", "MOVIE_ID");
                FillDDL(ddlTheater, conn, "SELECT THEATER_ID, THEATER_NAME FROM THEATER ORDER BY THEATER_NAME", "THEATER_NAME", "THEATER_ID");
                FillDDL(ddlHall, conn, "SELECT HALL_ID, HALL_NAME FROM HALL ORDER BY HALL_NAME", "HALL_NAME", "HALL_ID");
            }
        }

        private void FillDDL(DropDownList ddl, OracleConnection conn, string sql, string textField, string valueField)
        {
            var da = new OracleDataAdapter(sql, conn);
            var dt = new DataTable();
            da.Fill(dt);
            ddl.DataSource = dt;
            ddl.DataTextField = textField;
            ddl.DataValueField = valueField;
            ddl.DataBind();
            ddl.Items.Insert(0, new ListItem("-- Select --", "0"));
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        {
            hfShowId.Value = "0";
            txtDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            txtTime.Text = "";
            txtEndTime.Text = "";
            lblModalTitle.Text = "Add Showtime";
            LoadDropdowns();
            ShowModal = true;
            LoadShowtimes();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDate.Text) ||
                string.IsNullOrWhiteSpace(txtTime.Text) ||
                string.IsNullOrWhiteSpace(txtEndTime.Text) ||
                ddlMovie.SelectedValue == "0" ||
                ddlTheater.SelectedValue == "0" ||
                ddlHall.SelectedValue == "0")
            {
                ShowAlert("Please fill in all required fields.", "warning");
                ShowModal = true; LoadDropdowns(); LoadShowtimes(); return;
            }

            if (TimeSpan.Parse(txtEndTime.Text) <= TimeSpan.Parse(txtTime.Text))
            {
                ShowAlert("End time must be after start time.", "warning");
                ShowModal = true; LoadDropdowns(); LoadShowtimes(); return;
            }

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    int movieId = int.Parse(ddlMovie.SelectedValue);
                    int theaterId = int.Parse(ddlTheater.SelectedValue);
                    int hallId = int.Parse(ddlHall.SelectedValue);

                    var showDate = DateTime.Parse(txtDate.Text);
                    var showTime = DateTime.Parse(txtDate.Text + " " + txtTime.Text);
                    var showEnd = DateTime.Parse(txtDate.Text + " " + txtEndTime.Text);

                    // Use first available customer as system placeholder
                    // The real customer gets linked when a ticket is booked
                    var cmdCust = new OracleCommand("SELECT MIN(CUSTOMER_ID) FROM CUSTOMER", conn);
                    var custObj = cmdCust.ExecuteScalar();
                    if (custObj == null || custObj == DBNull.Value)
                    {
                        ShowAlert("Please add at least one customer before creating a showtime.", "warning");
                        ShowModal = true; LoadDropdowns(); LoadShowtimes(); return;
                    }
                    int systemCustomerId = Convert.ToInt32(custObj);

                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            if (hfShowId.Value == "0") // NEW showtime
                            {
                                var cmdShow = new OracleCommand(
                                    "INSERT INTO SHOWTIME(SHOW_ID, SHOW_DATE, SHOW_TIME, SHOW_END_TIME) " +
                                    "VALUES((SELECT NVL(MAX(SHOW_ID),0)+1 FROM SHOWTIME), :sdate, :stime, :send)", conn);
                                cmdShow.Transaction = tran;
                                cmdShow.Parameters.Add(":sdate", OracleDbType.Date).Value = showDate;
                                cmdShow.Parameters.Add(":stime", OracleDbType.TimeStamp).Value = showTime;
                                cmdShow.Parameters.Add(":send", OracleDbType.TimeStamp).Value = showEnd;
                                cmdShow.ExecuteNonQuery();

                                var cmdGetId = new OracleCommand("SELECT MAX(SHOW_ID) FROM SHOWTIME", conn);
                                cmdGetId.Transaction = tran;
                                int newShowId = Convert.ToInt32(cmdGetId.ExecuteScalar());

                                var cmdJ = new OracleCommand(
                                    "INSERT INTO \"ShowHallMovCust\"(SHOW_ID, HALL_ID, THEATER_ID, MOVIE_ID, CUSTOMER_ID) " +
                                    "VALUES(:showId, :hallId, :theaterId, :movieId, :custId)", conn);
                                cmdJ.Transaction = tran;
                                cmdJ.Parameters.Add(":showId", OracleDbType.Int32).Value = newShowId;
                                cmdJ.Parameters.Add(":hallId", OracleDbType.Int32).Value = hallId;
                                cmdJ.Parameters.Add(":theaterId", OracleDbType.Int32).Value = theaterId;
                                cmdJ.Parameters.Add(":movieId", OracleDbType.Int32).Value = movieId;
                                cmdJ.Parameters.Add(":custId", OracleDbType.Int32).Value = systemCustomerId;
                                cmdJ.ExecuteNonQuery();

                                ShowAlert("Showtime added successfully!", "success");
                            }
                            else // EDIT existing showtime
                            {
                                int showId = int.Parse(hfShowId.Value);

                                var cmdShow = new OracleCommand(
                                    "UPDATE SHOWTIME SET SHOW_DATE=:sdate, SHOW_TIME=:stime, SHOW_END_TIME=:send " +
                                    "WHERE SHOW_ID=:id", conn);
                                cmdShow.Transaction = tran;
                                cmdShow.Parameters.Add(":sdate", OracleDbType.Date).Value = showDate;
                                cmdShow.Parameters.Add(":stime", OracleDbType.TimeStamp).Value = showTime;
                                cmdShow.Parameters.Add(":send", OracleDbType.TimeStamp).Value = showEnd;
                                cmdShow.Parameters.Add(":id", OracleDbType.Int32).Value = showId;
                                cmdShow.ExecuteNonQuery();

                                var cmdJ = new OracleCommand(
                                    "UPDATE \"ShowHallMovCust\" " +
                                    "SET MOVIE_ID=:movieId, THEATER_ID=:theaterId, HALL_ID=:hallId " +
                                    "WHERE SHOW_ID=:id", conn);
                                cmdJ.Transaction = tran;
                                cmdJ.Parameters.Add(":movieId", OracleDbType.Int32).Value = movieId;
                                cmdJ.Parameters.Add(":theaterId", OracleDbType.Int32).Value = theaterId;
                                cmdJ.Parameters.Add(":hallId", OracleDbType.Int32).Value = hallId;
                                cmdJ.Parameters.Add(":id", OracleDbType.Int32).Value = showId;
                                cmdJ.ExecuteNonQuery();

                                ShowAlert("Showtime updated successfully!", "success");
                            }

                            tran.Commit();
                        }
                        catch { tran.Rollback(); throw; }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, "danger");
                ShowModal = true;
                LoadDropdowns();
            }
            LoadShowtimes();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ShowModal = false;
            LoadShowtimes();
        }

        protected void gvShowtimes_RowEditing(object sender, GridViewEditEventArgs e)
        {
            int id = int.Parse(gvShowtimes.DataKeys[e.NewEditIndex].Value.ToString());
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();

                var cmd = new OracleCommand("SELECT * FROM SHOWTIME WHERE SHOW_ID=:id", conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hfShowId.Value = id.ToString();
                        txtDate.Text = Convert.ToDateTime(reader["SHOW_DATE"]).ToString("yyyy-MM-dd");
                        txtTime.Text = Convert.ToDateTime(reader["SHOW_TIME"]).ToString("HH:mm");
                        txtEndTime.Text = Convert.ToDateTime(reader["SHOW_END_TIME"]).ToString("HH:mm");
                    }
                }

                LoadDropdowns();
                var cmdJ = new OracleCommand(
                    "SELECT MOVIE_ID, THEATER_ID, HALL_ID FROM \"ShowHallMovCust\" " +
                    "WHERE SHOW_ID=:id AND ROWNUM=1", conn);
                cmdJ.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                using (var r = cmdJ.ExecuteReader())
                {
                    if (r.Read())
                    {
                        ddlMovie.SelectedValue = r["MOVIE_ID"].ToString();
                        ddlTheater.SelectedValue = r["THEATER_ID"].ToString();
                        ddlHall.SelectedValue = r["HALL_ID"].ToString();
                    }
                }

                lblModalTitle.Text = "Edit Showtime";
                ShowModal = true;
            }
            gvShowtimes.EditIndex = -1;
            LoadShowtimes();
        }

        protected void gvShowtimes_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvShowtimes.EditIndex = -1;
            LoadShowtimes();
        }

        protected void gvShowtimes_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvShowtimes.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    var checkCmd = new OracleCommand(
                        "SELECT COUNT(*) FROM \"TktShowHallMovCust\" WHERE SHOW_ID=:id", conn);
                    checkCmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                    int ticketCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (ticketCount > 0)
                    {
                        ShowAlert("Cannot delete: this showtime has " + ticketCount + " ticket(s). Delete those tickets first.", "warning");
                        LoadShowtimes(); return;
                    }

                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            var c1 = new OracleCommand("DELETE FROM \"ShowHallMovCust\" WHERE SHOW_ID=:id", conn);
                            c1.Transaction = tran;
                            c1.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            c1.ExecuteNonQuery();

                            var c2 = new OracleCommand("DELETE FROM SHOWTIME WHERE SHOW_ID=:id", conn);
                            c2.Transaction = tran;
                            c2.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            c2.ExecuteNonQuery();

                            tran.Commit();
                            ShowAlert("Showtime deleted successfully!", "success");
                        }
                        catch { tran.Rollback(); throw; }
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Cannot delete: " + ex.Message, "danger"); }
            LoadShowtimes();
        }

        private void ShowAlert(string message, string bootstrapType)
        {
            lblMessage.Text = "<i class='bi bi-info-circle-fill me-2'></i>" + message;
            lblMessage.CssClass = "alert alert-" + bootstrapType + " d-block mb-3";
            lblMessage.Visible = true;
        }
    }
}