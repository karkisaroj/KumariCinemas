<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TheaterMovieReport.aspx.cs" Inherits="KumariCinemas.TheaterMovieReport" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8"/><meta name="viewport" content="width=device-width, initial-scale=1"/>
    <title>Theater Movie Report - Kumari Cinemas</title>
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
        .badge-id{background:#ecfdf5;color:#059669;font-weight:600;padding:4px 10px;border-radius:20px;font-size:.77rem}
        .badge-genre{background:#fefce8;color:#ca8a04;padding:3px 9px;border-radius:20px;font-size:.75rem;font-weight:600}
        .form-label{font-size:.78rem;font-weight:600;color:#64748b;margin-bottom:4px;text-transform:uppercase}
        .form-select{border-radius:8px;font-size:.88rem;padding:9px 13px}
        .form-select:focus{border-color:#4f46e5;box-shadow:0 0 0 3px rgba(79,70,229,.12)}
        .btn-search{background-color:#4f46e5;color:#fff;border-radius:8px;padding:9px 24px;font-size:.87rem;border:none}.btn-search:hover{background-color:#4338ca;color:#fff}
        .detail-card{background:#f8fafc;border:1px solid #e2e8f0;border-radius:10px;padding:20px;margin-bottom:20px}
        .detail-label{font-size:.75rem;color:#94a3b8;font-weight:600;text-transform:uppercase;letter-spacing:.4px}
        .detail-value{font-size:1rem;font-weight:600;color:#1e293b}
    </style>
</head>
<body>
<nav class="navbar navbar-expand-lg kc-navbar">
    <div class="container-fluid px-3">
        <a class="navbar-brand kc-brand" href="Default.aspx">Kumari <span>Cinemas</span></a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navMenu"><span class="navbar-toggler-icon"></span></button>
        <div class="collapse navbar-collapse" id="navMenu">
            <ul class="navbar-nav ms-auto gap-1">
                <li class="nav-item"><a class="nav-link kc-link" href="Default.aspx"><i class="bi bi-speedometer2 me-1"></i>Dashboard</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Customers.aspx"><i class="bi bi-people me-1"></i>Customers</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Movies.aspx"><i class="bi bi-film me-1"></i>Movies</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Theaters.aspx"><i class="bi bi-building me-1"></i>Theaters</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Halls.aspx"><i class="bi bi-door-open me-1"></i>Halls</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Showtimes.aspx"><i class="bi bi-calendar3 me-1"></i>Showtimes</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Tickets.aspx"><i class="bi bi-ticket-perforated me-1"></i>Tickets</a></li>
                <li class="nav-item dropdown">
                    <a class="nav-link kc-link active dropdown-toggle" href="#" data-bs-toggle="dropdown"><i class="bi bi-bar-chart me-1"></i>Reports</a>
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
        <div class="mb-3">
            <h4 class="page-title mb-0"><i class="bi bi-building me-2"></i>Theater / City / Hall — Movie Report</h4>
            <small class="text-muted">Select a theater, city and hall to view associated movies and showtimes</small>
        </div>
        <div class="row align-items-end mb-4">
            <div class="col-md-3">
                <label class="form-label"><i class="bi bi-building me-1"></i>Theater</label>
                <asp:DropDownList ID="ddlTheater" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlTheater_SelectedIndexChanged"></asp:DropDownList>
            </div>
            <div class="col-md-3 mt-2 mt-md-0">
                <label class="form-label"><i class="bi bi-geo-alt me-1"></i>City</label>
                <asp:DropDownList ID="ddlCity" runat="server" CssClass="form-select" Enabled="false"></asp:DropDownList>
            </div>
            <div class="col-md-3 mt-2 mt-md-0">
                <label class="form-label"><i class="bi bi-door-open me-1"></i>Hall</label>
                <asp:DropDownList ID="ddlHall" runat="server" CssClass="form-select"></asp:DropDownList>
            </div>
            <div class="col-md-3 mt-2 mt-md-0">
                <asp:Button ID="btnSearch" runat="server" Text="Generate Report" CssClass="btn btn-search w-100" OnClick="btnSearch_Click"/>
            </div>
        </div>

        <asp:Panel ID="pnlResult" runat="server" Visible="false">
            <div class="detail-card">
                <h5 class="fw-bold mb-3"><i class="bi bi-info-circle me-2"></i>Selection Details</h5>
                <div class="row g-3">
                    <div class="col-md-4"><div class="detail-label">Theater</div><div class="detail-value"><asp:Label ID="lblTheaterName" runat="server"/></div></div>
                    <div class="col-md-4"><div class="detail-label">City</div><div class="detail-value"><asp:Label ID="lblCity" runat="server"/></div></div>
                    <div class="col-md-4"><div class="detail-label">Hall</div><div class="detail-value"><asp:Label ID="lblHallName" runat="server"/></div></div>
                </div>
            </div>

            <h6 class="fw-bold mb-3"><i class="bi bi-film me-2"></i>Movies &amp; Showtimes</h6>
            <div class="table-responsive">
                <asp:GridView ID="gvMovies" runat="server" CssClass="table table-hover align-middle mb-0"
                    AutoGenerateColumns="false" GridLines="None">
                    <Columns>
                        <asp:TemplateField HeaderText="Show ID"><ItemTemplate><span class="badge-id">#<%# Eval("SHOW_ID") %></span></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Movie Title"><ItemTemplate><div class="fw-semibold"><%# Eval("MOVIE_TITLE") %></div></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Genre"><ItemTemplate><span class="badge-genre"><%# Eval("MOVIE_GENRE") %></span></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Duration"><ItemTemplate><i class="bi bi-clock text-muted me-1"></i><%# Eval("MOVIE_DURATION") %> mins</ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Language"><ItemTemplate><i class="bi bi-translate text-muted me-1"></i><%# Eval("MOVIE_LANGUAGE") %></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Show Date"><ItemTemplate><i class="bi bi-calendar2 text-muted me-1"></i><%# Convert.ToDateTime(Eval("SHOW_DATE")).ToString("dd MMM yyyy") %></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Time"><ItemTemplate><i class="bi bi-clock text-muted me-1"></i><%# Eval("SHOW_TIME") %> - <%# Eval("SHOW_END_TIME") %></ItemTemplate></asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate><div class="text-center text-muted py-5">No movies/showtimes found for this Theater-City-Hall combination.</div></EmptyDataTemplate>
                </asp:GridView>
            </div>
        </asp:Panel>
    </div>
</div>
</form>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>