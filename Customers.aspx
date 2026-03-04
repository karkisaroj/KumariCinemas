<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Customers.aspx.cs" Inherits="KumariCinemas.Customers" EnableEventValidation="false" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Customers - Kumari Cinemas</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <style>
        body { background-color: #f4f6f9; }

        .navbar { background-color: #1a2332 !important; padding: 0 24px; }
        .navbar-brand { color: #fff !important; font-weight: 700; font-size: 1.15rem; }
        .navbar-brand span { color: #f0a500; }
        .navbar .nav-link { color: rgba(255,255,255,0.75) !important; font-size: 0.81rem; padding: 18px 9px !important; transition: color 0.2s; }
        .navbar .nav-link:hover, .navbar .nav-link.active { color: #fff !important; border-bottom: 2px solid #f0a500; }

        .page-card { background: #fff; border-radius: 12px; box-shadow: 0 2px 12px rgba(0,0,0,0.07); padding: 28px; }
        .page-title { font-size: 1.35rem; font-weight: 700; color: #1a2332; }

        .table thead th { background-color: #1a2332; color: #fff; font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.5px; border: none; padding: 13px 16px; }
        .table tbody tr:hover { background-color: #f0f4ff; }
        .table td { vertical-align: middle; font-size: 0.88rem; padding: 11px 16px; }
        .badge-id { background-color: #e8eaf6; color: #3949ab; font-weight: 600; padding: 4px 10px; border-radius: 20px; font-size: 0.77rem; }

        .search-wrapper { position: relative; display: inline-block; }
        .search-wrapper i { position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: #9e9e9e; z-index: 1; }
        .search-input { padding-left: 36px !important; border-radius: 8px; width: 300px; font-size: 0.87rem; }

        .btn-add { background-color: #1a2332; color: #fff; border-radius: 8px; padding: 9px 20px; font-size: 0.87rem; border: none; }
        .btn-add:hover { background-color: #2e3f5c; color: #fff; }
        .btn-edit-row { background: #e3f2fd; color: #1565c0; border: none; border-radius: 6px; padding: 5px 11px; font-size: 0.79rem; }
        .btn-edit-row:hover { background: #bbdefb; color: #0d47a1; }
        .btn-delete-row { background: #fce4ec; color: #c62828; border: none; border-radius: 6px; padding: 5px 11px; font-size: 0.79rem; }
        .btn-delete-row:hover { background: #ef9a9a; color: #b71c1c; }

        .modal-header { background-color: #1a2332; color: #fff; border-radius: 12px 12px 0 0; padding: 18px 24px; }
        .modal-header .btn-close { filter: invert(1); opacity: 0.8; }
        .modal-content { border-radius: 12px; border: none; box-shadow: 0 10px 40px rgba(0,0,0,0.15); }
        .modal-body { padding: 24px 24px 8px; }
        .modal-footer { padding: 12px 24px 22px; border: none; }
        .form-label { font-size: 0.78rem; font-weight: 600; color: #546e7a; margin-bottom: 4px; text-transform: uppercase; }
        .form-control { border-radius: 8px; font-size: 0.88rem; padding: 9px 13px; border: 1px solid #dee2e6; }
        .form-control:focus { border-color: #1a2332; box-shadow: 0 0 0 3px rgba(26,35,50,0.1); }
        .btn-save-modal { background-color: #1a2332; color: #fff; border-radius: 8px; padding: 9px 26px; font-size: 0.88rem; border: none; }
        .btn-save-modal:hover { background-color: #2e3f5c; color: #fff; }
        .btn-cancel-modal { background-color: #fff; color: #546e7a; border: 1px solid #cfd8dc; border-radius: 8px; padding: 9px 18px; font-size: 0.88rem; }

        .alert-success { background-color: #e8f5e9; color: #2e7d32; border: none; border-radius: 8px; font-size: 0.87rem; }

        .empty-row td { text-align: center; padding: 50px; color: #90a4ae; font-size: 0.9rem; }
    </style>
</head>
<body>

<nav class="navbar navbar-expand-lg">
    <a class="navbar-brand" href="Default.aspx">Kumari <span>Cinemas</span></a>
    <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navMenu">
        <span class="navbar-toggler-icon" style="filter:invert(1)"></span>
    </button>
    <div class="collapse navbar-collapse" id="navMenu">
        <ul class="navbar-nav ms-2">
            <li class="nav-item"><a class="nav-link" href="Default.aspx"><i class="bi bi-speedometer2 me-1"></i>Dashboard</a></li>
            <li class="nav-item"><a class="nav-link active" href="Customers.aspx"><i class="bi bi-people me-1"></i>Customers</a></li>
            <li class="nav-item"><a class="nav-link" href="Movies.aspx"><i class="bi bi-film me-1"></i>Movies</a></li>
            <li class="nav-item"><a class="nav-link" href="Theaters.aspx"><i class="bi bi-building me-1"></i>Theaters</a></li>
            <li class="nav-item"><a class="nav-link" href="Halls.aspx"><i class="bi bi-door-open me-1"></i>Halls</a></li>
            <li class="nav-item"><a class="nav-link" href="Showtimes.aspx"><i class="bi bi-calendar3 me-1"></i>Showtimes</a></li>
            <li class="nav-item"><a class="nav-link" href="Tickets.aspx"><i class="bi bi-ticket-perforated me-1"></i>Tickets</a></li>
            <li class="nav-item"><a class="nav-link" href="UserTicketReport.aspx"><i class="bi bi-person-lines-fill me-1"></i>User Report</a></li>
            <li class="nav-item"><a class="nav-link" href="TheaterMovieReport.aspx"><i class="bi bi-bar-chart me-1"></i>Theater Report</a></li>
            <li class="nav-item"><a class="nav-link" href="MovieOccupancyReport.aspx"><i class="bi bi-graph-up me-1"></i>Occupancy</a></li>
        </ul>
    </div>
</nav>

<form id="form1" runat="server">
<div class="container-fluid px-4 py-4">

    <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="alert alert-success d-block mb-3"></asp:Label>

    <div class="page-card">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <div>
                <h4 class="page-title mb-0"><i class="bi bi-people-fill me-2"></i>Customers</h4>
                <small class="text-muted">Manage all registered cinema customers</small>
            </div>
            <asp:Button ID="btnShowAdd" runat="server" Text="+ Add Customer"
                CssClass="btn btn-add" OnClick="btnShowAdd_Click" />
        </div>

        <div class="search-wrapper mb-3">
            <i class="bi bi-search"></i>
            <input type="text" id="searchBox" class="form-control search-input"
                placeholder="Search by name, email, phone..." onkeyup="filterTable()" />
        </div>

        <div class="table-responsive">
            <asp:GridView ID="gvCustomers" runat="server"
                CssClass="table table-hover align-middle mb-0"
                AutoGenerateColumns="false"
                DataKeyNames="CUSTOMER_ID"
                OnRowEditing="gvCustomers_RowEditing"
                OnRowDeleting="gvCustomers_RowDeleting"
                OnRowCancelingEdit="gvCustomers_RowCancelingEdit"
                GridLines="None">
                <Columns>
                    <asp:TemplateField HeaderText="ID">
                        <ItemTemplate>
                            <span class="badge-id">#<%# Eval("CUSTOMER_ID") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Name">
                        <ItemTemplate>
                            <div class="fw-semibold"><%# Eval("CUSTOMER_NAME") %></div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Email">
                        <ItemTemplate>
                            <i class="bi bi-envelope text-muted me-1"></i><%# Eval("CUSTOMER_EMAIL") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Phone">
                        <ItemTemplate>
                            <i class="bi bi-telephone text-muted me-1"></i><%# Eval("CUSTOMER_PHONE") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Registered On">
                        <ItemTemplate>
                            <i class="bi bi-calendar2 text-muted me-1"></i>
                            <%# Convert.ToDateTime(Eval("CUSTOMER_REGISTRATION_DATE")).ToString("dd MMM yyyy") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:Button runat="server" Text="Edit" CssClass="btn btn-edit-row me-1"
                                CommandName="Edit" />
                            <asp:Button runat="server" Text="Delete" CssClass="btn btn-delete-row"
                                CommandName="Delete"
                                OnClientClick="return confirm('Delete this customer?');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <tr class="empty-row"><td colspan="6"><i class="bi bi-people d-block mb-2" style="font-size:2rem"></i>No customers found.</td></tr>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>

</div>

<div class="modal fade" id="customerModal" tabindex="-1" aria-hidden="true" data-bs-backdrop="static">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title fw-bold">
                    <i class="bi bi-person-plus-fill me-2"></i>
                    <asp:Label ID="lblModalTitle" runat="server" Text="Add Customer"></asp:Label>
                </h5>
                <asp:Button ID="btnCancel" runat="server" Text="x" CssClass="btn-close"
                    OnClick="btnCancel_Click" />
            </div>
            <div class="modal-body">
                <asp:HiddenField ID="hfCustomerId" runat="server" Value="0" />
                <div class="mb-3">
                    <label class="form-label">Customer Name</label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control"
                        placeholder="Enter full name"></asp:TextBox>
                </div>
                <div class="mb-3">
                    <label class="form-label">Email Address</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"
                        TextMode="Email" placeholder="example@email.com"></asp:TextBox>
                </div>
                <div class="mb-3">
                    <label class="form-label">Phone Number</label>
                    <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control"
                        placeholder="+977-98XXXXXXXX"></asp:TextBox>
                </div>
                <div class="mb-3">
                    <label class="form-label">Registration Date</label>
                    <asp:TextBox ID="txtRegDate" runat="server" CssClass="form-control"
                        TextMode="Date"></asp:TextBox>
                </div>
            </div>
            <div class="modal-footer">
                <asp:Button ID="btnCancelFooter" runat="server" Text="Cancel"
                    CssClass="btn btn-cancel-modal" OnClick="btnCancel_Click" />
                <asp:Button ID="btnSave" runat="server" Text="Save Customer"
                    CssClass="btn btn-save-modal" OnClick="btnSave_Click" />
            </div>
        </div>
    </div>
</div>

</form>

<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
<script>
    function filterTable() {
        var input = document.getElementById("searchBox").value.toLowerCase();
        var rows = document.querySelectorAll(".table tbody tr");
        rows.forEach(function(row) {
            row.style.display = row.innerText.toLowerCase().includes(input) ? "" : "none";
        });
    }

    window.onload = function () {
        var alert = document.querySelector('.alert-success');
        if (alert) {
            setTimeout(function () {
                alert.style.transition = 'opacity 0.5s';
                alert.style.opacity = '0';
                setTimeout(function () { alert.style.display = 'none'; }, 500);
            }, 3000);
        }
        var showModal = <%=ShowModal.ToString().ToLower()%>;
        if (showModal) {
            var modal = new bootstrap.Modal(document.getElementById('customerModal'));
            modal.show();
        }
    };
</script>
</body>
</html>
