<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Tickets.aspx.cs" Inherits="KumariCinemas.Tickets" EnableEventValidation="false" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8"/><meta name="viewport" content="width=device-width, initial-scale=1"/>
    <title>Tickets - Kumari Cinemas</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet"/>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet"/>
    <style>
        body{background:#ffffff;color:#1e293b}
        .kc-navbar{background-color:#ffffff!important;padding:0 8px;border-bottom:1px solid #e2e8f0;box-shadow:0 1px 3px rgba(0,0,0,.04)}
        .kc-brand{color:#1e293b!important;font-weight:700;font-size:1.15rem}
        .kc-brand span{color:#4f46e5}
        .kc-link{color:#64748b!important;font-size:.81rem;padding:18px 9px!important;border-bottom:2px solid transparent;transition:color .2s}
        .kc-link:hover,.kc-link.active{color:#1e293b!important;border-bottom:2px solid #4f46e5}
        .page-card{background:#fff;border:1px solid #e2e8f0;border-radius:12px;box-shadow:0 1px 3px rgba(0,0,0,.04);padding:28px}
        .page-title{font-size:1.35rem;font-weight:700;color:#1e293b}
        .table thead th{background-color:#4f46e5;color:#fff;font-size:.74rem;text-transform:uppercase;letter-spacing:.5px;border:none;padding:13px 16px}
        .table tbody tr:hover{background-color:#f8fafc}
        .table td{vertical-align:middle;font-size:.88rem;padding:11px 16px}
        .badge-id{background:#ede9fe;color:#7c3aed;font-weight:600;padding:4px 10px;border-radius:20px;font-size:.77rem}
        .badge-booked{background:#fff7ed;color:#ea580c;padding:3px 10px;border-radius:20px;font-size:.75rem;font-weight:600}
        .badge-purchased{background:#ecfdf5;color:#059669;padding:3px 10px;border-radius:20px;font-size:.75rem;font-weight:600}
        .badge-cancelled{background:#fef2f2;color:#dc2626;padding:3px 10px;border-radius:20px;font-size:.75rem;font-weight:600}
        .badge-auto-cancelled{background:#ede9fe;color:#7c3aed;padding:3px 10px;border-radius:20px;font-size:.75rem;font-weight:600}
        .badge-used{background:#eef2ff;color:#4f46e5;padding:3px 10px;border-radius:20px;font-size:.75rem;font-weight:600}
        .search-wrapper{position:relative;display:inline-block}
        .search-wrapper i{position:absolute;left:12px;top:50%;transform:translateY(-50%);color:#94a3b8;z-index:1}
        .search-input{padding-left:36px!important;border-radius:8px;width:300px;font-size:.87rem}
        .btn-add{background-color:#4f46e5;color:#fff;border-radius:8px;padding:9px 20px;font-size:.87rem;border:none}
        .btn-add:hover{background-color:#4338ca;color:#fff}
        .btn-confirm-purchase{background:#ecfdf5;color:#059669;border:none;border-radius:6px;padding:5px 11px;font-size:.79rem}
        .btn-confirm-purchase:hover{background:#d1fae5}
        .btn-cancel-ticket{background:#fef2f2;color:#dc2626;border:none;border-radius:6px;padding:5px 11px;font-size:.79rem}
        .btn-cancel-ticket:hover{background:#fecaca}
        .btn-delete-row{background:#fef2f2;color:#dc2626;border:none;border-radius:6px;padding:5px 11px;font-size:.79rem}
        .btn-delete-row:hover{background:#fecaca}
        .modal-header{background-color:#4f46e5;color:#fff;border-radius:12px 12px 0 0;padding:18px 24px}
        .modal-header .btn-close{filter:invert(1);opacity:.8}
        .modal-content{border-radius:12px;border:none;box-shadow:0 10px 40px rgba(0,0,0,.12)}
        .modal-body{padding:24px 24px 8px}
        .modal-footer{padding:12px 24px 22px;border:none}
        .form-label{font-size:.78rem;font-weight:600;color:#64748b;margin-bottom:4px;text-transform:uppercase}
        .form-control,.form-select{border-radius:8px;font-size:.88rem;padding:9px 13px}
        .form-control:focus,.form-select:focus{border-color:#4f46e5;box-shadow:0 0 0 3px rgba(79,70,229,.12)}
        .btn-save-modal{background-color:#4f46e5;color:#fff;border-radius:8px;padding:9px 26px;font-size:.88rem;border:none}
        .btn-save-modal:hover{background-color:#4338ca;color:#fff}
        .btn-cancel-modal{background:#fff;color:#64748b;border:1px solid #e2e8f0;border-radius:8px;padding:9px 18px;font-size:.88rem}
        .auto-fill-box{background:#f8fafc;border:1px solid #e2e8f0;border-radius:8px;padding:9px 13px;font-size:.88rem;color:#1e293b;min-height:38px}
        .auto-fill-placeholder{color:#94a3b8;font-style:italic}
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
                <li class="nav-item"><a class="nav-link kc-link" href="Customers.aspx"><i class="bi bi-people me-1"></i>Customers</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Movies.aspx"><i class="bi bi-film me-1"></i>Movies</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Theaters.aspx"><i class="bi bi-building me-1"></i>Theaters</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Halls.aspx"><i class="bi bi-door-open me-1"></i>Halls</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Showtimes.aspx"><i class="bi bi-calendar3 me-1"></i>Showtimes</a></li>
                <li class="nav-item"><a class="nav-link kc-link active" href="Tickets.aspx"><i class="bi bi-ticket-perforated me-1"></i>Tickets</a></li>
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
            <div>
                <h4 class="page-title mb-0"><i class="bi bi-ticket-perforated me-2"></i>Tickets</h4>
                <small class="text-muted">Manage all ticket bookings</small>
            </div>
            <asp:Button ID="btnShowAdd" runat="server" Text="+ Book Ticket" CssClass="btn btn-add" OnClick="btnShowAdd_Click"/>
        </div>

        <div class="search-wrapper mb-3">
            <i class="bi bi-search"></i>
            <input type="text" id="searchBox" class="form-control search-input" placeholder="Search tickets..." onkeyup="filterTable()"/>
        </div>

        <div class="table-responsive">
            <asp:GridView ID="gvTickets" runat="server" CssClass="table table-hover align-middle mb-0"
                AutoGenerateColumns="false" DataKeyNames="TICKET_ID" GridLines="None"
                OnRowDeleting="gvTickets_RowDeleting"
                OnRowCommand="gvTickets_RowCommand"
                OnRowEditing="gvTickets_RowEditing"
                OnRowCancelingEdit="gvTickets_RowCancelingEdit">
                <Columns>
                    <asp:TemplateField HeaderText="ID">
                        <ItemTemplate><span class="badge-id">#<%# Eval("TICKET_ID") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Customer">
                        <ItemTemplate><div class="fw-semibold"><%# Eval("CUSTOMER_NAME") %></div></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Movie">
                        <ItemTemplate><i class="bi bi-film text-muted me-1"></i><%# Eval("MOVIE_TITLE") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Theater">
                        <ItemTemplate><i class="bi bi-building text-muted me-1"></i><%# Eval("THEATER_NAME") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Hall">
                        <ItemTemplate><i class="bi bi-door-open text-muted me-1"></i><%# Eval("HALL_NAME") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Seat">
                        <ItemTemplate><span class="fw-bold"><%# Eval("SEAT_NO") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Price">
                        <ItemTemplate>Rs. <%# Eval("TICKET_PRICE") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span class='<%# 
                                Eval("TICKET_STATUS").ToString()=="Booked"         ? "badge-booked" :
                                Eval("TICKET_STATUS").ToString()=="Purchased"      ? "badge-purchased" :
                                Eval("TICKET_STATUS").ToString()=="Auto-Cancelled" ? "badge-auto-cancelled" :
                                Eval("TICKET_STATUS").ToString()=="Cancelled"      ? "badge-cancelled" : "badge-used" 
                            %>'><%# Eval("TICKET_STATUS") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Booked On">
                        <ItemTemplate>
                            <small class="text-muted"><%# Convert.ToDateTime(Eval("BOOKING_TIME")).ToString("dd MMM yy HH:mm") %></small>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:Button runat="server" Text="✓ Confirm Payment" CssClass="btn btn-confirm-purchase"
                                CommandName="ConfirmPurchase" CommandArgument='<%# Eval("TICKET_ID") %>'
                                OnClientClick="return confirm('Confirm payment received for this ticket?');"
                                Visible='<%# Eval("TICKET_STATUS").ToString()=="Booked" %>'/>
                            <asp:Button runat="server" Text="Cancel" CssClass="btn btn-cancel-ticket ms-1"
                                CommandName="CancelTicket" CommandArgument='<%# Eval("TICKET_ID") %>'
                                OnClientClick="return confirm('Cancel this ticket?');"
                                Visible='<%# Eval("TICKET_STATUS").ToString()=="Booked" %>'/>
                            <asp:Button runat="server" Text="Delete" CssClass="btn btn-delete-row ms-1"
                                CommandName="Delete" OnClientClick="return confirm('Delete this ticket permanently?');"/>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="text-center text-muted py-5">No tickets found.</div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>
</div>

<div class="modal fade" id="ticketModal" tabindex="-1" data-bs-backdrop="static">
    <div class="modal-dialog modal-dialog-centered modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title fw-bold"><i class="bi bi-ticket-perforated me-2"></i>Book New Ticket</h5>
                <asp:Button ID="btnCancel" runat="server" Text="x" CssClass="btn-close" OnClick="btnCancel_Click"/>
            </div>
            <div class="modal-body">

                <asp:HiddenField ID="hiddenMovieId"   runat="server" Value="0" />
                <asp:HiddenField ID="hiddenHallId"    runat="server" Value="0" />
                <asp:HiddenField ID="hiddenTheaterId" runat="server" Value="0" />

                <div class="row">

                    <div class="col-12 mb-3">
                        <label class="form-label">
                            <i class="bi bi-calendar3 me-1"></i>Showtime
                            <small class="text-muted fw-normal ms-1">(Movie, Hall &amp; Theater will fill automatically)</small>
                        </label>
                        <asp:DropDownList ID="dropShowtime" runat="server"
                            CssClass="form-select"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="dropShowtime_SelectedIndexChanged">
                        </asp:DropDownList>
                    </div>

                    <div class="col-md-4 mb-3">
                        <label class="form-label"><i class="bi bi-film me-1"></i>Movie</label>
                        <div class="auto-fill-box">
                            <asp:Label ID="labelMovie" runat="server">
                                <span class="auto-fill-placeholder">Auto-filled</span>
                            </asp:Label>
                        </div>
                    </div>

                    <div class="col-md-4 mb-3">
                        <label class="form-label"><i class="bi bi-door-open me-1"></i>Hall</label>
                        <div class="auto-fill-box">
                            <asp:Label ID="labelHall" runat="server">
                                <span class="auto-fill-placeholder">Auto-filled</span>
                            </asp:Label>
                        </div>
                    </div>

                    <div class="col-md-4 mb-3">
                        <label class="form-label"><i class="bi bi-building me-1"></i>Theater</label>
                        <div class="auto-fill-box">
                            <asp:Label ID="labelTheater" runat="server">
                                <span class="auto-fill-placeholder">Auto-filled</span>
                            </asp:Label>
                        </div>
                    </div>

                    <div class="col-md-6 mb-3">
                        <label class="form-label"><i class="bi bi-person me-1"></i>Customer</label>
                        <asp:DropDownList ID="dropCustomer" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>

                    <div class="col-md-3 mb-3">
                        <label class="form-label"><i class="bi bi-grid me-1"></i>Seat No</label>
                        <asp:TextBox ID="txtSeat" runat="server" CssClass="form-control" placeholder="e.g. 12"></asp:TextBox>
                    </div>

                    <div class="col-md-3 mb-3">
                        <label class="form-label"><i class="bi bi-currency-rupee me-1"></i>Price (Rs.)</label>
                        <asp:TextBox ID="txtPrice" runat="server" CssClass="form-control" TextMode="Number" placeholder="e.g. 500"></asp:TextBox>
                    </div>

                </div>
            </div>
            <div class="modal-footer">
                <asp:Button ID="btnCancelFooter" runat="server" Text="Cancel" CssClass="btn btn-cancel-modal" OnClick="btnCancel_Click"/>
                <asp:Button ID="btnSave" runat="server" Text="Book Ticket" CssClass="btn btn-save-modal" OnClick="btnSave_Click"/>
            </div>
        </div>
    </div>
</div>

</form>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
<script>
    function filterTable() {
        var searchText = document.getElementById("searchBox").value.toLowerCase();
        document.querySelectorAll(".table tbody tr").forEach(function (row) {
            row.style.display = row.innerText.toLowerCase().includes(searchText) ? "" : "none";
        });
    }

    window.onload = function () {
        var alertBox = document.querySelector('.alert');
        if (alertBox && alertBox.innerText.trim() !== '') {
            setTimeout(function () {
                alertBox.style.transition = 'opacity .5s';
                alertBox.style.opacity = '0';
                setTimeout(function () { alertBox.style.display = 'none'; }, 500);
            }, 3000);
        }
        if (<%= ShowModal.ToString().ToLower() %>) {
            new bootstrap.Modal(document.getElementById('ticketModal')).show();
        }
    };
</script>
</body>
</html>