using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class Movies : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadMovieGrid();
        }

        private void LoadMovieGrid()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var adapter = new OracleDataAdapter(
                    "SELECT m.MOVIE_ID, m.MOVIE_TITLE, m.MOVIE_GENRE, m.MOVIE_DURATION, m.MOVIE_LANGUAGE, m.MOVIE_RELEASE_DATE, " +
                    "NVL((SELECT LISTAGG(tname, ', ') WITHIN GROUP (ORDER BY tname) FROM " +
                    "(SELECT DISTINCT t.THEATER_NAME AS tname FROM THEATER_MOVIE_CUSTOMER tmc " +
                    "JOIN THEATER t ON tmc.THEATER_ID = t.THEATER_ID " +
                    "WHERE tmc.MOVIE_ID = m.MOVIE_ID)), 'Not Assigned') AS THEATERS " +
                    "FROM MOVIE m ORDER BY m.MOVIE_ID", conn);
                var table = new DataTable();
                adapter.Fill(table);
                gvMovies.DataSource = table;
                gvMovies.DataBind();
            }
        }

        private void LoadTheaterCheckboxes(int movieId = 0)
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var da = new OracleDataAdapter(
                    "SELECT THEATER_ID, THEATER_NAME FROM THEATER ORDER BY THEATER_NAME", conn);
                var dt = new DataTable();
                da.Fill(dt);
                cblTheaters.DataSource = dt;
                cblTheaters.DataTextField = "THEATER_NAME";
                cblTheaters.DataValueField = "THEATER_ID";
                cblTheaters.DataBind();

                if (movieId > 0)
                {
                    // Check which theaters are already assigned to this movie
                    var da2 = new OracleDataAdapter(
                        "SELECT DISTINCT THEATER_ID FROM THEATER_MOVIE_CUSTOMER WHERE MOVIE_ID=:mid", conn);
                    da2.SelectCommand.Parameters.Add(":mid", OracleDbType.Int32).Value = movieId;
                    var assignedTable = new DataTable();
                    da2.Fill(assignedTable);
                    var assignedIds = new System.Collections.Generic.HashSet<string>();
                    foreach (DataRow row in assignedTable.Rows)
                        assignedIds.Add(row["THEATER_ID"].ToString());

                    foreach (ListItem item in cblTheaters.Items)
                        item.Selected = assignedIds.Contains(item.Value);
                }
            }
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        {
            hfMovieId.Value = "0";
            txtTitle.Text = "";
            txtDuration.Text = "";
            txtReleaseDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            ddlGenre.SelectedIndex = 0;
            ddlLanguage.SelectedIndex = 0;
            lblModalTitle.Text = "Add Movie";
            LoadTheaterCheckboxes();
            ShowModal = true;
            LoadMovieGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtDuration.Text) ||
                ddlGenre.SelectedValue == "" || ddlLanguage.SelectedValue == "")
            {
                ShowAlert("Please fill in all required fields.", "warning");
                LoadTheaterCheckboxes(hfMovieId.Value == "0" ? 0 : int.Parse(hfMovieId.Value));
                ShowModal = true; LoadMovieGrid(); return;
            }

            // Duration must be a positive number
            if (!int.TryParse(txtDuration.Text.Trim(), out int duration) || duration <= 0)
            {
                ShowAlert("Duration must be a positive number (e.g. 120 for 2 hours).", "warning");
                LoadTheaterCheckboxes(hfMovieId.Value == "0" ? 0 : int.Parse(hfMovieId.Value));
                ShowModal = true; LoadMovieGrid(); return;
            }

            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    int movieId;
                    if (hfMovieId.Value == "0") // adding new
                    {
                        var cmd = new OracleCommand(
                            "INSERT INTO MOVIE(MOVIE_ID, MOVIE_TITLE, MOVIE_GENRE, MOVIE_DURATION, MOVIE_LANGUAGE, MOVIE_RELEASE_DATE) " +
                            "VALUES((SELECT NVL(MAX(MOVIE_ID),0)+1 FROM MOVIE), :title, :genre, :duration, :language, :rdate) " +
                            "RETURNING MOVIE_ID INTO :newMovieId", conn);
                        cmd.Parameters.Add(":title", OracleDbType.Varchar2).Value = txtTitle.Text.Trim();
                        cmd.Parameters.Add(":genre", OracleDbType.Varchar2).Value = ddlGenre.SelectedValue;
                        cmd.Parameters.Add(":duration", OracleDbType.Int32).Value = duration;
                        cmd.Parameters.Add(":language", OracleDbType.Varchar2).Value = ddlLanguage.SelectedValue;
                        cmd.Parameters.Add(":rdate", OracleDbType.Date).Value = DateTime.Parse(txtReleaseDate.Text);
                        cmd.Parameters.Add(":newMovieId", OracleDbType.Int32).Direction = System.Data.ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        movieId = Convert.ToInt32(cmd.Parameters[":newMovieId"].Value);
                        ShowAlert("Movie added successfully!", "success");
                    }
                    else // editing existing
                    {
                        movieId = int.Parse(hfMovieId.Value);
                        var cmd = new OracleCommand(
                            "UPDATE MOVIE SET MOVIE_TITLE=:title, MOVIE_GENRE=:genre, MOVIE_DURATION=:duration, MOVIE_LANGUAGE=:language, MOVIE_RELEASE_DATE=:rdate " +
                            "WHERE MOVIE_ID=:id", conn);
                        cmd.Parameters.Add(":title", OracleDbType.Varchar2).Value = txtTitle.Text.Trim();
                        cmd.Parameters.Add(":genre", OracleDbType.Varchar2).Value = ddlGenre.SelectedValue;
                        cmd.Parameters.Add(":duration", OracleDbType.Int32).Value = duration;
                        cmd.Parameters.Add(":language", OracleDbType.Varchar2).Value = ddlLanguage.SelectedValue;
                        cmd.Parameters.Add(":rdate", OracleDbType.Date).Value = DateTime.Parse(txtReleaseDate.Text);
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = movieId;
                        cmd.ExecuteNonQuery();
                        ShowAlert("Movie updated successfully!", "success");
                    }

                    // Save theater assignments for selected theaters
                    var cmdMinCust = new OracleCommand("SELECT MIN(CUSTOMER_ID) FROM CUSTOMER", conn);
                    var minCustObj = cmdMinCust.ExecuteScalar();
                    if (minCustObj != DBNull.Value && minCustObj != null)
                    {
                        int minCustomerId = Convert.ToInt32(minCustObj);
                        foreach (ListItem item in cblTheaters.Items)
                        {
                            if (item.Selected)
                            {
                                int theaterId = int.Parse(item.Value);

                                // Ensure CUSTOMER_MOVIE entry exists
                                var cmdCM = new OracleCommand(
                                    @"MERGE INTO CUSTOMER_MOVIE cm USING (SELECT :mid AS MOVIE_ID, :cid AS CUSTOMER_ID FROM DUAL) src
                                      ON (cm.MOVIE_ID = src.MOVIE_ID AND cm.CUSTOMER_ID = src.CUSTOMER_ID)
                                      WHEN NOT MATCHED THEN INSERT (MOVIE_ID, CUSTOMER_ID) VALUES (src.MOVIE_ID, src.CUSTOMER_ID)", conn);
                                cmdCM.Parameters.Add(":mid", OracleDbType.Int32).Value = movieId;
                                cmdCM.Parameters.Add(":cid", OracleDbType.Int32).Value = minCustomerId;
                                cmdCM.ExecuteNonQuery();

                                // Ensure THEATER_MOVIE_CUSTOMER entry exists
                                var cmdTMC = new OracleCommand(
                                    @"MERGE INTO THEATER_MOVIE_CUSTOMER tmc USING (SELECT :tid AS THEATER_ID, :mid AS MOVIE_ID, :cid AS CUSTOMER_ID FROM DUAL) src
                                      ON (tmc.THEATER_ID = src.THEATER_ID AND tmc.MOVIE_ID = src.MOVIE_ID AND tmc.CUSTOMER_ID = src.CUSTOMER_ID)
                                      WHEN NOT MATCHED THEN INSERT (THEATER_ID, MOVIE_ID, CUSTOMER_ID) VALUES (src.THEATER_ID, src.MOVIE_ID, src.CUSTOMER_ID)", conn);
                                cmdTMC.Parameters.Add(":tid", OracleDbType.Int32).Value = theaterId;
                                cmdTMC.Parameters.Add(":mid", OracleDbType.Int32).Value = movieId;
                                cmdTMC.Parameters.Add(":cid", OracleDbType.Int32).Value = minCustomerId;
                                cmdTMC.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); LoadTheaterCheckboxes(hfMovieId.Value == "0" ? 0 : int.Parse(hfMovieId.Value)); ShowModal = true; }
            LoadMovieGrid();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ShowModal = false;
            LoadMovieGrid();
        }

        protected void gvMovies_RowEditing(object sender, GridViewEditEventArgs e)
        {
            int id = int.Parse(gvMovies.DataKeys[e.NewEditIndex].Value.ToString());
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var cmd = new OracleCommand("SELECT * FROM MOVIE WHERE MOVIE_ID=:id", conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hfMovieId.Value = id.ToString();
                        txtTitle.Text = reader["MOVIE_TITLE"].ToString();
                        txtDuration.Text = reader["MOVIE_DURATION"].ToString();
                        txtReleaseDate.Text = Convert.ToDateTime(reader["MOVIE_RELEASE_DATE"]).ToString("yyyy-MM-dd");
                        ddlGenre.SelectedValue = reader["MOVIE_GENRE"].ToString();
                        ddlLanguage.SelectedValue = reader["MOVIE_LANGUAGE"].ToString();
                        lblModalTitle.Text = "Edit Movie";
                        ShowModal = true;
                    }
                }
            }
            LoadTheaterCheckboxes(id);
            gvMovies.EditIndex = -1;
            LoadMovieGrid();
        }

        protected void gvMovies_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvMovies.EditIndex = -1;
            LoadMovieGrid();
        }

        protected void gvMovies_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvMovies.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Collect ALL ticket IDs linked to this movie
                            var cmdGetAllTickets = new OracleCommand(
                                @"SELECT DISTINCT TICKET_ID FROM ""TktShowHallMovCust"" WHERE MOVIE_ID=:id", conn);
                            cmdGetAllTickets.Transaction = transaction;
                            cmdGetAllTickets.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            var allTicketIds = new System.Collections.Generic.List<int>();
                            using (var reader = cmdGetAllTickets.ExecuteReader())
                            {
                                while (reader.Read())
                                    allTicketIds.Add(Convert.ToInt32(reader["TICKET_ID"]));
                            }

                            // Delete ALL TktShowHallMovCust rows for this movie
                            var cmdDeleteAllTkt = new OracleCommand(
                                @"DELETE FROM ""TktShowHallMovCust"" WHERE MOVIE_ID=:id", conn);
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

                            // Collect all SHOW_IDs linked to this movie
                            var cmdGetShows = new OracleCommand(
                                @"SELECT DISTINCT SHOW_ID FROM ""ShowHallMovCust"" WHERE MOVIE_ID=:id", conn);
                            cmdGetShows.Transaction = transaction;
                            cmdGetShows.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            var showIds = new System.Collections.Generic.List<int>();
                            using (var reader = cmdGetShows.ExecuteReader())
                            {
                                while (reader.Read())
                                    showIds.Add(Convert.ToInt32(reader["SHOW_ID"]));
                            }

                            // Delete from ShowHallMovCust for this movie
                            var cmdShowJunction = new OracleCommand(
                                @"DELETE FROM ""ShowHallMovCust"" WHERE MOVIE_ID=:id", conn);
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
                                "DELETE FROM HALL_THEATER_MOVIE_CUSTOMER WHERE MOVIE_ID=:id", conn);
                            cmdHallThr.Transaction = transaction;
                            cmdHallThr.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            cmdHallThr.ExecuteNonQuery();

                            // Delete from THEATER_MOVIE_CUSTOMER
                            var cmdThrMov = new OracleCommand(
                                "DELETE FROM THEATER_MOVIE_CUSTOMER WHERE MOVIE_ID=:id", conn);
                            cmdThrMov.Transaction = transaction;
                            cmdThrMov.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            cmdThrMov.ExecuteNonQuery();

                            // Delete from CUSTOMER_MOVIE
                            var cmdCustMov = new OracleCommand(
                                "DELETE FROM CUSTOMER_MOVIE WHERE MOVIE_ID=:id", conn);
                            cmdCustMov.Transaction = transaction;
                            cmdCustMov.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            cmdCustMov.ExecuteNonQuery();

                            // Finally delete the movie
                            var cmd = new OracleCommand("DELETE FROM MOVIE WHERE MOVIE_ID=:id", conn);
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
                    ShowAlert("Movie deleted successfully!", "success");
                }
            }
            catch (Exception ex) { ShowAlert("Cannot delete: " + ex.Message, "danger"); }
            LoadMovieGrid();
        }

        private void ShowAlert(string message, string bootstrapType)
        {
            lblMessage.Text = "<i class='bi bi-info-circle-fill me-2'></i>" + message;
            lblMessage.CssClass = "alert alert-" + bootstrapType + " d-block mb-3";
            lblMessage.Visible = true;
        }
    }
}