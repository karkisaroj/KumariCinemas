using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class Tickets : System.Web.UI.Page
    {
        // Controls whether the Book Ticket modal stays open after postback
        public bool ShowModal = false;

        // Database connection string from Web.config
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTicketGrid();
                LoadFormDropdowns();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Auto-cancel any 'Booked' tickets where showtime is less than 1 hour away
            AutoCancelExpiredTickets();
        }


        private void LoadTicketGrid()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT t.TICKET_ID,
                           NVL(c.CUSTOMER_NAME, 'N/A')  AS CUSTOMER_NAME,
                           NVL(m.MOVIE_TITLE,   'N/A')  AS MOVIE_TITLE,
                           NVL(th.THEATER_NAME, 'N/A')  AS THEATER_NAME,
                           NVL(h.HALL_NAME,     'N/A')  AS HALL_NAME,
                           t.SEAT_NO, t.TICKET_PRICE, t.TICKET_STATUS, t.BOOKING_TIME
                    FROM TICKET t
                    LEFT JOIN ""TktShowHallMovCust"" x ON t.TICKET_ID   = x.TICKET_ID
                    LEFT JOIN CUSTOMER c               ON x.CUSTOMER_ID = c.CUSTOMER_ID
                    LEFT JOIN MOVIE m                  ON x.MOVIE_ID    = m.MOVIE_ID
                    LEFT JOIN THEATER th               ON x.THEATER_ID  = th.THEATER_ID
                    LEFT JOIN HALL h                   ON x.HALL_ID     = h.HALL_ID
                    ORDER BY t.TICKET_ID DESC";

                var adapter = new OracleDataAdapter(sql, conn);
                var table = new DataTable();
                adapter.Fill(table);
                gvTickets.DataSource = table;
                gvTickets.DataBind();
            }
        }


        private void LoadFormDropdowns()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();

                // Customer dropdown
                FillDropdown(dropCustomer, conn,
                    "SELECT CUSTOMER_ID, CUSTOMER_NAME FROM CUSTOMER ORDER BY CUSTOMER_NAME",
                    "CUSTOMER_NAME", "CUSTOMER_ID");

                // Showtime dropdown — label shows Date + Time + Movie + Hall + Theater
                // so the user knows exactly what they are picking
                FillDropdown(dropShowtime, conn,
                    @"SELECT shmc.SHOW_ID,
                             TO_CHAR(s.SHOW_DATE, 'DD Mon YYYY') || ' ' ||
                             TO_CHAR(s.SHOW_TIME, 'HH24:MI')    || ' | ' ||
                             m.MOVIE_TITLE                       || ' | ' ||
                             h.HALL_NAME                         || ' | ' ||
                             th.THEATER_NAME                     AS SHOW_LABEL
                      FROM ""ShowHallMovCust"" shmc
                      JOIN SHOWTIME s  ON shmc.SHOW_ID    = s.SHOW_ID
                      JOIN MOVIE m     ON shmc.MOVIE_ID   = m.MOVIE_ID
                      JOIN HALL h      ON shmc.HALL_ID    = h.HALL_ID
                      JOIN THEATER th  ON shmc.THEATER_ID = th.THEATER_ID
                      ORDER BY s.SHOW_DATE, s.SHOW_TIME",
                    "SHOW_LABEL", "SHOW_ID");
            }

            // Reset the auto-filled display labels and hidden IDs
            ResetAutoFillFields();
        }

        // Helper: fills any DropDownList from a SQL query
        private void FillDropdown(DropDownList dropdown, OracleConnection conn,
                                   string sql, string textColumn, string valueColumn)
        {
            var adapter = new OracleDataAdapter(sql, conn);
            var table = new DataTable();
            adapter.Fill(table);
            dropdown.DataSource = table;
            dropdown.DataTextField = textColumn;
            dropdown.DataValueField = valueColumn;
            dropdown.DataBind();
            dropdown.Items.Insert(0, new ListItem("-- Select --", "0"));
        }

        // Clears the Movie/Hall/Theater display labels and hidden ID fields
        private void ResetAutoFillFields()
        {
            labelMovie.Text = "<span class='auto-fill-placeholder'>Auto-filled</span>";
            labelHall.Text = "<span class='auto-fill-placeholder'>Auto-filled</span>";
            labelTheater.Text = "<span class='auto-fill-placeholder'>Auto-filled</span>";
            hiddenMovieId.Value = "0";
            hiddenHallId.Value = "0";
            hiddenTheaterId.Value = "0";
        }


        protected void dropShowtime_SelectedIndexChanged(object sender, EventArgs e)
        {
            // If user chose "-- Select --", reset everything
            if (dropShowtime.SelectedValue == "0")
            {
                ResetAutoFillFields();
                ShowModal = true;
                return;
            }

            int selectedShowId = int.Parse(dropShowtime.SelectedValue);

            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();

                // Look up the Movie, Hall, Theater linked to this Showtime
                // via the ShowHallMovCust junction table
                string sql = @"
                    SELECT m.MOVIE_ID,    m.MOVIE_TITLE,
                           h.HALL_ID,     h.HALL_NAME,
                           th.THEATER_ID, th.THEATER_NAME
                    FROM ""ShowHallMovCust"" shmc
                    JOIN MOVIE   m  ON shmc.MOVIE_ID   = m.MOVIE_ID
                    JOIN HALL    h  ON shmc.HALL_ID    = h.HALL_ID
                    JOIN THEATER th ON shmc.THEATER_ID = th.THEATER_ID
                    WHERE shmc.SHOW_ID = :showId
                    AND ROWNUM = 1";

                var cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add(":showId", OracleDbType.Int32).Value = selectedShowId;

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Show the names in the read-only display boxes
                        labelMovie.Text = reader["MOVIE_TITLE"].ToString();
                        labelHall.Text = reader["HALL_NAME"].ToString();
                        labelTheater.Text = reader["THEATER_NAME"].ToString();

                        // Store the IDs in hidden fields for use when saving
                        hiddenMovieId.Value = reader["MOVIE_ID"].ToString();
                        hiddenHallId.Value = reader["HALL_ID"].ToString();
                        hiddenTheaterId.Value = reader["THEATER_ID"].ToString();
                    }
                    else
                    {
                        // Showtime exists but has no linked data in ShowHallMovCust yet
                        ResetAutoFillFields();
                        ShowAlert("No Movie/Hall/Theater linked to this Showtime yet.", "warning");
                    }
                }
            }

            ShowModal = true;
            LoadTicketGrid();
        }


        protected void btnShowAdd_Click(object sender, EventArgs e)
        {
            txtSeat.Text = "";
            txtPrice.Text = "";
            LoadFormDropdowns();
            ShowModal = true;
            LoadTicketGrid();
        }


        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Validate all required fields
            if (dropCustomer.SelectedValue == "0" ||
                dropShowtime.SelectedValue == "0" ||
                hiddenMovieId.Value == "0" ||
                hiddenHallId.Value == "0" ||
                hiddenTheaterId.Value == "0" ||
                string.IsNullOrWhiteSpace(txtSeat.Text) ||
                string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                ShowAlert("Please select a Showtime and Customer, then enter Seat No and Price.", "warning");
                ShowModal = true;
                LoadFormDropdowns();
                // Re-select the previously chosen showtime so auto-fill stays visible
                if (dropShowtime.Items.FindByValue(dropShowtime.SelectedValue) != null)
                    dropShowtime.SelectedValue = dropShowtime.SelectedValue;
                LoadTicketGrid();
                return;
            }

            if (!int.TryParse(txtSeat.Text.Trim(), out int seatNumber))
            {
                ShowAlert("Seat number must be a whole number (e.g. 12).", "warning");
                ShowModal = true;
                LoadFormDropdowns();
                LoadTicketGrid();
                return;
            }

            if (!decimal.TryParse(txtPrice.Text.Trim(), out decimal basePrice))
            {
                ShowAlert("Price must be a valid number (e.g. 500).", "warning");
                ShowModal = true;
                LoadFormDropdowns();
                LoadTicketGrid();
                return;
            }

            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Read values from form
                            int showId = int.Parse(dropShowtime.SelectedValue);
                            int movieId = int.Parse(hiddenMovieId.Value);
                            int hallId = int.Parse(hiddenHallId.Value);
                            int theaterId = int.Parse(hiddenTheaterId.Value);
                            int customerId = int.Parse(dropCustomer.SelectedValue);

                            // Apply weekend/new-release dynamic pricing
                            decimal finalPrice = CalculateDynamicPrice(conn, basePrice, showId, movieId);

                            // Get next available Ticket ID
                            var idCmd = new OracleCommand("SELECT NVL(MAX(TICKET_ID), 0) + 1 FROM TICKET", conn);
                            idCmd.Transaction = transaction;
                            int newTicketId = Convert.ToInt32(idCmd.ExecuteScalar());

                            // Insert the ticket row
                            // PURCHASE_DATE uses DateTime.Now (NOT NULL column — cannot be NULL)
                            var insertTicket = new OracleCommand(
                                @"INSERT INTO TICKET (TICKET_ID, BOOKING_TIME, PURCHASE_DATE, TICKET_STATUS, TICKET_PRICE, SEAT_NO)
                                  VALUES (:id, :bookingTime, :purchaseDate, 'Booked', :price, :seat)", conn);
                            insertTicket.Transaction = transaction;
                            insertTicket.Parameters.Add(":id", OracleDbType.Int32).Value = newTicketId;
                            insertTicket.Parameters.Add(":bookingTime", OracleDbType.Date).Value = DateTime.Now;
                            insertTicket.Parameters.Add(":purchaseDate", OracleDbType.Date).Value = DateTime.Now;
                            insertTicket.Parameters.Add(":price", OracleDbType.Decimal).Value = finalPrice;
                            insertTicket.Parameters.Add(":seat", OracleDbType.Int32).Value = seatNumber;
                            insertTicket.ExecuteNonQuery();

                            // Insert into junction table linking Ticket → Show/Hall/Theater/Movie/Customer
                            var insertJunction = new OracleCommand(
                                @"INSERT INTO ""TktShowHallMovCust"" (TICKET_ID, SHOW_ID, HALL_ID, THEATER_ID, MOVIE_ID, CUSTOMER_ID)
                                  VALUES (:ticketId, :showId, :hallId, :theaterId, :movieId, :customerId)", conn);
                            insertJunction.Transaction = transaction;
                            insertJunction.Parameters.Add(":ticketId", OracleDbType.Int32).Value = newTicketId;
                            insertJunction.Parameters.Add(":showId", OracleDbType.Int32).Value = showId;
                            insertJunction.Parameters.Add(":hallId", OracleDbType.Int32).Value = hallId;
                            insertJunction.Parameters.Add(":theaterId", OracleDbType.Int32).Value = theaterId;
                            insertJunction.Parameters.Add(":movieId", OracleDbType.Int32).Value = movieId;
                            insertJunction.Parameters.Add(":customerId", OracleDbType.Int32).Value = customerId;
                            insertJunction.ExecuteNonQuery();

                            transaction.Commit();

                            string pricingNote = (finalPrice > basePrice) ? " (Weekend/New Release pricing applied)" : "";
                            ShowAlert("Ticket #" + newTicketId + " booked! Price: Rs. " + finalPrice + pricingNote, "success");
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error booking ticket: " + ex.Message, "danger");
                ShowModal = true;
                LoadFormDropdowns();
            }

            LoadTicketGrid();
        }


        private decimal CalculateDynamicPrice(OracleConnection conn, decimal basePrice, int showId, int movieId)
        {
            try
            {
                // Get the showtime date
                var showCmd = new OracleCommand("SELECT SHOW_DATE FROM SHOWTIME WHERE SHOW_ID = :id", conn);
                showCmd.Parameters.Add(":id", OracleDbType.Int32).Value = showId;
                DateTime showDate = Convert.ToDateTime(showCmd.ExecuteScalar());

                decimal multiplier = 1.0m;

                if (showDate.DayOfWeek == DayOfWeek.Friday || showDate.DayOfWeek == DayOfWeek.Saturday)
                    multiplier = 1.2m;

                var movieCmd = new OracleCommand("SELECT MOVIE_RELEASE_DATE FROM MOVIE WHERE MOVIE_ID = :id", conn);
                movieCmd.Parameters.Add(":id", OracleDbType.Int32).Value = movieId;
                var releaseResult = movieCmd.ExecuteScalar();

                if (releaseResult != null && releaseResult != DBNull.Value)
                {
                    DateTime releaseDate = Convert.ToDateTime(releaseResult);
                    double daysSinceRelease = (showDate - releaseDate).TotalDays;
                    if (daysSinceRelease >= 0 && daysSinceRelease <= 7)
                        multiplier = Math.Max(multiplier, 1.3m);
                }

                return Math.Round(basePrice * multiplier, 2);
            }
            catch
            {
                return basePrice; 
            }
        }


        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ShowModal = false;
            LoadTicketGrid();
        }


        protected void gvTickets_RowEditing(object sender, GridViewEditEventArgs e)
        { gvTickets.EditIndex = -1; LoadTicketGrid(); }

        protected void gvTickets_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        { gvTickets.EditIndex = -1; LoadTicketGrid(); }

        protected void gvTickets_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int ticketId = int.Parse(gvTickets.DataKeys[e.RowIndex].Value.ToString());

            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            var deleteJunction = new OracleCommand(
                                @"DELETE FROM ""TktShowHallMovCust"" WHERE TICKET_ID = :id", conn);
                            deleteJunction.Transaction = transaction;
                            deleteJunction.Parameters.Add(":id", OracleDbType.Int32).Value = ticketId;
                            deleteJunction.ExecuteNonQuery();

                            // Then delete from ticket table
                            var deleteTicket = new OracleCommand(
                                "DELETE FROM TICKET WHERE TICKET_ID = :id", conn);
                            deleteTicket.Transaction = transaction;
                            deleteTicket.Parameters.Add(":id", OracleDbType.Int32).Value = ticketId;
                            deleteTicket.ExecuteNonQuery();

                            transaction.Commit();
                            ShowAlert("Ticket #" + ticketId + " deleted.", "success");
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Cannot delete: " + ex.Message, "danger"); }

            LoadTicketGrid();
        }

        protected void gvTickets_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "CancelTicket")
            {
                int ticketId = int.Parse(e.CommandArgument.ToString());
                try
                {
                    using (var conn = new OracleConnection(connectionString))
                    {
                        conn.Open();
                        var cmd = new OracleCommand(
                            "UPDATE TICKET SET TICKET_STATUS = 'Cancelled' WHERE TICKET_ID = :id", conn);
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = ticketId;
                        cmd.ExecuteNonQuery();
                    }
                    ShowAlert("Ticket #" + ticketId + " has been cancelled.", "success");
                }
                catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); }
                LoadTicketGrid();
            }
            else if (e.CommandName == "ConfirmPurchase")
            {
                int ticketId = int.Parse(e.CommandArgument.ToString());
                try
                {
                    using (var conn = new OracleConnection(connectionString))
                    {
                        conn.Open();
                        var cmd = new OracleCommand(
                            "UPDATE TICKET SET TICKET_STATUS = 'Purchased' WHERE TICKET_ID = :id", conn);
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = ticketId;
                        cmd.ExecuteNonQuery();
                    }
                    ShowAlert("Ticket #" + ticketId + " confirmed! Payment received.", "success");
                }
                catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); }
                LoadTicketGrid();
            }
        }


        private void AutoCancelExpiredTickets()
        {
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        UPDATE TICKET t
                        SET    t.TICKET_STATUS = 'Auto-Cancelled'
                        WHERE  t.TICKET_STATUS = 'Booked'
                        AND EXISTS (
                            SELECT 1
                            FROM   ""TktShowHallMovCust"" j
                            JOIN   SHOWTIME s ON j.SHOW_ID = s.SHOW_ID
                            WHERE  j.TICKET_ID = t.TICKET_ID
                            AND    (s.SHOW_DATE + s.SHOW_TIME - SYSDATE) * 24 < 1
                        )";

                    var cmd = new OracleCommand(sql, conn);
                    int cancelled = cmd.ExecuteNonQuery();
                    if (cancelled > 0) LoadTicketGrid();
                }
            }
            catch
            {
            }
        }


        private void ShowAlert(string message, string bootstrapType)
        {
            lblMessage.Text = "<i class='bi bi-info-circle-fill me-2'></i>" + message;
            lblMessage.CssClass = "alert alert-" + bootstrapType + " d-block mb-3";
            lblMessage.Visible = true;
        }
    }
}