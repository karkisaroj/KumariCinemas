using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;

namespace KumariCinemas
{
    public partial class Customers : System.Web.UI.Page
    {
        public bool ShowModal = false;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadCustomerGrid();
        }

        private void LoadCustomerGrid()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var adapter = new OracleDataAdapter(
                    "SELECT CUSTOMER_ID, CUSTOMER_NAME, CUSTOMER_EMAIL, CUSTOMER_PHONE, CUSTOMER_REGISTRATION_DATE FROM CUSTOMER ORDER BY CUSTOMER_ID", conn);
                var table = new DataTable();
                adapter.Fill(table);
                gvCustomers.DataSource = table;
                gvCustomers.DataBind();
            }
        }

        protected void btnShowAdd_Click(object sender, EventArgs e)
        {
            hfCustomerId.Value = "0";
            txtName.Text = "";
            txtEmail.Text = "";
            txtPhone.Text = "";
            txtRegDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            lblModalTitle.Text = "Add Customer";
            ShowModal = true;
            LoadCustomerGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text) || string.IsNullOrWhiteSpace(txtRegDate.Text))
            {
                ShowAlert("Please fill in all required fields.", "warning");
                ShowModal = true; LoadCustomerGrid(); return;
            }

            if (!txtEmail.Text.Contains("@") || !txtEmail.Text.Contains("."))
            {
                ShowAlert("Please enter a valid email address (e.g. name@email.com).", "warning");
                ShowModal = true; LoadCustomerGrid(); return;
            }

            string phone = txtPhone.Text.Trim();
            string digitsOnly = phone.StartsWith("+") ? phone.Substring(1) : phone;
            if (!long.TryParse(digitsOnly, out _))
            {
                ShowAlert("Phone number must contain only digits (e.g. 9841234567).", "warning");
                ShowModal = true; LoadCustomerGrid(); return;
            }

            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    if (hfCustomerId.Value == "0") 
                    {
                        var cmd = new OracleCommand(
                            "INSERT INTO CUSTOMER(CUSTOMER_ID, CUSTOMER_NAME, CUSTOMER_EMAIL, CUSTOMER_PHONE, CUSTOMER_REGISTRATION_DATE) " +
                            "VALUES((SELECT NVL(MAX(CUSTOMER_ID),0)+1 FROM CUSTOMER), :name, :email, :phone, :regdate)", conn);
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":email", OracleDbType.Varchar2).Value = txtEmail.Text.Trim();
                        cmd.Parameters.Add(":phone", OracleDbType.Varchar2).Value = phone;
                        cmd.Parameters.Add(":regdate", OracleDbType.Date).Value = DateTime.Parse(txtRegDate.Text);
                        cmd.ExecuteNonQuery();
                        ShowAlert("Customer added successfully!", "success");
                    }
                    else // editing existing
                    {
                        var cmd = new OracleCommand(
                            "UPDATE CUSTOMER SET CUSTOMER_NAME=:name, CUSTOMER_EMAIL=:email, CUSTOMER_PHONE=:phone, CUSTOMER_REGISTRATION_DATE=:regdate " +
                            "WHERE CUSTOMER_ID=:id", conn);
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":email", OracleDbType.Varchar2).Value = txtEmail.Text.Trim();
                        cmd.Parameters.Add(":phone", OracleDbType.Varchar2).Value = phone;
                        cmd.Parameters.Add(":regdate", OracleDbType.Date).Value = DateTime.Parse(txtRegDate.Text);
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = int.Parse(hfCustomerId.Value);
                        cmd.ExecuteNonQuery();
                        ShowAlert("Customer updated successfully!", "success");
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, "danger"); ShowModal = true; }
            LoadCustomerGrid();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ShowModal = false;
            LoadCustomerGrid();
        }

        protected void gvCustomers_RowEditing(object sender, GridViewEditEventArgs e)
        {
            int id = int.Parse(gvCustomers.DataKeys[e.NewEditIndex].Value.ToString());
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();
                var cmd = new OracleCommand("SELECT * FROM CUSTOMER WHERE CUSTOMER_ID=:id", conn);
                cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hfCustomerId.Value = id.ToString();
                        txtName.Text = reader["CUSTOMER_NAME"].ToString();
                        txtEmail.Text = reader["CUSTOMER_EMAIL"].ToString();
                        txtPhone.Text = reader["CUSTOMER_PHONE"].ToString();
                        txtRegDate.Text = Convert.ToDateTime(reader["CUSTOMER_REGISTRATION_DATE"]).ToString("yyyy-MM-dd");
                        lblModalTitle.Text = "Edit Customer";
                        ShowModal = true;
                    }
                }
            }
            gvCustomers.EditIndex = -1;
            LoadCustomerGrid();
        }

        protected void gvCustomers_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvCustomers.EditIndex = -1;
            LoadCustomerGrid();
        }

        protected void gvCustomers_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = int.Parse(gvCustomers.DataKeys[e.RowIndex].Value.ToString());
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    // Block delete if customer has tickets linked
                    var checkCmd = new OracleCommand(
                        "SELECT COUNT(*) FROM \"TktShowHallMovCust\" WHERE CUSTOMER_ID=:id", conn);
                    checkCmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                    int ticketCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (ticketCount > 0)
                    {
                        ShowAlert("Cannot delete: this customer has " + ticketCount + " ticket(s). Delete their tickets first.", "warning");
                        LoadCustomerGrid(); return;
                    }

                    var cmd = new OracleCommand("DELETE FROM CUSTOMER WHERE CUSTOMER_ID=:id", conn);
                    cmd.Parameters.Add(":id", OracleDbType.Int32).Value = id;
                    cmd.ExecuteNonQuery();
                    ShowAlert("Customer deleted successfully!", "success");
                }
            }
            catch (Exception ex) { ShowAlert("Cannot delete: " + ex.Message, "danger"); }
            LoadCustomerGrid();
        }

        private void ShowAlert(string message, string bootstrapType)
        {
            lblMessage.Text = "<i class='bi bi-info-circle-fill me-2'></i>" + message;
            lblMessage.CssClass = "alert alert-" + bootstrapType + " d-block mb-3";
            lblMessage.Visible = true;
        }
    }
}