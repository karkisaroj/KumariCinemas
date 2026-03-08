using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class Theaters : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadTheaterGrid();
        }

        private void LoadTheaterGrid()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var adapter = new OracleDataAdapter(
                    "SELECT THEATER_ID, THEATER_NAME, THEATER_CITY, THEATER_LOCATION FROM THEATER ORDER BY THEATER_ID", conn);
                var table = new DataTable();
                adapter.Fill(table);
                gvTheaters.DataSource = table;
                gvTheaters.DataBind();
            }
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        {
            hfTheaterId.Value = "0";
            txtName.Text = "";
            txtCity.Text = "";
            txtLocation.Text = "";
            lblModalTitle.Text = "Add Theater";
            ShowModal = true;
            LoadTheaterGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCity.Text))
            {
                ShowAlert("Please fill in all required fields.", "warning");
                ShowModal = true; LoadTheaterGrid(); return;
            }

            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    if (hfTheaterId.Value == "0") // adding new
                    {
                        var cmd = new OracleCommand(
                            "INSERT INTO THEATER(THEATER_ID, THEATER_NAME, THEATER_CITY, THEATER_LOCATION) " +
                            "VALUES((SELECT NVL(MAX(THEATER_ID),0)+1 FROM THEATER), :name, :city, :loc)", conn);
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":city", OracleDbType.Varchar2).Value = txtCity.Text.Trim();
                        cmd.Parameters.Add(":loc", OracleDbType.Varchar2).Value = txtLocation.Text.Trim();
                        cmd.ExecuteNonQuery();
                        ShowAlert("Theater added successfully!", "success");
                    }
                    else // editing existing
                    {
                        var cmd = new OracleCommand(
                            "UPDATE THEATER SET THEATER_NAME=:name, THEATER_CITY=:city, THEATER_LOCATION=:loc WHERE THEATER_ID=:id", conn);
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":city", OracleDbType.Varchar2).Value = txtCity.Text.Trim();
                        cmd.Parameters.Add(":loc", OracleDbType.Varchar2).Value = txtLocation.Text.Trim();
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfTheaterId.Value);
                        cmd.ExecuteNonQuery();
                        ShowAlert("Theater updated successfully!", "success");
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); ShowModal = true; }
            LoadTheaterGrid();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ShowModal = false;
            LoadTheaterGrid();
        }

        protected void gvTheaters_RowEditing(object sender, GridViewEditEventArgs e)
        {
            int id = int.Parse(gvTheaters.DataKeys[e.NewEditIndex].Value.ToString());
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var cmd = new OracleCommand("SELECT * FROM THEATER WHERE THEATER_ID=:id", conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hfTheaterId.Value = id.ToString();
                        txtName.Text = reader["THEATER_NAME"].ToString();
                        txtCity.Text = reader["THEATER_CITY"].ToString();
                        txtLocation.Text = reader["THEATER_LOCATION"].ToString();
                        lblModalTitle.Text = "Edit Theater";
                        ShowModal = true;
                    }
                }
            }
            gvTheaters.EditIndex = -1;
            LoadTheaterGrid();
        }

        protected void gvTheaters_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvTheaters.EditIndex = -1;
            LoadTheaterGrid();
        }

        protected void gvTheaters_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvTheaters.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    // Block delete if theater is linked to any showtime
                    var checkCmd = new OracleCommand(
                        "SELECT COUNT(*) FROM \"ShowHallMovCust\" WHERE THEATER_ID=:id", conn);
                    checkCmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                    int showtimeCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (showtimeCount > 0)
                    {
                        ShowAlert("Cannot delete: this theater is used in " + showtimeCount + " showtime(s). Delete those showtimes first.", "warning");
                        LoadTheaterGrid(); return;
                    }

                    var cmd = new OracleCommand("DELETE FROM THEATER WHERE THEATER_ID=:id", conn);
                    cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                    cmd.ExecuteNonQuery();
                    ShowAlert("Theater deleted successfully!", "success");
                }
            }
            catch (Exception ex) { ShowAlert("Cannot delete: " + ex.Message, "danger"); }
            LoadTheaterGrid();
        }

        private void ShowAlert(string message, string bootstrapType)
        {
            lblMessage.Text = "<i class='bi bi-info-circle-fill me-2'></i>" + message;
            lblMessage.CssClass = "alert alert-" + bootstrapType + " d-block mb-3";
            lblMessage.Visible = true;
        }
    }
}