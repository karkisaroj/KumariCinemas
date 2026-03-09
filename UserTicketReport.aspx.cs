using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class UserTicketReport : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCustomerDropdown();
            }
        }

        private void LoadCustomerDropdown()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var adapter = new OracleDataAdapter(
                    "SELECT CUSTOMER_ID, CUSTOMER_NAME FROM CUSTOMER ORDER BY CUSTOMER_NAME", conn);
                var table = new DataTable();
                adapter.Fill(table);
                ddlCustomer.DataSource = table;
                ddlCustomer.DataTextField = "CUSTOMER_NAME";
                ddlCustomer.DataValueField = "CUSTOMER_ID";
                ddlCustomer.DataBind();
                ddlCustomer.Items.Insert(0, new ListItem("-- Select a Customer --", "0"));
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (ddlCustomer.SelectedValue == "0")
            {
                ShowAlert("Please select a customer.", "warning");
                pnlResult.Visible = false;
                return;
            }

            int customerId = int.Parse(ddlCustomer.SelectedValue);

            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();

                var cmdCust = new OracleCommand(
                    @"SELECT CUSTOMER_ID, CUSTOMER_NAME, CUSTOMER_EMAIL, CUSTOMER_PHONE, CUSTOMER_REGISTRATION_DATE 
                      FROM CUSTOMER WHERE CUSTOMER_ID = :id", conn);
                cmdCust.Parameters.Add(":id", OracleDbType.Int32).Value = customerId;

                using (var reader = cmdCust.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lblCustId.Text = "#" + reader["CUSTOMER_ID"].ToString();
                        lblCustName.Text = reader["CUSTOMER_NAME"].ToString();
                        lblCustEmail.Text = reader["CUSTOMER_EMAIL"].ToString();
                        lblCustPhone.Text = reader["CUSTOMER_PHONE"].ToString();
                        lblCustRegDate.Text = Convert.ToDateTime(reader["CUSTOMER_REGISTRATION_DATE"]).ToString("dd MMM yyyy");
                    }
                }

                string ticketSql = @"
                    SELECT t.TICKET_ID,
                           NVL(m.MOVIE_TITLE, 'N/A')   AS MOVIE_TITLE,
                           NVL(th.THEATER_NAME, 'N/A')  AS THEATER_NAME,
                           NVL(h.HALL_NAME, 'N/A')      AS HALL_NAME,
                           t.SEAT_NO,
                           t.TICKET_PRICE,
                           t.TICKET_STATUS,
                           t.BOOKING_TIME,
                           s.SHOW_DATE
                    FROM TICKET t
                    JOIN ""TktShowHallMovCust"" x   ON t.TICKET_ID   = x.TICKET_ID
                    LEFT JOIN CUSTOMER c            ON x.CUSTOMER_ID = c.CUSTOMER_ID
                    LEFT JOIN MOVIE m               ON x.MOVIE_ID    = m.MOVIE_ID
                    LEFT JOIN THEATER th            ON x.THEATER_ID  = th.THEATER_ID
                    LEFT JOIN HALL h                ON x.HALL_ID     = h.HALL_ID
                    LEFT JOIN SHOWTIME s            ON x.SHOW_ID     = s.SHOW_ID
                    WHERE x.CUSTOMER_ID = :custId
                      AND t.BOOKING_TIME >= ADD_MONTHS(SYSDATE, -6)
                    ORDER BY t.BOOKING_TIME DESC";

                var adapterTickets = new OracleDataAdapter(ticketSql, conn);
                adapterTickets.SelectCommand.Parameters.Add(":custId", OracleDbType.Int32).Value = customerId;
                var ticketTable = new DataTable();
                adapterTickets.Fill(ticketTable);

                gvTickets.DataSource = ticketTable;
                gvTickets.DataBind();

                string summarySql = @"
                    SELECT COUNT(t.TICKET_ID) AS TOTAL_TICKETS,
                           NVL(SUM(t.TICKET_PRICE), 0) AS TOTAL_SPENT
                    FROM TICKET t
                    JOIN ""TktShowHallMovCust"" x ON t.TICKET_ID = x.TICKET_ID
                    WHERE x.CUSTOMER_ID = :custId
                      AND t.BOOKING_TIME >= ADD_MONTHS(SYSDATE, -6)";

                var cmdSummary = new OracleCommand(summarySql, conn);
                cmdSummary.Parameters.Add(":custId", OracleDbType.Int32).Value = customerId;

                using (var rdr = cmdSummary.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        lblTotalTickets.Text = rdr["TOTAL_TICKETS"].ToString();
                        lblTotalSpent.Text = "Rs. " + Convert.ToDecimal(rdr["TOTAL_SPENT"]).ToString("N2");
                    }
                }

                pnlResult.Visible = true;
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