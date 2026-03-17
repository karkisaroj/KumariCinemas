using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class MovieOccupancyReport : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadMovieDropdown();
            }
        }

        private void LoadMovieDropdown()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var adapter = new OracleDataAdapter(
                    "SELECT MOVIE_ID, MOVIE_TITLE FROM MOVIE ORDER BY MOVIE_TITLE", conn);
                var table = new DataTable();
                adapter.Fill(table);
                ddlMovie.DataSource = table;
                ddlMovie.DataTextField = "MOVIE_TITLE";
                ddlMovie.DataValueField = "MOVIE_ID";
                ddlMovie.DataBind();
                ddlMovie.Items.Insert(0, new ListItem("-- Select a Movie --", "0"));
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (ddlMovie.SelectedValue == "0")
            {
                ShowAlert("Please select a movie.", "warning");
                pnlResult.Visible = false;
                return;
            }

            int movieId = int.Parse(ddlMovie.SelectedValue);

            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();

                var cmdMovie = new OracleCommand(
                    @"SELECT MOVIE_TITLE, MOVIE_GENRE, MOVIE_DURATION, MOVIE_LANGUAGE, MOVIE_RELEASE_DATE
                      FROM MOVIE WHERE MOVIE_ID = :id", conn);
                cmdMovie.Parameters.Add(":id", OracleDbType.Int32).Value = movieId;

                using (var reader = cmdMovie.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lblMovieTitle.Text = reader["MOVIE_TITLE"].ToString();
                        lblGenre.Text = reader["MOVIE_GENRE"].ToString();
                        lblDuration.Text = reader["MOVIE_DURATION"].ToString() + " mins";
                        lblLanguage.Text = reader["MOVIE_LANGUAGE"].ToString();
                        lblReleaseDate.Text = Convert.ToDateTime(reader["MOVIE_RELEASE_DATE"]).ToString("dd MMM yyyy");
                    }
                }

                //    Occupancy % = (paid tickets / hall capacity) * 100
                string sql = @"
                    SELECT *
                    FROM (
                        SELECT th.THEATER_NAME,
                               th.THEATER_CITY,
                               h.HALL_NAME,
                               h.HALL_CAPACITY,
                               COUNT(t.TICKET_ID) AS PAID_TICKETS,
                               ROUND((COUNT(t.TICKET_ID) / h.HALL_CAPACITY) * 100, 2) AS OCCUPANCY_PCT,
                               ROW_NUMBER() OVER (ORDER BY ROUND((COUNT(t.TICKET_ID) / h.HALL_CAPACITY) * 100, 2) DESC) AS RANK_NO
                        FROM ""TktShowHallMovCust"" x
                        JOIN TICKET t     ON x.TICKET_ID   = t.TICKET_ID
                        JOIN THEATER th   ON x.THEATER_ID  = th.THEATER_ID
                        JOIN HALL h       ON x.HALL_ID     = h.HALL_ID
                        WHERE x.MOVIE_ID = :movieId
                          AND (t.TICKET_STATUS = 'Purchased' OR t.TICKET_STATUS='Paid')
                          AND h.HALL_CAPACITY > 0
                        GROUP BY th.THEATER_NAME, th.THEATER_CITY, h.HALL_NAME, h.HALL_CAPACITY
                        ORDER BY OCCUPANCY_PCT DESC
                    )
                    WHERE RANK_NO <= 3";

                var adapterOcc = new OracleDataAdapter(sql, conn);
                adapterOcc.SelectCommand.Parameters.Add(":movieId", OracleDbType.Int32).Value = movieId;
                var occTable = new DataTable();
                adapterOcc.Fill(occTable);

                gvOccupancy.DataSource = occTable;
                gvOccupancy.DataBind();

                pnlResult.Visible = true;
            }
        }

        protected void gvOccupancy_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // No additional processing needed — rank styling is handled via CSS classes in markup
        }

        private void ShowAlert(string message, string bootstrapType)
        {
            lblMessage.Text = "<i class='bi bi-info-circle-fill me-2'></i>" + message;
            lblMessage.CssClass = "alert alert-" + bootstrapType + " d-block mb-3";
            lblMessage.Visible = true;
        }
    }
}