import {
  Bell,
  ChevronDown,
  CheckCircle2,
  Clock3,
  FileText,
  CalendarDays,
  History as HistoryIcon,
  Heart,
  Plus,
} from "lucide-react";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import Sidebar from "../../components/Sidebar/Sidebar";
import "./Dashboard.css";
import { clearSession, errorMessage, getStoredUser, updateStoredUser } from "../../api/client";
import { authApi, dashboardApi, prescriptionsApi } from "../../api/services";

function Dashboard() {
  const navigate = useNavigate();
  const [isProfileOpen, setIsProfileOpen] = useState(false);
  const [dashboard, setDashboard] = useState(null);
  const [prescriptionCount, setPrescriptionCount] = useState(0);
  const [user, setUser] = useState(getStoredUser());
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState("");

  useEffect(() => {
    Promise.all([dashboardApi.get(), prescriptionsApi.list(), authApi.me()])
      .then(([data, prescriptions, currentUser]) => {
        setDashboard(data);
        setPrescriptionCount(prescriptions?.length ?? 0);
        setUser(currentUser);
        updateStoredUser(currentUser);
      })
      .catch((error) => setPageError(errorMessage(error, "Could not load your dashboard.")))
      .finally(() => setLoading(false));
  }, []);
  const firstName = user?.fullName?.split(" ")[0] || "there";
  const greeting = new Date().getHours() < 12 ? "Good Morning" : new Date().getHours() < 17 ? "Good Afternoon" : "Good Evening";
  return (
    <div className="dashboard-page">
      <Sidebar activePage="Dashboard" />

      <main className="dashboard-main">
        {/* Header */}

        <header className="dashboard-header">
          <div>
            <h1>
              {greeting}, {firstName}! <span>👋</span>
            </h1>

            <p>Here&apos;s your medication summary for today.</p>
          </div>

          <div className="dashboard-user-area">
            <button type="button" className="notification-button">
              <Bell size={19} />
              <span className="notification-dot"></span>
            </button>
            <div className="dashboard-profile-wrapper">
              <button
                type="button"
                className="dashboard-profile-button"
                onClick={() => setIsProfileOpen((previous) => !previous)}
              >
                <div className="dashboard-profile-avatar">S</div>

                <div className="dashboard-profile-text">
                  <strong>{user?.fullName || firstName}</strong>
                  <span>Patient</span>
                </div>

                <span
                  className={`dashboard-profile-arrow ${
                    isProfileOpen ? "open" : ""
                  }`}
                >
                  <ChevronDown size={16} />
                </span>
              </button>

              {isProfileOpen && (
                <div className="dashboard-profile-dropdown">
                  <div className="dashboard-dropdown-user">
                    <div className="dashboard-dropdown-avatar">S</div>

                    <div>
                      <h4>{user?.fullName || firstName}</h4>
                      <p>{user?.email || user?.mobileNumber || "MediReminder account"}</p>
                    </div>
                  </div>

                  <div className="dashboard-dropdown-divider" />

                  <button
                    type="button"
                    className="dashboard-dropdown-item"
                    onClick={() => {
                      setIsProfileOpen(false);
                      navigate("/profile");
                    }}
                  >
                    My Profile
                  </button>

                  <button
                    type="button"
                    className="dashboard-dropdown-item"
                    onClick={() => {
                      setIsProfileOpen(false);
                      navigate("/settings");
                    }}
                  >
                    Settings
                  </button>

                  <div className="dashboard-dropdown-divider" />

                  <button
                    type="button"
                    className="dashboard-dropdown-item dashboard-dropdown-logout"
                    onClick={() => {
                      setIsProfileOpen(false);
                      clearSession();
                      navigate("/login");
                    }}
                  >
                    Log Out
                  </button>
                </div>
              )}
            </div>
          </div>
        </header>

        {/* Summary cards */}

        <section className="dashboard-summary-grid">
          <article className="summary-card">
            <span className="summary-label">Today&apos;s Medicines</span>

            <strong className="summary-value">{dashboard?.todayDoseCount ?? 0}</strong>

            <div className="summary-footer success-text">
              <CheckCircle2 size={14} />
              <span>{dashboard?.takenToday ?? 0} taken</span>

              <span className="summary-divider">•</span>

              <span className="muted-text">{(dashboard?.pendingToday ?? 0) + (dashboard?.upcomingToday ?? 0)} pending</span>
            </div>
          </article>

          <article className="summary-card">
            <span className="summary-label">Adherence Rate</span>

            <strong className="summary-value">{dashboard?.adherencePercentage ?? 0}%</strong>

            <div className="summary-footer success-text">
              <span>Based on recorded doses this week</span>
            </div>
          </article>

          <article className="summary-card">
            <span className="summary-label">Next Dose</span>

            <strong className="summary-value">{dashboard?.nextDose?.time || "—"}</strong>

            <div className="summary-footer muted-text">{dashboard?.nextDose?.medicine || "No upcoming dose"}</div>
          </article>

          <article className="summary-card">
            <span className="summary-label">Uploaded Prescriptions</span>

            <strong className="summary-value">{prescriptionCount}</strong>

            <button type="button" className="summary-link" onClick={() => navigate("/upload-prescription")}>
              View Details
            </button>
          </article>
        </section>
        {(loading || pageError) && <p role={pageError ? "alert" : "status"}>{loading ? "Loading your dashboard..." : pageError}</p>}

        {/* Main dashboard */}

        <section className="dashboard-content-grid">
          {/* Schedule */}

          <article className="dashboard-panel schedule-panel">
            <div className="panel-heading">
              <h2>Today&apos;s Schedule</h2>

              <button
                type="button"
                className="panel-link"
                onClick={() => navigate("/calendar")}
              >
                View Calendar
              </button>
            </div>

            <div className="schedule-list">
              {dashboard?.todaySchedule?.length ? dashboard.todaySchedule.map((item) => (
                <div className="schedule-item" key={`${item.medicineId}-${item.scheduledDateTime}`}>
                  <div className={`schedule-status-icon ${item.status.toLowerCase()}`}>
                    {item.status === "Taken" ? <CheckCircle2 size={16} /> : <Clock3 size={16} />}
                  </div>
                  <div className="schedule-time">{item.time}</div>
                  <div className="schedule-medicine">{item.medicine}</div>
                  <span className={`status-badge ${item.status.toLowerCase()}`}>{item.status}</span>
                </div>
              )) : !loading && <p>No medicines scheduled for today.</p>}
            </div>
          </article>

          {/* Right column */}

          <div className="dashboard-right-column">
            <article className="dashboard-panel quick-actions-panel">
              <div className="panel-heading">
                <h2>Quick Actions</h2>
              </div>

              <div className="quick-actions-grid">
                <button
                  type="button"
                  className="quick-action"
                  onClick={() => navigate("/add-medicine")}
                >
                  <span className="quick-icon blue">
                    <Plus size={18} />
                  </span>

                  <span>Add Medicine</span>
                </button>

                <button
                  type="button"
                  className="quick-action"
                  onClick={() => navigate("/upload-prescription")}
                >
                  <span className="quick-icon purple">
                    <FileText size={18} />
                  </span>

                  <span>Upload Prescription</span>
                </button>

                <button
                  type="button"
                  className="quick-action"
                  onClick={() => navigate("/history")}
                >
                  <span className="quick-icon pink">
                    <HistoryIcon size={18} />
                  </span>

                  <span>View History</span>
                </button>

                <button
                  type="button"
                  className="quick-action"
                  onClick={() => navigate("/calendar")}
                >
                  <span className="quick-icon cyan">
                    <CalendarDays size={18} />
                  </span>

                  <span>Calendar</span>
                </button>
              </div>
            </article>

            <article className="health-card">
              <div className="health-card-content">
                <span>Your Health Matters</span>

                <p>Stay consistent. Stay healthy.</p>
              </div>

              <div className="health-icon">
                <Heart size={34} />
              </div>
            </article>
          </div>
        </section>
      </main>
    </div>
  );
}

export default Dashboard;
