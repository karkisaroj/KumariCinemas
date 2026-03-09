using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class TheaterMovieReport : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTheaterDropdown();
                LoadHallDropdown();
            }
        }

        private void LoadTheaterDropdown()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var adapter = new OracleDataAdapter(
                    "SELECT THEATER_ID, THEATER_NAME || ' (' || THEATER_CITY || ')' AS THEATER_LABEL FROM THEATER ORDER BY THEATER_NAME", conn);
                var table = new DataTable();
                adapter.Fill(table);
                ddlTheater.DataSource = table;
                ddlTheater.DataTextField = "THEATER_LABEL";
                ddlTheater.DataValueField = "THEATER_ID";
                ddlTheater.DataBind();
                ddlTheater.Items.Insert(0, new ListItem("-- Select Theater --", "0"));
            }
        }

        private void LoadHallDropdown()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var adapter = new OracleDataAdapter(
                    "SELECT HALL_ID, HALL_NAME || ' (' || HALL_TYPE || ')' AS HALL_LABEL FROM HALL ORDER BY HALL_NAME", conn);
                var table = new DataTable();
                adapter.Fill(table);
                ddlHall.DataSource = table;
                ddlHall.DataTextField = "HALL_LABEL";
                ddlHall.DataValueField = "HALL_ID";
                ddlHall.DataBind();
                ddlHall.Items.Insert(0, new ListItem("-- Select Hall --", "0"));
            }
        }

        protected void ddlTheater_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlTheater.SelectedValue == "0")
            {
                ddlCity.Items.Clear();
                ddlCity.Items.Insert(0, new ListItem("-- Select Theater first --", "0"));
                return;
            }

            int theaterId = int.Parse(ddlTheater.SelectedValue);
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var cmd = new OracleCommand("SELECT THEATER_CITY FROM THEATER WHERE THEATER_ID = :id", conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = theaterId;
                string city = cmd.ExecuteScalar()?.ToString() ?? "";

                ddlCity.Items.Clear();
                ddlCity.Items.Add(new ListItem(city, city));
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (ddlTheater.SelectedValue == "0" || ddlHall.SelectedValue == "0")
            {
                ShowAlert("Please select a Theater and a Hall.", "warning");
                pnlResult.Visible = false;
                return;
            }

            int theaterId = int.Parse(ddlTheater.SelectedValue);
            int hallId = int.Parse(ddlHall.SelectedValue);

            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();

                var cmdTheater = new OracleCommand(
                    "SELECT THEATER_NAME, THEATER_CITY FROM THEATER WHERE THEATER_ID = :id", conn);
                cmdTheater.Parameters.Add(":id", OracleDbType.Int32).Value = theaterId;
                using (var rdr = cmdTheater.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        lblTheaterName.Text = rdr["THEATER_NAME"].ToString();
                        lblCity.Text = rdr["THEATER_CITY"].ToString();
                    }
                }

                var cmdHall = new OracleCommand("SELECT HALL_NAME FROM HALL WHERE HALL_ID = :id", conn);
                cmdHall.Parameters.Add(":id", OracleDbType.Int32).Value = hallId;
                lblHallName.Text = cmdHall.ExecuteScalar()?.ToString() ?? "N/A";

                string sql = @"
                    SELECT s.SHOW_ID,
                           m.MOVIE_TITLE,
                           m.MOVIE_GENRE,
                           m.MOVIE_DURATION,
                           m.MOVIE_LANGUAGE,
                           s.SHOW_DATE,
                           TO_CHAR(s.SHOW_TIME, 'HH24:MI')     AS SHOW_TIME,
                           TO_CHAR(s.SHOW_END_TIME, 'HH24:MI') AS SHOW_END_TIME
                    FROM ""ShowHallMovCust"" shmc
                    JOIN SHOWTIME s  ON shmc.SHOW_ID    = s.SHOW_ID
                    JOIN MOVIE m     ON shmc.MOVIE_ID   = m.MOVIE_ID
                    JOIN THEATER th  ON shmc.THEATER_ID = th.THEATER_ID
                    JOIN HALL h      ON shmc.HALL_ID    = h.HALL_ID
                    WHERE shmc.THEATER_ID = :theaterId
                      AND shmc.HALL_ID    = :hallId
                    ORDER BY s.SHOW_DATE DESC, s.SHOW_TIME";

                var adapterMovies = new OracleDataAdapter(sql, conn);
                adapterMovies.SelectCommand.Parameters.Add(":theaterId", OracleDbType.Int32).Value = theaterId;
                adapterMovies.SelectCommand.Parameters.Add(":hallId", OracleDbType.Int32).Value = hallId;
                var movieTable = new DataTable();
                adapterMovies.Fill(movieTable);

                gvMovies.DataSource = movieTable;
                gvMovies.DataBind();

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