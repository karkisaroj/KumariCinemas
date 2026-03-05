using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class Tickets : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connStr = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        { 
            if (!IsPostBack) 
            { 
                LoadTickets(); 
                LoadDropdowns(); 
            } 
        }

        private void LoadTickets()
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT t.TICKET_ID,
                                      NVL(c.CUSTOMER_NAME,'N/A') AS CUSTOMER_NAME,
                                      NVL(m.MOVIE_TITLE,'N/A')   AS MOVIE_TITLE,
                                      NVL(th.THEATER_NAME,'N/A') AS THEATER_NAME,
                                      NVL(h.HALL_NAME,'N/A')     AS HALL_NAME,
                                      t.SEAT_NO, t.TICKET_PRICE, t.TICKET_STATUS, t.BOOKING_TIME
                               FROM TICKET t
                               LEFT JOIN ""TktShowHallMovCust"" x ON t.TICKET_ID   = x.TICKET_ID
                               LEFT JOIN CUSTOMER c           ON x.CUSTOMER_ID = c.CUSTOMER_ID
                               LEFT JOIN MOVIE m              ON x.MOVIE_ID    = m.MOVIE_ID
                               LEFT JOIN THEATER th           ON x.THEATER_ID  = th.THEATER_ID
                               LEFT JOIN HALL h               ON x.HALL_ID     = h.HALL_ID
                               ORDER BY t.TICKET_ID DESC";
                var da = new OracleDataAdapter(sql, conn);
                var dt = new DataTable(); da.Fill(dt);
                gvTickets.DataSource = dt; gvTickets.DataBind();
            }
        }

        private void LoadDropdowns()
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                FillDDL(ddlCustomer, conn, "SELECT CUSTOMER_ID, CUSTOMER_NAME FROM CUSTOMER ORDER BY CUSTOMER_NAME", "CUSTOMER_NAME", "CUSTOMER_ID");
                FillDDL(ddlMovie, conn, "SELECT MOVIE_ID, MOVIE_TITLE FROM MOVIE ORDER BY MOVIE_TITLE", "MOVIE_TITLE", "MOVIE_ID");
                FillDDL(ddlTheater, conn, "SELECT THEATER_ID, THEATER_NAME FROM THEATER ORDER BY THEATER_NAME", "THEATER_NAME", "THEATER_ID");
                FillDDL(ddlHall, conn, "SELECT HALL_ID, HALL_NAME FROM HALL ORDER BY HALL_NAME", "HALL_NAME", "HALL_ID");
                FillDDL(ddlShowtime, conn, "SELECT SHOW_ID, TO_CHAR(SHOW_DATE,'DD Mon YYYY')||' '||TO_CHAR(SHOW_TIME,'HH24:MI') AS SHOW_LABEL FROM SHOWTIME ORDER BY SHOW_DATE,SHOW_TIME", "SHOW_LABEL", "SHOW_ID");
            }
        }

        private void FillDDL(DropDownList ddl, OracleConnection conn, string sql, string textField, string valueField)
        {
            var da = new OracleDataAdapter(sql, conn);
            var dt = new DataTable(); da.Fill(dt);
            ddl.DataSource = dt; ddl.DataTextField = textField; ddl.DataValueField = valueField; ddl.DataBind();
            ddl.Items.Insert(0, new ListItem("-- Select --", "0"));
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        { txtSeat.Text = ""; txtPrice.Text = ""; LoadDropdowns(); ShowModal = true; LoadTickets(); }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (ddlCustomer.SelectedValue == "0" || ddlMovie.SelectedValue == "0" || ddlTheater.SelectedValue == "0" ||
                ddlHall.SelectedValue == "0" || ddlShowtime.SelectedValue == "0" ||
                string.IsNullOrWhiteSpace(txtSeat.Text) || string.IsNullOrWhiteSpace(txtPrice.Text))
            { 
                ShowAlert("Please fill in all fields.", "warning"); 
                ShowModal = true; 
                LoadDropdowns(); 
                LoadTickets(); 
                return; 
            }

            // Validate seat number is numeric
            if (!int.TryParse(txtSeat.Text.Trim(), out int seatNo))
            {
                ShowAlert("Seat number must be a valid number (e.g., 12, 45).", "warning");
                ShowModal = true;
                LoadDropdowns();
                LoadTickets();
                return;
            }

            // Validate price is numeric
            if (!decimal.TryParse(txtPrice.Text.Trim(), out decimal price))
            {
                ShowAlert("Price must be a valid number.", "warning");
                ShowModal = true;
                LoadDropdowns();
                LoadTickets();
                return;
            }

            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            var cmdId = new OracleCommand("SELECT NVL(MAX(TICKET_ID),0)+1 FROM TICKET", conn);
                            cmdId.Transaction = tran;
                            int newId = Convert.ToInt32(cmdId.ExecuteScalar());

                            var cmd = new OracleCommand("INSERT INTO TICKET(TICKET_ID,BOOKING_TIME,PURCHASE_DATE,TICKET_STATUS,TICKET_PRICE,SEAT_NO) VALUES(:id,SYSDATE,SYSDATE,'Booked',:price,:seat)", conn);
                            cmd.Transaction = tran;
                            cmd.Parameters.Add(":id", OracleDbType.Int32).Value = newId;
                            cmd.Parameters.Add(":price", OracleDbType.Decimal).Value = price;
                            cmd.Parameters.Add(":seat", OracleDbType.Int32).Value = seatNo;
                            cmd.ExecuteNonQuery();

                            var cmd2 = new OracleCommand("INSERT INTO \"TktShowHallMovCust\"(TICKET_ID,SHOW_ID,HALL_ID,THEATER_ID,MOVIE_ID,CUSTOMER_ID) VALUES(:tid,:sid,:hid,:thid,:mid,:cid)", conn);
                            cmd2.Transaction = tran;
                            cmd2.Parameters.Add(":tid", OracleDbType.Int32).Value = newId;
                            cmd2.Parameters.Add(":sid", OracleDbType.Int32).Value = int.Parse(ddlShowtime.SelectedValue);
                            cmd2.Parameters.Add(":hid", OracleDbType.Int32).Value = int.Parse(ddlHall.SelectedValue);
                            cmd2.Parameters.Add(":thid", OracleDbType.Int32).Value = int.Parse(ddlTheater.SelectedValue);
                            cmd2.Parameters.Add(":mid", OracleDbType.Int32).Value = int.Parse(ddlMovie.SelectedValue);
                            cmd2.Parameters.Add(":cid", OracleDbType.Int32).Value = int.Parse(ddlCustomer.SelectedValue);
                            cmd2.ExecuteNonQuery();

                            tran.Commit();
                            ShowAlert("Ticket booked! Ticket #" + newId, "success");
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
            LoadTickets();
        }

        protected void btnCancel_Click(object sender, EventArgs e) { ShowModal = false; LoadTickets(); }

        protected void gvTickets_RowEditing(object sender, GridViewEditEventArgs e)
        { gvTickets.EditIndex = -1; LoadTickets(); }

        protected void gvTickets_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        { gvTickets.EditIndex = -1; LoadTickets(); }

        protected void gvTickets_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvTickets.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            var c1 = new OracleCommand(@"DELETE FROM ""TktShowHallMovCust"" WHERE TICKET_ID=:id", conn);
                            c1.Transaction = tran;
                            c1.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            c1.ExecuteNonQuery();

                            var c2 = new OracleCommand("DELETE FROM TICKET WHERE TICKET_ID=:id", conn);
                            c2.Transaction = tran;
                            c2.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                            c2.ExecuteNonQuery();

                            tran.Commit();
                            ShowAlert("Ticket deleted!", "success");
                        }
                        catch
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Cannot delete: " + ex.Message, "danger"); }
            LoadTickets();
        }

        protected void gvTickets_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "CancelTicket")
            {
                int id = int.Parse(e.CommandArgument.ToString());
                try
                {
                    using (var conn = new OracleConnection(connStr))
                    {
                        conn.Open();
                        var cmd = new OracleCommand("UPDATE TICKET SET TICKET_STATUS='Cancelled' WHERE TICKET_ID=:id", conn);
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                        cmd.ExecuteNonQuery();
                    }
                    ShowAlert("Ticket #" + id + " cancelled.", "success");
                }
                catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); }
                LoadTickets();
            }
        }

        private void ShowAlert(string msg, string type)
        { lblMessage.Text = "<i class='bi bi-check-circle-fill me-2'></i>" + msg; lblMessage.CssClass = "alert alert-" + type + " d-block mb-3"; lblMessage.Visible = true; }
    }
}