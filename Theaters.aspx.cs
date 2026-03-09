using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class Theaters : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadTheaterGrid();
        }

        private void LoadTheaterGrid()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var adapter = new OracleDataAdapter(
                    "SELECT THEATER_ID, THEATER_NAME, THEATER_CITY, THEATER_LOCATION FROM THEATER ORDER BY THEATER_ID", conn);
                var table = new DataTable();
                adapter.Fill(table);
                gvTheaters.DataSource = table;
                gvTheaters.DataBind();
            }
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        {
            hfTheaterId.Value = "0";
            txtName.Text = "";
            txtCity.Text = "";
            txtLocation.Text = "";
            lblModalTitle.Text = "Add Theater";
            ShowModal = true;
            LoadTheaterGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCity.Text))
            {
                ShowAlert("Please fill in all required fields.", "warning");
                ShowModal = true; LoadTheaterGrid(); return;
            }

            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    if (hfTheaterId.Value == "0") // adding new
                    {
                        var cmd = new OracleCommand(
                            "INSERT INTO THEATER(THEATER_ID, THEATER_NAME, THEATER_CITY, THEATER_LOCATION) " +
                            "VALUES((SELECT NVL(MAX(THEATER_ID),0)+1 FROM THEATER), :name, :city, :loc)", conn);
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":city", OracleDbType.Varchar2).Value = txtCity.Text.Trim();
                        cmd.Parameters.Add(":loc", OracleDbType.Varchar2).Value = txtLocation.Text.Trim();
                        cmd.ExecuteNonQuery();
                        ShowAlert("Theater added successfully!", "success");
                    }
                    else // editing existing
                    {
                        var cmd = new OracleCommand(
                            "UPDATE THEATER SET THEATER_NAME=:name, THEATER_CITY=:city, THEATER_LOCATION=:loc WHERE THEATER_ID=:id", conn);
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":city", OracleDbType.Varchar2).Value = txtCity.Text.Trim();
                        cmd.Parameters.Add(":loc", OracleDbType.Varchar2).Value = txtLocation.Text.Trim();
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfTheaterId.Value);
                        cmd.ExecuteNonQuery();
                        ShowAlert("Theater updated successfully!", "success");
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); ShowModal = true; }
            LoadTheaterGrid();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ShowModal = false;
            LoadTheaterGrid();
        }

        protected void gvTheaters_RowEditing(object sender, GridViewEditEventArgs e)
        {
            int id = int.Parse(gvTheaters.DataKeys[e.NewEditIndex].Value.ToString());
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var cmd = new OracleCommand("SELECT * FROM THEATER WHERE THEATER_ID=:id", conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hfTheaterId.Value = id.ToString();
                        txtName.Text = reader["THEATER_NAME"].ToString();
                        txtCity.Text = reader["THEATER_CITY"].ToString();
                        txtLocation.Text = reader["THEATER_LOCATION"].ToString();
                        lblModalTitle.Text = "Edit Theater";
                        ShowModal = true;
                    }
                }
            }
            gvTheaters.EditIndex = -1;
            LoadTheaterGrid();
        }

        protected void gvTheaters_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvTheaters.EditIndex = -1;
            LoadTheaterGrid();
        }

        protected void gvTheaters_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvTheaters.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Collect ALL ticket IDs linked to this theater from TktShowHallMovCust
                            var cmdGetAllTickets = new OracleCommand(
                                @"SELECT DISTINCT TICKET_ID FROM ""TktShowHallMovCust"" WHERE THEATER_ID=:id", conn);
                            cmdGetAllTickets.Transaction = transaction;
                            cmdGetAllTickets.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            var allTicketIds = new System.Collections.Generic.List<int>();
                            using (var reader = cmdGetAllTickets.ExecuteReader())
                            {
                                while (reader.Read())
                                    allTicketIds.Add(Convert.ToInt32(reader["TICKET_ID"]));
                            }

                            // Delete ALL TktShowHallMovCust rows for this theater
                            var cmdDeleteAllTkt = new OracleCommand(
                                @"DELETE FROM ""TktShowHallMovCust"" WHERE THEATER_ID=:id", conn);
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

                            // Collect all SHOW_IDs linked to this theater
                            var cmdGetShows = new OracleCommand(
                                @"SELECT DISTINCT SHOW_ID FROM ""ShowHallMovCust"" WHERE THEATER_ID=:id", conn);
                            cmdGetShows.Transaction = transaction;
                            cmdGetShows.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            var showIds = new System.Collections.Generic.List<int>();
                            using (var reader = cmdGetShows.ExecuteReader())
                            {
                                while (reader.Read())
                                    showIds.Add(Convert.ToInt32(reader["SHOW_ID"]));
                            }

                            // Delete from ShowHallMovCust for this theater
                            var cmdShowJunction = new OracleCommand(
                                @"DELETE FROM ""ShowHallMovCust"" WHERE THEATER_ID=:id", conn);
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

                            // Delete from HALL_THEATER_MOVIE_CUSTOMER (FK_HALLTHRMOVCUST_THEATER)
                            var cmdHallThrMovCust = new OracleCommand(
                                "DELETE FROM HALL_THEATER_MOVIE_CUSTOMER WHERE THEATER_ID=:id", conn);
                            cmdHallThrMovCust.Transaction = transaction;
                            cmdHallThrMovCust.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            cmdHallThrMovCust.ExecuteNonQuery();

                            // Delete from THEATER_MOVIE_CUSTOMER
                            var cmdThrMovCust = new OracleCommand(
                                "DELETE FROM THEATER_MOVIE_CUSTOMER WHERE THEATER_ID=:id", conn);
                            cmdThrMovCust.Transaction = transaction;
                            cmdThrMovCust.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            cmdThrMovCust.ExecuteNonQuery();

                            // Finally delete the theater
                            var cmd = new OracleCommand("DELETE FROM THEATER WHERE THEATER_ID=:id", conn);
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
                    ShowAlert("Theater deleted successfully!", "success");
                }
            }
            catch (Exception ex) { ShowAlert("Cannot delete: " + ex.Message, "danger"); }
            LoadTheaterGrid();
        }

        private void ShowAlert(string message, string bootstrapType)
        {
            lblMessage.Text = "<i class='bi bi-info-circle-fill me-2'></i>" + message;
            lblMessage.CssClass = "alert alert-" + bootstrapType + " d-block mb-3";
            lblMessage.Visible = true;
        }
    }
}