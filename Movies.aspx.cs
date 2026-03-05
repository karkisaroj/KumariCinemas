using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace KumariCinemas
{
    public partial class Movies : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connStr = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e) { if (!IsPostBack) LoadMovies(); }

        private void LoadMovies()
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                var da = new OracleDataAdapter("SELECT MOVIE_ID,MOVIE_TITLE,MOVIE_GENRE,MOVIE_DURATION,MOVIE_LANGUAGE,MOVIE_RELEASE_DATE FROM MOVIE ORDER BY MOVIE_ID", conn);
                var dt = new DataTable(); da.Fill(dt); gvMovies.DataSource = dt; gvMovies.DataBind();
            }
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        {
            hfMovieId.Value = "0"; txtTitle.Text = ""; txtDuration.Text = "";
            txtReleaseDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            ddlGenre.SelectedIndex = 0; ddlLanguage.SelectedIndex = 0;
            lblModalTitle.Text = "Add Movie"; ShowModal = true; LoadMovies();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtDuration.Text))
            { ShowAlert("Please fill in all required fields.", "warning"); ShowModal = true; LoadMovies(); return; }
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    if (hfMovieId.Value == "0")
                    {
                        var cmd = new OracleCommand("INSERT INTO MOVIE(MOVIE_ID,MOVIE_TITLE,MOVIE_GENRE,MOVIE_DURATION,MOVIE_LANGUAGE,MOVIE_RELEASE_DATE) VALUES((SELECT NVL(MAX(MOVIE_ID),0)+1 FROM MOVIE),:title,:genre,:duration,:language,:rdate)", conn);
                        cmd.Parameters.Add(":title", OracleDbType.Varchar2).Value = txtTitle.Text.Trim();
                        cmd.Parameters.Add(":genre", OracleDbType.Varchar2).Value = ddlGenre.SelectedValue;
                        cmd.Parameters.Add(":duration", OracleDbType.Int32).Value = int.Parse(txtDuration.Text);
                        cmd.Parameters.Add(":language", OracleDbType.Varchar2).Value = ddlLanguage.SelectedValue;
                        cmd.Parameters.Add(":rdate", OracleDbType.Date).Value = DateTime.Parse(txtReleaseDate.Text);
                        cmd.ExecuteNonQuery(); ShowAlert("Movie added successfully!", "success");
                    }
                    else
                    {
                        var cmd = new OracleCommand("UPDATE MOVIE SET MOVIE_TITLE=:title,MOVIE_GENRE=:genre,MOVIE_DURATION=:duration,MOVIE_LANGUAGE=:language,MOVIE_RELEASE_DATE=:rdate WHERE MOVIE_ID=:id", conn);
                        cmd.Parameters.Add(":title", OracleDbType.Varchar2).Value = txtTitle.Text.Trim();
                        cmd.Parameters.Add(":genre", OracleDbType.Varchar2).Value = ddlGenre.SelectedValue;
                        cmd.Parameters.Add(":duration", OracleDbType.Int32).Value = int.Parse(txtDuration.Text);
                        cmd.Parameters.Add(":language", OracleDbType.Varchar2).Value = ddlLanguage.SelectedValue;
                        cmd.Parameters.Add(":rdate", OracleDbType.Date).Value = DateTime.Parse(txtReleaseDate.Text);
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfMovieId.Value);
                        cmd.ExecuteNonQuery(); ShowAlert("Movie updated successfully!", "success");
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); ShowModal = true; }
            LoadMovies();
        }

        protected void btnCancel_Click(object sender, EventArgs e) { ShowModal = false; LoadMovies(); }

        protected void gvMovies_RowEditing(object sender, System.Web.UI.WebControls.GridViewEditEventArgs e)
        {
            int id = int.Parse(gvMovies.DataKeys[e.NewEditIndex].Value.ToString());
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                var cmd = new OracleCommand("SELECT * FROM MOVIE WHERE MOVIE_ID=:id", conn);
                cmd.Parameters.Add(":id", id);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        hfMovieId.Value = id.ToString(); txtTitle.Text = r["MOVIE_TITLE"].ToString();
                        txtDuration.Text = r["MOVIE_DURATION"].ToString();
                        txtReleaseDate.Text = Convert.ToDateTime(r["MOVIE_RELEASE_DATE"]).ToString("yyyy-MM-dd");
                        ddlGenre.SelectedValue = r["MOVIE_GENRE"].ToString();
                        ddlLanguage.SelectedValue = r["MOVIE_LANGUAGE"].ToString();
                        lblModalTitle.Text = "Edit Movie"; ShowModal = true;
                    }
                }
            }
            gvMovies.EditIndex = -1; LoadMovies();
        }

        protected void gvMovies_RowCancelingEdit(object sender, System.Web.UI.WebControls.GridViewCancelEditEventArgs e)
        { gvMovies.EditIndex = -1; LoadMovies(); }

        protected void gvMovies_RowDeleting(object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvMovies.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    var cmd = new OracleCommand("DELETE FROM MOVIE WHERE MOVIE_ID=:id", conn);
                    cmd.Parameters.Add(":id", id); cmd.ExecuteNonQuery();
                }
                ShowAlert("Movie deleted successfully!", "success");
            }
            catch (Exception ex) { ShowAlert("Cannot delete: " + ex.Message, "danger"); }
            LoadMovies();
        }

        private void ShowAlert(string msg, string type)
        { lblMessage.Text = "<i class='bi bi-check-circle-fill me-2'></i>" + msg; lblMessage.CssClass = "alert alert-" + type + " d-block mb-3"; lblMessage.Visible = true; }
    }
}