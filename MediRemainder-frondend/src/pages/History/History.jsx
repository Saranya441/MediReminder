import { useEffect, useState } from "react";

import {
  BarChart3,
  CalendarDays,
  Check,
  ListChecks,
  Minus,
  MoreVertical,
  Pill,
  TrendingUp,
  X,
} from "lucide-react";

import Sidebar from "../../components/Sidebar/Sidebar";
import "./History.css";
import { errorMessage } from "../../api/client";
import { historyApi } from "../../api/services";

function History() {
  const [statusFilter, setStatusFilter] = useState("All");
  const [dateFilter, setDateFilter] = useState("This Week");
  const [selectedDate, setSelectedDate] = useState("");

  const [medicineHistory, setMedicineHistory] = useState([]);
  const [summary, setSummary] = useState({ totalCount: 0, takenCount: 0, missedCount: 0, skippedCount: 0, adherencePercentage: 0 });
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState("");

  useEffect(() => {
    setLoading(true);
    historyApi.get({
      status: statusFilter,
      dateFilter: selectedDate ? undefined : dateFilter,
      selectedDate: selectedDate || undefined,
    }).then((result) => {
      setMedicineHistory(result.history || []);
      setSummary({
        totalCount: result.totalCount, takenCount: result.takenCount,
        missedCount: result.missedCount, skippedCount: result.skippedCount,
        adherencePercentage: result.adherencePercentage,
      });
      setPageError("");
    }).catch((error) => setPageError(errorMessage(error, "Could not load dose history.")))
      .finally(() => setLoading(false));
  }, [statusFilter, dateFilter, selectedDate]);

  const statusButtons = ["All", "Taken", "Missed", "Skipped"];

  const filteredHistory = medicineHistory;
  const takenCount = summary.takenCount;
  const missedCount = summary.missedCount;
  const skippedCount = summary.skippedCount;
  const adherence = summary.adherencePercentage;

  const getStatusIcon = (status) => {
    if (status === "Taken") {
      return <Check size={25} />;
    }

    if (status === "Missed") {
      return <X size={22} />;
    }

    return <Minus size={22} />;
  };

  const handleDateDropdownChange = (event) => {
    setDateFilter(event.target.value);

    setSelectedDate("");
  };

  const handleExactDateChange = (event) => {
    const value = event.target.value;

    setSelectedDate(value);

    if (value) {
      setDateFilter("All Dates");
    }
  };

  const handleClearFilters = () => {
    setStatusFilter("All");
    setDateFilter("This Week");
    setSelectedDate("");
  };

  return (
    <div className="history-page-layout">
      <Sidebar activePage="History" />

      <main className="history-page-main">
        {/* HEADER */}

        <header className="history-top-section">
          <div className="history-page-heading">
            <p className="history-breadcrumb">History</p>

            <h1>History</h1>

            <p className="history-subtitle">
              Track your medicine intake and adherence.
            </p>
          </div>

          <div className="history-motivation-card">
            <div className="history-motivation-icon">
              <TrendingUp size={25} />
            </div>

            <p>
              Small steps every day
              <br />
              make a big difference.
            </p>
          </div>
        </header>

        {(loading || pageError) && <p role={pageError ? "alert" : "status"}>{loading ? "Loading dose history..." : pageError}</p>}

        {/* ADHERENCE */}

        <section className="history-adherence-card">
          <div className="history-section-heading">
            <div className="history-section-icon">
              <BarChart3 size={22} />
            </div>

            <div>
              <h2>Medicine Adherence</h2>

              <p>Your overall medicine intake this week</p>
            </div>
          </div>

          <div className="history-adherence-grid">
            <div className="history-stat-card adherence">
              <div
                className="history-progress-circle"
                style={{
                  "--progress": `${adherence * 3.6}deg`,
                }}
              >
                <div className="history-progress-inner">{adherence}%</div>
              </div>

              <div className="history-stat-info">
                <strong>{adherence}%</strong>

                <span>Adherence</span>

                <small>Great job!</small>
              </div>
            </div>

            <div className="history-stat-card taken">
              <div className="history-stat-icon">
                <Pill size={27} />
              </div>

              <div className="history-stat-info">
                <strong>{takenCount}</strong>

                <span>Taken Doses</span>

                <small>On schedule</small>
              </div>
            </div>

            <div className="history-stat-card missed">
              <div className="history-stat-icon">
                <X size={26} />
              </div>

              <div className="history-stat-info">
                <strong>{missedCount}</strong>

                <span>Missed Doses</span>

                <small>Keep going</small>
              </div>
            </div>

            <div className="history-stat-card skipped">
              <div className="history-stat-icon">
                <Minus size={27} />
              </div>

              <div className="history-stat-info">
                <strong>{skippedCount}</strong>

                <span>Skipped Doses</span>

                <small>Stay consistent</small>
              </div>
            </div>
          </div>
        </section>

        {/* FILTERS */}

        <section className="history-filter-card">
          {/* STATUS FILTER */}

          <div className="history-status-filter">
            <h3>Filter by Status</h3>

            <div className="history-filter-buttons">
              {statusButtons.map((status) => (
                <button
                  key={status}
                  type="button"
                  className={`history-filter-button ${
                    statusFilter === status ? "active" : ""
                  }`}
                  onClick={() => setStatusFilter(status)}
                >
                  {status}
                </button>
              ))}
            </div>
          </div>

          <div className="history-filter-divider" />

          {/* DATE FILTER */}

          <div className="history-date-filter">
            <h3>Filter by Date</h3>

            <div className="history-date-filter-row">
              {/* CLICKABLE CALENDAR ICON */}

              <div
                className={`history-date-icon history-calendar-picker ${
                  selectedDate ? "active-date" : ""
                }`}
              >
                <CalendarDays size={20} />

                <input
                  type="date"
                  value={selectedDate}
                  onChange={handleExactDateChange}
                  className="history-hidden-date-input"
                  aria-label="Select exact date"
                />
              </div>

              {/* DROPDOWN */}

              <select
                className="history-date-dropdown"
                value={dateFilter}
                onChange={handleDateDropdownChange}
              >
                <option value="All Dates">All Dates</option>

                <option value="Today">Today</option>

                <option value="Yesterday">Yesterday</option>

                <option value="This Week">This Week</option>

                <option value="This Month">This Month</option>

                <option value="Last Month">Last Month</option>
              </select>

              {/* CLEAR EXACT DATE */}

              {selectedDate && (
                <button
                  type="button"
                  className="history-clear-exact-date"
                  onClick={() => {
                    setSelectedDate("");
                    setDateFilter("This Week");
                  }}
                  title="Clear selected date"
                >
                  <X size={15} />
                </button>
              )}
            </div>
          </div>
        </section>

        {/* ACTIVE FILTER */}

        {selectedDate && (
          <div className="history-selected-date-info">
            <span>Showing records for:</span>

            <strong>
              {new Date(`${selectedDate}T00:00:00`).toLocaleDateString(
                "en-US",
                {
                  day: "numeric",
                  month: "long",
                  year: "numeric",
                },
              )}
            </strong>

            <button
              type="button"
              onClick={() => {
                setSelectedDate("");
                setDateFilter("This Week");
              }}
            >
              Clear
            </button>
          </div>
        )}

        {/* HISTORY LIST */}

        <section className="history-list-card">
          <div className="history-section-heading history-list-heading">
            <div className="history-section-icon">
              <ListChecks size={22} />
            </div>

            <div>
              <h2>Medicine History</h2>

              <p>Your medicine intake records</p>
            </div>
          </div>

          <div className="history-record-list">
            {loading ? <p role="status">Loading records...</p> : filteredHistory.length > 0 ? (
              filteredHistory.map((item) => (
                <article className="history-record" key={item.id}>
                  <div
                    className={`history-record-status-icon ${item.status.toLowerCase()}`}
                  >
                    {getStatusIcon(item.status)}
                  </div>

                  <div className="history-record-info">
                    <h3>{item.medicine}</h3>

                    <p>
                      {item.time}

                      <span>•</span>

                      {item.date}
                    </p>

                    <small>{item.instruction}</small>
                  </div>

                  <div
                    className={`history-status-badge ${item.status.toLowerCase()}`}
                  >
                    {item.status}
                  </div>

                  <button
                    type="button"
                    className="history-record-menu"
                    aria-label={`More options for ${item.medicine}`}
                  >
                    <MoreVertical size={19} />
                  </button>
                </article>
              ))
            ) : (
              <div className="history-empty-state">
                <CalendarDays size={38} />

                <h3>No medicine records found</h3>

                <p>No records match the selected filters.</p>

                <button
                  type="button"
                  className="history-empty-clear"
                  onClick={handleClearFilters}
                >
                  Clear Filters
                </button>
              </div>
            )}
          </div>
        </section>
      </main>
    </div>
  );
}

export default History;
