using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class Showtimes : System.Web.UI.Page
    {
        // Control declarations
        protected Label lblMessage;
        protected Button btnShowAdd;
        protected GridView gvShowtimes;
        protected Label lblModalTitle;
        protected Button btnCancel;
        protected HiddenField hfShowId;
        protected DropDownList ddlMovie;
        protected DropDownList ddlTheater;
        protected DropDownList ddlHall;
        protected TextBox txtDate;
        protected TextBox txtTime;
        protected TextBox txtEndTime;
        protected Button btnCancelFooter;
        protected Button btnSave;

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
                
                // Query joins through the ShowHallMovCust junction table
                // Use ROWNUM to get first matching record for each showtime
                string sql = "SELECT s.SHOW_ID, " +
                             "s.SHOW_DATE, " +
                             "TO_CHAR(s.SHOW_TIME,'HH24:MI') AS SHOW_TIME, " +
                             "TO_CHAR(s.SHOW_END_TIME,'HH24:MI') AS SHOW_END_TIME, " +
                             "NVL((SELECT m.MOVIE_TITLE FROM \"ShowHallMovCust\" shmc " +
                             "JOIN MOVIE m ON shmc.MOVIE_ID = m.MOVIE_ID " +
                             "WHERE shmc.SHOW_ID = s.SHOW_ID AND ROWNUM = 1), 'N/A') AS MOVIE_TITLE, " +
                             "NVL((SELECT t.THEATER_NAME FROM \"ShowHallMovCust\" shmc " +
                             "JOIN THEATER t ON shmc.THEATER_ID = t.THEATER_ID " +
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
            LoadDropdowns();
            lblModalTitle.Text = "Add Showtime";
            ShowModal = true;
            LoadShowtimes();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDate.Text) || string.IsNullOrWhiteSpace(txtTime.Text) ||
                string.IsNullOrWhiteSpace(txtEndTime.Text) || ddlMovie.SelectedValue == "0" || 
                ddlTheater.SelectedValue == "0" || ddlHall.SelectedValue == "0")
            {
                ShowAlert("Please fill in all required fields.", "warning");
                ShowModal = true;
                LoadDropdowns();
                LoadShowtimes();
                return;
            }

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    var showDate = DateTime.Parse(txtDate.Text);
                    var showTime = DateTime.Parse(txtDate.Text + " " + txtTime.Text);
                    var showEnd = DateTime.Parse(txtDate.Text + " " + txtEndTime.Text);

                    if (hfShowId.Value == "0")
                    {
                        // Insert into SHOWTIME table
                        var cmd = new OracleCommand(@"INSERT INTO SHOWTIME(SHOW_ID,SHOW_DATE,SHOW_TIME,SHOW_END_TIME) 
                            VALUES((SELECT NVL(MAX(SHOW_ID),0)+1 FROM SHOWTIME),:sdate,:stime,:send)", conn);
                        cmd.Parameters.Add(":sdate", OracleDbType.Date).Value = showDate;
                        cmd.Parameters.Add(":stime", OracleDbType.TimeStamp).Value = showTime;
                        cmd.Parameters.Add(":send", OracleDbType.TimeStamp).Value = showEnd;
                        cmd.ExecuteNonQuery();
                        
                        // Get the newly created SHOW_ID
                        var cmdGetId = new OracleCommand("SELECT MAX(SHOW_ID) FROM SHOWTIME", conn);
                        var newShowId = Convert.ToInt32(cmdGetId.ExecuteScalar());
                        
                        // Insert into junction table
                        // Get the first available customer ID from database
                        var cmdGetCustId = new OracleCommand("SELECT MIN(CUSTOMER_ID) FROM CUSTOMER", conn);
                        var systemCustomerId = Convert.ToInt32(cmdGetCustId.ExecuteScalar());
                        
                        var cmdJunction = new OracleCommand(
                            "INSERT INTO \"ShowHallMovCust\" " +
                            "(SHOW_ID, HALL_ID, THEATER_ID, MOVIE_ID, CUSTOMER_ID) " +
                            "VALUES (:showId, :hallId, :theaterId, :movieId, :custId)", conn);
                        cmdJunction.Parameters.Add(":showId", OracleDbType.Int32).Value = newShowId;
                        cmdJunction.Parameters.Add(":hallId", OracleDbType.Int32).Value = int.Parse(ddlHall.SelectedValue);
                        cmdJunction.Parameters.Add(":theaterId", OracleDbType.Int32).Value = int.Parse(ddlTheater.SelectedValue);
                        cmdJunction.Parameters.Add(":movieId", OracleDbType.Int32).Value = int.Parse(ddlMovie.SelectedValue);
                        cmdJunction.Parameters.Add(":custId", OracleDbType.Int32).Value = systemCustomerId;
                        cmdJunction.ExecuteNonQuery();
                        
                        ShowAlert("Showtime added successfully!", "success");
                    }
                    else
                    {
                        // Update SHOWTIME table
                        var cmd = new OracleCommand(@"UPDATE SHOWTIME SET SHOW_DATE=:sdate,SHOW_TIME=:stime,SHOW_END_TIME=:send 
                            WHERE SHOW_ID=:id", conn);
                        cmd.Parameters.Add(":sdate", OracleDbType.Date).Value = showDate;
                        cmd.Parameters.Add(":stime", OracleDbType.TimeStamp).Value = showTime;
                        cmd.Parameters.Add(":send", OracleDbType.TimeStamp).Value = showEnd;
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfShowId.Value);
                        cmd.ExecuteNonQuery();
                        
                        // Update junction table (only rows for showtime configuration)
                        var cmdGetCustId = new OracleCommand("SELECT MIN(CUSTOMER_ID) FROM CUSTOMER", conn);
                        var systemCustomerId = Convert.ToInt32(cmdGetCustId.ExecuteScalar());
                        
                        var cmdJunction = new OracleCommand(
                            "UPDATE \"ShowHallMovCust\" " +
                            "SET MOVIE_ID=:movieId, THEATER_ID=:theaterId, HALL_ID=:hallId " +
                            "WHERE SHOW_ID=:id AND CUSTOMER_ID = :custId", conn);
                        cmdJunction.Parameters.Add(":movieId", OracleDbType.Int32).Value = int.Parse(ddlMovie.SelectedValue);
                        cmdJunction.Parameters.Add(":theaterId", OracleDbType.Int32).Value = int.Parse(ddlTheater.SelectedValue);
                        cmdJunction.Parameters.Add(":hallId", OracleDbType.Int32).Value = int.Parse(ddlHall.SelectedValue);
                        cmdJunction.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfShowId.Value);
                        cmdJunction.Parameters.Add(":custId", OracleDbType.Int32).Value = systemCustomerId;
                        cmdJunction.ExecuteNonQuery();
                        
                        ShowAlert("Showtime updated successfully!", "success");
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
                // Query SHOWTIME and junction table
                var cmd = new OracleCommand(
                    "SELECT s.SHOW_ID, s.SHOW_DATE, " +
                    "TO_CHAR(s.SHOW_TIME,'HH24:MI') AS SHOW_TIME, " +
                    "TO_CHAR(s.SHOW_END_TIME,'HH24:MI') AS SHOW_END_TIME, " +
                    "shtmc.MOVIE_ID, shtmc.THEATER_ID, shtmc.HALL_ID " +
                    "FROM SHOWTIME s " +
                    "LEFT JOIN \"ShowHallMovCust\" shtmc ON s.SHOW_ID = shtmc.SHOW_ID " +
                    "WHERE s.SHOW_ID=:id AND ROWNUM = 1", conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        hfShowId.Value = id.ToString();
                        txtDate.Text = Convert.ToDateTime(r["SHOW_DATE"]).ToString("yyyy-MM-dd");
                        txtTime.Text = r["SHOW_TIME"].ToString();
                        txtEndTime.Text = r["SHOW_END_TIME"].ToString();
                        
                        LoadDropdowns();
                        
                        // Set dropdown selected values if data exists
                        if (r["MOVIE_ID"] != DBNull.Value)
                            ddlMovie.SelectedValue = r["MOVIE_ID"].ToString();
                        if (r["THEATER_ID"] != DBNull.Value)
                            ddlTheater.SelectedValue = r["THEATER_ID"].ToString();
                        if (r["HALL_ID"] != DBNull.Value)
                            ddlHall.SelectedValue = r["HALL_ID"].ToString();
                        
                        lblModalTitle.Text = "Edit Showtime";
                        ShowModal = true;
                    }
                }
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
                    // Delete from junction table first (foreign key constraint)
                    var cmdJunction = new OracleCommand("DELETE FROM \"ShowHallMovCust\" WHERE SHOW_ID=:id", conn);
                    cmdJunction.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                    cmdJunction.ExecuteNonQuery();
                    
                    // Then delete from SHOWTIME table
                    var cmd = new OracleCommand("DELETE FROM SHOWTIME WHERE SHOW_ID=:id", conn);
                    cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                    cmd.ExecuteNonQuery();
                }
                ShowAlert("Showtime deleted successfully!", "success");
            }
            catch (Exception ex)
            {
                ShowAlert("Cannot delete: " + ex.Message, "danger");
            }
            LoadShowtimes();
        }

        private void ShowAlert(string msg, string type)
        {
            lblMessage.Text = "<i class='bi bi-check-circle-fill me-2'></i>" + msg;
            lblMessage.CssClass = "alert alert-" + type + " d-block mb-3";
            lblMessage.Visible = true;
        }
    }
}