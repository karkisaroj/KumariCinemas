using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace KumariCinemas
{
    public partial class Theaters : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connStr = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e) { if (!IsPostBack) LoadTheaters(); }

        private void LoadTheaters()
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                var da = new OracleDataAdapter("SELECT THEATER_ID,THEATER_NAME,THEATER_CITY,THEATER_LOCATION FROM THEATER ORDER BY THEATER_ID", conn);
                var dt = new DataTable(); da.Fill(dt); gvTheaters.DataSource = dt; gvTheaters.DataBind();
            }
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        { hfTheaterId.Value = "0"; txtName.Text = ""; txtCity.Text = ""; txtLocation.Text = ""; lblModalTitle.Text = "Add Theater"; ShowModal = true; LoadTheaters(); }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCity.Text))
            { ShowAlert("Please fill in required fields.", "warning"); ShowModal = true; LoadTheaters(); return; }
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    if (hfTheaterId.Value == "0")
                    {
                        var cmd = new OracleCommand("INSERT INTO THEATER(THEATER_ID,THEATER_NAME,THEATER_CITY,THEATER_LOCATION) VALUES((SELECT NVL(MAX(THEATER_ID),0)+1 FROM THEATER),:name,:city,:loc)", conn);
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":city", OracleDbType.Varchar2).Value = txtCity.Text.Trim();
                        cmd.Parameters.Add(":loc", OracleDbType.Varchar2).Value = txtLocation.Text.Trim();
                        cmd.ExecuteNonQuery(); ShowAlert("Theater added successfully!", "success");
                    }
                    else
                    {
                        var cmd = new OracleCommand("UPDATE THEATER SET THEATER_NAME=:name,THEATER_CITY=:city,THEATER_LOCATION=:loc WHERE THEATER_ID=:id", conn);
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":city", OracleDbType.Varchar2).Value = txtCity.Text.Trim();
                        cmd.Parameters.Add(":loc", OracleDbType.Varchar2).Value = txtLocation.Text.Trim();
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfTheaterId.Value);
                        cmd.ExecuteNonQuery(); ShowAlert("Theater updated successfully!", "success");
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); ShowModal = true; }
            LoadTheaters();
        }

        protected void btnCancel_Click(object sender, EventArgs e) { ShowModal = false; LoadTheaters(); }

        protected void gvTheaters_RowEditing(object sender, System.Web.UI.WebControls.GridViewEditEventArgs e)
        {
            int id = int.Parse(gvTheaters.DataKeys[e.NewEditIndex].Value.ToString());
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                var cmd = new OracleCommand("SELECT * FROM THEATER WHERE THEATER_ID=:id", conn);
                cmd.Parameters.Add(":id", id);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read()) { hfTheaterId.Value = id.ToString(); txtName.Text = r["THEATER_NAME"].ToString(); txtCity.Text = r["THEATER_CITY"].ToString(); txtLocation.Text = r["THEATER_LOCATION"].ToString(); lblModalTitle.Text = "Edit Theater"; ShowModal = true; }
                }
            }
            gvTheaters.EditIndex = -1; LoadTheaters();
        }

        protected void gvTheaters_RowCancelingEdit(object sender, System.Web.UI.WebControls.GridViewCancelEditEventArgs e)
        { gvTheaters.EditIndex = -1; LoadTheaters(); }

        protected void gvTheaters_RowDeleting(object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvTheaters.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connStr)) { conn.Open(); var cmd = new OracleCommand("DELETE FROM THEATER WHERE THEATER_ID=:id", conn); cmd.Parameters.Add(":id", id); cmd.ExecuteNonQuery(); }
                ShowAlert("Theater deleted successfully!", "success");
            }
            catch (Exception ex) { ShowAlert("Cannot delete: " + ex.Message, "danger"); }
            LoadTheaters();
        }

        private void ShowAlert(string msg, string type)
        { lblMessage.Text = "<i class='bi bi-check-circle-fill me-2'></i>" + msg; lblMessage.CssClass = "alert alert-" + type + " d-block mb-3"; lblMessage.Visible = true; }
    }
}