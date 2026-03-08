<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="KumariCinemas.Default" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8"/><meta name="viewport" content="width=device-width, initial-scale=1"/>
    <title>Dashboard - Kumari Cinemas</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet"/>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet"/>
    <style>
        body{background:#f4f6f9}
        .kc-navbar{background-color:#1a2332!important;padding:0 8px}
        .kc-brand{color:#fff!important;font-weight:700;font-size:1.15rem}.kc-brand span{color:#f0a500}
        .kc-link{color:rgba(255,255,255,.75)!important;font-size:.81rem;padding:18px 9px!important;border-bottom:2px solid transparent;transition:color .2s}
        .kc-link:hover,.kc-link.active{color:#fff!important;border-bottom:2px solid #f0a500}
        .hero{background:linear-gradient(135deg,#1a2332 0%,#2e4057 100%);border-radius:16px;padding:48px 40px;color:#fff;margin-bottom:28px;position:relative;overflow:hidden}
        .hero h1{font-size:2rem;font-weight:800;margin-bottom:8px}
        .hero p{color:rgba(255,255,255,.7);font-size:1rem;margin-bottom:24px}
        .hero-btn{background-color:#808080;color:#1a2332;border:none;border-radius:8px;padding:11px 28px;font-weight:700;font-size:.9rem;text-decoration:none;display:inline-block}
        .hero-btn:hover{background-color:#f4f6f9;color:#1a2332}
        .stat-card{background:#fff;border-radius:12px;padding:24px;box-shadow:0 2px 12px rgba(0,0,0,.06);display:flex;align-items:center;gap:18px;transition:transform .2s,box-shadow .2s;text-decoration:none;color:inherit}
        .stat-card:hover{transform:translateY(-3px);box-shadow:0 6px 20px rgba(0,0,0,.1);color:inherit}
        .stat-icon{width:56px;height:56px;border-radius:14px;display:flex;align-items:center;justify-content:center;font-size:1.5rem;flex-shrink:0}
        .stat-label{font-size:.78rem;color:#90a4ae;font-weight:600;text-transform:uppercase;letter-spacing:.4px}
        .stat-value{font-size:1.9rem;font-weight:800;color:#1a2332;line-height:1.1}
        .section-title{font-size:1.05rem;font-weight:700;color:#1a2332;margin-bottom:16px}
        .quick-card{background:#fff;border-radius:12px;padding:20px 18px;box-shadow:0 2px 10px rgba(0,0,0,.05);text-align:center;text-decoration:none;color:#1a2332;transition:transform .2s,box-shadow .2s;display:block}
        .quick-card:hover{transform:translateY(-3px);box-shadow:0 6px 20px rgba(0,0,0,.1);color:#1a2332}
        .quick-card i{font-size:1.8rem;display:block;margin-bottom:8px}
        .quick-card span{font-size:.82rem;font-weight:600}
        .page-card{background:#fff;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,.07);padding:24px}
        .table thead th{background-color:#1a2332;color:#fff;font-size:.74rem;text-transform:uppercase;letter-spacing:.5px;border:none;padding:12px 14px}
        .table td{vertical-align:middle;font-size:.87rem;padding:11px 14px}
        .badge-booked{background:#e8f5e9;color:#2e7d32;padding:3px 10px;border-radius:20px;font-size:.75rem;font-weight:600}
        .badge-cancelled{background:#fce4ec;color:#c62828;padding:3px 10px;border-radius:20px;font-size:.75rem;font-weight:600}
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
                <li class="nav-item"><a class="nav-link kc-link active" href="Default.aspx"><i class="bi bi-speedometer2 me-1"></i>Dashboard</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Customers.aspx"><i class="bi bi-people me-1"></i>Customers</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Movies.aspx"><i class="bi bi-film me-1"></i>Movies</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Theaters.aspx"><i class="bi bi-building me-1"></i>Theaters</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Halls.aspx"><i class="bi bi-door-open me-1"></i>Halls</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Showtimes.aspx"><i class="bi bi-calendar3 me-1"></i>Showtimes</a></li>
                <li class="nav-item"><a class="nav-link kc-link" href="Tickets.aspx"><i class="bi bi-ticket-perforated me-1"></i>Tickets</a></li>
            </ul>
        </div>
    </div>
</nav>
<form id="form1" runat="server">
<div class="container-fluid px-4 py-4">
    <div class="hero">
        <h1>Welcome to Kumari Cinemas</h1>
        <p>Manage customers, movies, theaters, halls, showtimes and ticket bookings all in one place.</p>
        <a href="Tickets.aspx" class="hero-btn"><i class="bi bi-ticket-perforated me-2"></i>Book a Ticket</a>
    </div>
    <div class="row g-3 mb-4">
        <div class="col-6 col-md-4 col-lg-2">
            <a href="Customers.aspx" class="stat-card">
                <div class="stat-icon" style="background:#e8eaf6"><i class="bi bi-people-fill" style="color:#3949ab"></i></div>
                <div><div class="stat-label">Customers</div><div class="stat-value"><asp:Label ID="lblCustomers" runat="server" Text="0"/></div></div>
            </a>
        </div>
        <div class="col-6 col-md-4 col-lg-2">
            <a href="Movies.aspx" class="stat-card">
                <div class="stat-icon" style="background:#fce4ec"><i class="bi bi-film" style="color:#c62828"></i></div>
                <div><div class="stat-label">Movies</div><div class="stat-value"><asp:Label ID="lblMovies" runat="server" Text="0"/></div></div>
            </a>
        </div>
        <div class="col-6 col-md-4 col-lg-2">
            <a href="Theaters.aspx" class="stat-card">
                <div class="stat-icon" style="background:#e8f5e9"><i class="bi bi-building" style="color:#2e7d32"></i></div>
                <div><div class="stat-label">Theaters</div><div class="stat-value"><asp:Label ID="lblTheaters" runat="server" Text="0"/></div></div>
            </a>
        </div>
        <div class="col-6 col-md-4 col-lg-2">
            <a href="Halls.aspx" class="stat-card">
                <div class="stat-icon" style="background:#fff3e0"><i class="bi bi-door-open" style="color:#e65100"></i></div>
                <div><div class="stat-label">Halls</div><div class="stat-value"><asp:Label ID="lblHalls" runat="server" Text="0"/></div></div>
            </a>
        </div>
        <div class="col-6 col-md-4 col-lg-2">
            <a href="Showtimes.aspx" class="stat-card">
                <div class="stat-icon" style="background:#e3f2fd"><i class="bi bi-calendar3" style="color:#1565c0"></i></div>
                <div><div class="stat-label">Showtimes</div><div class="stat-value"><asp:Label ID="lblShowtimes" runat="server" Text="0"/></div></div>
            </a>
        </div>
        <div class="col-6 col-md-4 col-lg-2">
            <a href="Tickets.aspx" class="stat-card">
                <div class="stat-icon" style="background:#f3e5f5"><i class="bi bi-ticket-perforated" style="color:#6a1b9a"></i></div>
                <div><div class="stat-label">Tickets</div><div class="stat-value"><asp:Label ID="lblTickets" runat="server" Text="0"/></div></div>
            </a>
        </div>
    </div>
    <div class="mb-4">
        <div class="section-title"><i class="bi bi-grid me-2"></i>Quick Navigation</div>
        <div class="row g-3">
            <div class="col-4 col-md-2"><a href="Customers.aspx" class="quick-card"><i class="bi bi-people-fill" style="color:#3949ab"></i><span>Customers</span></a></div>
            <div class="col-4 col-md-2"><a href="Movies.aspx" class="quick-card"><i class="bi bi-film" style="color:#c62828"></i><span>Movies</span></a></div>
            <div class="col-4 col-md-2"><a href="Theaters.aspx" class="quick-card"><i class="bi bi-building" style="color:#2e7d32"></i><span>Theaters</span></a></div>
            <div class="col-4 col-md-2"><a href="Halls.aspx" class="quick-card"><i class="bi bi-door-open" style="color:#e65100"></i><span>Halls</span></a></div>
            <div class="col-4 col-md-2"><a href="Showtimes.aspx" class="quick-card"><i class="bi bi-calendar3" style="color:#1565c0"></i><span>Showtimes</span></a></div>
            <div class="col-4 col-md-2"><a href="Tickets.aspx" class="quick-card"><i class="bi bi-ticket-perforated" style="color:#6a1b9a"></i><span>Tickets</span></a></div>
        </div>
    </div>
</div>
</form>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
</body></html>