using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace KumariCinemas
{
    public partial class Halls : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connStr = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e) { if (!IsPostBack) LoadHalls(); }

        private void LoadHalls()
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                var da = new OracleDataAdapter("SELECT HALL_ID,HALL_NAME,HALL_CAPACITY,HALL_TYPE FROM HALL ORDER BY HALL_ID", conn);
                var dt = new DataTable(); da.Fill(dt); gvHalls.DataSource = dt; gvHalls.DataBind();
            }
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        { hfHallId.Value = "0"; txtName.Text = ""; txtCapacity.Text = ""; ddlType.SelectedIndex = 0; lblModalTitle.Text = "Add Hall"; ShowModal = true; LoadHalls(); }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCapacity.Text))
            { ShowAlert("Please fill in required fields.", "warning"); ShowModal = true; LoadHalls(); return; }
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    if (hfHallId.Value == "0")
                    {
                        var cmd = new OracleCommand("INSERT INTO HALL(HALL_ID,HALL_NAME,HALL_CAPACITY,HALL_TYPE) VALUES((SELECT NVL(MAX(HALL_ID),0)+1 FROM HALL),:name,:cap,:type)", conn);
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":cap", OracleDbType.Int32).Value = int.Parse(txtCapacity.Text);
                        cmd.Parameters.Add(":type", OracleDbType.Varchar2).Value = ddlType.SelectedValue;
                        cmd.ExecuteNonQuery(); ShowAlert("Hall added successfully!", "success");
                    }
                    else
                    {
                        var cmd = new OracleCommand("UPDATE HALL SET HALL_NAME=:name,HALL_CAPACITY=:cap,HALL_TYPE=:type WHERE HALL_ID=:id", conn);
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":cap", OracleDbType.Int32).Value = int.Parse(txtCapacity.Text);
                        cmd.Parameters.Add(":type", OracleDbType.Varchar2).Value = ddlType.SelectedValue;
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfHallId.Value);
                        cmd.ExecuteNonQuery(); ShowAlert("Hall updated successfully!", "success");
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); ShowModal = true; }
            LoadHalls();
        }

        protected void btnCancel_Click(object sender, EventArgs e) { ShowModal = false; LoadHalls(); }

        protected void gvHalls_RowEditing(object sender, System.Web.UI.WebControls.GridViewEditEventArgs e)
        {
            int id = int.Parse(gvHalls.DataKeys[e.NewEditIndex].Value.ToString());
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                var cmd = new OracleCommand("SELECT * FROM HALL WHERE HALL_ID=:id", conn);
                cmd.Parameters.Add(":id", id);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read()) { hfHallId.Value = id.ToString(); txtName.Text = r["HALL_NAME"].ToString(); txtCapacity.Text = r["HALL_CAPACITY"].ToString(); ddlType.SelectedValue = r["HALL_TYPE"].ToString(); lblModalTitle.Text = "Edit Hall"; ShowModal = true; }
                }
            }
            gvHalls.EditIndex = -1; LoadHalls();
        }

        protected void gvHalls_RowCancelingEdit(object sender, System.Web.UI.WebControls.GridViewCancelEditEventArgs e)
        { gvHalls.EditIndex = -1; LoadHalls(); }

        protected void gvHalls_RowDeleting(object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvHalls.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connStr)) { conn.Open(); var cmd = new OracleCommand("DELETE FROM HALL WHERE HALL_ID=:id", conn); cmd.Parameters.Add(":id", id); cmd.ExecuteNonQuery(); }
                ShowAlert("Hall deleted successfully!", "success");
            }
            catch (Exception ex) { ShowAlert("Cannot delete: " + ex.Message, "danger"); }
            LoadHalls();
        }

        private void ShowAlert(string msg, string type)
        { lblMessage.Text = "<i class='bi bi-check-circle-fill me-2'></i>" + msg; lblMessage.CssClass = "alert alert-" + type + " d-block mb-3"; lblMessage.Visible = true; }
    }
}