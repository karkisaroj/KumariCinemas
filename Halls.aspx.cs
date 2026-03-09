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
                    "SELECT HALL_ID, HALL_NAME, HALL_CAPACITY, HALL_TYPE FROM HALL ORDER BY HALL_ID", conn);
                var table = new DataTable();
                adapter.Fill(table);
                gvHalls.DataSource = table;
                gvHalls.DataBind();
            }
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        {
            hfHallId.Value = "0";
            txtName.Text = "";
            txtCapacity.Text = "";
            ddlType.SelectedIndex = 0;
            lblModalTitle.Text = "Add Hall";
            ShowModal = true;
            LoadHallGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCapacity.Text) ||
                ddlType.SelectedValue == "")
            {
                ShowAlert("Please fill in all required fields.", "warning");
                ShowModal = true; LoadHallGrid(); return;
            }

            // Capacity must be a positive number
            if (!int.TryParse(txtCapacity.Text.Trim(), out int capacity) || capacity <= 0)
            {
                ShowAlert("Capacity must be a positive number (e.g. 150).", "warning");
                ShowModal = true; LoadHallGrid(); return;
            }

            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    if (hfHallId.Value == "0") // adding new
                    {
                        var cmd = new OracleCommand(
                            "INSERT INTO HALL(HALL_ID, HALL_NAME, HALL_CAPACITY, HALL_TYPE) " +
                            "VALUES((SELECT NVL(MAX(HALL_ID),0)+1 FROM HALL), :name, :cap, :type)", conn);
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":cap", OracleDbType.Int32).Value = capacity;
                        cmd.Parameters.Add(":type", OracleDbType.Varchar2).Value = ddlType.SelectedValue;
                        cmd.ExecuteNonQuery();
                        ShowAlert("Hall added successfully!", "success");
                    }
                    else // editing existing
                    {
                        var cmd = new OracleCommand(
                            "UPDATE HALL SET HALL_NAME=:name, HALL_CAPACITY=:cap, HALL_TYPE=:type WHERE HALL_ID=:id", conn);
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":cap", OracleDbType.Int32).Value = capacity;
                        cmd.Parameters.Add(":type", OracleDbType.Varchar2).Value = ddlType.SelectedValue;
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfHallId.Value);
                        cmd.ExecuteNonQuery();
                        ShowAlert("Hall updated successfully!", "success");
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); ShowModal = true; }
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