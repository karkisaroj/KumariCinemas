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
            }
        }

        private string GetCount(OracleConnection conn, string table)
        {
            using (var cmd = new OracleCommand("SELECT COUNT(*) FROM " + table, conn))
                return cmd.ExecuteScalar().ToString();
        }
    }
}