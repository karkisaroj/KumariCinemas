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

                    // Validate that the selected hall belongs to the selected theater
                    var cmdValidate = new OracleCommand(
                        "SELECT COUNT(*) FROM HALL_THEATER_MOVIE_CUSTOMER WHERE HALL_ID=:hid AND THEATER_ID=:tid", conn);
                    cmdValidate.Parameters.Add(":hid", OracleDbType.Int32).Value = int.Parse(ddlHall.SelectedValue);
                    cmdValidate.Parameters.Add(":tid", OracleDbType.Int32).Value = int.Parse(ddlTheater.SelectedValue);
                    int hallTheaterCount = Convert.ToInt32(cmdValidate.ExecuteScalar());

                    if (hallTheaterCount == 0)
                    {
                        ShowAlert("The selected hall is not available in the selected theater. Please choose a valid hall-theater combination.", "warning");
                        ShowModal = true;
                        LoadDropdowns();
                        LoadShowtimes();
                        return;
                    }

                    var showDate = DateTime.Parse(txtDate.Text);
                    var showTime = DateTime.Parse(txtDate.Text + " " + txtTime.Text);
                    var showEnd = DateTime.Parse(txtDate.Text + " " + txtEndTime.Text);

                    int hallId = int.Parse(ddlHall.SelectedValue);
                    int theaterId = int.Parse(ddlTheater.SelectedValue);
                    int movieId = int.Parse(ddlMovie.SelectedValue);

                    // Get the first available customer ID from database
                    var cmdGetCustId = new OracleCommand("SELECT MIN(CUSTOMER_ID) FROM CUSTOMER", conn);
                    var systemCustomerId = Convert.ToInt32(cmdGetCustId.ExecuteScalar());

                    // Ensure normalization chain entries exist
                    EnsureJunctionChain(conn, systemCustomerId, movieId, theaterId, hallId);

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
                        
                        // Insert into ShowHallMovCust junction table
                        var cmdJunction = new OracleCommand(
                            "INSERT INTO \"ShowHallMovCust\" " +
                            "(SHOW_ID, HALL_ID, THEATER_ID, MOVIE_ID, CUSTOMER_ID) " +
                            "VALUES (:showId, :hallId, :theaterId, :movieId, :custId)", conn);
                        cmdJunction.Parameters.Add(":showId", OracleDbType.Int32).Value = newShowId;
                        cmdJunction.Parameters.Add(":hallId", OracleDbType.Int32).Value = hallId;
                        cmdJunction.Parameters.Add(":theaterId", OracleDbType.Int32).Value = theaterId;
                        cmdJunction.Parameters.Add(":movieId", OracleDbType.Int32).Value = movieId;
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
                        
                        // Update junction table
                        var cmdJunction = new OracleCommand(
                            "UPDATE \"ShowHallMovCust\" " +
                            "SET MOVIE_ID=:movieId, THEATER_ID=:theaterId, HALL_ID=:hallId " +
                            "WHERE SHOW_ID=:id AND CUSTOMER_ID = :custId", conn);
                        cmdJunction.Parameters.Add(":movieId", OracleDbType.Int32).Value = movieId;
                        cmdJunction.Parameters.Add(":theaterId", OracleDbType.Int32).Value = theaterId;
                        cmdJunction.Parameters.Add(":hallId", OracleDbType.Int32).Value = hallId;
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
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            var cmdGetTickets = new OracleCommand(
                                @"SELECT TICKET_ID FROM ""TktShowHallMovCust"" WHERE SHOW_ID=:id", conn);
                            cmdGetTickets.Transaction = transaction;
                            cmdGetTickets.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            var ticketIds = new System.Collections.Generic.List<int>();
                            using (var reader = cmdGetTickets.ExecuteReader())
                            {
                                while (reader.Read())
                                    ticketIds.Add(Convert.ToInt32(reader["TICKET_ID"]));
                            }

                            var cmdTktJunction = new OracleCommand(
                                @"DELETE FROM ""TktShowHallMovCust"" WHERE SHOW_ID=:id", conn);
                            cmdTktJunction.Transaction = transaction;
                            cmdTktJunction.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            cmdTktJunction.ExecuteNonQuery();

                            foreach (var ticketId in ticketIds)
                            {
                                var cmdDeleteTicket = new OracleCommand(
                                    "DELETE FROM TICKET WHERE TICKET_ID=:tid", conn);
                                cmdDeleteTicket.Transaction = transaction;
                                cmdDeleteTicket.Parameters.Add(":tid", OracleDbType.Int32).Value = ticketId;
                                cmdDeleteTicket.ExecuteNonQuery();
                            }

                            var cmdJunction = new OracleCommand(
                                @"DELETE FROM ""ShowHallMovCust"" WHERE SHOW_ID=:id", conn);
                            cmdJunction.Transaction = transaction;
                            cmdJunction.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            cmdJunction.ExecuteNonQuery();

                            var cmd = new OracleCommand("DELETE FROM SHOWTIME WHERE SHOW_ID=:id", conn);
                            cmd.Transaction = transaction;
                            cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            cmd.ExecuteNonQuery();

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                ShowAlert("Showtime deleted successfully!", "success");
            }
            catch (Exception ex)
            {
                ShowAlert("Cannot delete: " + ex.Message, "danger");
            }
            LoadShowtimes();
        }

        private void EnsureJunctionChain(OracleConnection conn, int customerId, int movieId, int theaterId, int hallId)
        {
            // Ensure CUSTOMER_MOVIE row exists
            var cmd1 = new OracleCommand(
                @"MERGE INTO CUSTOMER_MOVIE cm USING (SELECT :mid AS MOVIE_ID, :cid AS CUSTOMER_ID FROM DUAL) src
                  ON (cm.MOVIE_ID = src.MOVIE_ID AND cm.CUSTOMER_ID = src.CUSTOMER_ID)
                  WHEN NOT MATCHED THEN INSERT (MOVIE_ID, CUSTOMER_ID) VALUES (src.MOVIE_ID, src.CUSTOMER_ID)", conn);
            cmd1.Parameters.Add(":mid", OracleDbType.Int32).Value = movieId;
            cmd1.Parameters.Add(":cid", OracleDbType.Int32).Value = customerId;
            cmd1.ExecuteNonQuery();

            // Ensure THEATER_MOVIE_CUSTOMER row exists
            var cmd2 = new OracleCommand(
                @"MERGE INTO THEATER_MOVIE_CUSTOMER tmc USING (SELECT :tid AS THEATER_ID, :mid AS MOVIE_ID, :cid AS CUSTOMER_ID FROM DUAL) src
                  ON (tmc.THEATER_ID = src.THEATER_ID AND tmc.MOVIE_ID = src.MOVIE_ID AND tmc.CUSTOMER_ID = src.CUSTOMER_ID)
                  WHEN NOT MATCHED THEN INSERT (THEATER_ID, MOVIE_ID, CUSTOMER_ID) VALUES (src.THEATER_ID, src.MOVIE_ID, src.CUSTOMER_ID)", conn);
            cmd2.Parameters.Add(":tid", OracleDbType.Int32).Value = theaterId;
            cmd2.Parameters.Add(":mid", OracleDbType.Int32).Value = movieId;
            cmd2.Parameters.Add(":cid", OracleDbType.Int32).Value = customerId;
            cmd2.ExecuteNonQuery();

            // Ensure HALL_THEATER_MOVIE_CUSTOMER row exists
            var cmd3 = new OracleCommand(
                @"MERGE INTO HALL_THEATER_MOVIE_CUSTOMER htmc USING (SELECT :hid AS HALL_ID, :tid AS THEATER_ID, :mid AS MOVIE_ID, :cid AS CUSTOMER_ID FROM DUAL) src
                  ON (htmc.HALL_ID = src.HALL_ID AND htmc.THEATER_ID = src.THEATER_ID AND htmc.MOVIE_ID = src.MOVIE_ID AND htmc.CUSTOMER_ID = src.CUSTOMER_ID)
                  WHEN NOT MATCHED THEN INSERT (HALL_ID, THEATER_ID, MOVIE_ID, CUSTOMER_ID) VALUES (src.HALL_ID, src.THEATER_ID, src.MOVIE_ID, src.CUSTOMER_ID)", conn);
            cmd3.Parameters.Add(":hid", OracleDbType.Int32).Value = hallId;
            cmd3.Parameters.Add(":tid", OracleDbType.Int32).Value = theaterId;
            cmd3.Parameters.Add(":mid", OracleDbType.Int32).Value = movieId;
            cmd3.Parameters.Add(":cid", OracleDbType.Int32).Value = customerId;
            cmd3.ExecuteNonQuery();
        }

        private void ShowAlert(string msg, string type)
        {
            lblMessage.Text = "<i class='bi bi-check-circle-fill me-2'></i>" + msg;
            lblMessage.CssClass = "alert alert-" + type + " d-block mb-3";
            lblMessage.Visible = true;
        }
    }
}