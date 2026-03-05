<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Halls.aspx.cs" Inherits="KumariCinemas.Halls" EnableEventValidation="false" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8"/><meta name="viewport" content="width=device-width, initial-scale=1"/>
    <title>Halls - Kumari Cinemas</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet"/>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet"/>
    <style>
        body{background:#f4f6f9}
        .kc-navbar{background-color:#1a2332!important;padding:0 8px}.kc-brand{color:#fff!important;font-weight:700;font-size:1.15rem}.kc-brand span{color:#f0a500}
        .kc-link{color:rgba(255,255,255,.75)!important;font-size:.81rem;padding:18px 9px!important;border-bottom:2px solid transparent;transition:color .2s}
        .kc-link:hover,.kc-link.active{color:#fff!important;border-bottom:2px solid #f0a500}
        .page-card{background:#fff;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,.07);padding:28px}
        .page-title{font-size:1.35rem;font-weight:700;color:#1a2332}
        .table thead th{background-color:#1a2332;color:#fff;font-size:.74rem;text-transform:uppercase;letter-spacing:.5px;border:none;padding:13px 16px}
        .table tbody tr:hover{background-color:#fff8e1}.table td{vertical-align:middle;font-size:.88rem;padding:11px 16px}
        .badge-id{background:#fff3e0;color:#e65100;font-weight:600;padding:4px 10px;border-radius:20px;font-size:.77rem}
        .badge-type{background:#e3f2fd;color:#1565c0;padding:3px 9px;border-radius:20px;font-size:.75rem;font-weight:600}
        .search-wrapper{position:relative;display:inline-block}
        .search-wrapper i{position:absolute;left:12px;top:50%;transform:translateY(-50%);color:#9e9e9e;z-index:1}
        .search-input{padding-left:36px!important;border-radius:8px;width:300px;font-size:.87rem}
        .btn-add{background-color:#1a2332;color:#fff;border-radius:8px;padding:9px 20px;font-size:.87rem;border:none}.btn-add:hover{background-color:#2e3f5c;color:#fff}
        .btn-edit-row{background:#e3f2fd;color:#1565c0;border:none;border-radius:6px;padding:5px 11px;font-size:.79rem}.btn-edit-row:hover{background:#bbdefb}
        .btn-delete-row{background:#fce4ec;color:#c62828;border:none;border-radius:6px;padding:5px 11px;font-size:.79rem}.btn-delete-row:hover{background:#ef9a9a}
        .modal-header{background-color:#1a2332;color:#fff;border-radius:12px 12px 0 0;padding:18px 24px}
        .modal-header .btn-close{filter:invert(1);opacity:.8}
        .modal-content{border-radius:12px;border:none;box-shadow:0 10px 40px rgba(0,0,0,.15)}
        .modal-body{padding:24px 24px 8px}.modal-footer{padding:12px 24px 22px;border:none}
        .form-label{font-size:.78rem;font-weight:600;color:#546e7a;margin-bottom:4px;text-transform:uppercase}
        .form-control,.form-select{border-radius:8px;font-size:.88rem;padding:9px 13px}
        .form-control:focus,.form-select:focus{border-color:#1a2332;box-shadow:0 0 0 3px rgba(26,35,50,.1)}
        .btn-save-modal{background-color:#1a2332;color:#fff;border-radius:8px;padding:9px 26px;font-size:.88rem;border:none}.btn-save-modal:hover{background-color:#2e3f5c;color:#fff}
        .btn-cancel-modal{background:#fff;color:#546e7a;border:1px solid #cfd8dc;border-radius:8px;padding:9px 18px;font-size:.88rem}
    </style>
</head>
<body>
<nav class="navbar navbar-expand-lg kc-navbar">
    <div class="container-fluid px-3">
        <a class="navbar-brand kc-brand" href="Default.aspx">Kumari <span>Cinemas</span></a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navMenu">
            <span class="navbar-toggler-icon" style="filter:invert(1)"></span>
        </button>
        <div class="collapse navbar-collapse" id="navMenu">
            <ul class="navbar-nav ms-auto gap-1">
                <li class="nav-item"><a class="nav-link kc-link" href="Default.aspx"><i class="bi bi-speedometer2 me-1"></i>Dashboard</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Customers.aspx"><i class="bi bi-people me-1"></i>Customers</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Movies.aspx"><i class="bi bi-film me-1"></i>Movies</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Theaters.aspx"><i class="bi bi-building me-1"></i>Theaters</a></li>
                <li class="nav-item"><a class="nav-link kc-link active" href="Halls.aspx"><i class="bi bi-door-open me-1"></i>Halls</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Showtimes.aspx"><i class="bi bi-calendar3 me-1"></i>Showtimes</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Tickets.aspx"><i class="bi bi-ticket-perforated me-1"></i>Tickets</a></li>
            </ul>
        </div>
    </div>
</nav>
<form id="form1" runat="server">
<div class="container-fluid px-4 py-4">
    <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="alert d-block mb-3"></asp:Label>
    <div class="page-card">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <div><h4 class="page-title mb-0"><i class="bi bi-door-open me-2"></i>Halls</h4><small class="text-muted">Manage all screening halls</small></div>
            <asp:Button ID="btnShowAdd" runat="server" Text="+ Add Hall" CssClass="btn btn-add" OnClick="btnShowAdd_Click"/>
        </div>
        <div class="search-wrapper mb-3"><i class="bi bi-search"></i><input type="text" id="searchBox" class="form-control search-input" placeholder="Search halls..." onkeyup="filterTable()"/></div>
        <div class="table-responsive">
            <asp:GridView ID="gvHalls" runat="server" CssClass="table table-hover align-middle mb-0"
                AutoGenerateColumns="false" DataKeyNames="HALL_ID" GridLines="None"
                OnRowEditing="gvHalls_RowEditing" OnRowDeleting="gvHalls_RowDeleting"
                OnRowCancelingEdit="gvHalls_RowCancelingEdit">
                <Columns>
                    <asp:TemplateField HeaderText="ID"><ItemTemplate><span class="badge-id">#<%# Eval("HALL_ID") %></span></ItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Hall Name"><ItemTemplate><div class="fw-semibold"><%# Eval("HALL_NAME") %></div></ItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Capacity"><ItemTemplate><i class="bi bi-people text-muted me-1"></i><%# Eval("HALL_CAPACITY") %> seats</ItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Type"><ItemTemplate><span class="badge-type"><%# Eval("HALL_TYPE") %></span></ItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:Button runat="server" Text="Edit" CssClass="btn btn-edit-row me-1" CommandName="Edit"/>
                            <asp:Button runat="server" Text="Delete" CssClass="btn btn-delete-row" CommandName="Delete" OnClientClick="return confirm('Delete this hall?');"/>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="text-center text-muted py-5">No halls found.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>
</div>
<div class="modal fade" id="hallModal" tabindex="-1" data-bs-backdrop="static">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title fw-bold"><i class="bi bi-door-open me-2"></i><asp:Label ID="lblModalTitle" runat="server" Text="Add Hall"></asp:Label></h5>
                <asp:Button ID="btnCancel" runat="server" Text="x" CssClass="btn-close" OnClick="btnCancel_Click"/>
            </div>
            <div class="modal-body">
                <asp:HiddenField ID="hfHallId" runat="server" Value="0"/>
                <div class="mb-3"><label class="form-label">Hall Name</label><asp:TextBox ID="txtName" runat="server" CssClass="form-control" placeholder="e.g. Hall A"></asp:TextBox></div>
                <div class="mb-3"><label class="form-label">Capacity (seats)</label><asp:TextBox ID="txtCapacity" runat="server" CssClass="form-control" TextMode="Number" placeholder="e.g. 150"></asp:TextBox></div>
                <div class="mb-3"><label class="form-label">Hall Type</label>
                    <asp:DropDownList ID="ddlType" runat="server" CssClass="form-select">
                        <asp:ListItem Value="">-- Select Type --</asp:ListItem>
                        <asp:ListItem>Standard</asp:ListItem><asp:ListItem>IMAX</asp:ListItem>
                        <asp:ListItem>4DX</asp:ListItem><asp:ListItem>Dolby</asp:ListItem><asp:ListItem>VIP</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
            <div class="modal-footer">
                <asp:Button ID="btnCancelFooter" runat="server" Text="Cancel" CssClass="btn btn-cancel-modal" OnClick="btnCancel_Click"/>
                <asp:Button ID="btnSave" runat="server" Text="Save Hall" CssClass="btn btn-save-modal" OnClick="btnSave_Click"/>
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
        if (a && a.innerText.trim() !== '') { setTimeout(function () { a.style.transition = 'opacity .5s'; a.style.opacity = '0'; setTimeout(function () { a.style.display = 'none'; }, 500); }, 3000); }
        if (<%= ShowModal.ToString().ToLower() %>) { new bootstrap.Modal(document.getElementById('hallModal')).show(); }
    };
</script>
</body></html>