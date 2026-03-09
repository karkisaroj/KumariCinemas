<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Customers.aspx.cs" Inherits="KumariCinemas.Customers" EnableEventValidation="false" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8"/><meta name="viewport" content="width=device-width, initial-scale=1"/>
    <title>Customers - Kumari Cinemas</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet"/>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet"/>
    <style>
        body{background:#ffffff;color:#1e293b}
        .kc-navbar{background-color:#ffffff!important;padding:0 8px;border-bottom:1px solid #e2e8f0;box-shadow:0 1px 3px rgba(0,0,0,.04)}.kc-brand{color:#1e293b!important;font-weight:700;font-size:1.15rem}.kc-brand span{color:#4f46e5}
        .kc-link{color:#64748b!important;font-size:.81rem;padding:18px 9px!important;border-bottom:2px solid transparent;transition:color .2s}
        .kc-link:hover,.kc-link.active{color:#1e293b!important;border-bottom:2px solid #4f46e5}
        .page-card{background:#fff;border:1px solid #e2e8f0;border-radius:12px;box-shadow:0 1px 3px rgba(0,0,0,.04);padding:28px}
        .page-title{font-size:1.35rem;font-weight:700;color:#1e293b}
        .table thead th{background-color:#4f46e5;color:#fff;font-size:.74rem;text-transform:uppercase;letter-spacing:.5px;border:none;padding:13px 16px}
        .table tbody tr:hover{background-color:#f8fafc}.table td{vertical-align:middle;font-size:.88rem;padding:11px 16px}
        .badge-id{background:#eef2ff;color:#4f46e5;font-weight:600;padding:4px 10px;border-radius:20px;font-size:.77rem}
        .search-wrapper{position:relative;display:inline-block}
        .search-wrapper i{position:absolute;left:12px;top:50%;transform:translateY(-50%);color:#94a3b8;z-index:1}
        .search-input{padding-left:36px!important;border-radius:8px;width:300px;font-size:.87rem}
        .btn-add{background-color:#4f46e5;color:#fff;border-radius:8px;padding:9px 20px;font-size:.87rem;border:none}.btn-add:hover{background-color:#4338ca;color:#fff}
        .btn-edit-row{background:#eef2ff;color:#4f46e5;border:none;border-radius:6px;padding:5px 11px;font-size:.79rem}.btn-edit-row:hover{background:#e0e7ff}
        .btn-delete-row{background:#fef2f2;color:#dc2626;border:none;border-radius:6px;padding:5px 11px;font-size:.79rem}.btn-delete-row:hover{background:#fecaca}
        .modal-header{background-color:#4f46e5;color:#fff;border-radius:12px 12px 0 0;padding:18px 24px}
        .modal-header .btn-close{filter:invert(1);opacity:.8}
        .modal-content{border-radius:12px;border:none;box-shadow:0 10px 40px rgba(0,0,0,.12)}
        .modal-body{padding:24px 24px 8px}.modal-footer{padding:12px 24px 22px;border:none}
        .form-label{font-size:.78rem;font-weight:600;color:#64748b;margin-bottom:4px;text-transform:uppercase}
        .form-control{border-radius:8px;font-size:.88rem;padding:9px 13px}
        .form-control:focus{border-color:#4f46e5;box-shadow:0 0 0 3px rgba(79,70,229,.12)}
        .btn-save-modal{background-color:#4f46e5;color:#fff;border-radius:8px;padding:9px 26px;font-size:.88rem;border:none}.btn-save-modal:hover{background-color:#4338ca;color:#fff}
        .btn-cancel-modal{background:#fff;color:#64748b;border:1px solid #e2e8f0;border-radius:8px;padding:9px 18px;font-size:.88rem}
    </style>
</head>
<body>
<nav class="navbar navbar-expand-lg kc-navbar">
    <div class="container-fluid px-3">
        <a class="navbar-brand kc-brand" href="Default.aspx">Kumari <span>Cinemas</span></a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navMenu">
            <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navMenu">
            <ul class="navbar-nav ms-auto gap-1">
                <li class="nav-item"><a class="nav-link kc-link" href="Default.aspx"><i class="bi bi-speedometer2 me-1"></i>Dashboard</a></li>
                <li class="nav-item"><a class="nav-link kc-link active" href="Customers.aspx"><i class="bi bi-people me-1"></i>Customers</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Movies.aspx"><i class="bi bi-film me-1"></i>Movies</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Theaters.aspx"><i class="bi bi-building me-1"></i>Theaters</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Halls.aspx"><i class="bi bi-door-open me-1"></i>Halls</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Showtimes.aspx"><i class="bi bi-calendar3 me-1"></i>Showtimes</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Tickets.aspx"><i class="bi bi-ticket-perforated me-1"></i>Tickets</a></li>
                <li class="nav-item dropdown">
                    <a class="nav-link kc-link dropdown-toggle" href="#" data-bs-toggle="dropdown"><i class="bi bi-bar-chart me-1"></i>Reports</a>
                    <ul class="dropdown-menu dropdown-menu-end">
                        <li><a class="dropdown-item" href="UserTicketReport.aspx"><i class="bi bi-person-lines-fill me-2"></i>User Ticket Report</a></li>
                        <li><a class="dropdown-item" href="TheaterMovieReport.aspx"><i class="bi bi-building me-2"></i>Theater Movie Report</a></li>
                        <li><a class="dropdown-item" href="MovieOccupancyReport.aspx"><i class="bi bi-graph-up me-2"></i>Movie Occupancy Report</a></li>
                    </ul>
                </li>
            </ul>
        </div>
    </div>
</nav>
<form id="form1" runat="server">
<div class="container-fluid px-4 py-4">
    <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="alert d-block mb-3"></asp:Label>
    <div class="page-card">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <div><h4 class="page-title mb-0"><i class="bi bi-people-fill me-2"></i>Customers</h4><small class="text-muted">Manage all registered cinema customers</small></div>
            <asp:Button ID="btnShowAdd" runat="server" Text="+ Add Customer" CssClass="btn btn-add" OnClick="btnShowAdd_Click"/>
        </div>
        <div class="search-wrapper mb-3">
            <i class="bi bi-search"></i>
            <input type="text" id="searchBox" class="form-control search-input" placeholder="Search customers..." onkeyup="filterTable()"/>
        </div>
        <div class="table-responsive">
            <asp:GridView ID="gvCustomers" runat="server" CssClass="table table-hover align-middle mb-0"
                AutoGenerateColumns="false" DataKeyNames="CUSTOMER_ID" GridLines="None"
                OnRowEditing="gvCustomers_RowEditing" OnRowDeleting="gvCustomers_RowDeleting"
                OnRowCancelingEdit="gvCustomers_RowCancelingEdit">
                <Columns>
                    <asp:TemplateField HeaderText="ID"><ItemTemplate><span class="badge-id">#<%# Eval("CUSTOMER_ID") %></span></ItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Name"><ItemTemplate><div class="fw-semibold"><%# Eval("CUSTOMER_NAME") %></div></ItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Email"><ItemTemplate><i class="bi bi-envelope text-muted me-1"></i><%# Eval("CUSTOMER_EMAIL") %></ItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Phone"><ItemTemplate><i class="bi bi-telephone text-muted me-1"></i><%# Eval("CUSTOMER_PHONE") %></ItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Registered On"><ItemTemplate><i class="bi bi-calendar2 text-muted me-1"></i><%# Convert.ToDateTime(Eval("CUSTOMER_REGISTRATION_DATE")).ToString("dd MMM yyyy") %></ItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:Button runat="server" Text="Edit" CssClass="btn btn-edit-row me-1" CommandName="Edit"/>
                            <asp:Button runat="server" Text="Delete" CssClass="btn btn-delete-row" CommandName="Delete" OnClientClick="return confirm('Delete this customer?');"/>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="text-center text-muted py-5">No customers found.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>
</div>
<div class="modal fade" id="customerModal" tabindex="-1" data-bs-backdrop="static">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title fw-bold"><i class="bi bi-person-plus-fill me-2"></i><asp:Label ID="lblModalTitle" runat="server" Text="Add Customer"></asp:Label></h5>
                <asp:Button ID="btnCancel" runat="server" Text="x" CssClass="btn-close" OnClick="btnCancel_Click"/>
            </div>
            <div class="modal-body">
                <asp:HiddenField ID="hfCustomerId" runat="server" Value="0"/>
                <div class="mb-3"><label class="form-label">Customer Name</label><asp:TextBox ID="txtName" runat="server" CssClass="form-control" placeholder="Enter full name"></asp:TextBox></div>
                <div class="mb-3"><label class="form-label">Email Address</label><asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="example@email.com"></asp:TextBox></div>
                <div class="mb-3"><label class="form-label">Phone Number</label><asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" placeholder="+977-98XXXXXXXX"></asp:TextBox></div>
                <div class="mb-3"><label class="form-label">Registration Date</label><asp:TextBox ID="txtRegDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox></div>
            </div>
            <div class="modal-footer">
                <asp:Button ID="btnCancelFooter" runat="server" Text="Cancel" CssClass="btn btn-cancel-modal" OnClick="btnCancel_Click"/>
                <asp:Button ID="btnSave" runat="server" Text="Save Customer" CssClass="btn btn-save-modal" OnClick="btnSave_Click"/>
            </div>
        </div>
    </div>
</div>
</form>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
<script>
    function filterTable(){var i=document.getElementById("searchBox").value.toLowerCase();document.querySelectorAll(".table tbody tr").forEach(function(r){r.style.display=r.innerText.toLowerCase().includes(i)?"":"none"});}
    window.onload = function () {
        var a = document.querySelector('.alert');
        if (a && a.innerText.trim() !== '') {
            setTimeout(function () { a.style.transition = 'opacity .5s'; a.style.opacity = '0'; setTimeout(function () { a.style.display = 'none'; }, 500); }, 3000);
        }
        if (<%= ShowModal.ToString().ToLower() %>) {
            new bootstrap.Modal(document.getElementById('customerModal')).show();
        }
    };
</script>
</body></html>