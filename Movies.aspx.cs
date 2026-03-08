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
                    "SELECT MOVIE_ID, MOVIE_TITLE, MOVIE_GENRE, MOVIE_DURATION, MOVIE_LANGUAGE, MOVIE_RELEASE_DATE FROM MOVIE ORDER BY MOVIE_ID", conn);
                var table = new DataTable();
                adapter.Fill(table);
                gvMovies.DataSource = table;
                gvMovies.DataBind();
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
            ShowModal = true;
            LoadMovieGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtDuration.Text) ||
                ddlGenre.SelectedValue == "" || ddlLanguage.SelectedValue == "")
            {
                ShowAlert("Please fill in all required fields.", "warning");
                ShowModal = true; LoadMovieGrid(); return;
            }

            // Duration must be a positive number
            if (!int.TryParse(txtDuration.Text.Trim(), out int duration) || duration <= 0)
            {
                ShowAlert("Duration must be a positive number (e.g. 120 for 2 hours).", "warning");
                ShowModal = true; LoadMovieGrid(); return;
            }

            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    if (hfMovieId.Value == "0") // adding new
                    {
                        var cmd = new OracleCommand(
                            "INSERT INTO MOVIE(MOVIE_ID, MOVIE_TITLE, MOVIE_GENRE, MOVIE_DURATION, MOVIE_LANGUAGE, MOVIE_RELEASE_DATE) " +
                            "VALUES((SELECT NVL(MAX(MOVIE_ID),0)+1 FROM MOVIE), :title, :genre, :duration, :language, :rdate)", conn);
                        cmd.Parameters.Add(":title", OracleDbType.Varchar2).Value = txtTitle.Text.Trim();
                        cmd.Parameters.Add(":genre", OracleDbType.Varchar2).Value = ddlGenre.SelectedValue;
                        cmd.Parameters.Add(":duration", OracleDbType.Int32).Value = duration;
                        cmd.Parameters.Add(":language", OracleDbType.Varchar2).Value = ddlLanguage.SelectedValue;
                        cmd.Parameters.Add(":rdate", OracleDbType.Date).Value = DateTime.Parse(txtReleaseDate.Text);
                        cmd.ExecuteNonQuery();
                        ShowAlert("Movie added successfully!", "success");
                    }
                    else // editing existing
                    {
                        var cmd = new OracleCommand(
                            "UPDATE MOVIE SET MOVIE_TITLE=:title, MOVIE_GENRE=:genre, MOVIE_DURATION=:duration, MOVIE_LANGUAGE=:language, MOVIE_RELEASE_DATE=:rdate " +
                            "WHERE MOVIE_ID=:id", conn);
                        cmd.Parameters.Add(":title", OracleDbType.Varchar2).Value = txtTitle.Text.Trim();
                        cmd.Parameters.Add(":genre", OracleDbType.Varchar2).Value = ddlGenre.SelectedValue;
                        cmd.Parameters.Add(":duration", OracleDbType.Int32).Value = duration;
                        cmd.Parameters.Add(":language", OracleDbType.Varchar2).Value = ddlLanguage.SelectedValue;
                        cmd.Parameters.Add(":rdate", OracleDbType.Date).Value = DateTime.Parse(txtReleaseDate.Text);
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfMovieId.Value);
                        cmd.ExecuteNonQuery();
                        ShowAlert("Movie updated successfully!", "success");
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); ShowModal = true; }
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

                    // Block delete if movie is linked to any showtime
                    var checkCmd = new OracleCommand(
                        "SELECT COUNT(*) FROM \"ShowHallMovCust\" WHERE MOVIE_ID=:id", conn);
                    checkCmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                    int showtimeCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (showtimeCount > 0)
                    {
                        ShowAlert("Cannot delete: this movie is linked to " + showtimeCount + " showtime(s). Delete those showtimes first.", "warning");
                        LoadMovieGrid(); return;
                    }

                    var cmd = new OracleCommand("DELETE FROM MOVIE WHERE MOVIE_ID=:id", conn);
                    cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                    cmd.ExecuteNonQuery();
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