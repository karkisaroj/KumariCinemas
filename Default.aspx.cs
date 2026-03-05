using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace KumariCinemas
{
    public partial class Default : System.Web.UI.Page
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadDashboard();
        }

        private void LoadDashboard()
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                lblCustomers.Text = GetCount(conn, "CUSTOMER");
                lblMovies.Text = GetCount(conn, "MOVIE");
                lblTheaters.Text = GetCount(conn, "THEATER");
                lblHalls.Text = GetCount(conn, "HALL");
                lblShowtimes.Text = GetCount(conn, "SHOWTIME");
                lblTickets.Text = GetCount(conn, "TICKET");

                try
                {
                    string sql = @"SELECT * FROM (
                        SELECT t.TICKET_ID,
                               NVL(c.CUSTOMER_NAME,'N/A') AS CUSTOMER_NAME,
                               NVL(m.MOVIE_TITLE,'N/A')   AS MOVIE_TITLE,
                               NVL(th.THEATER_NAME,'N/A') AS THEATER_NAME,
                               t.SEAT_NO, t.TICKET_PRICE, t.TICKET_STATUS
                        FROM TICKET t
                        LEFT JOIN TktShowHallMovCust x ON t.TICKET_ID   = x.TICKET_ID
                        LEFT JOIN CUSTOMER c           ON x.CUSTOMER_ID = c.CUSTOMER_ID
                        LEFT JOIN MOVIE m              ON x.MOVIE_ID    = m.MOVIE_ID
                        LEFT JOIN THEATER th           ON x.THEATER_ID  = th.THEATER_ID
                        ORDER BY t.TICKET_ID DESC
                    ) WHERE ROWNUM <= 10";
                    var da = new OracleDataAdapter(sql, conn);
                    var dt = new DataTable();
                    da.Fill(dt);
                    gvRecent.DataSource = dt;
                    gvRecent.DataBind();
                }
                catch
                {
                    gvRecent.DataSource = new DataTable();
                    gvRecent.DataBind();
                }
            }
        }

        private string GetCount(OracleConnection conn, string table)
        {
            using (var cmd = new OracleCommand("SELECT COUNT(*) FROM " + table, conn))
                return cmd.ExecuteScalar().ToString();
        }
    }
}