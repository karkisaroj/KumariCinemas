using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace KumariCinemas
{
    public partial class Customers : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connStr = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                LoadCustomers();
        }

        private void LoadCustomers()
        {
            using (OracleConnection conn = new OracleConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT CUSTOMER_ID, CUSTOMER_NAME, CUSTOMER_EMAIL, 
                                      CUSTOMER_PHONE, CUSTOMER_REGISTRATION_DATE 
                               FROM CUSTOMER ORDER BY CUSTOMER_ID";
                OracleDataAdapter da = new OracleDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvCustomers.DataSource = dt;
                gvCustomers.DataBind();
            }
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        {
            hfCustomerId.Value = "0";
            txtName.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtPhone.Text = string.Empty;
            txtRegDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            lblModalTitle.Text = "Add Customer";
            ShowModal = true;
            LoadCustomers();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text) ||
                string.IsNullOrWhiteSpace(txtRegDate.Text))
            {
                ShowAlert("Please fill in all required fields.", "warning");
                ShowModal = true;
                LoadCustomers();
                return;
            }

            try
            {
                using (OracleConnection conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    if (hfCustomerId.Value == "0")
                    {
                        string sql = @"INSERT INTO CUSTOMER 
                                        (CUSTOMER_ID, CUSTOMER_NAME, CUSTOMER_EMAIL, CUSTOMER_PHONE, CUSTOMER_REGISTRATION_DATE)
                                       VALUES 
                                        ((SELECT NVL(MAX(CUSTOMER_ID),0)+1 FROM CUSTOMER), :name, :email, :phone, :regdate)";

                        using (OracleCommand cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                            cmd.Parameters.Add(":email", OracleDbType.Varchar2).Value = txtEmail.Text.Trim();
                            cmd.Parameters.Add(":phone", OracleDbType.Varchar2).Value = txtPhone.Text.Trim();
                            cmd.Parameters.Add(":regdate", OracleDbType.Date).Value = DateTime.Parse(txtRegDate.Text);
                            cmd.ExecuteNonQuery();
                        }
                        ShowAlert("Customer added successfully!", "success");
                    }
                    else
                    {
                        string sql = @"UPDATE CUSTOMER 
                                       SET CUSTOMER_NAME=:name, CUSTOMER_EMAIL=:email, 
                                           CUSTOMER_PHONE=:phone, CUSTOMER_REGISTRATION_DATE=:regdate 
                                       WHERE CUSTOMER_ID=:id";

                        using (OracleCommand cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                            cmd.Parameters.Add(":email", OracleDbType.Varchar2).Value = txtEmail.Text.Trim();
                            cmd.Parameters.Add(":phone", OracleDbType.Varchar2).Value = txtPhone.Text.Trim();
                            cmd.Parameters.Add(":regdate", OracleDbType.Date).Value = DateTime.Parse(txtRegDate.Text);
                            cmd.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfCustomerId.Value);
                            cmd.ExecuteNonQuery();
                        }
                        ShowAlert("Customer updated successfully!", "success");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, "danger");
                ShowModal = true;
            }

            LoadCustomers();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ShowModal = false;
            LoadCustomers();
        }

        protected void gvCustomers_RowEditing(object sender, System.Web.UI.WebControls.GridViewEditEventArgs e)
        {
            int customerId = int.Parse(gvCustomers.DataKeys[e.NewEditIndex].Value.ToString());

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand(
                    "SELECT * FROM CUSTOMER WHERE CUSTOMER_ID = :id", conn))
                {
                    cmd.Parameters.Add(":id", customerId);
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            hfCustomerId.Value = customerId.ToString();
                            txtName.Text = reader["CUSTOMER_NAME"].ToString();
                            txtEmail.Text = reader["CUSTOMER_EMAIL"].ToString();
                            txtPhone.Text = reader["CUSTOMER_PHONE"].ToString();
                            txtRegDate.Text = Convert.ToDateTime(reader["CUSTOMER_REGISTRATION_DATE"]).ToString("yyyy-MM-dd");
                            lblModalTitle.Text = "Edit Customer";
                            ShowModal = true;
                        }
                    }
                }
            }

            gvCustomers.EditIndex = -1;
            LoadCustomers();
        }

        protected void gvCustomers_RowCancelingEdit(object sender, System.Web.UI.WebControls.GridViewCancelEditEventArgs e)
        {
            gvCustomers.EditIndex = -1;
            LoadCustomers();
        }

        protected void gvCustomers_RowDeleting(object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
        {
            int customerId = int.Parse(gvCustomers.DataKeys[e.RowIndex].Value.ToString());

            try
            {
                using (OracleConnection conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand(
                        "DELETE FROM CUSTOMER WHERE CUSTOMER_ID = :id", conn))
                    {
                        cmd.Parameters.Add(":id", customerId);
                        cmd.ExecuteNonQuery();
                    }
                }
                ShowAlert("Customer deleted successfully!", "success");
            }
            catch (Exception ex)
            {
                ShowAlert("Cannot delete: " + ex.Message, "danger");
            }

            LoadCustomers();
        }

        private void ShowAlert(string message, string type)
        {
            lblMessage.Text = "<i class='bi bi-check-circle-fill me-2'></i>" + message;
            lblMessage.CssClass = "alert alert-" + type + " d-block mb-3";
            lblMessage.Visible = true;
        }
    }
}