using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class Halls : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadHallGrid();
        }

        private void LoadHallGrid()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var adapter = new OracleDataAdapter(
                    "SELECT h.HALL_ID, h.HALL_NAME, h.HALL_CAPACITY, h.HALL_TYPE, " +
                    "NVL((SELECT t.THEATER_NAME FROM HALL_THEATER_MOVIE_CUSTOMER htmc " +
                    "JOIN THEATER t ON htmc.THEATER_ID = t.THEATER_ID " +
                    "WHERE htmc.HALL_ID = h.HALL_ID AND ROWNUM = 1), 'Not Assigned') AS THEATER_NAME " +
                    "FROM HALL h ORDER BY h.HALL_ID", conn);
                var table = new DataTable();
                adapter.Fill(table);
                gvHalls.DataSource = table;
                gvHalls.DataBind();
            }
        }

        private void LoadTheaterDropdown()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var da = new OracleDataAdapter(
                    "SELECT THEATER_ID, THEATER_NAME FROM THEATER ORDER BY THEATER_NAME", conn);
                var dt = new DataTable();
                da.Fill(dt);
                ddlTheater.DataSource = dt;
                ddlTheater.DataTextField = "THEATER_NAME";
                ddlTheater.DataValueField = "THEATER_ID";
                ddlTheater.DataBind();
                ddlTheater.Items.Insert(0, new ListItem("-- Select Theater --", "0"));
            }
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        {
            hfHallId.Value = "0";
            txtName.Text = "";
            txtCapacity.Text = "";
            ddlType.SelectedIndex = 0;
            lblModalTitle.Text = "Add Hall";
            LoadTheaterDropdown();
            ShowModal = true;
            LoadHallGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCapacity.Text) ||
                ddlType.SelectedValue == "")
            {
                ShowAlert("Please fill in all required fields.", "warning");
                LoadTheaterDropdown();
                ShowModal = true; LoadHallGrid(); return;
            }

            // Capacity must be a positive number
            if (!int.TryParse(txtCapacity.Text.Trim(), out int capacity) || capacity <= 0)
            {
                ShowAlert("Capacity must be a positive number (e.g. 150).", "warning");
                LoadTheaterDropdown();
                ShowModal = true; LoadHallGrid(); return;
            }

            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    int hallId;
                    if (hfHallId.Value == "0") // adding new
                    {
                        var cmd = new OracleCommand(
                            "INSERT INTO HALL(HALL_ID, HALL_NAME, HALL_CAPACITY, HALL_TYPE) " +
                            "VALUES((SELECT NVL(MAX(HALL_ID),0)+1 FROM HALL), :name, :cap, :type) " +
                            "RETURNING HALL_ID INTO :newHallId", conn);
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":cap", OracleDbType.Int32).Value = capacity;
                        cmd.Parameters.Add(":type", OracleDbType.Varchar2).Value = ddlType.SelectedValue;
                        cmd.Parameters.Add(":newHallId", OracleDbType.Int32).Direction = System.Data.ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        hallId = Convert.ToInt32(cmd.Parameters[":newHallId"].Value);
                        ShowAlert("Hall added successfully!", "success");
                    }
                    else // editing existing
                    {
                        hallId = int.Parse(hfHallId.Value);
                        var cmd = new OracleCommand(
                            "UPDATE HALL SET HALL_NAME=:name, HALL_CAPACITY=:cap, HALL_TYPE=:type WHERE HALL_ID=:id", conn);
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":cap", OracleDbType.Int32).Value = capacity;
                        cmd.Parameters.Add(":type", OracleDbType.Varchar2).Value = ddlType.SelectedValue;
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = hallId;
                        cmd.ExecuteNonQuery();
                        ShowAlert("Hall updated successfully!", "success");
                    }

                    // Save theater assignment if a theater was selected
                    if (ddlTheater.SelectedValue != "0")
                    {
                        int theaterId = int.Parse(ddlTheater.SelectedValue);

                        // Use the first available movie and customer as system placeholders
                        var cmdMinMovie = new OracleCommand("SELECT MIN(MOVIE_ID) FROM MOVIE", conn);
                        var minMovieObj = cmdMinMovie.ExecuteScalar();
                        var cmdMinCust = new OracleCommand("SELECT MIN(CUSTOMER_ID) FROM CUSTOMER", conn);
                        var minCustObj = cmdMinCust.ExecuteScalar();

                        if (minMovieObj != DBNull.Value && minCustObj != DBNull.Value)
                        {
                            int minMovieId = Convert.ToInt32(minMovieObj);
                            int minCustId = Convert.ToInt32(minCustObj);

                            // Ensure CUSTOMER_MOVIE entry exists
                            var cmdCM = new OracleCommand(
                                @"MERGE INTO CUSTOMER_MOVIE cm USING (SELECT :mid AS MOVIE_ID, :cid AS CUSTOMER_ID FROM DUAL) src
                                  ON (cm.MOVIE_ID = src.MOVIE_ID AND cm.CUSTOMER_ID = src.CUSTOMER_ID)
                                  WHEN NOT MATCHED THEN INSERT (MOVIE_ID, CUSTOMER_ID) VALUES (src.MOVIE_ID, src.CUSTOMER_ID)", conn);
                            cmdCM.Parameters.Add(":mid", OracleDbType.Int32).Value = minMovieId;
                            cmdCM.Parameters.Add(":cid", OracleDbType.Int32).Value = minCustId;
                            cmdCM.ExecuteNonQuery();

                            // Ensure THEATER_MOVIE_CUSTOMER entry exists
                            var cmdTMC = new OracleCommand(
                                @"MERGE INTO THEATER_MOVIE_CUSTOMER tmc USING (SELECT :tid AS THEATER_ID, :mid AS MOVIE_ID, :cid AS CUSTOMER_ID FROM DUAL) src
                                  ON (tmc.THEATER_ID = src.THEATER_ID AND tmc.MOVIE_ID = src.MOVIE_ID AND tmc.CUSTOMER_ID = src.CUSTOMER_ID)
                                  WHEN NOT MATCHED THEN INSERT (THEATER_ID, MOVIE_ID, CUSTOMER_ID) VALUES (src.THEATER_ID, src.MOVIE_ID, src.CUSTOMER_ID)", conn);
                            cmdTMC.Parameters.Add(":tid", OracleDbType.Int32).Value = theaterId;
                            cmdTMC.Parameters.Add(":mid", OracleDbType.Int32).Value = minMovieId;
                            cmdTMC.Parameters.Add(":cid", OracleDbType.Int32).Value = minCustId;
                            cmdTMC.ExecuteNonQuery();

                            // Ensure HALL_THEATER_MOVIE_CUSTOMER entry exists (hall-theater relationship)
                            var cmdHtmc = new OracleCommand(
                                @"MERGE INTO HALL_THEATER_MOVIE_CUSTOMER htmc USING (SELECT :hid AS HALL_ID, :tid AS THEATER_ID, :mid AS MOVIE_ID, :cid AS CUSTOMER_ID FROM DUAL) src
                                  ON (htmc.HALL_ID = src.HALL_ID AND htmc.THEATER_ID = src.THEATER_ID AND htmc.MOVIE_ID = src.MOVIE_ID AND htmc.CUSTOMER_ID = src.CUSTOMER_ID)
                                  WHEN NOT MATCHED THEN INSERT (HALL_ID, THEATER_ID, MOVIE_ID, CUSTOMER_ID) VALUES (src.HALL_ID, src.THEATER_ID, src.MOVIE_ID, src.CUSTOMER_ID)", conn);
                            cmdHtmc.Parameters.Add(":hid", OracleDbType.Int32).Value = hallId;
                            cmdHtmc.Parameters.Add(":tid", OracleDbType.Int32).Value = theaterId;
                            cmdHtmc.Parameters.Add(":mid", OracleDbType.Int32).Value = minMovieId;
                            cmdHtmc.Parameters.Add(":cid", OracleDbType.Int32).Value = minCustId;
                            cmdHtmc.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); LoadTheaterDropdown(); ShowModal = true; }
            LoadHallGrid();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ShowModal = false;
            LoadHallGrid();
        }

        protected void gvHalls_RowEditing(object sender, GridViewEditEventArgs e)
        {
            int id = int.Parse(gvHalls.DataKeys[e.NewEditIndex].Value.ToString());
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var cmd = new OracleCommand("SELECT * FROM HALL WHERE HALL_ID=:id", conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hfHallId.Value = id.ToString();
                        txtName.Text = reader["HALL_NAME"].ToString();
                        txtCapacity.Text = reader["HALL_CAPACITY"].ToString();
                        ddlType.SelectedValue = reader["HALL_TYPE"].ToString();
                        lblModalTitle.Text = "Edit Hall";
                        ShowModal = true;
                    }
                }

                // Load theater dropdown and set current theater assignment
                LoadTheaterDropdown();
                var cmdGetTheater = new OracleCommand(
                    "SELECT THEATER_ID FROM HALL_THEATER_MOVIE_CUSTOMER WHERE HALL_ID=:id AND ROWNUM=1", conn);
                cmdGetTheater.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                var theaterIdObj = cmdGetTheater.ExecuteScalar();
                if (theaterIdObj != null && theaterIdObj != DBNull.Value)
                    ddlTheater.SelectedValue = theaterIdObj.ToString();
            }
            gvHalls.EditIndex = -1;
            LoadHallGrid();
        }

        protected void gvHalls_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvHalls.EditIndex = -1;
            LoadHallGrid();
        }

        protected void gvHalls_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvHalls.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Collect ALL ticket IDs linked to this hall
                            var cmdGetAllTickets = new OracleCommand(
                                @"SELECT DISTINCT TICKET_ID FROM ""TktShowHallMovCust"" WHERE HALL_ID=:id", conn);
                            cmdGetAllTickets.Transaction = transaction;
                            cmdGetAllTickets.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            var allTicketIds = new System.Collections.Generic.List<int>();
                            using (var reader = cmdGetAllTickets.ExecuteReader())
                            {
                                while (reader.Read())
                                    allTicketIds.Add(Convert.ToInt32(reader["TICKET_ID"]));
                            }

                            // Delete ALL TktShowHallMovCust rows for this hall
                            var cmdDeleteAllTkt = new OracleCommand(
                                @"DELETE FROM ""TktShowHallMovCust"" WHERE HALL_ID=:id", conn);
                            cmdDeleteAllTkt.Transaction = transaction;
                            cmdDeleteAllTkt.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            cmdDeleteAllTkt.ExecuteNonQuery();

                            // Delete the tickets
                            foreach (var ticketId in allTicketIds)
                            {
                                var cmdDeleteTicket = new OracleCommand(
                                    "DELETE FROM TICKET WHERE TICKET_ID=:tid", conn);
                                cmdDeleteTicket.Transaction = transaction;
                                cmdDeleteTicket.Parameters.Add(":tid", OracleDbType.Int32).Value = ticketId;
                                cmdDeleteTicket.ExecuteNonQuery();
                            }

                            // Collect all SHOW_IDs linked to this hall
                            var cmdGetShows = new OracleCommand(
                                @"SELECT DISTINCT SHOW_ID FROM ""ShowHallMovCust"" WHERE HALL_ID=:id", conn);
                            cmdGetShows.Transaction = transaction;
                            cmdGetShows.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            var showIds = new System.Collections.Generic.List<int>();
                            using (var reader = cmdGetShows.ExecuteReader())
                            {
                                while (reader.Read())
                                    showIds.Add(Convert.ToInt32(reader["SHOW_ID"]));
                            }

                            // Delete from ShowHallMovCust for this hall
                            var cmdShowJunction = new OracleCommand(
                                @"DELETE FROM ""ShowHallMovCust"" WHERE HALL_ID=:id", conn);
                            cmdShowJunction.Transaction = transaction;
                            cmdShowJunction.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            cmdShowJunction.ExecuteNonQuery();

                            // Delete orphaned showtimes
                            foreach (var showId in showIds)
                            {
                                var cmdCheckShow = new OracleCommand(
                                    @"SELECT COUNT(*) FROM ""ShowHallMovCust"" WHERE SHOW_ID=:sid", conn);
                                cmdCheckShow.Transaction = transaction;
                                cmdCheckShow.Parameters.Add(":sid", OracleDbType.Int32).Value = showId;
                                int remaining = Convert.ToInt32(cmdCheckShow.ExecuteScalar());
                                if (remaining == 0)
                                {
                                    var cmdDeleteShow = new OracleCommand(
                                        "DELETE FROM SHOWTIME WHERE SHOW_ID=:sid", conn);
                                    cmdDeleteShow.Transaction = transaction;
                                    cmdDeleteShow.Parameters.Add(":sid", OracleDbType.Int32).Value = showId;
                                    cmdDeleteShow.ExecuteNonQuery();
                                }
                            }

                            // Delete from HALL_THEATER_MOVIE_CUSTOMER
                            var cmdHallThr = new OracleCommand(
                                "DELETE FROM HALL_THEATER_MOVIE_CUSTOMER WHERE HALL_ID=:id", conn);
                            cmdHallThr.Transaction = transaction;
                            cmdHallThr.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            cmdHallThr.ExecuteNonQuery();

                            // Finally delete the hall
                            var cmd = new OracleCommand("DELETE FROM HALL WHERE HALL_ID=:id", conn);
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
                    ShowAlert("Hall deleted successfully!", "success");
                }
            }
            catch (Exception ex) { ShowAlert("Cannot delete: " + ex.Message, "danger"); }
            LoadHallGrid();
        }

        private void ShowAlert(string message, string bootstrapType)
        {
            lblMessage.Text = "<i class='bi bi-info-circle-fill me-2'></i>" + message;
            lblMessage.CssClass = "alert alert-" + bootstrapType + " d-block mb-3";
            lblMessage.Visible = true;
        }
    }
}